using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace joocBot.Models
{
    public class Participant : Inspection
    {
        public double DamageDone { get; set; }
        public double SupportHealingDone { get; set; }
    }
}
