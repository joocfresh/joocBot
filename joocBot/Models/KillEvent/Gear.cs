using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace joocBot.Models
{
    public class Gear
    {
        public String? Type { get; set; }
        public int Count { get; set; }
        public int Quality { get; set; }
        public object[]? ActiveSpells { get; set; }
        public object[]? PassiveSpells { get; set; }
        public object? LegendarySoul { get; set; }
    }
}
