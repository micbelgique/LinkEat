using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LUIS_Trainer_LinkEat.Models;
using Microsoft.EntityFrameworkCore;

namespace LUIS_Trainer_LinkEat.Repositories
{
    public class AppointmentRepository
    {
        private readonly AppDbContext dbContext;

        public AppointmentRepository(AppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        #region Methods

        public async Task Create(Appointment apt)
        {
            dbContext.Appointments.Add(apt);
            await dbContext.SaveChangesAsync();
        }

        public async Task<List<Appointment>> GetByDateAsync(DateTime date)
        {
            //Day, month, year +  Hour, minutes
            List<Appointment> correspondingAppointments = await dbContext.Appointments.Where(appointment => this.SameDate(appointment.Date, date)).ToListAsync();
            //We're searching for appointments that are corresponding with the date that has been sent
            return correspondingAppointments;
        }

        public async Task<List<Appointment>> GetByPlace(int id)
        {
            //Day, month, year +  Hour, minutes
            List<Appointment> correspondingAppointments = await dbContext.Appointments.Where(appointment => appointment.PlaceId == id && this.SameDate(new DateTime(appointment.Date.Year, appointment.Date.Month, appointment.Date.Day), new DateTime(DateTime.Now.Year, DateTime.Now.Month, DateTime.Now.Day))).ToListAsync();
            //We're searching for appointments that are corresponding with the date that has been sent
            return correspondingAppointments;
        }

        public async Task<Appointment> GetByUser(int id)
        {
            var correspondingAppointment = await dbContext.Appointments.FirstOrDefaultAsync(appointment => appointment.UserId == id);
            return correspondingAppointment;
        }

        #endregion

        #region Utils

        public bool SameDate(DateTime d1, DateTime d2)
        {
            return (d1.ToShortDateString() == d2.ToShortDateString() && d1.Hour == d2.Hour && d1.Minute == d2.Minute);
        }

        #endregion
    }
}
