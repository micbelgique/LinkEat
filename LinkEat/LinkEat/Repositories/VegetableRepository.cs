using LinkEat.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LinkEat.Repositories
{
    public class VegetableRepository
    {
        private AppDbContext dbContext;

        public VegetableRepository(AppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        #region Methods

        public async Task<List<Vegetable>> GetAllAsync()
        {
            return await dbContext.Vegetables.ToListAsync();
        }

        public async Task<Vegetable> GetById(int id)
        {
            var matchingVegetable = await dbContext.Vegetables.FirstOrDefaultAsync(vegetable => vegetable.Id == id);
            if (matchingVegetable == null) throw new Exception("Impossible to find that sauce !");

            return matchingVegetable;
        }

        #endregion
    }
}
