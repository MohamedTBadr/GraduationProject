using System;
using System.Collections.Generic;
using System.Text;

namespace BLL.DTOs.CategoryDTOs
{
    public record CreateCategoryRequest(string Name);
    public record UpdateCategoryRequest(string Name);
}
