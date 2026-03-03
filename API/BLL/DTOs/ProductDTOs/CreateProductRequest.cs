using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.DTOs.ProductDTOs
{
    public class CreateProductRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }

    }
}
