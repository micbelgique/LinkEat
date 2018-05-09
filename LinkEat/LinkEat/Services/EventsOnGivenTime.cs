using LinkEat.Models;
using LinkEat.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace LinkEat.Services
{
    public class EventsOnGivenTime
    {
        private readonly IConfiguration _configuration;
        private readonly string _token;
        private readonly string _phone;

        public EventsOnGivenTime(IConfiguration configuration)
        {
            _configuration = configuration;
            _token = configuration["Slack:Token"];
            _phone = configuration["Twilio:Phone"];

            CheckEvents();
        }

        private async void CheckEvents()
        {
            DbContextOptionsBuilder<AppDbContext> options = new DbContextOptionsBuilder<AppDbContext>();
            options.UseSqlServer(_configuration.GetConnectionString("DefaultConnection"));

            PlaceRepository placeRepository;
            AppointmentRepository appointmentRepository;
            UserRepository userRepository;
            DateTime currentTime;

            List<Appointment> appointments;

            while (true)
            {
                using(AppDbContext appDbContext = new AppDbContext(options.Options))
                {
                    placeRepository = new PlaceRepository(appDbContext);
                    appointmentRepository = new AppointmentRepository(appDbContext);
                    userRepository = new UserRepository(appDbContext);

                    currentTime = DateTime.Now;

                    appointments = await appointmentRepository.GetByDateAsync(currentTime);
                    foreach (var appointment in appointments)
                    {
                        AppointmentReminder(appointment, currentTime, appointments, placeRepository, userRepository);
                        appDbContext.SaveChanges();
                    }

                    if (currentTime.Hour == 11 && currentTime.Minute == 0)
                    {
                        Place place = await placeRepository.GetById(1);
                        Order(place);
                    }

                    if (currentTime.Hour == 10 && currentTime.Minute == 30)
                    {
                        Reminder();
                    }
                }
                
                Thread.Sleep(30000);
            }
        }

        #region HELPERS
        private async void AppointmentReminder(Appointment appointment, DateTime currentTime, List<Appointment> appointments, PlaceRepository placeRepository, UserRepository userRepository)
        {
            if (appointment.Date < currentTime.AddMinutes(15) && appointment.Date > currentTime)
            {
                if (!appointment.Reminded)
                {
                    Place place = await placeRepository.GetById(appointment.PlaceId);
                    List<Appointment> sameTimeAppointmentsInSamePlace = appointments.FindAll(appointementToMatch => appointementToMatch.Date.Hour == appointment.Date.Hour && appointementToMatch.Date.Minute == appointment.Date.Minute && appointment.PlaceId == appointementToMatch.PlaceId);
                    User user = await userRepository.GetById(appointment.UserId);
                    SlackReminder reminder;

                    if (sameTimeAppointmentsInSamePlace.Count > 1)
                    {
                        reminder = new SlackReminder
                        {
                            user = user.SlackId,
                            text = "N'oubliez pas que vous vous êtes inscrit pour " + place.Name + " dont le départ à lieu à " + appointment.Date.Hour + "h" + ((appointment.Date.Minute == 0)? "" : appointment.Date.Minute.ToString()),
                            time = 15
                        };
                    }else{
                        string message = "N'oubliez pas que vous vous êtes inscrit pour " + place.Name + " dont le départ à lieu à " + appointment.Date.Hour + "h" + ((appointment.Date.Minute == 0)? "" : appointment.Date.Minute.ToString());

                        List<Appointment> samePlaceAppointments = appointments.FindAll(a => a.PlaceId == appointment.PlaceId);
                        int count = 0;
                        DateTime date = appointment.Date;
                        while (count < 1 && date.Hour < 14)
                        {
                            foreach (var samePlaceAppointment in samePlaceAppointments)
                            {
                                if (samePlaceAppointment.Date > appointment.Date)
                                {
                                    if (samePlaceAppointment.Date.Hour == date.Hour && samePlaceAppointment.Date.Minute == date.Minute)
                                    {
                                        count++;
                                    }
                                }
                            }

                            date = date.AddMinutes(30);
                        }

                        if (count >= 1)
                        {
                            message += "\nCependant, vous êtes la seule personne à avoir choisi ce créneau horaire. Je vous propose donc d'aller à ce même restaurant, mais à " + date.Hour + "h" + ((date.Minute == 0)? "" : date.Minute.ToString());
                        }else{
                            List<Appointment> sameTimeAppointments = appointments.FindAll(a => a.Date.Hour == appointment.Date.Hour && a.Date.Minute == appointment.Date.Minute);
                            int countPeople = 0;
                            int placeId = 1;
                            int placesLength = placeRepository.GetAllAsync().Result.Count;

                            //Bad idea since length is not equal id, but okay for proto
                            while (countPeople < 1 && placeId < placesLength)
                            {
                                foreach (var sameTimeAppointment in sameTimeAppointments)
                                {
                                    if (sameTimeAppointment.PlaceId == placeId)
                                    {
                                        countPeople++;
                                    }
                                }

                                placeId++;
                            }

                            if (countPeople >= 1)
                            {
                                Place newPlace = await placeRepository.GetById(placeId);
                                message += "\nCependant, vous êtes la seule personne à avoir choisi ce créneau horaire. Je vous propose donc d'aller au " + newPlace.Name + " à " + appointment.Date.Hour + "h" + ((appointment.Date.Minute == 0)? "" : appointment.Date.Minute.ToString());
                            }else{
                                countPeople = 0;
                                placeId = 1;
                                DateTime newDate = appointment.Date;

                                //Bad idea since length is not equal id, but okay for proto
                                while (newDate.Hour < 14)
                                {
                                    while (countPeople < 1 && placeId < placesLength)
                                    {
                                        foreach (var anyTimeAppointment in appointments)
                                        {
                                            if (anyTimeAppointment.Date > appointment.Date)
                                            {
                                                if (anyTimeAppointment.PlaceId == placeId)
                                                {
                                                    if (anyTimeAppointment.Date.Hour == date.Hour && anyTimeAppointment.Date.Minute == date.Minute)
                                                    {
                                                        count++;
                                                    }
                                                }
                                            }
                                        }

                                        placeId++;
                                    }
                                    newDate = newDate.AddMinutes(30);
                                }

                                if (countPeople >= 1)
                                {
                                    Place newPlace = await placeRepository.GetById(placeId);
                                    message += "\nCependant, vous êtes la seule personne à avoir choisi ce créneau horaire. Je vous propose donc d'aller au " + newPlace.Name + " à " + newDate.Hour + "h" + ((newDate.Minute == 0)? "" : newDate.Minute.ToString());
                                }else
                                {
                                    message += "\nMalheureusement je n'ai pas de regroupement alternatif à vous proposer aujourd'hui.";
                                }
                            }
                        }

                        reminder = new SlackReminder
                        {
                            user = user.SlackId,
                            text = message,
                            time = 15
                        };
                    }

                    using(var client = new HttpClient())
                    {
                        var stringContent = new StringContent(JsonConvert.SerializeObject(reminder), Encoding.UTF8, "application/json");
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
                        await client.PostAsync( "https://slack.com/api/reminders.add", stringContent);
                    }

                    appointment.Reminded = true;
                }
            } else if(appointment.Date.ToShortDateString() == currentTime.ToShortDateString() && appointment.Date.Hour == currentTime.Hour && appointment.Date.Minute == currentTime.Minute) {
                if (!appointment.RemindedDepart)
                {
                    Place place = await placeRepository.GetById(appointment.PlaceId);
                    User user = await userRepository.GetById(appointment.UserId);
                    SlackReminder reminder = new SlackReminder
                    {
                        user = user.SlackId,
                        text = "Il est l'heure de partir pour " + place.Name,
                        time = 5
                    };

                    using(var client = new HttpClient())
                    {
                        var stringContent = new StringContent(JsonConvert.SerializeObject(reminder), Encoding.UTF8, "application/json");
                        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
                        await client.PostAsync( "https://slack.com/api/reminders.add", stringContent);
                    }

                    appointment.RemindedDepart = true;
                }
            }
        }

        private void Order(Place place)
        {
            PhoneNumber to = new PhoneNumber(place.Phone);
            PhoneNumber from = new PhoneNumber(_phone);
            CallResource call = CallResource.Create(to, from,
                url: new Uri("https://linkeat.azurewebsites.net/api/twilio/"),
                method: Twilio.Http.HttpMethod.Get);
        }

        private async void Reminder()
        {
            using(var client = new HttpClient())
            {
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
                var response = await client.GetAsync( "https://slack.com/api/channels.list");
                var channels = await response.Content.ReadAsAsync<List<Channel>>();
                var channel = channels.Find(c => c.name.ToLowerInvariant().Equals("random"));

                var reminder = new SlackMessage
                {
                    channel = channel.id,
                    text = "N'oubliez pas de commander ou de choisir où manger ce midi! :D",
                    as_user = false,
                    username = "Link Eat Bot"
                };

                var stringContent = new StringContent(JsonConvert.SerializeObject(reminder), Encoding.UTF8, "application/json");
                client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _token);
                await client.PostAsync( "https://slack.com/api/chat.postMessage", stringContent);
            }
        }
        #endregion

        #region INNERCLASSES
        private class Channel
        {
            public string id { get; set; }
            public string name { get; set; }
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
