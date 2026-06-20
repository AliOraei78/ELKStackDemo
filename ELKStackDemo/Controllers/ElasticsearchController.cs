using Microsoft.AspNetCore.Mvc;
using ELKStackDemo.Models;
using ELKStackDemo.Services;

namespace ELKStackDemo.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ElasticsearchController : ControllerBase
    {
        private readonly ElasticsearchService _esService;
        private readonly ILogger<ElasticsearchController> _logger;

        public ElasticsearchController(ElasticsearchService esService, ILogger<ElasticsearchController> logger)
        {
            _esService = esService;
            _logger = logger;
        }

        [HttpPost("create-index")]
        public async Task<IActionResult> CreateIndex()
        {
            await _esService.CreateIndexWithMappingAsync();
            _logger.LogInformation("Elasticsearch index 'products' created/verified");
            return Ok("Index created successfully");
        }

        [HttpPost("index")]
        public async Task<IActionResult> IndexProduct([FromBody] Product product)
        {
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Invalid product data received: {Errors}", ModelState);
                return BadRequest(ModelState);
            }

            try
            {
                await _esService.IndexDocumentAsync(product);
                _logger.LogInformation("Product indexed successfully: {Name}", product.Name);
                return Ok("Product indexed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to index product");
                return StatusCode(500, "Failed to index product");
            }
        }

        [HttpGet("search/{keyword}")]
        public async Task<IActionResult> Search(string keyword)
        {
            var results = await _esService.SearchAsync(keyword);
            return Ok(results);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAll()
        {
            var results = await _esService.GetAllDocumentsAsync();
            return Ok(results);
        }

        [HttpPost("create-index-mapping")]
        public async Task<IActionResult> CreateIndexWithMapping()
        {
            await _esService.CreateIndexWithMappingAsync();
            _logger.LogInformation("Index with custom mapping created successfully");
            return Ok("Index with Mapping created successfully");
        }

        [HttpPost("bulk-index")]
        public async Task<IActionResult> BulkIndex([FromBody] List<Product> products)
        {
            await _esService.BulkIndexAsync(products);
            _logger.LogInformation("Bulk indexed {Count} products", products.Count);
            return Ok($"Successfully indexed {products.Count} products");
        }

        [HttpGet("search-match/{keyword}")]
        public async Task<IActionResult> SearchMatch(string keyword)
        {
            var results = await _esService.SearchMatchAsync(keyword);
            _logger.LogInformation("Match search performed for: {Keyword}, Results: {Count}", keyword, results.Count);
            return Ok(results);
        }

        [HttpGet("search-term/{category}")]
        public async Task<IActionResult> SearchTerm(string category)
        {
            var results = await _esService.SearchTermAsync(category);
            return Ok(results);
        }

        [HttpGet("search-bool")]
        public async Task<IActionResult> SearchBool(string keyword, string? category = null)
        {
            var results = await _esService.SearchBoolAsync(keyword, category);
            return Ok(results);
        }

        [HttpGet("search-multi/{keyword}")]
        public async Task<IActionResult> SearchMultiMatch(string keyword)
        {
            var results = await _esService.SearchMultiMatchAsync(keyword);
            return Ok(results);
        }

        [HttpGet("advanced-search")]
        public async Task<IActionResult> AdvancedSearch(
    string? keyword,
    string? category,
    decimal? minPrice,
    decimal? maxPrice)
        {
            var results = await _esService.AdvancedSearchAsync(keyword, category, minPrice, maxPrice);
            _logger.LogInformation("Advanced search performed - Keyword: {Keyword}, Results: {Count}", keyword, results.Count);
            return Ok(results);
        }

        [HttpGet("aggregations")]
        public async Task<IActionResult> GetAggregations()
        {
            var result = await _esService.GetAggregationsAsync();
            _logger.LogInformation("Aggregations retrieved successfully");
            return Ok(result);
        }

        [HttpGet("cluster-health")]
        public async Task<IActionResult> GetClusterHealth()
        {
            try
            {
                var health = await _esService.GetClusterHealthAsync();
                _logger.LogInformation("Cluster health checked successfully");
                return Ok(health);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check cluster health");
                return StatusCode(500, new { Message = "Failed to retrieve cluster health" });
            }
        }
    }
}