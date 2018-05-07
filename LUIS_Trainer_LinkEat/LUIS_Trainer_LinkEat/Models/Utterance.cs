using System;
using System.Collections.Generic;
using System.Text;

namespace LUIS_Trainer_LinkEat.Models
{
    public class Utterance
    {
        public string text { get; set; }
        public string intentName { get; set; }
        public List<Entity> entityLabels { get; set; }
    }
}
