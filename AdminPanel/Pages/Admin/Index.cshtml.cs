using AdminPanel.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AdminPanel.Pages.Admin
{
    public class IndexModel : PageModel
    {
        private readonly IAgentStore _store;
        private readonly IPolicyStore _policies;
        private readonly ISettingsStore _settings;

        public IndexModel(IAgentStore store, IPolicyStore policies, ISettingsStore settings)
        {
            _store = store;
            _policies = policies;
            _settings = settings;
        }

        public IReadOnlyDictionary<string, AgentRecord> Agents { get; private set; } =
            new Dictionary<string, AgentRecord>();

        public Dictionary<string, AgentPolicy> Policies { get; private set; } =
            new(StringComparer.OrdinalIgnoreCase);

        public string CurrentUnlockPassword => _settings.UnlockPassword ?? string.Empty;

        public void OnGet()
        {
            _store.Cleanup(TimeSpan.FromSeconds(10));
            Agents = _store.Snapshot();
            Policies = Agents.Keys.ToDictionary(k => k, k => _policies.GetPolicy(k),
                StringComparer.OrdinalIgnoreCase);
        }

        // ---------- Global ----------

        public IActionResult OnPostSetPassword(string newPassword)
        {
            _settings.UnlockPassword = newPassword ?? string.Empty;
            TempData["Msg"] = "Unlock password updated.";
            return RedirectToPage();
        }

        // ---------- Time controls ----------

        public IActionResult OnPostAddTime(string machine, int minutes)
        {
            if (string.IsNullOrWhiteSpace(machine)) return RedirectToPage();
            if (minutes < 1) minutes = 1;

            var p = _policies.GetPolicy(machine);
            var now = DateTimeOffset.Now;
            var baseTime = p.AllowedUntil > now ? p.AllowedUntil : now;

            p.AllowedUntil = baseTime.AddMinutes(minutes);
            p.RequireLock = false;
            p.Message = $"Added {minutes} minute(s)";
            _policies.SetPolicy(machine, p);

            return RedirectToPage();
        }

        public IActionResult OnPostSetTime(string machine, int minutes)
        {
            if (string.IsNullOrWhiteSpace(machine)) return RedirectToPage();
            if (minutes < 0) minutes = 0;

            var p = _policies.GetPolicy(machine);
            var now = DateTimeOffset.Now;

            p.AllowedUntil = minutes == 0 ? now : now.AddMinutes(minutes);
            p.RequireLock = false;
            p.Message = $"Set time to {minutes} minute(s) from now";
            _policies.SetPolicy(machine, p);

            return RedirectToPage();
        }

        public IActionResult OnPostSetGrace(string machine, int graceMinutes)
        {
            if (string.IsNullOrWhiteSpace(machine)) return RedirectToPage();
            if (graceMinutes < 0) graceMinutes = 0;
            if (graceMinutes > 240) graceMinutes = 240;

            var p = _policies.GetPolicy(machine);
            p.ManualUnlockGraceMinutes = graceMinutes;
            p.Message = $"Password time set to {graceMinutes} minute(s)";
            _policies.SetPolicy(machine, p);

            return RedirectToPage();
        }

        // ---------- Power controls ----------

        public IActionResult OnPostSleep(string machine)
        {
            if (string.IsNullOrWhiteSpace(machine)) return RedirectToPage();

            var p = _policies.GetPolicy(machine);
            p.PendingCommand = "sleep";
            p.Message = "Sleep requested";
            _policies.SetPolicy(machine, p);

            TempData["Msg"] = $"Sleep sent to {machine}.";
            return RedirectToPage();
        }

        public IActionResult OnPostShutdown(string machine)
        {
            if (string.IsNullOrWhiteSpace(machine)) return RedirectToPage();

            var p = _policies.GetPolicy(machine);
            p.PendingCommand = "shutdown";
            p.Message = "Shutdown requested";
            _policies.SetPolicy(machine, p);

            TempData["Msg"] = $"Shutdown sent to {machine}.";
            return RedirectToPage();
        }
    }
}
