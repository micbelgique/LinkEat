using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LinkEat.Models;
using Microsoft.EntityFrameworkCore;

namespace LinkEat.Repositories
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

        public async Task Delete(Appointment apt)
        {
            dbContext.Appointments.Remove(apt);
            await dbContext.SaveChangesAsync();
        }

        public async Task<List<Appointment>> GetByDateAsync(DateTime date)
        {
            //Day, month, year +  Hour, minutes
            List<Appointment> correspondingAppointments = await dbContext.Appointments.Where(appointment => this.SameDate(appointment.Date, date)).ToListAsync();
            //We're searching for appointments that are corresponding with the date that has been sent
            return correspondingAppointments;
        }

        public async Task<Appointment> GetByDateAndByUserAsync(DateTime date, int id)
        {
            //Day, month, year +  Hour, minutes
            Appointment correspondingAppointment = await dbContext.Appointments.FirstOrDefaultAsync(appointment => this.SameDate(appointment.Date, date) && appointment.UserId == id);
            //We're searching for appointments that are corresponding with the date that has been sent
            return correspondingAppointment;
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

        private bool SameDate(DateTime d1, DateTime d2)
        {
            return (d1.Year == d2.Year && d1.Month == d2.Month && d1.Day == d2.Day);
        }

        #endregion
    }
}
