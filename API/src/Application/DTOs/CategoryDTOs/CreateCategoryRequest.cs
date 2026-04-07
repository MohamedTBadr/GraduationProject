using System;
using System.Collections.Generic;
using System.Text;

namespace Application.DTOs.CategoryDTOs
{
    public record CreateCategoryRequest(string name);
    public record UpdateCategoryRequest(string name);
}
