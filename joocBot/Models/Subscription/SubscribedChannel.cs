using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace joocBot.Models
{
    public class SubscribedChannel
    {
        public ulong Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public bool IsSubscribed { get; set; }
        public bool IsAuthorized { get; set; }

        public void Update(SubscribedChannel channel)
        {
            IsAuthorized = channel.IsAuthorized;
            IsSubscribed = channel.IsSubscribed;
            Description = channel.Description;
        }
    }
}
