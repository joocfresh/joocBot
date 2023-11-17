using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace joocBot.Models
{
    public class Equipment
    {
        public Gear? MainHand { get; set; }
        public Gear? OffHand { get; set; }
        public Gear? Head { get; set; }
        public Gear? Armor { get; set; }
        public Gear? Shoes { get; set; }
        public Gear? Bag { get; set; }
        public Gear? Cape { get; set; }
        public Gear? Mount { get; set; }
        public Gear? Potion { get; set; }
        public Gear? Food { get; set; }
    }
}
