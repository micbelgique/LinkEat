using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LUIS_Trainer_LinkEat.Models;
using Microsoft.EntityFrameworkCore;

namespace LUIS_Trainer_LinkEat.Repositories
{
    public class SauceRepository
    {
        private AppDbContext dbContext;

        public SauceRepository(AppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        #region Methods

        public async Task<List<Sauce>> GetAllAsync()
        {
            return await dbContext.Sauces.ToListAsync();
        }

        public async Task<Sauce> GetById(int id)
        {
            var matchingSauce = await dbContext.Sauces.FirstOrDefaultAsync(sauce => sauce.Id == id);
            if (matchingSauce == null) throw new Exception("Impossible to find that sauce !");

            return matchingSauce;
        }

        #endregion
    }
}
