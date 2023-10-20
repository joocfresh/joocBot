using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace joocBot.Models
{
    internal class Player
    {
        public string Id { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Guildid { get; set; } = string.Empty;
        public string GuildName { get; set; } = string.Empty;
        public string AllianceId { get; set; } = string.Empty;
        public string AllianceName { get; set; } = string.Empty;
        public string Avatar { get; set; } = string.Empty;
        public string AvatarRing { get; set; } = string.Empty;
        public long KillFame { get; set; } = default;
        public long DeathFame { get; set; } = default;
        public float FameRatio { get; set; } = default;
        public object? totalKills { get; set; }
        public object? gvgKills { get; set; }
        public object? gvgWon { get; set; }
    }
}
