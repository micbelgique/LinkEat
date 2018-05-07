using System.Collections.Generic;
using System.Threading.Tasks;
using LUIS_Trainer_LinkEat.Models;
using Microsoft.EntityFrameworkCore;

namespace LUIS_Trainer_LinkEat.Repositories
{
    public class CategoryRepository
    {
        private AppDbContext dbContext;

        public CategoryRepository(AppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        #region Methods

        public async Task<List<Category>> GetAllAsync()
        {
            return await dbContext.Categories.ToListAsync();
        } 

        #endregion
    }
}
