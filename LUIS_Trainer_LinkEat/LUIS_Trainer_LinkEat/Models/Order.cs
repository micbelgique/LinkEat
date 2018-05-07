using System;

namespace LUIS_Trainer_LinkEat.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int PlaceId { get; set; }
        public int UserId { get; set; }
        public DateTime Date { get; set; }
        public string Meal { get; set; }
        public int MealId { get; set; }
        public bool Confirmed { get; set; }
    }
}
