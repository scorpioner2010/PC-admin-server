using AdminPanel.Services;
using Microsoft.AspNetCore.Mvc;

namespace AdminPanel.Controllers
{
    public sealed class AgentStatusDto
    {
        public string? Machine { get; set; }
        public string? OS { get; set; }
        public long UptimeSec { get; set; }
        public string? Time { get; set; }   // ISO8601 string
    }

    public sealed class UnlockRequestDto
    {
        public string? Machine { get; set; }
        public string? Password { get; set; }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class AgentController : ControllerBase
    {
        private readonly IAgentStore _store;
        private readonly IPolicyStore _policies;
        private readonly ISettingsStore _settings;

        public AgentController(IAgentStore store, IPolicyStore policies, ISettingsStore settings)
        {
            _store = store;
            _policies = policies;
            _settings = settings;
        }

        [HttpPost("status")]
        public IActionResult Status([FromBody] AgentStatusDto dto)
        {
            if (dto is null || string.IsNullOrWhiteSpace(dto.Machine))
                return BadRequest(new { error = "Invalid payload" });

            var machine = dto.Machine.Trim();

            // ✅ DTO -> Services.AgentStatus (Time як DateTimeOffset)
            DateTimeOffset when;
            if (!DateTimeOffset.TryParse(dto.Time, out when))
                when = DateTimeOffset.Now;

            var status = new AgentStatus
            {
                Machine = machine,
                OS = dto.OS ?? string.Empty,
                UptimeSec = dto.UptimeSec,
                Time = when
            };

            // зберегти останній статус
            _store.Upsert(status);

            // політика + одноразова команда
            var policy = _policies.GetPolicy(machine);

            string? command = policy.PendingCommand;
            if (!string.IsNullOrWhiteSpace(command))
            {
                policy.PendingCommand = null;     // очистити після доставки
                _policies.SetPolicy(machine, policy);
            }

            return Ok(new
            {
                message = "Status stored",
                policy = new
                {
                    allowedUntil = policy.AllowedUntil.ToString("O"),
                    requireLock  = policy.RequireLock,
                    manualUnlockGraceMinutes = policy.ManualUnlockGraceMinutes,
                    unlockPassword = _settings.UnlockPassword ?? string.Empty
                },
                command
            });
        }

        [HttpPost("unlock")]
        public IActionResult Unlock([FromBody] UnlockRequestDto dto)
        {
            if (dto is null || string.IsNullOrWhiteSpace(dto.Machine))
                return BadRequest(new { error = "Invalid payload" });

            var expected = _settings.UnlockPassword ?? string.Empty;
            if (!string.Equals(dto.Password ?? string.Empty, expected, StringComparison.Ordinal))
                return Unauthorized(new { error = "Wrong password" });

            var machine = dto.Machine.Trim();
            var p = _policies.GetPolicy(machine);
            p.RequireLock  = false;
            p.AllowedUntil = DateTimeOffset.Now.AddMinutes(p.ManualUnlockGraceMinutes);
            p.Message      = "Manual unlock";
            _policies.SetPolicy(machine, p);

            return Ok(new
            {
                message = "Unlocked",
                policy = new
                {
                    allowedUntil = p.AllowedUntil.ToString("O"),
                    requireLock  = p.RequireLock,
                    manualUnlockGraceMinutes = p.ManualUnlockGraceMinutes
                }
            });
        }
    }
}
