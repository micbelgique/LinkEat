using LUIS_Trainer_LinkEat.Models;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using LUIS_Trainer_LinkEat.Repositories;
using Newtonsoft.Json;

namespace LUIS_Trainer_LinkEat
{
    public class LuisManager
    {
        private readonly MealRepository _mealRepository;
        private readonly SauceRepository _sauceRepository;
        private readonly VegetableRepository _vegetableRepository;
        private readonly PlaceRepository _placeRepository;

        private const string _appVersion = "0.1";
        private readonly string _authoringKey;
        private readonly string _path;

        public LuisManager(IConfiguration configuration, MealRepository mealRepository, SauceRepository sauceRepository, VegetableRepository vegetableRepository, PlaceRepository placeRepository)
        {
            _mealRepository = mealRepository;
            _sauceRepository = sauceRepository;
            _vegetableRepository = vegetableRepository;
            _placeRepository = placeRepository;

            string appId = configuration["LUIS:AppId"];
            _authoringKey = configuration["LUIS:SubscriptionKey"];
            _path = $"https://westeurope.api.cognitive.microsoft.com/luis/api/v2.0/apps/{appId}/versions/{_appVersion}/";
        }

        #region LUIS FUNCTIONS
        //Maximum of 14900 utterances
        public async Task AddUtterances()
        {
            int max = 15000 / (int)Math.Ceiling(_mealRepository.GetAllAsync().Result.Count * _sauceRepository.GetAllAsync().Result.Count * Math.Pow(_vegetableRepository.GetAllAsync().Result.Count + 3, 2) * (quantifiers.Count + 1) * _placeRepository.GetAllAsync().Result.Count * 3 * times.Count);
            
            int count = 0;
            int step = 0;

            do
            {
                step = 0;

                Console.WriteLine("Starting orders");
                List<Utterance> ordersUtterances = await GenerateOrders();
                step += ordersUtterances.Count;

                //Max 100 utterances at a time
                for (int i = 0; i <= ordersUtterances.Count / 100; i++)
                {
                    List<Utterance> utterancesToSend;
                    if (i == ordersUtterances.Count / 100)
                    {
                        utterancesToSend = ordersUtterances.GetRange(i * 100, ordersUtterances.Count % 100);
                    }
                    else
                    {
                        utterancesToSend = ordersUtterances.GetRange(i * 100, 100);
                    }

                    string uri = _path + "examples";
                    string requestBody = JsonConvert.SerializeObject(utterancesToSend);

                    HttpResponseMessage response = await SendPost(uri, requestBody);
                    Console.WriteLine("Result code: " + response.StatusCode);

                    if (response.StatusCode == (HttpStatusCode)207)
                    {
                        Console.WriteLine("Request partially failed: See response content for more insight");
                        Console.ReadLine();
                        Console.WriteLine("-------------------------------------------------------");
                        string result = await response.Content.ReadAsStringAsync();
                        Console.WriteLine(JsonPrettyPrint(result));
                        Console.WriteLine("-------------------------------------------------------");
                        Console.ReadLine();
                    }
                    else
                    {
                        Console.WriteLine("Added utterances.");
                    }
                }

                Console.WriteLine("Orders done");
                Console.WriteLine("Starting appointments");

                List<Utterance> appointmentsUtterances = await GenerateAppointments();
                step += appointmentsUtterances.Count;

                //Max 100 utterances at a time
                for (int i = 0; i <= appointmentsUtterances.Count / 100; i++)
                {
                    List<Utterance> utterancesToSend;
                    if(i == appointmentsUtterances.Count / 100)
                    {
                        utterancesToSend = appointmentsUtterances.GetRange(i * 100, appointmentsUtterances.Count % 100);
                    }else{
                        utterancesToSend = appointmentsUtterances.GetRange(i * 100, 100);
                    }
                
                    string uri = _path + "examples";
                    string requestBody = JsonConvert.SerializeObject(utterancesToSend);

                    HttpResponseMessage response = await SendPost(uri, requestBody);
                    Console.WriteLine("Result code: " + response.StatusCode);

                    if (response.StatusCode == (HttpStatusCode) 207)
                    {
                        Console.WriteLine("Request partially failed: See response content for more insight");
                        Console.ReadLine();
                        Console.WriteLine("-------------------------------------------------------");
                        string result = await response.Content.ReadAsStringAsync();
                        Console.WriteLine(JsonPrettyPrint(result));
                        Console.WriteLine("-------------------------------------------------------");
                        Console.ReadLine();
                    }
                    else
                    {
                        Console.WriteLine("Added utterances.");
                    }
                }

                count += step;
            }while((count+step) <= 15000);

            await Train("");
            Console.WriteLine("Training started.");
            Console.ReadLine();
        }

        public async Task Train(string requestBody)
        {
            string uri = _path + "train";

            await SendPost(uri, requestBody);
            Console.WriteLine("Sent training request.");
            await Status();
        }
        #endregion

        #region LUIS HELPERS
        public async Task<HttpResponseMessage> SendGet(string uri)
        {
            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                request.Method = HttpMethod.Get;
                request.RequestUri = new Uri(uri);
                request.Headers.Add("Ocp-Apim-Subscription-Key", _authoringKey);
                return await client.SendAsync(request);
            }
        }

        public async Task<HttpResponseMessage> SendPost(string uri, string requestBody)
        {
            using (var client = new HttpClient())
            using (var request = new HttpRequestMessage())
            {
                request.Method = HttpMethod.Post;
                request.RequestUri = new Uri(uri);
                request.Content = new StringContent(requestBody, Encoding.UTF8, "text/json");
                request.Headers.Add("Ocp-Apim-Subscription-Key", _authoringKey);

                return await client.SendAsync(request);
            }
        }

        public async Task Status()
        {
            var response = await SendGet(_path + "train");
            var result = await response.Content.ReadAsStringAsync();
            Console.WriteLine("Requested training status.");
            Console.WriteLine(result);
        }

        private string JsonPrettyPrint(string json)
        {
            if (string.IsNullOrEmpty(json))
                return string.Empty;

            json = json.Replace(Environment.NewLine, "").Replace("\t", "");

            StringBuilder sb = new StringBuilder();
            bool quote = false;
            bool ignore = false;
            int offset = 0;
            int indentLength = 3;

            foreach (char ch in json)
            {
                switch (ch)
                {
                    case '"':
                        if (!ignore) quote = !quote;
                        break;
                    case '\'':
                        if (quote) ignore = !ignore;
                        break;
                }

                if (quote)
                    sb.Append(ch);
                else
                {
                    switch (ch)
                    {
                        case '{':
                        case '[':
                            sb.Append(ch);
                            sb.Append(Environment.NewLine);
                            sb.Append(new string(' ', ++offset * indentLength));
                            break;
                        case '}':
                        case ']':
                            sb.Append(Environment.NewLine);
                            sb.Append(new string(' ', --offset * indentLength));
                            sb.Append(ch);
                            break;
                        case ',':
                            sb.Append(ch);
                            sb.Append(Environment.NewLine);
                            sb.Append(new string(' ', offset * indentLength));
                            break;
                        case ':':
                            sb.Append(ch);
                            sb.Append(' ');
                            break;
                        default:
                            if (ch != ' ') sb.Append(ch);
                            break;
                    }
                }
            }

            return sb.ToString().Trim();
        }
        #endregion

        #region UTTERANCE GENERATORS

        private readonly List<string> orderingSentenceStarters = new List<string>
        {
            "",
            "Je veux ",
            "Je veux avoir ",
            "Je veux commander ",
            "Je voudrais ",
            "Je voudrais avoir ",
            "Je voudrais commander ",
            "Moi vouloir ",
            "J'aimerais ",
            "J'aimerais avoir ",
            "J'aimerais commander ",
            "Je souhaite ",
            "Je souhaite avoir ",
            "Je souhaite commander ",
            "Je souhaiterais ",
            "Je souhaiterais avoir ",
            "Je souhaiterais commander ",
            "Apporte moi ",
            "Donne moi ",
            "Commande moi ",
            "File moi ",
            "Femme apporte moi ",
            "Homme apporte moi ",
            "LinkEat apporte moi ",
            "Femme donne moi ",
            "Homme donne moi ",
            "LinkEat donne moi ",
            "Femme commande moi ",
            "Homme commande moi ",
            "LinkEat commande moi ",
            "Femme file moi ",
            "Homme file moi ",
            "LinkEat file moi "
        };

        private readonly List<string> bookingSentenceStarters = new List<string>
        {
            "",
            "Je veux aller ",
            "Je voudrais aller ",
            "J'aimerais aller ",
            "Réserve ",
            "LinkEat réserve ",
            "RDV ",
            "Rendez-vous "
        };

        private readonly List<string> quantifiers = new List<string>
        {
            "un",
            "1",
            "2",
            "3"
        };

        private readonly List<string> times = new List<string>
        {
            "midi",
            "midi trente",
            "midi 30",
            "12h",
            "12h30",
            "13h",
            "13h30",
            "14h"
        };

        private async Task<List<Utterance>> GenerateOrders()
        {
            List<Utterance> orders = new List<Utterance>();

            List<Meal> meals = await _mealRepository.GetAllAsync();
            List<Sauce> sauces = await _sauceRepository.GetAllAsync();
            List<Vegetable> vegetables = await _vegetableRepository.GetAllAsync();

            //To generate meal without sauce
            sauces.Add(new Sauce{
                Id = 0,
                Name = ""
            });

            Random rdm = new Random();

            //Random pick for orderingSentenceStarters
            int index = rdm.Next(0, orderingSentenceStarters.Count - 1);
            string orderingSentenceStarter = orderingSentenceStarters[index];

            for (int mealIndex = 0; mealIndex < meals.Count; mealIndex++)
            {
                //Check if the dish already has a sauce or not
                if(sauces.FindIndex(sauce => meals[mealIndex].Dish.ToLowerInvariant().Contains(sauce.Name)) != -1 && meals[mealIndex].SauceId != null)
                {
                    int sauceIndex = rdm.Next(0, sauces.Count);

                    //Unspecific vegetables
                    List<string> nonSpecificVegetables = new List<string>{"crudités", "crud", ""};
                    int nonSpecificVegetableIndex = rdm.Next(0, nonSpecificVegetables.Count);

                    orders.Add(GetOrderingUtterance(orderingSentenceStarter, "", meals[mealIndex].Dish, sauces[sauceIndex].Name, new List<string>{nonSpecificVegetables[nonSpecificVegetableIndex]}));

                    int quantifierIndex = rdm.Next(0, quantifiers.Count);
                    orders.Add(GetOrderingUtterance(orderingSentenceStarter, quantifiers[quantifierIndex], meals[mealIndex].Dish, sauces[sauceIndex].Name,  new List<string>{nonSpecificVegetables[nonSpecificVegetableIndex]}));

                    //Loop for some possible vegetables combinations
                    int chosenVegetablesAmount = rdm.Next(0, 3);
                    List<string> chosenVegetables = new List<string>();

                    while(chosenVegetablesAmount != 0){
                        string chosenVegetable = vegetables[rdm.Next(0, vegetables.Count)].Name;
                        if (!chosenVegetables.Contains(chosenVegetable))
                        {
                            chosenVegetables.Add(chosenVegetable);
                            chosenVegetablesAmount--;
                        }
                    }

                    orders.Add(GetOrderingUtterance(orderingSentenceStarter, "", meals[mealIndex].Dish, sauces[sauceIndex].Name, chosenVegetables));
                    orders.Add(GetOrderingUtterance(orderingSentenceStarter, quantifiers[quantifierIndex], meals[mealIndex].Dish, sauces[sauceIndex].Name, chosenVegetables));
                }else{
                    //Unspecific vegetables
                    List<string> nonSpecificVegetables = new List<string>{"crudités", "crud", ""};
                    int nonSpecificVegetableIndex = rdm.Next(0, nonSpecificVegetables.Count);

                    orders.Add(GetOrderingUtterance(orderingSentenceStarter, "", meals[mealIndex].Dish, "", new List<string>{nonSpecificVegetables[nonSpecificVegetableIndex]}));

                    int quantifierIndex = rdm.Next(0, quantifiers.Count);
                    orders.Add(GetOrderingUtterance(orderingSentenceStarter, quantifiers[quantifierIndex], meals[mealIndex].Dish, "",  new List<string>{nonSpecificVegetables[nonSpecificVegetableIndex]}));

                    //Loop for some possible vegetables combinations
                    int chosenVegetablesAmount = rdm.Next(0, 3);
                    List<string> chosenVegetables = new List<string>();

                    while(chosenVegetablesAmount != 0){
                        string chosenVegetable = vegetables[rdm.Next(0, vegetables.Count)].Name;
                        if (!chosenVegetables.Contains(chosenVegetable))
                        {
                            chosenVegetables.Add(chosenVegetable);
                            chosenVegetablesAmount--;
                        }
                    }

                    orders.Add(GetOrderingUtterance(orderingSentenceStarter, "", meals[mealIndex].Dish, "", chosenVegetables));
                    orders.Add(GetOrderingUtterance(orderingSentenceStarter, quantifiers[quantifierIndex], meals[mealIndex].Dish, "", chosenVegetables));
                }
            }

            return orders;
        }
        
        private async Task<List<Utterance>> GenerateAppointments()
        {
            List<Utterance> appointments = new List<Utterance>();

            List<Place> places = await _placeRepository.GetAllAsync();

            Random rdm = new Random();

            //Random pick for bookingSentenceStarters
            int index = rdm.Next(0, bookingSentenceStarters.Count - 1);
            string bookingSentenceStarter = bookingSentenceStarters[index];

            for (int placeIndex = 0; placeIndex < places.Count; placeIndex++)
            {
                int timeIndex = rdm.Next(0, times.Count);

                appointments.Add(GetAppointmentUtterance(bookingSentenceStarter, places[placeIndex].Name, "", times[timeIndex]));
                appointments.Add(GetAppointmentUtterance(bookingSentenceStarter, places[placeIndex].Name, "pour ", times[timeIndex]));
                appointments.Add(GetAppointmentUtterance(bookingSentenceStarter, places[placeIndex].Name, "à ", times[timeIndex]));
            }

            return appointments;
        }
        #endregion

        #region UTTERANCE HELPERS

        private Utterance GetOrderingUtterance(string orderingSentence, string quantifier, string dish, string sauce, List<string> orderedVegetables)
        {
            FormattableString formattedOrder = $"{orderingSentence}{quantifier}{((quantifier.Length > 0)? " " + dish : dish)} {sauce}";

            for(int i = 0; i < orderedVegetables.Count; i++)
            {
                formattedOrder = $"{formattedOrder.ToString()} {orderedVegetables[i]}";
            }

            string order = formattedOrder.ToString();

            List<Entity> entities = new List<Entity>
            {
                new Entity
                {
                    entityName = "dish",
                    startCharIndex = order.IndexOf(dish, StringComparison.Ordinal),
                    endCharIndex = order.IndexOf(dish, StringComparison.Ordinal) + (dish.Length - 1)
                }
            };


            if (sauce.Length > 0)
            {
                //Last index because meal may contain same sauce name
                entities.Add(new Entity
                {
                    entityName = "sauce",
                    startCharIndex = order.LastIndexOf(sauce, StringComparison.Ordinal),
                    endCharIndex = order.LastIndexOf(sauce, StringComparison.Ordinal) + (sauce.Length - 1)
                });
            }

            if (orderedVegetables.Count > 0 && orderedVegetables.FindIndex(vegetable => vegetable.Equals("")) == -1)
            {
                Random rdm = new Random();
                for(int i = 0; i < orderedVegetables.Count; i++)
                {
                    if(rdm.Next(0, 2) == 0)
                    {
                        entities.Add(new Entity
                        {
                            entityName = "vegetables",
                            startCharIndex = order.IndexOf(orderedVegetables[i], StringComparison.Ordinal),
                            endCharIndex = order.IndexOf(orderedVegetables[i], StringComparison.Ordinal) + (orderedVegetables[i].Length - 1)
                        });
                    }else{
                        int startChar = order.IndexOf(orderedVegetables[i], StringComparison.Ordinal);
                        order = order.Substring(0, startChar) + ((rdm.Next(0, 2) == 0) ? " pas de " : " sans ") + order.Substring(startChar);

                        entities.Add(new Entity
                        {
                            entityName = "vegetables",
                            startCharIndex = startChar,
                            endCharIndex = order.IndexOf(orderedVegetables[i], StringComparison.Ordinal) + (orderedVegetables[i].Length - 1)
                        });
                    }
                }
                
                entities.Add(new Entity
                {
                    entityName = "meal",
                    startCharIndex = order.IndexOf(dish, StringComparison.Ordinal),
                    endCharIndex = order.IndexOf(orderedVegetables[orderedVegetables.Count - 1], StringComparison.Ordinal) + (orderedVegetables[orderedVegetables.Count - 1].Length - 1)
                });
            }
            else
            {
                if (sauce.Length > 0)
                {
                    entities.Add(new Entity
                    {
                        entityName = "meal",
                        startCharIndex = order.IndexOf(dish, StringComparison.Ordinal),
                        endCharIndex = order.IndexOf(sauce, StringComparison.Ordinal) + (sauce.Length - 1)
                    });
                }
                else
                {
                    entities.Add(new Entity
                    {
                        entityName = "meal",
                        startCharIndex = order.IndexOf(dish, StringComparison.Ordinal),
                        endCharIndex = order.IndexOf(dish, StringComparison.Ordinal) + (dish.Length - 1)
                    });
                }
            }

            return new Utterance
            {
                text = order,
                intentName = "Order",
                entityLabels = entities
            };
        }

        private Utterance GetAppointmentUtterance(string bookingSentenceStarter, string place, string article, string time)
        {
            Random rdm = new Random();
            FormattableString formattedOrder = $"{bookingSentenceStarter}{((rdm.Next(0, 2) == 0) ? " au " : " ")}{place} {article}{time}";

            string appointment = formattedOrder.ToString();

            List<Entity> entities = new List<Entity>
            {
                new Entity
                {
                    entityName = "place",
                    startCharIndex = appointment.IndexOf(place, StringComparison.Ordinal),
                    endCharIndex = appointment.IndexOf(place, StringComparison.Ordinal) + (place.Length - 1)
                },
                new Entity
                {
                    entityName = "time",
                    startCharIndex = appointment.IndexOf(time, StringComparison.Ordinal),
                    endCharIndex = appointment.IndexOf(time, StringComparison.Ordinal) + (time.Length - 1)
                }
            };

            return new Utterance
            {
                text = appointment,
                intentName = "Appointment",
                entityLabels = entities
            };
        }
        #endregion
    }
}
