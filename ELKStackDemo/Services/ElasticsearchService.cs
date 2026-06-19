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
            var settings = new ElasticsearchClientSettings(new Uri("http://localhost:9200"))
                .Authentication(new BasicAuthentication("elastic", "A123456a"))
                .DefaultIndex(IndexName)
                .RequestTimeout(TimeSpan.FromMinutes(2));

            _client = new ElasticsearchClient(settings);
        }

        // Create Index With Explicit Mapping
        public async Task CreateIndexWithMappingAsync()
        {
            var existsResponse = await _client.Indices.ExistsAsync(IndexName);
            if (existsResponse.Exists)
            {
                await _client.Indices.DeleteAsync(IndexName);
            }

            var createResponse = await _client.Indices.CreateAsync(IndexName, c => c
                .Mappings(m => m
                    .Properties<Product>(p => p
                        // Provide property selector as the first argument, configuration as the second
                        .Text(x => x.Name, t => t
                            .Analyzer("standard")
                            .Fields(new Elastic.Clients.Elasticsearch.Mapping.Properties
                            {
                                { "keyword", new Elastic.Clients.Elasticsearch.Mapping.KeywordProperty() }
                            })
                        )
                        .Text(x => x.Description, t => t.Analyzer("standard"))

                        // Replaced .Number() with explicit .DoubleNumber()
                        .DoubleNumber(x => x.Price)

                        .Date(x => x.CreatedAt)
                        .Keyword(x => x.Category)
                    )
                )
            );

            if (!createResponse.IsValidResponse)
            {
                throw new Exception($"Failed to create index: {createResponse.ElasticsearchServerError?.Error}");
            }
        }

        public async Task BulkIndexAsync(List<Product> products)
        {
            var bulkResponse = await _client.BulkAsync(b => b
                .IndexMany(products)
            );

            // Replaced EnsureSuccess() with IsValidResponse check
            if (!bulkResponse.IsValidResponse)
            {
                throw new Exception($"Bulk indexing failed: {bulkResponse.DebugInformation}");
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
                .Query(q => q.MatchAll(m => { }))
            );
            return response.Documents.ToList();
        }

        // Basic Search using Match Query
        public async Task<List<Product>> SearchMatchAsync(string keyword)
        {
            var response = await _client.SearchAsync<Product>(s => s
                .Index(IndexName)
                .Query(q => q
                    .Match(m => m
                        .Field(f => f.Name)
                        .Query(keyword)
                    )
                )
                .Size(10)
            );

            return response.Documents.ToList();
        }

        // Exact Search using Term Query
        public async Task<List<Product>> SearchTermAsync(string category)
        {
            var response = await _client.SearchAsync<Product>(s => s
                .Index(IndexName)
                .Query(q => q
                    .Term(t => t
                        .Field(f => f.Category)
                        .Value(category)
                    )
                )
            );

            return response.Documents.ToList();
        }

        // Basic Bool Query (Match + Term Combination)
        public async Task<List<Product>> SearchBoolAsync(string keyword, string? category = null)
        {
            var response = await _client.SearchAsync<Product>(s => s
                .Index(IndexName)
                .Query(q => q
                    .Bool(b => b
                        .Must(m => m
                            .Match(mm => mm
                                .Field(f => f.Name)
                                .Query(keyword)
                            )
                        )
                        .Filter(f => f
                            .Term(t => t
                                .Field(ff => ff.Category)
                                .Value(category)
                            )
                        )
                    )
                )
            );

            return response.Documents.ToList();
        }

        // Multi-Field Search using MultiMatch Query
        public async Task<List<Product>> SearchMultiMatchAsync(string keyword)
        {
            var response = await _client.SearchAsync<Product>(s => s
                .Index(IndexName)
                .Query(q => q
                    .MultiMatch(m => m
                        .Query(keyword)
                        .Fields(new[] { "name^3", "description" }) 
                    )
                )
                .Size(10)
            );

            return response.Documents.ToList();
        }
    }
}