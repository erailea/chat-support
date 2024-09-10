using ChatSupport.Models;
using ChatSupport.Services;
using ChatSupport.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ChatSupport.Controllers
{
    [Route("api/chat")]
    [ApiController]
    public class ChatController : ControllerBase
    {

        private readonly ILogger<ChatController> _logger;
        private readonly IChatService _chatService;

        public ChatController(
            ILogger<ChatController> logger,
            IChatService chatService
        )
        {
            _logger = logger;
            _chatService = chatService;
        }

        [HttpPost("create")]
        [ProducesResponseType(typeof(ChatSession), 200)]
        public async Task<ActionResult>
        CreateChatSession()
        {
            try
            {
                var resultChatSession =
                    await _chatService.CreateChatSessionAsync();

                return Ok(resultChatSession);
            }
            catch (Exception e)
            {
                _logger
                    .LogError(e,
                    "An error occurred while processing the request.");
                return StatusCode(500, "An error occurred");
            }
        }

        [HttpGet("poll/{chatSessionId}")]
        [ProducesResponseType(typeof(ChatSession), 200)]
        public async Task<ActionResult>
        PollChatSession(string chatSessionId)
        {
            try
            {
                await _chatService.PollChatSessionAsync(new MongoDB.Bson.ObjectId(chatSessionId));
                return Ok();
            }
            catch (Exception e)
            {
                _logger
                    .LogError(e,
                    "An error occurred while processing the request.");
                return StatusCode(500, "An error occurred");
            }
        }

        [HttpPost("send")]
        [ProducesResponseType(typeof(ChatSession), 200)]
        public async Task<ActionResult>
        SendChatMessage([FromBody] ChatMessage chatMessage)
        {
            try
            {
                await _chatService.SendChatMessageAsync(chatMessage);
                return Ok();
            }
            catch (Exception e)
            {
                _logger
                    .LogError(e,
                    "An error occurred while processing the request.");
                return StatusCode(500, "An error occurred");
            }
        }

        //send message for agent
        [HttpPost("send/agent")]
        [ProducesResponseType(typeof(ChatSession), 200)]
        public async Task<ActionResult>
        SendAgentChatMessage([FromBody] ChatAgentMessage chatMessage)
        {
            try
            {
                await _chatService.SendAgentChatMessageAsync(chatMessage);
                return Ok();
            }
            catch (Exception e)
            {
                _logger
                    .LogError(e,
                    "An error occurred while processing the request.");
                return StatusCode(500, "An error occurred");
            }
        }
    }


}