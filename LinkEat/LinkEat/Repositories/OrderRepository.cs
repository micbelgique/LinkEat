using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LinkEat.Models;
using Microsoft.EntityFrameworkCore;

namespace LinkEat.Repositories
{
    public class OrderRepository
    {
        private AppDbContext dbContext;

        public OrderRepository(AppDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        #region Methods

        #region Create

        public async Task Create(Order order)
        {
            dbContext.Orders.Add(order);
            await dbContext.SaveChangesAsync();
        }

        #endregion

        #region Read

        public async Task<List<Order>> GetAllAsync()
        {
            return await dbContext.Orders.ToListAsync();
        }

        public async Task<Order> GetByUser(int userId)
        {
            Order matchingOrder = await dbContext.Orders.Where(o => IsSameDate(o.Date, DateTime.Today)).FirstOrDefaultAsync(order => order.UserId == userId);
            if (matchingOrder == null) throw new Exception("Impossible to find that order !");

            return matchingOrder;
        }

        public async Task<List<Order>> GetByPlace(int placeId)
        {
            var matchingOrders = await dbContext.Orders.Where(order => IsSameDate(order.Date, DateTime.Today) && order.PlaceId == placeId).ToListAsync();
            if (matchingOrders == null) throw new Exception("Impossible to find that order !");

            return matchingOrders;
        }

        public async Task<List<Order>> GetByMeal(int mealId)
        {
            var matchingOrders = await dbContext.Orders.Where(order => IsSameDate(order.Date, DateTime.Today) && order.MealId == mealId).ToListAsync();
            if (matchingOrders == null) throw new Exception("Impossible to find that order !");

            return matchingOrders;
        }

        public async Task<Order> GetById(int id)
        {
            var matchingOrder = await dbContext.Orders.FirstOrDefaultAsync(order => order.Id == id);
            if (matchingOrder == null) throw new Exception("Impossible to find that order");

            return matchingOrder;
        }

        #endregion

        #region Update

        public async Task Update(int id, Order order)
        {
            if (id != order.Id || order == null) throw new Exception("Invalid request");
            dbContext.Orders.Update(order);

            await dbContext.SaveChangesAsync();
        }

        #endregion

        #region Delete

        public async Task Delete(Order order)
        {
            dbContext.Orders.Remove(order);
            await dbContext.SaveChangesAsync();
        }

        #endregion

        #endregion

        #region Utils

        private bool IsSameDate(DateTime d1, DateTime d2)
        {
            return (d1.Year == d2.Year && d1.Month == d2.Month && d1.Day == d2.Day);
        }

        #endregion
    }
}
