using LinkEat.Models;
using LinkEat.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using Twilio.AspNet.Core;
using Twilio.Rest.Api.V2010.Account;
using Twilio.TwiML;
using Twilio.TwiML.Voice;
using Twilio.Types;

namespace LinkEat.Controllers
{
    [Route("api/twilio")]
    public class TwilioController : Twilio.AspNet.Core.TwilioController
    {
        private readonly IConfiguration _config;
        private readonly string _token;
        private readonly string _phone;

        private readonly OrderRepository _orderRepository;
        private readonly MealRepository _mealRepository;
        private readonly PlaceRepository _placeRepository;

        public TwilioController(IConfiguration configuration, OrderRepository orderRepository, MealRepository mealRepository, PlaceRepository placeRepository)
        {
            _config = configuration;
            _token = configuration["Slack:Token"];
            _phone = configuration["Twilio:Phone"];

            _orderRepository = orderRepository;
            _mealRepository = mealRepository;
            _placeRepository = placeRepository;
        }

        [HttpGet]
        [Produces("application/xml")]
        public XmlDocument GetOrder()
        {
            List<XElement> todaysOrder = new List<XElement>
            {
                new XElement("Gather", new XAttribute("action", "http://linkeattest.azurewebsites.net/api/twilio/"), new XAttribute("timeout", "60"), new XAttribute("numDigits", "1"), new XElement("Say",
                new XAttribute("language", "fr-FR"),
                new XAttribute("voice", "Alice"),
                "Bonjour, j'appelle pour la commande du MIC."
            ), new XElement("Pause",
                new XAttribute("length", "1")
            ), new XElement("Say",
                new XAttribute("language", "fr-FR"),
                new XAttribute("voice", "Alice"),
                "Veuillez appuyer sur le chiffre, un, lorsque vous être prêt à noter."
            )), new XElement("Say",
                new XAttribute("language", "fr-FR"),
                new XAttribute("voice", "Alice"),
                "Désolé, mais l'attente étant trop longue je vous rappellerai plus tard."
            )};

            var xml = new XDocument(new XDeclaration("1.0", "utf-8", "yes"), new XElement("Response", todaysOrder));
            
            var xmlDocument = new XmlDocument();
            using(var xmlReader = xml.CreateReader())
            {
                xmlDocument.Load(xmlReader);
            }
            return xmlDocument;
        }

        //[HttpPut] 
        //public async System.Threading.Tasks.Task Put() 
        //{ 
        //    /* Twilio config for calling */
        //    Place place = await _placeRepository.GetById(1);

        //    PhoneNumber to = new PhoneNumber(place.Phone);
        //    PhoneNumber from = new PhoneNumber(_phone);
        //    CallResource call = CallResource.Create(to, from,
        //        url: new Uri("http://linkeattest.azurewebsites.net/api/twilio/"),
        //        method: Twilio.Http.HttpMethod.Get);
        //}

        [HttpPost] 
        public async Task<TwiMLResult> Post() 
        { 
            var response = new VoiceResponse();
            var gather = new Gather(timeout: 3, numDigits: 1, action: new Uri("http://linkeattest.azurewebsites.net/api/twilio/"));

            List<Meal> meals = await _mealRepository.GetAllAsync();
            foreach (var meal in meals)
            {
                try
                {
                    List<Order> orders = await _orderRepository.GetByMeal(meal.Id);
                    List<string> commands = new List<string>();
                    List<int> commandAmount = new List<int>();

                    foreach(var order in orders)
                    {
                        if (commands.Contains(order.Meal))
                        {
                            commandAmount[commands.IndexOf(order.Meal)]++;
                        }else{
                            commands.Add(order.Meal);
                            commandAmount.Add(1);
                        }
                    }

                    for(int commandIndex = 0; commandIndex < commands.Count; commandIndex++)
                    {
                        gather.Say($"{commandAmount[commandIndex]} {commands[commandIndex]}", Say.VoiceEnum.Alice, language: Say.LanguageEnum.FrFr);
                        gather.Pause(1);
                    }
                }catch (Exception){

                }
            }

            gather.Say("Appuyez sur le chiffre, un, si vous souhaitez réentendre le message", Say.VoiceEnum.Alice, language: Say.LanguageEnum.FrFr);

            response.Append(gather);

            response.Say("Merci, au revoir et bonne journée!", Say.VoiceEnum.Alice, language: Say.LanguageEnum.FrFr);
             
            return TwiML(response); 
        }
    }
}
