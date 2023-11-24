using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace joocBot.Models
{
    public class Member
    {
        public string Id { get; set; } = string.Empty;
        public string PlayerId { get; set; } = string.Empty;
        public string PlayerName { get; set; } = string.Empty;
        public string GuildName { get; set; } = string.Empty;
        public string DiscordName { get; set; } = string.Empty;
        public long LastKillEvent { get; set; } = default;
        public long LastDeathEvent { get; set; } = default;

        public void Update(Member member)
        {
            GuildName = member.GuildName;
            PlayerName = member.PlayerName;
            PlayerId = member.PlayerId;
            DiscordName = member.DiscordName;
            LastKillEvent = member.LastKillEvent;
            LastDeathEvent = member.LastDeathEvent;
        }
    }
}
