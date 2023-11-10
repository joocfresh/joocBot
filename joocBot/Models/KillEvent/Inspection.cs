using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace joocBot.Models
{
    public class Inspection
    {
        public double AverageItemPower { get; set; }
        public Equipment? Equipment { get; set; }
        public Gear[]? Inventory { get; set; }
        public string? Name { get; set; }
        public string? Id { get; set; }
        public string GuildId { get; set; } = string.Empty;
        public string GuildName { get; set; } = string.Empty;
        public string AllianceId { get; set; } = string.Empty;
        public string AllianceName { get; set; } = string.Empty;
        public string AllianceTag { get; set; } = string.Empty;
        public string? Avatar { get; set; }
        public string? AvatarRing { get; set;}
        public long KillFame { get; set; } = default;
        public long DeathFame { get; set; } = default;
        public float FameRatio { get; set; } = default;
        public LifetimeStatistics? LifetimeStatistics {  get; set; }
    }
}
