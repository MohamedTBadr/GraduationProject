using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.Entities
{
    public class Product
    {
       public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public ServiceType ServiceType { get; set; }

        public Guid ServiceTypeId { get; set; }

        public Category Category { get; set; }

        public Guid CategoryId { get; set; } 

        


    }
}
