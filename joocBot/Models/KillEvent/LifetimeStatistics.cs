using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace joocBot.Models
{
    public class LifetimeStatistics
    {
        public object? PvE {  get; set; }
        public object? Gathering { get; set; }
        public object? Crafting { get; set; }
        public int CrystalLeague { get; set; }
        public int FishingFame { get; set; }
        public int FarmingFame { get; set; }
        public object? Timestamp { get; set; }
    }
}
