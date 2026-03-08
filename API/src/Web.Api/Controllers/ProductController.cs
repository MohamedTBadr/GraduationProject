using Application;
using Application.DTOs.ProductDTOs;
using Application.Interfaces;
using BLL;
using Microsoft.AspNetCore.Mvc;
using Shared;

namespace Web.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController(IProductService productService) : ControllerBase
    {
        // GET api/products
        [HttpGet]
        public async Task<IActionResult> GetAllAsync([FromQuery] PaginatedRequest request)
        {
            var result = await productService.GetAllAsync(request);
            return Ok(result);
        }

        // GET api/products/{id}
        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetByIdAsync(Guid id)
        {
            var result = await productService.GetByIdAsync(id);
            return Ok(result);
        }

        // GET api/products/by-category/{categoryId}
        [HttpGet("by-category/{categoryId:guid}")]
        public async Task<IActionResult> GetByCategoryAsync(Guid categoryId, [FromQuery] PaginatedRequest request)
        {
            var result = await productService.GetByCategoryIdAsync(categoryId, request);
            return Ok(result);
        }

        // GET api/products/by-vendor/{vendorId}
        [HttpGet("by-vendor/{vendorId:guid}")]
        public async Task<IActionResult> GetByVendorAsync(Guid vendorId, [FromQuery] PaginatedRequest request)
        {
            var result = await productService.GetByVendorIdAsync(vendorId, request);
            return Ok(result);
        }

        // GET api/products/by-service-type/{serviceTypeId}
        [HttpGet("by-service-type/{serviceTypeId:guid}")]
        public async Task<IActionResult> GetByServiceTypeAsync(Guid serviceTypeId, [FromQuery] PaginatedRequest request)
        {
            var result = await productService.GetByServiceTypeIdAsync(serviceTypeId, request);
            return Ok(result);
        }

        // POST api/products
        [HttpPost]
        public async Task<IActionResult> CreateAsync([FromBody] CreateProductRequest dto)
        {
            var result = await productService.CreateAsync(dto);

            if (result.IsSuccess)
                return CreatedAtAction(nameof(GetByIdAsync), new { id = result.Value.Id }, result);

            return Ok(result); // filter handles the failure
        }

        // PUT api/products/{id}
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateAsync(Guid id, [FromBody] UpdateProductDTO dto)
        {
            if (id != dto.Id)
                return BadRequest(Error.Validation("Product.IdMismatch", "Route id and body id do not match."));

            var result = await productService.UpdateAsync(dto);
            return Ok(result);
        }

        // DELETE api/products/{id}
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteAsync(Guid id)
        {
            var result = await productService.DeleteAsync(id);

            if (result.IsSuccess)
                return NoContent();

            return Ok(result); // filter handles the failure
        }
    }
}