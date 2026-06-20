using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.Aggregations;
using Elastic.Clients.Elasticsearch.IndexManagement;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Elastic.Transport;
using ELKStackDemo.Models;

namespace ELKStackDemo.Services
{
    public class ElasticsearchService
    {
        private readonly ElasticsearchClient _client;
        private const string IndexName = "products";

        public ElasticsearchService(IConfiguration configuration)
        {
            var esUri = new Uri("http://localhost:9200");
            var settings = new ElasticsearchClientSettings(esUri);

            var apiKey = configuration["Elasticsearch:ApiKey"];
            if (!string.IsNullOrEmpty(apiKey))
            {
                settings = settings.Authentication(new ApiKey(apiKey));
            }

            settings = settings.RequestTimeout(TimeSpan.FromMinutes(2))
                               .DefaultIndex(IndexName);

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
                .Settings(s => s
                .NumberOfShards(1)           // One node
                .NumberOfReplicas(0)         
            )
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

        // Advanced search with Bool + Filters
        public async Task<List<Product>> AdvancedSearchAsync(string? keyword, string? category, decimal? minPrice, decimal? maxPrice)
        {
            var mustQueries = new List<Action<QueryDescriptor<Product>>>();
            var filterQueries = new List<Action<QueryDescriptor<Product>>>();

            if (!string.IsNullOrEmpty(keyword))
            {
                mustQueries.Add(q => q.MultiMatch(mm => mm.Query(keyword).Fields(new[] { "name^3", "description" })));
            }

            if (!string.IsNullOrEmpty(category))
            {
                filterQueries.Add(q => q.Term(t => t.Field(p => p.Category).Value(category)));
            }

            if (minPrice.HasValue || maxPrice.HasValue)
            {
                // FIX: Wrap NumberRange inside the .Range() descriptor
                filterQueries.Add(q => q.Range(r => r
                    .NumberRange(nr => nr
                        .Field(p => p.Price)
                        .Gte(minPrice.HasValue ? (double)minPrice.Value : null)
                        .Lte(maxPrice.HasValue ? (double)maxPrice.Value : null)
                    )
                ));
            }

            var response = await _client.SearchAsync<Product>(s => s
                .Index(IndexName)
                .Size(20)
                // Correct sorting syntax for Elastic.Clients.Elasticsearch 9.x
                .Sort(so => so.Field(f => f
                    .Field(p => p.Price)
                    .Order(SortOrder.Desc)
                ))
                .Query(q => q.Bool(b =>
                {
                    if (mustQueries.Any()) b.Must(mustQueries.ToArray());
                    if (filterQueries.Any()) b.Filter(filterQueries.ToArray());
                }))
            );

            return response.Documents.ToList();
        }

        // Aggregations (Terms + Average + Range)
        public async Task<object> GetAggregationsAsync()
        {
            var response = await _client.SearchAsync<Product>(s => s
                .Index(IndexName)
                .Size(0) // Aggregation only, no documents
                .Aggregations(a => a
                    .Add("categories", agg => agg
                        .Terms(t => t.Field(f => f.Category))
                    )
                    // FIX: Renamed Average() to Avg() to prevent System.Linq compiler confusion
                    .Add("avg_price", agg => agg
                        .Avg(avg => avg.Field(f => f.Price))
                    )
                    .Add("price_ranges", agg => agg
                        .Range(r => r
                            .Field(f => f.Price)
                            // FIX: Use the fully qualified AggregationRange type
                            .Ranges(new Elastic.Clients.Elasticsearch.Aggregations.AggregationRange[]
                            {
                                new() { To = 20000000, Key = "cheap" },
                                new() { From = 20000000, To = 40000000, Key = "medium" },
                                new() { From = 40000000, Key = "expensive" }
                            })
                        )
                    )
                )
            );

            if (!response.IsValidResponse)
            {
                System.Diagnostics.Debug.WriteLine($"Aggregations failed: {response.DebugInformation}");
                return new { Error = "Failed to retrieve aggregations" };
            }

            // Extracting results and mapping them to standard C# objects so Swagger can serialize them
            return new
            {
                Categories = response.Aggregations?.GetStringTerms("categories")?.Buckets
                    .Select(b => new
                    {
                        Category = b.Key.Value,
                        Count = b.DocCount
                    }).ToList(),

                AveragePrice = response.Aggregations?.GetAverage("avg_price")?.Value,

                PriceRanges = response.Aggregations?.GetRange("price_ranges")?.Buckets
                    .Select(b => new
                    {
                        Range = b.Key,
                        Count = b.DocCount
                    }).ToList()
            };
        }

        // Cluster Health Monitoring - Final Stable Version
        public async Task<object> GetClusterHealthAsync()
        {
            try
            {
                var healthResponse = await _client.Cluster.HealthAsync();

                // Use GetAsync with an empty request object or use the GetAsync overload 
                // that does not require a generic type argument to fetch index names.
                var indicesResponse = await _client.Indices.GetAsync(new GetIndexRequest("_all"));

                return new
                {
                    ClusterName = healthResponse.ClusterName,
                    Status = healthResponse.Status.ToString(),
                    NumberOfNodes = healthResponse.NumberOfNodes,
                    ActiveShards = healthResponse.ActiveShards,
                    RelocatingShards = healthResponse.RelocatingShards,
                    InitializingShards = healthResponse.InitializingShards,
                    UnassignedShards = healthResponse.UnassignedShards,
                    TimedOut = healthResponse.TimedOut,
                    // Access indices count from the response
                    IndicesCount = indicesResponse.Indices?.Count ?? 0,
                    Timestamp = DateTime.UtcNow
                };
            }
            catch (Exception ex)
            {
                Console.Error.WriteLine($"Error retrieving cluster health: {ex.Message}");
                return new
                {
                    Error = "Failed to retrieve full cluster details",
                    Status = "Yellow",
                    Message = ex.Message
                };
            }
        }

        public async Task<List<Product>> GetAllProductsAsync()
        {
            var response = await _client.SearchAsync<Product>(s => s
                .Index(IndexName)
                .Size(100)
                .Query(q => q.MatchAll())
            );
            return response.Documents.ToList();
        }

        public async Task UpdateProductAsync(Product product)
        {
            product.UpdatedAt = DateTime.UtcNow;
            await _client.IndexAsync(product);
        }

        public async Task DeleteProductAsync(int id)
        {
            await _client.DeleteAsync<Product>(id);
        }
    }
}