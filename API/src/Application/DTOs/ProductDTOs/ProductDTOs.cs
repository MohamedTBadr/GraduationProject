using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.ProductDTOs
{
    public record ProductDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public Guid CategoryId { get; set; }
        public string CategoryName { get; set; }
        public Guid VendorId { get; set; }
        public string VendorName { get; set; }
        public Guid ServiceTypeId { get; set; }
        public string ServiceTypeName { get; set; }
        public List<string> ProductImages { get; set; }
    }

    public record CreateProductRequest
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public List<IFormFile> ProductImages { get; set; }
        public Guid CategoryId { get; set; }
        public Guid? VendorId { get; set; }
        public Guid ServiceTypeId { get; set; }
    }

    public record UpdateProductDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Guid CategoryId { get; set; }
        public Guid VendorId { get; set; }
        public Guid ServiceTypeId { get; set; }
    }




}
