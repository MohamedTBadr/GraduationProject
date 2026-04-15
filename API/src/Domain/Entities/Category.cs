using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Domain.Entities
{
    /// <summary>
    /// Represents a category entity.
    /// </summary>
    public class Category: BaseEntity
    {
        public Guid Id { get; set; }
        public string Name { get; set; }
    }
}
