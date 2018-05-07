using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LinkEat.Models;
using Microsoft.EntityFrameworkCore;

namespace LinkEat.Repositories
{
    public class MealRepository
    {
        private AppDbContext dbContext;

        public MealRepository(AppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        #region Methods

        public async Task<List<Meal>> GetAllAsync()
        {
            return await dbContext.Meals.ToListAsync();
        }

        public async Task<Meal> GetById(int id)
        {
            var matchingMeal = await dbContext.Meals.FirstOrDefaultAsync(meal => meal.Id == id);

            if (matchingMeal == null) throw new Exception("Impossible to find that item !");

            return matchingMeal;
        }

        #endregion
    }
}
