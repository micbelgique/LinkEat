using System.Collections.Generic;

namespace LinkEat.Models
{
    public class Meal
    {
        public int Id { get; set; }
        public int PlaceId { get; set; }
        public string Dish { get; set; }
        public string Description { get; set; }
        public int? SauceId { get; set; }
        public virtual ICollection<MealVegetable> MealVegetables { get; set; }
        public double Price { get; set; }
    }
}
