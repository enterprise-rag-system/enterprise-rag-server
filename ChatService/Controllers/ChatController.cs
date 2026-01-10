using ChatService.Interfaces;
using ChatService.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ChatService.Controllers;

[ApiController]
[Route("api/v1/chat")]
[Authorize]
public class ChatController : ControllerBase
{
        private readonly IChatService _chatService;
        private readonly ILogger<ChatController> _logger;

        public ChatController(
            IChatService chatService,
            ILogger<ChatController> logger)
        {
            _chatService = chatService;
            _logger = logger;
        }

        // -------------------------------
        // POST: Add user chat message
        // -------------------------------
        [HttpPost("projects/{projectId:guid}/messages")]
        [ProducesResponseType(StatusCodes.Status202Accepted)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> AddMessage(
            Guid projectId,
            [FromBody] AddChatMessageRequest request,
            CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(request.Message))
            {
                return BadRequest("Message cannot be empty.");
            }

            _logger.LogInformation(
                "HTTP request received to add chat message for ProjectId {ProjectId}",
                projectId);

            var reesponse = await _chatService.AddUserMessageAsync(
                projectId,
                request.Message,
                ct);

            // 202 because AI response is async
            return Ok(reesponse);
        }

        // -------------------------------
        // GET: Fetch chat history
        // -------------------------------
        [HttpGet("projects/{projectId:guid}/messages")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> GetMessages(
            Guid projectId,
            [FromQuery] int limit = 100,
            CancellationToken ct = default)
        {
            if (limit <= 0 || limit > 500)
            {
                return BadRequest("Limit must be between 1 and 500.");
            }

            var response = await _chatService.GetChatHistoryAsync(
                projectId,
                limit,
                ct);

            return Ok(response);
        }

}
