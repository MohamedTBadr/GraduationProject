using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    public class EventType
    {
        public Guid Id { get; set; }
        public string Name { get; set; }

        public ICollection<Event> Events { get; set; }

        public ICollection<Service> Services { get; set; }
    }
}
