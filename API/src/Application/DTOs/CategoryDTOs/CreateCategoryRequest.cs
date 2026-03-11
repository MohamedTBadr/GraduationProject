using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.CategoryDTOs
{
    public record CreateCategoryRequest(string Name);
    public record UpdateCategoryRequest(string Name);
}
