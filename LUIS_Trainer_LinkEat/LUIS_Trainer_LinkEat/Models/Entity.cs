﻿using System;
using System.Collections.Generic;
using System.Text;

namespace LUIS_Trainer_LinkEat.Models
{
    public class Entity
    {
        public string entityName { get; set; }
        public int startCharIndex { get; set; }
        public int endCharIndex { get; set; }
    }
}
