using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace joocBot.Models
{
    // Disable the warning.
    #pragma warning disable IDE1006
    public class BattleEvent
    {
        public int groupMemberCount {  get; set; }
        public int numberOfParticipants { get; set; }
        public long EventId { get; set;}
        public DateTime TimeStamp { get; set;}
        public int Version { get; set; }
        public Inspection? Killer { get; set; }
        public Inspection? Victim { get; set; }
        public int TotalVictimKillFame { get; set; }
        public object? Location { get; set; }
        public Participant[]? Participants { get; set; }
        public Inspection[]? GroupMembers { get; set; }
        public object? GVGMatch { get; set; }
        public long BattleId { get; set; }
        public string? KillArea { get; set; }
        public object? Category { get; set; }
        public string? Type { get; set; }
    }
}
