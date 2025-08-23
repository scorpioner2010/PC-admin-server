using AdminPanel.Services;
using Microsoft.AspNetCore.Mvc;

namespace AdminPanel.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AgentController : ControllerBase
    {
        private readonly IAgentStore _store;
        private readonly IPolicyStore _policies;

        public AgentController(IAgentStore store, IPolicyStore policies)
        {
            _store = store;
            _policies = policies;
        }

        public sealed class IncomingStatus
        {
            public string Machine { get; set; } = "";
            public string OS { get; set; } = "";
            public long UptimeSec { get; set; }
            public string Time { get; set; } = "";
        }

        [HttpPost("status")]
        public IActionResult ReceiveStatus([FromBody] IncomingStatus data)
        {
            if (data == null || string.IsNullOrWhiteSpace(data.Machine))
                return BadRequest(new { message = "No data or missing machine" });

            var parsedTime = DateTimeOffset.TryParse(data.Time, out var dto) ? dto : DateTimeOffset.Now;

            _store.Upsert(new AgentStatus
            {
                Machine = data.Machine,
                OS = data.OS,
                UptimeSec = data.UptimeSec,
                Time = parsedTime
            });

            var policy = _policies.GetPolicy(data.Machine);

            return Ok(new
            {
                message = "Status stored",
                policy = new
                {
                    allowedUntil = policy.AllowedUntil.ToString("O"),
                    requireLock = policy.RequireLock,
                    message = policy.Message
                }
            });
        }
    }
}