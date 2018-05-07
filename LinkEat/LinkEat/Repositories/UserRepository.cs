using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LinkEat.Models;
using Microsoft.EntityFrameworkCore;

namespace LinkEat.Repositories
{
    public class UserRepository
    {
        private AppDbContext dbContext;

        public UserRepository(AppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        #region Methods

        public async Task<User> Create(User inputUser)
        {
            dbContext.Users.Add(inputUser);
            await dbContext.SaveChangesAsync();
            return dbContext.Users.FirstOrDefault(u => u.FirstName == inputUser.FirstName && u.LastName == inputUser.LastName);
        }

        public async Task<List<User>> GetAllAsync()
        {
            return await dbContext.Users.ToListAsync();
        }

        public async Task<User> GetById(int id)
        {
            var searchedUser = await dbContext.Users.FirstOrDefaultAsync(user => user.Id == id);

            if (searchedUser == null) throw new Exception("Impossible to find that item");

            return searchedUser;
        }

        public async Task<User> GetBySlackId(string id)
        {
            var searchedUser = await dbContext.Users.FirstOrDefaultAsync(user => user.SlackId.Equals(id));

            if(searchedUser == null) throw new Exception("Impossible to find user");

            return searchedUser;
        }

        public async Task Update(int id, User inputUser)
        {
            if (id != inputUser.Id || inputUser == null) throw new Exception("Invalid request");

            dbContext.Users.Update(inputUser);
            await dbContext.SaveChangesAsync();
        }

        public async Task Delete(int id)
        {
            var userToDelete = await GetById(id);

            dbContext.Users.Remove(userToDelete);
            await dbContext.SaveChangesAsync();
        }

        #endregion
    }
}
