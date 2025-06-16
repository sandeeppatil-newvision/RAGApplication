using Azure;
using Azure.Search.Documents;
using Azure.Search.Documents.Models;

namespace RAG.API.Services
{
    public class AzureSearchService
    {
        private readonly SearchClient _searchClient;

        public AzureSearchService(IConfiguration config)
        {
            var endpoint = new Uri(config["AzureSearch:Endpoint"]);
            var indexName = config["AzureSearch:IndexName"];
            var apiKey = config["AzureSearch:ApiKey"];

            _searchClient = new SearchClient(endpoint, indexName, new AzureKeyCredential(apiKey));
        }

        public async Task<List<string>> SearchRelevantChunksAsync(string query)
        {
            var options = new SearchOptions()
            {
                IncludeTotalCount = true
            };

            // Add the "content" field to Select to ensure only relevant fields are returned
            options.Select.Add("content");

            // Use GetResults() to access the deserialized documents, not the raw response
            var response = await _searchClient.SearchAsync<SearchDocument>(query, options).ConfigureAwait(false);
            var results = response.Value.GetResults();

            return results
                .Select(r => r.Document.TryGetValue("content", out var content) ? content?.ToString() : null)
                .Where(text => !string.IsNullOrEmpty(text))
                .ToList()!;
        }
    }
}
