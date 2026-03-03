using System;
using System.Collections.Generic;
using System.Text;

namespace DAL.Entities
{
    /// <summary>
    /// Represents a package entity.
    /// </summary>
    public class Package
    {

        public string Name { get; set; }
        public string Description { get; set; }
        public decimal Price { get; set; }
        public decimal Discount { get; set; }

        public List<string> Items { get; set; } = new();


    }
}
