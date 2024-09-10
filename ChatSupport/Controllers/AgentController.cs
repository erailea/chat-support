using ChatSupport.Exceptions;
using ChatSupport.Models;
using ChatSupport.Services;
using ChatSupport.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace ChatSupport.Controllers
{
    [Route("api/agent")]
    [ApiController]
    public class AgentController : ControllerBase
    {
        private readonly IAgentService _agentService;

        private readonly ILogger<AgentController> _logger;

        public AgentController(
            IAgentService agentService,
            ILogger<AgentController> logger
        )
        {
            _agentService = agentService;
            _logger = logger;
        }

        [HttpPost("connect")]
        [ProducesResponseType(typeof(AgentDto), 200)]
        public async Task<ActionResult>
        Connect([FromBody] AgentConnectRequestDto agent)
        {
            try
            {
                if (agent == null)
                {
                    return BadRequest();
                }

                var resultAgent =
                    await _agentService.ConnectAsync(new MongoDB.Bson.ObjectId(agent.AgentId));

                return Ok(new AgentDto(resultAgent));
            }
            catch (AgentNotFoundException e)
            {
                return NotFound(e.Message);
            }
            catch (AgentShiftConflictException e)
            {
                return StatusCode(403, e.Message);
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
