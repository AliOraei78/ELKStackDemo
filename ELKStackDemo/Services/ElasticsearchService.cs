using Elastic.Clients.Elasticsearch;
using ELKStackDemo.Models;

namespace ELKStackDemo.Services
{
    public class ElasticsearchService
    {
        private readonly ElasticsearchClient _client;
        private const string IndexName = "products";

        public ElasticsearchService()
        {
            var settings = new ElasticsearchClientSettings(new Uri("http://localhost:9200"))
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

        // Insert Document
        public async Task IndexDocumentAsync(Product product)
        {
            await _client.IndexAsync(product, IndexName);
        }

        // Get Document by Id
        public async Task<Product?> GetDocumentAsync(int id)
        {
            // FIX: Pass the IndexName first, then convert the int ID to a string
            var response = await _client.GetAsync<Product>(IndexName, id.ToString());
            return response.Source;
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

            return response.Documents.ToList();
        }

        // Delete Document
        public async Task DeleteDocumentAsync(int id)
        {
            // FIX: Removed the generic type specification <Product> since we are targetting by ID,
            // and provided the IndexName first, followed by the string ID.
            await _client.DeleteAsync(IndexName, id.ToString());
        }
    }
}