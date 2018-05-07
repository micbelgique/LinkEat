using LinkEat.Repositories;
using LinkEat.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Bot.Connector;
using Microsoft.Bot.Connector.Authentication;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using Microsoft.Bot.Schema;
using LinkEat.Services;

namespace LinkEat.Controllers
{
    [Route("api/[controller]")]
    public class MessagesController: Controller
    {
        private readonly IConfiguration _configuration;
        private readonly string _luisAppId;
        private readonly string _luisSubscriptionKey;
        private readonly string _slackToken;

        private readonly MealRepository _mealRepository;
        private readonly OrderRepository _orderRepository;
        private readonly PlaceRepository _placeRepository;
        private readonly AppointmentRepository _appointmentRepository;
        private readonly UserRepository _userRepository;

        public MessagesController(IConfiguration configuration, MealRepository mealRepository, OrderRepository orderRepository, PlaceRepository placeRepository, AppointmentRepository appointmentRepository, UserRepository userRepository)
        {
            _configuration = configuration;
            _luisAppId = configuration["LUIS:AppId"];
            _luisSubscriptionKey = configuration["LUIS:SubscriptionKey"];
            _slackToken = configuration["Slack:Token"];

            _mealRepository = mealRepository;
            _orderRepository = orderRepository;
            _placeRepository = placeRepository;
            _appointmentRepository = appointmentRepository;
            _userRepository = userRepository;
        }

        [Authorize(Roles = "Bot")]
        [HttpPost]
        public async Task<OkResult> Post([FromBody] Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                //MicrosoftAppCredentials.TrustServiceUrl(activity.ServiceUrl);
                var credentials = new MicrosoftAppCredentials(_configuration["BotConnectionInfo:AppId"], _configuration["BotConnectionInfo:AppPassword"]);
                var connector = new ConnectorClient(new Uri(activity.ServiceUrl), credentials);

                await connector.Conversations.ReplyToActivityAsync(activity.CreateReply("Je suis en train de traiter votre requête."));

                string botReply;

                switch (activity.Text.ToLowerInvariant())
                {
                    case "what's my appointment?":
                        try
                        {
                            User user = await _userRepository.GetBySlackId(activity.From.Id.Split(":")[0]);
                            try
                            {
                                Appointment appointment = await _appointmentRepository.GetByDateAndByUserAsync(DateTime.Today, user.Id);
                                try
                                {
                                    var place = await _placeRepository.GetById(appointment.PlaceId);
                                    botReply = $"Vous avez un rendez-vous prévu au {place.Name} pour {appointment.Date.Hour}h{appointment.Date.Minute}.";
                                }catch (Exception){
                                    botReply = "Je suis désolé, mais il semble qu'une erreur empêche de déterminer votre lieu de rendez-vous.";
                                }
                            }catch (Exception){
                                botReply = "Je suis désolé mais il semblerait que vous n'ayez pas de rendez-vous aujourd'hui.";
                            }
                        }catch (Exception){
                            botReply = "Je suis désolé mais je ne vous ai pas trouvé dans ma liste d'utilisateurs.";
                        }
                        break;

                    case "cancel my appointment":
                        try
                        {
                            User user = await _userRepository.GetBySlackId(activity.From.Id.Split(":")[0]);
                            try
                            {
                                Appointment appointment = await _appointmentRepository.GetByDateAndByUserAsync(DateTime.Today, user.Id);
                                try
                                {
                                    await _appointmentRepository.Delete(appointment);
                                    botReply = "Le rendez-vous a bel et bien été annulé";
                                }catch (Exception){
                                    botReply = "Je suis désolé, mais il semble qu'une erreur empêche la suppression de votre rendez-vous.";
                                }
                            }catch (Exception){
                                botReply = "Je suis désolé mais il semblerait que vous n'ayez pas de rendez-vous aujourd'hui.";
                            }

                        }catch (Exception){
                            botReply = "Je suis désolé mais je ne vous ai pas trouvé dans ma liste d'utilisateurs.";
                        }
                        break;
                    
                    case "what's my order?":
                        try
                        {
                            User user = await _userRepository.GetBySlackId(activity.From.Id.Split(":")[0]);
                            try
                            {
                                Order order = await _orderRepository.GetByUser(user.Id);
                                try
                                {
                                    var place = await _placeRepository.GetById(order.PlaceId);
                                    botReply = $"Vous avez commandé {order.Meal} chez {place.Name}.";
                                }catch (Exception){
                                    botReply = "Je suis désolé, mais il semble qu'une erreur empêche de retrouver votre commande.";
                                }
                            }catch (Exception){
                                botReply = "Je suis désolé mais il semblerait que vous n'ayez pas commandé aujourd'hui.";
                            }
                        }catch (Exception){
                            botReply = "Je suis désolé mais je ne vous ai pas trouvé dans ma liste d'utilisateurs.";
                        }
                        break;

                    case "cancel my order":
                        try
                        {
                            User user = await _userRepository.GetBySlackId(activity.From.Id.Split(":")[0]);
                            try
                            {
                                Order order = await _orderRepository.GetByUser(user.Id);
                                try
                                {
                                    await _orderRepository.Delete(order);
                                    botReply = "La commande a bel et bien été annulée";
                                }catch (Exception){
                                    botReply = "Je suis désolé, mais il semble qu'une erreur empêche la suppression de votre commande.";
                                }
                            }catch (Exception){
                                botReply = "Je suis désolé mais il semblerait que vous n'ayez pas passé de commande aujourd'hui.";
                            }

                        }catch (Exception){
                            botReply = "Je suis désolé mais je ne vous ai pas trouvé dans ma liste d'utilisateurs.";
                        }
                        break;

                    case "list today's orders":
                        try
                        {
                            List<Order> orders = await _orderRepository.GetByPlace(1);
                            botReply = "Les commandes d'aujourd'hui sont:\n";

                            foreach(Order order in orders)
                            {
                                botReply += $"{order.Meal}.\n";
                            }
                        }catch (Exception){
                            botReply = "Je suis désolé mais je n'ai pas trouvé de commande pour aujourd'hui.";
                        }
                        break;

                    case "list today's appointments":
                        try
                        {
                            List<Appointment> appointments = await _appointmentRepository.GetByDateAsync(DateTime.Today);
                            botReply = "Les rendez-vous d'aujourd'hui sont:\n";

                            foreach(Appointment appointment in appointments)
                            {
                                var place = await _placeRepository.GetById(appointment.PlaceId);
                                botReply += $"{place.Name} pour {appointment.Date.Hour}h{appointment.Date.Minute}.\n";
                            }
                        }catch (Exception){
                            botReply = "Je suis désolé mais je n'ai pas trouvé de commande pour aujourd'hui.";
                        }
                        break;

                    default:
                        using(var client = new HttpClient())
                        {
                            var queryString = HttpUtility.ParseQueryString(string.Empty);

                            // The request header contains your subscription key
                            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", _luisSubscriptionKey);

                            // The "q" parameter contains the utterance to send to LUIS
                            queryString["q"] = activity.Text;

                            // These optional request parameters are set to their default values
                            queryString["timezoneOffset"] = "0";
                            queryString["verbose"] = "false";
                            queryString["spellCheck"] = "false";
                            queryString["staging"] = "false";

                            var uri = "https://westeurope.api.cognitive.microsoft.com/luis/v2.0/apps/" + _luisAppId + "?" + queryString;
                            var response = await client.GetAsync(uri);

                            var strResponseContent = await response.Content.ReadAsStringAsync();
                            DeserializerModel luisPrediction = JsonConvert.DeserializeObject<DeserializerModel>(strResponseContent);
                            botReply = luisPrediction.topScoringIntent.intent;

                            User user;

                            try
                            {
                                user = await _userRepository.GetBySlackId(activity.From.Id.Split(":")[0]);
                            }catch (Exception){
                                Object request = new 
                                {
                                    token = _slackToken,
                                    user = activity.From.Id.Split(":")[0]
                                };

                                var stringContent = new StringContent(JsonConvert.SerializeObject(request), Encoding.UTF8, "application/json");
                                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _slackToken);
                                var requestResponse = await client.PostAsync( "https://slack.com/api/users.profile.get", stringContent);
                                var userInfos = await requestResponse.Content.ReadAsAsync<ProfileDeserializerModel>();


                                if(userInfos.profile.first_name.Length > 0 && userInfos.profile.last_name.Length > 0)
                                {
                                    user = await _userRepository.Create(new User
                                    {
                                        FirstName = userInfos.profile.first_name,
                                        LastName = userInfos.profile.last_name,
                                        Email = userInfos.profile.email,
                                        SlackId = activity.From.Id.Split(":")[0]
                                    });
                                }
                                else if(userInfos.profile.display_name_normalized.Length > 0)
                                {
                                    if(userInfos.profile.display_name_normalized.Contains(" "))
                                    {
                                        user = await _userRepository.Create(new User
                                        {
                                            FirstName = userInfos.profile.display_name_normalized.Split(" ")[0],
                                            LastName = userInfos.profile.display_name_normalized.Split(" ")[1],
                                            Email = userInfos.profile.email,
                                            SlackId = activity.From.Id.Split(":")[0]
                                        });
                                    }
                                    else
                                    {
                                        user = await _userRepository.Create(new User
                                        {
                                            FirstName = userInfos.profile.display_name_normalized,
                                            LastName = "",
                                            Email = userInfos.profile.email,
                                            SlackId = activity.From.Id.Split(":")[0]
                                        });
                                    }
                                }     
                                else if(userInfos.profile.real_name_normalized.Length > 0)
                                {
                                    if(userInfos.profile.real_name_normalized.Contains(" "))
                                    {
                                        user = await _userRepository.Create(new User
                                        {
                                            FirstName = userInfos.profile.real_name_normalized.Split(" ")[0],
                                            LastName = userInfos.profile.real_name_normalized.Split(" ")[1],
                                            Email = userInfos.profile.email,
                                            SlackId = activity.From.Id.Split(":")[0]
                                        });
                                    }
                                    else
                                    {
                                        user = await _userRepository.Create(new User
                                        {
                                            FirstName = userInfos.profile.real_name_normalized,
                                            LastName = "",
                                            Email = userInfos.profile.email,
                                            SlackId = activity.From.Id.Split(":")[0]
                                        });
                                    }
                                }
                                else
                                {
                                    user = await _userRepository.Create(new User
                                    {
                                        FirstName = "",
                                        LastName = "",
                                        Email = userInfos.profile.email,
                                        SlackId = activity.From.Id.Split(":")[0]
                                    });
                                }
                            }

                            if (luisPrediction.topScoringIntent.intent.Equals("Order"))
                            {
                                botReply = await Order(luisPrediction.entities, user.SlackId);
                            }else if (luisPrediction.topScoringIntent.intent.Equals("Appointment")){
                                botReply = await Book(luisPrediction.entities, user.SlackId);
                            }else if(luisPrediction.topScoringIntent.intent.Equals("Greetings")){
                                botReply = Greet(luisPrediction.entities);
                            }else{
                                botReply = "Désolé je n'ai pas compris.. Pouvez-vous répéter différemment?";
                            }
                         };
                        break;
                }

                var reply = activity.CreateReply(botReply);
                await connector.Conversations.ReplyToActivityAsync(reply);
            }
            else
            {
                //HandleSystemMessage(activity);
            }
            return Ok();
        }

        #region HELPERS
        private async Task<string> Order(List<Entity> entities, string userSlackId)
        {
            string answer = "Je n'ai pas compris votre commande.";

            string dishToOrder = "";
            string sauceToOrder = "";
            string cruditesToOrder = "";

            for(int entitiesIndex = 0; entitiesIndex < entities.Count; entitiesIndex++)
            {
                if (entities[entitiesIndex].type.Equals("dish"))
                {
                    dishToOrder = entities[entitiesIndex].entity;
                }else if (entities[entitiesIndex].type.Equals("sauce")){
                    if(sauceToOrder.Length > 0)
                    {
                        sauceToOrder += " " + entities[entitiesIndex].entity;
                    }else{
                        sauceToOrder = entities[entitiesIndex].entity;
                    }
                }else if (entities[entitiesIndex].type.Equals("vegetables")){
                    if (entities[entitiesIndex].entity.ToLowerInvariant().Contains("pas") || entities[entitiesIndex].entity.ToLowerInvariant().Contains("sans"))
                    {
                        cruditesToOrder = "sans crudités";
                    }else{
                        cruditesToOrder = "crudités";
                    }
                }
            }

            List<Meal> meals = await _mealRepository.GetAllAsync();
            int mealId = 0; //using id instead of bool so we don't need 2 var

            for(int mealIndex = 0; mealIndex < meals.Count; mealIndex++)
            {
                if (mealId == 0)
                {
                    if (dishToOrder.ToLowerInvariant().Contains(meals[mealIndex].Dish.ToLowerInvariant()))
                    {
                        mealId = meals[mealIndex].Id;
                    }else if(meals[mealIndex].Dish.ToLowerInvariant().Contains(dishToOrder.ToLowerInvariant()) && dishToOrder.Length > 0){
                        mealId = meals[mealIndex].Id;
                        dishToOrder = meals[mealIndex].Dish;
                    }
                }
            }

            if (dishToOrder.Length > 0)
            {
                if (mealId > 0)
                {
                    User user = await _userRepository.GetBySlackId(userSlackId);
                    Order newOrder = new Order
                    {
                        Date = DateTime.Now,
                        MealId = mealId,
                        Meal = $"{dishToOrder} {sauceToOrder} {cruditesToOrder}",
                        PlaceId = 1,
                        UserId = user.Id
                    };

                    await _orderRepository.Create(newOrder);
                    answer = $"Votre commande d'un {dishToOrder} {sauceToOrder} {cruditesToOrder} a bien été enregistrée.";

                    using(var client = new HttpClient())
                    {
                        DateTime now = DateTime.Now;
                        int hourDiff = 11 - now.Hour;
                        int minDiff = now.Minute;
                        int secDiff = now.Second;

                        int time = (hourDiff*60 - minDiff)*60 + (60 - secDiff);
                        
                        if(time > 0)
                        { 
                            var reminder = new SlackReminder
                            {
                                text = $"Il est l'heure de payer les {meals.Find(meal => meal.Id == mealId).Price}€ pour votre commande.",
                                time = time,
                                user = userSlackId
                            };

                            var stringContent = new StringContent(JsonConvert.SerializeObject(reminder), Encoding.UTF8, "application/json");
                            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _slackToken);
                            await client.PostAsync( "https://slack.com/api/reminders.add", stringContent);
                        }
                    }
                }else{
                    answer = "Désolé mais le plat que vous souhaitez n'est pas disponible.";
                }
            }

            return answer;
        }

        private async Task<string> Book(List<Entity> entities, string userSlackId)
        {
            string answer = "Je n'ai pas compris l'endroit où vous souhaitez allez.";

            List<Models.Place> places = await _placeRepository.GetAllAsync();
            Models.Place placeToGo = null;
            DateTime date = DateTime.MinValue;

            for(int placeIndex = 0; placeIndex < places.Count; placeIndex++)
            {
                for(int entityIndex = 0; entityIndex < entities.Count; entityIndex++)
                {
                    if (entities[entityIndex].type.ToLowerInvariant().Equals("place"))
                    {
                        if (places[placeIndex].Name.ToLowerInvariant().Contains(entities[entityIndex].entity.ToLowerInvariant()))
                        {
                            placeToGo = places[placeIndex];
                        }
                    }

                    if (entities[entityIndex].type.ToLowerInvariant().Equals("time"))
                    {
                        if (entities[entityIndex].entity.ToLowerInvariant().Contains("diner") || entities[entityIndex].entity.ToLowerInvariant().Contains("midi"))
                        {
                            if (entities[entityIndex].entity.ToLowerInvariant().Contains("trente") || entities[entityIndex].entity.ToLowerInvariant().Contains("30"))
                            {
                                date = DateTime.Today.AddHours(12).AddMinutes(30);
                            }else{
                                date = DateTime.Today.AddHours(12);
                            }
                        }else if (entities[entityIndex].entity.ToLowerInvariant().Contains("h")){
                            var timeAsked = entities[entityIndex].entity.ToLowerInvariant().Split('h');
                            date = DateTime.Today.AddHours(Double.Parse(timeAsked[0]));

                            if(timeAsked[1].Length > 0)
                            {
                                date = date.AddMinutes(Double.Parse(timeAsked[1]));
                            }
                        }
                    }else if(placeToGo != null){
                        answer = $"Je n'ai pas compris l'heure à laquelle vous souhaitez aller au {placeToGo.Name}";
                    }
                }
            }

            if (placeToGo != null && date != DateTime.MinValue)
            {
                User user = await _userRepository.GetBySlackId(userSlackId);
                Appointment appointment = new Appointment
                {
                    Date = date,
                    PlaceId = placeToGo.Id,
                    Reminded = false,
                    RemindedDepart = false,
                    UserId = user.Id
                };
                await _appointmentRepository.Create(appointment);
                answer = $"Votre rendez-vous au {placeToGo.Name} à {date.Hour}h{date.Minute} a bien été enregistré.";

                using(var client = new HttpClient())
                {
                    DateTime now = DateTime.Now;
                    int hourDiff = date.Hour - now.Hour;
                    int minDiff = date.Minute - now.Minute;
                    int secDiff = now.Second;

                    int time = (hourDiff*60 + minDiff)*60 + (60 - secDiff);

                    if(time > 0)
                    {
                        var reminder = new SlackReminder
                        {
                            text = $"Il est l'heure de partir pour {placeToGo.Name}",
                            time = time,
                            user = userSlackId
                        };

                        var stringContent = new StringContent(JsonConvert.SerializeObject(reminder), Encoding.UTF8, "application/json");
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _slackToken);
                        await client.PostAsync( "https://slack.com/api/reminders.add", stringContent);
                    }
                }
            }

            return answer;
        }

        private string Greet(List<Entity> entities)
        {
            string answer = "Bonjour";
            if(entities.Count == 1)
            {
                switch (entities[0].type)
                {
                    case "hello":
                        answer = "Salut! Comment vas-tu?";
                        break;

                    case "hru":
                        answer = "Je vais bien.. Et toi?";
                        break;

                    case "goodbye":
                        answer = "A la prochaine! :)";
                        break;
                }
            }

            return answer;
        }
        #endregion

        #region INNERCLASSES
        private class Entity
        {
            public string entity { get; set; }
            public string type { get; set; }
            public int startIndex { get; set; }
            public int endIndex { get; set; }
            public double score { get; set; }
        }

        private class Intent
        {
            public string intent { get; set; }
            public double score { get; set; }
        }

        private class DeserializerModel
        {
            public Intent topScoringIntent { get; set; }
            public List<Entity> entities { get; set; }
        }

        private class ProfileDeserializerModel
        {
            public Profile profile { get; set; }
        }

        private class Profile
        {
            public string first_name { get; set; }
            public string last_name { get; set; }
            public string real_name_normalized { get; set; }
            public string display_name_normalized { get; set; }
            public string email { get; set; }
        }

        private class SlackReminder
        {
            public string text { get; set; }
            public int time { get; set; } //the Unix timestamp (up to five years from now) OR the number of seconds until the reminder (if within 24 hours)
            public string user { get; set; }
        }

        private class SlackMessage
        {
            public string channel { get; set; }

            public string text { get; set; }

            public bool as_user { get; set; }

            public string username { get; set; }
        }
        #endregion
    }
}
