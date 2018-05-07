namespace LUIS_Trainer_LinkEat.Models
{
    public class Place
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public int CategoryId { get; set; }
        public string Phone { get; set; }
        public string Website { get; set; }
        public string OrderingAPI { get; set; }
    }
}
