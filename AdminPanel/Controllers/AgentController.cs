using System;
using Microsoft.AspNetCore.Mvc;
using AdminPanel.Services;

namespace AdminPanel.Controllers
{
    [ApiController]
    [Route("api/agent")]
    public class AgentController : ControllerBase
    {
        private readonly IAgentStore _agents;
        private readonly IPolicyStore _policies;
        private readonly ISettingsStore _settings;
        private readonly ICommandsQueue _commands;

        public AgentController(IAgentStore agents, IPolicyStore policies, ISettingsStore settings, ICommandsQueue commands)
        {
            _agents = agents;
            _policies = policies;
            _settings = settings;
            _commands = commands;
        }

        public class AgentStatusDto
        {
            public string Machine { get; set; } = "";
            public string OS { get; set; } = "";
            public long UptimeSec { get; set; }
            public string Time { get; set; } = "";
        }

        [HttpPost("status")]
        public IActionResult Status([FromBody] AgentStatusDto status)
        {
            if (string.IsNullOrWhiteSpace(status.Machine))
                return BadRequest(new { message = "Machine required" });

            var now = DateTimeOffset.UtcNow;

            // зберегти останній статус
            _agents.Upsert(status.Machine, status.OS, now);

            // отримати/оновити політику
            var policy = _policies.GetPolicy(status.Machine);
            policy.UnlockPassword = _settings.UnlockPassword;
            policy.ManualUnlockGraceMinutes = _settings.ManualUnlockMinutes;

            // видати policy + команди агенту
            var cmds = _commands.DequeueAll(status.Machine);

            return Ok(new
            {
                message = "Status stored",
                policy = new
                {
                    allowedUntil = policy.AllowedUntil.ToString("O"),
                    requireLock = policy.RequireLock,
                    manualUnlockGraceMinutes = policy.ManualUnlockGraceMinutes,
                    unlockPassword = policy.UnlockPassword,
                    volumePercent = policy.VolumePercent
                },
                commands = cmds
            });
        }

        // (не обов'язково) окремий unlock, якщо агент його викликає
        public class UnlockDto
        {
            public string Machine { get; set; } = "";
            public string Password { get; set; } = "";
        }

        [HttpPost("unlock")]
        public IActionResult Unlock([FromBody] UnlockDto dto)
        {
            var policy = _policies.GetPolicy(dto.Machine);
            if (dto.Password == _settings.UnlockPassword)
            {
                policy.AllowedUntil = DateTimeOffset.UtcNow.AddMinutes(_settings.ManualUnlockMinutes);
                return Ok(new
                {
                    message = "Unlocked",
                    policy = new
                    {
                        allowedUntil = policy.AllowedUntil.ToString("O")
                    }
                });
            }
            return Unauthorized(new { message = "Bad password" });
        }
    }
}
