using System.Collections.Generic;
using System.Linq;
using AdminPanel.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AdminPanel.Pages.Admin
{
    public class IndexModel : PageModel
    {
        private readonly IAgentStore _agents;
        private readonly IPolicyStore _policies;
        private readonly ISettingsStore _settings;
        private readonly ICommandsQueue _commands;

        public IndexModel(IAgentStore agents, IPolicyStore policies, ISettingsStore settings, ICommandsQueue commands)
        {
            _agents = agents;
            _policies = policies;
            _settings = settings;
            _commands = commands;
        }

        public Dictionary<string, AgentRecord> Agents { get; private set; } = new();
        public Dictionary<string, AgentPolicy> Policies { get; private set; } = new();
        public string CurrentUnlockPassword => _settings.UnlockPassword;

        public void OnGet()
        {
            // показуємо ВСІ ПК, навіть якщо офлайн
            Agents = _agents.GetAll();
            Policies = Agents.Keys.ToDictionary(k => k, k => _policies.GetPolicy(k));
        }

        public IActionResult OnPostSetPassword(string newPassword)
        {
            if (!string.IsNullOrWhiteSpace(newPassword))
            {
                _settings.SetUnlockPassword(newPassword);
                TempData["Msg"] = "Password updated";
            }
            return RedirectToPage();
        }

        public IActionResult OnPostAddTime(string machine, int minutes)
        {
            _policies.AddTime(machine, minutes);
            return RedirectToPage();
        }

        public IActionResult OnPostSetTime(string machine, int minutes)
        {
            _policies.SetTime(machine, minutes);
            return RedirectToPage();
        }

        public IActionResult OnPostShutdown(string machine)
        {
            _commands.EnqueueCommand(machine, new AgentCommand { Type = AgentCommandType.Shutdown });
            TempData["Msg"] = $"Shutdown sent to {machine}.";
            return RedirectToPage();
        }

        // Приймаємо гучність з JS без перезавантаження
        public IActionResult OnPostSetVolume(string machine, int volume)
        {
            if (volume < 0) volume = 0; if (volume > 100) volume = 100;
            _policies.SetVolume(machine, volume);
            _commands.EnqueueCommand(machine, new AgentCommand { Type = AgentCommandType.SetVolume, IntValue = volume });
            return new OkResult();
        }
    }
}
