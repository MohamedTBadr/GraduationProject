using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
namespace PAL.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductController(): APIController
    {
        [HttpGet]
        public IActionResult GetAllProducts()
        {
            return Ok(new List<string> { "Product 1", "Product 2", "Product 3" });
        }


        [HttpGet("{id}")]
        public IActionResult GetProductById(int id)
        {
            var product = $"Product {id}";
            return Ok(product);
        }
        //roles= admin,vendor,user
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public IActionResult CreateProduct([FromBody] string product)
        {
            return CreatedAtAction(nameof(GetProductById), new { id = 1 }, product);
        }

        //[HttpDelete("{id}")]
        //[Authorize(Roles = "Admin")]
        //public IActionResult DeleteProduct(Guid id)
        //{
            
        //}
    }
}
