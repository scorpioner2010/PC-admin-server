using AdminPanel.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace AdminPanel.Pages.Admin
{
    public class IndexModel : PageModel
    {
        private readonly IAgentStore _store;
        private readonly IPolicyStore _policies;

        public IndexModel(IAgentStore store, IPolicyStore policies)
        {
            _store = store;
            _policies = policies;
        }

        public IReadOnlyDictionary<string, AgentRecord> Agents { get; private set; } =
            new Dictionary<string, AgentRecord>();

        public Dictionary<string, AgentPolicy> Policies { get; private set; } =
            new(StringComparer.OrdinalIgnoreCase);

        public void OnGet()
        {
            // keep only agents active within last 10 seconds
            _store.Cleanup(TimeSpan.FromSeconds(10));

            Agents = _store.Snapshot();
            Policies = Agents.Keys.ToDictionary(k => k, k => _policies.GetPolicy(k),
                StringComparer.OrdinalIgnoreCase);
        }

        // Add time: AllowedUntil = max(AllowedUntil, Now) + minutes
        public IActionResult OnPostAddTime(string machine, int minutes)
        {
            if (string.IsNullOrWhiteSpace(machine)) return RedirectToPage();
            if (minutes < 1) minutes = 1;

            var p = _policies.GetPolicy(machine);
            var now = DateTimeOffset.Now;
            var baseTime = p.AllowedUntil > now ? p.AllowedUntil : now;

            p.AllowedUntil = baseTime.AddMinutes(minutes);
            p.RequireLock = false; // giving time should remove forced lock
            p.Message = $"Added {minutes} minute(s)";
            _policies.SetPolicy(machine, p);

            return RedirectToPage();
        }

        // Set time: AllowedUntil = Now + minutes (overwrites any existing time)
        public IActionResult OnPostSetTime(string machine, int minutes)
        {
            if (string.IsNullOrWhiteSpace(machine)) return RedirectToPage();
            if (minutes < 0) minutes = 0;

            var p = _policies.GetPolicy(machine);
            var now = DateTimeOffset.Now;

            if (minutes == 0)
            {
                // immediate lock by time (do not force RequireLock, let time rule handle it)
                p.AllowedUntil = now;
            }
            else
            {
                p.AllowedUntil = now.AddMinutes(minutes);
            }

            p.RequireLock = false; // time-based control only
            p.Message = $"Set time to {minutes} minute(s) from now";
            _policies.SetPolicy(machine, p);

            return RedirectToPage();
        }

        // Keep Lock / Unlock (optional; you can remove if not needed)
        public IActionResult OnPostBlock(string machine)
        {
            if (string.IsNullOrWhiteSpace(machine)) return RedirectToPage();

            var p = _policies.GetPolicy(machine);
            p.RequireLock = true;
            p.AllowedUntil = DateTimeOffset.Now;
            p.Message = "Forced lock by admin";
            _policies.SetPolicy(machine, p);

            return RedirectToPage();
        }

        public IActionResult OnPostUnblock(string machine)
        {
            if (string.IsNullOrWhiteSpace(machine)) return RedirectToPage();

            var p = _policies.GetPolicy(machine);
            p.RequireLock = false;
            if (p.AllowedUntil < DateTimeOffset.Now.AddMinutes(1))
                p.AllowedUntil = DateTimeOffset.Now.AddHours(1);
            p.Message = "Unlocked by admin";
            _policies.SetPolicy(machine, p);

            return RedirectToPage();
        }

        // Grace editor remains, if you already added it earlier:
        public IActionResult OnPostSetGrace(string machine, int graceMinutes)
        {
            if (string.IsNullOrWhiteSpace(machine)) return RedirectToPage();
            if (graceMinutes < 0) graceMinutes = 0;
            if (graceMinutes > 240) graceMinutes = 240;

            var p = _policies.GetPolicy(machine);
            p.ManualUnlockGraceMinutes = graceMinutes;
            p.Message = $"Manual unlock grace set to {graceMinutes} minute(s)";
            _policies.SetPolicy(machine, p);

            return RedirectToPage();
        }
    }
}
