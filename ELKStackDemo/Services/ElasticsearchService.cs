using Elastic.Clients.Elasticsearch;
using ELKStackDemo.Models;
using Elastic.Transport;

namespace ELKStackDemo.Services
{
    public class ElasticsearchService
    {
        private readonly ElasticsearchClient _client;
        private const string IndexName = "products";

        public ElasticsearchService()
        {
            // CHANGE THIS FROM https TO http
            var settings = new ElasticsearchClientSettings(new Uri("http://localhost:9200"))

                // Keep this if security is enabled inside the container. 
                // (If you disabled security completely in your docker setup, you can delete this line)
                .Authentication(new BasicAuthentication("elastic", "A123456a"))

                .DefaultIndex(IndexName)
                .RequestTimeout(TimeSpan.FromMinutes(2));

            _client = new ElasticsearchClient(settings);
        }

        // Create Index (if not exists)
        public async Task CreateIndexAsync()
        {
            var existsResponse = await _client.Indices.ExistsAsync(IndexName);
            if (!existsResponse.Exists)
            {
                await _client.Indices.CreateAsync(IndexName);
            }
        }

        // Insert / Index Document (Returns success status AND raw debug information)
        public async Task<(bool IsSuccess, string DebugInfo)> IndexDocumentAsync(Product product)
        {
            var response = await _client.IndexAsync(product, i => i.Index(IndexName));
            return (response.IsValidResponse, response.DebugInformation);
        }

        // Simple Search
        public async Task<List<Product>> SearchAsync(string keyword)
        {
            var response = await _client.SearchAsync<Product>(s => s
                .Index(IndexName)
                .Query(q => q
                    .Match(m => m
                        .Field(f => f.Name)
                        .Query(keyword)
                    )
                )
            );

            if (!response.IsValidResponse)
            {
                // Outputs raw request/response details to the debug console window
                System.Diagnostics.Debug.WriteLine($"Search failed: {response.DebugInformation}");
                return new List<Product>();
            }

            return response.Documents.ToList();
        }

        // Debugging Helper: Retrieve everything in the index
        public async Task<List<Product>> GetAllDocumentsAsync()
        {
            var response = await _client.SearchAsync<Product>(s => s
                .Index(IndexName)
                .Query(q => q.MatchAll(m => { })) // Fixed: Added an empty lambda action
            );
            return response.Documents.ToList();
        }
    }
}