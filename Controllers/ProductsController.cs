using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Hybrid;
using PostgresDemo.Data;
using PostgresDemo.Models;

namespace PostgresDemo.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController(ApplicationDbContext context, HybridCache cache) : ControllerBase
    {
        private readonly ApplicationDbContext _context = context;
        private readonly HybridCache _cache = cache;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts(CancellationToken cancellationToken)
        {
            var products = await _cache.GetOrCreateAsync(
                "products-all",
                async cancel => await _context.Products.ToListAsync(cancel),
                tags: ["products"],
                cancellationToken: cancellationToken
            );

            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id, CancellationToken cancellationToken)
        {
            var product = await _cache.GetOrCreateAsync(
                $"product-{id}",
                async cancel => await _context.Products.FindAsync([id], cancel),
                tags: ["products"],
                cancellationToken: cancellationToken
            );

            if (product == null)
            {
                return NotFound();
            }

            return product;
        }

        [HttpPost]
        public async Task<ActionResult<Product>> CreateProduct([FromBody] Product product, CancellationToken cancellationToken)
        {
            await _context.Products.AddAsync(product, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);

            await _cache.RemoveByTagAsync("products", cancellationToken);

            return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct([FromRoute] int id, [FromBody] Product product, CancellationToken cancellationToken)
        {
            var existingProduct = await _context.Products.FindAsync([id], cancellationToken);
            if (existingProduct == null)
            {
                return NotFound();
            }
            existingProduct.Name = product.Name;
            existingProduct.Price = product.Price;
            await _context.SaveChangesAsync(cancellationToken);

            await _cache.RemoveByTagAsync("products", cancellationToken);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct([FromRoute] int id, CancellationToken cancellationToken)
        {
            var product = await _context.Products.FindAsync([id], cancellationToken);
            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync(cancellationToken);
            await _cache.RemoveByTagAsync("products", cancellationToken);

            return NoContent();
        }
    }
}