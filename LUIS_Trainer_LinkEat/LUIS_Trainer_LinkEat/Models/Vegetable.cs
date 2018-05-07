using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace LUIS_Trainer_LinkEat.Models
{
    public class Vegetable
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public virtual ICollection<MealVegetable> MealVegetables { get; set; }
    }
}
