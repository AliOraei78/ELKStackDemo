using Microsoft.AspNetCore.Mvc;
using ELKStackDemo.Models;
using ELKStackDemo.Services;
using ELKStackDemo.DTOs;

namespace ELKStackDemo.Controllers
{
    [Route("api/products")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly ElasticsearchService _esService;
        private readonly ILogger<ProductController> _logger;

        public ProductController(ElasticsearchService esService, ILogger<ProductController> logger)
        {
            _esService = esService;
            _logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            _logger.LogInformation("Getting all products");
            var products = await _esService.GetAllProductsAsync();
            return Ok(products);
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] ProductCreateDto dto)
        {
            _logger.LogInformation("Creating new product: {Name}", dto.Name);

            var product = new Product
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                Category = dto.Category,
                Stock = dto.Stock
            };

            await _esService.IndexDocumentAsync(product);
            return CreatedAtAction(nameof(GetAll), new { id = product.Id }, product);
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search(string? keyword, string? category, decimal? minPrice, decimal? maxPrice)
        {
            _logger.LogInformation("Advanced search - Keyword: {Keyword}, Category: {Category}", keyword, category);
            var results = await _esService.AdvancedSearchAsync(keyword, category, minPrice, maxPrice);
            return Ok(results);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] ProductCreateDto dto)
        {
            _logger.LogInformation("Updating product {Id}", id);
            var product = new Product
            {
                Id = id,
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                Category = dto.Category,
                Stock = dto.Stock
            };

            await _esService.UpdateProductAsync(product);
            return Ok(product);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            _logger.LogWarning("Deleting product {Id}", id);
            await _esService.DeleteProductAsync(id);
            return NoContent();
        }
    }
}