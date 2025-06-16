using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RAG.API.Services;

namespace RAG.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ChatController : ControllerBase
    {
        private readonly AzureSearchService _searchService;
        private readonly AzureAIService _aiService;

        public ChatController(AzureSearchService searchService, AzureAIService aiService)
        {
            _searchService = searchService;
            _aiService = aiService;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] ChatRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
                return BadRequest("Question is required.");

            var relevantChunks = await _searchService.SearchRelevantChunksAsync(request.Message);
            var context = string.Join("\n", relevantChunks);
            var answer = await _aiService.AskQuestionAsync(context, request.Message);

            return Ok(new { question = request.Message, answer });
        }
    }
}
public class ChatRequest
{
    public string Message { get; set; }
}