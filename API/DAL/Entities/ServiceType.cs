using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.Entities
{
    /// <summary>
    /// Represents a service type entity.
    /// </summary>
    public class ServiceType
    {

        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}
