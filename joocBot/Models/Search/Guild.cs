using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace joocBot.Models
{
    public class Guild
    {
        public string? Id { get; set; } = string.Empty;
        public string? Name { get; set; } = string.Empty;
        public string? AllianceId { get; set; } = string.Empty;
        public string? AllianceName { get; set; } = string.Empty;
        public string? KillFame { get; set; } = default;
        public string? DeathFame { get; set; } = default;
    }
}
