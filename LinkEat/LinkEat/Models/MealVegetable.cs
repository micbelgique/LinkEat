using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LinkEat.Models
{
    public class MealVegetable
    {
        public int MealId { get; set; }
        public Meal Meal { get; set; }
        public int VegetableId { get; set; }
        public Vegetable Vegetable { get; set; }
    }
}
