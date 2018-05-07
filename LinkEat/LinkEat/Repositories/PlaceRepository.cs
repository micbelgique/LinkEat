using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LinkEat.Models;
using Microsoft.EntityFrameworkCore;

namespace LinkEat.Repositories
{
    public class PlaceRepository
    {
        private AppDbContext dbContext;

        public PlaceRepository(AppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        #region Methods

        public async Task<List<Place>> GetAllAsync()
        {
            return await dbContext.Places.ToListAsync();
        }

        public async Task<Place> GetById(int id)
        {
            var searchedPlace = await dbContext.Places.Where(place => place.Id == id).FirstOrDefaultAsync();

            if (searchedPlace == null) throw new Exception("Impossible to find the category !");

            return searchedPlace;
        }

        public async Task<Place> GetByCategoryId(int categoryId)
        {
            var searchedPlace = await dbContext.Places.Where(place => place.CategoryId == categoryId).FirstOrDefaultAsync();

            if (searchedPlace == null) throw new Exception("Impossible to find the category !");

            return searchedPlace;
        }

        #endregion
    }
}
