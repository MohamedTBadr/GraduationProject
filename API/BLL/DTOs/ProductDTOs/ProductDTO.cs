using DAL.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.DTOs.ProductDTOs
{
    public class ProductDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } 
            
        public string Description { get; set; }
        public ServiceType ServiceType { get; set; }

        public Guid ServiceTypeId { get; set; }


    }
}
