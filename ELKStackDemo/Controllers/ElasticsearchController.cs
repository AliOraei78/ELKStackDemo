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
            await _esService.CreateIndexAsync();
            _logger.LogInformation("Elasticsearch index 'products' created/verified");
            return Ok("Index created successfully");
        }

        [HttpPost("index")]
        public async Task<IActionResult> IndexProduct([FromBody] Product product)
        {
            await _esService.IndexDocumentAsync(product);
            _logger.LogInformation("Product indexed: {ProductName}", product.Name);
            return Ok("Product indexed successfully");
        }

        [HttpGet("search/{keyword}")]
        public async Task<IActionResult> Search(string keyword)
        {
            var results = await _esService.SearchAsync(keyword);
            return Ok(results);
        }
    }
}