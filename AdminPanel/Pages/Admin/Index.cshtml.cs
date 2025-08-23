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
            Agents = _store.Snapshot();
            Policies = Agents.Keys.ToDictionary(k => k, k => _policies.GetPolicy(k),
                StringComparer.OrdinalIgnoreCase);
        }

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

        public IActionResult OnPostTimer(string machine, int minutes)
        {
            if (string.IsNullOrWhiteSpace(machine)) return RedirectToPage();
            if (minutes < 1) minutes = 1;

            var p = _policies.GetPolicy(machine);
            p.RequireLock = false;
            p.AllowedUntil = DateTimeOffset.Now.AddMinutes(minutes);
            p.Message = $"Timer set to {minutes} minute(s)";
            _policies.SetPolicy(machine, p);

            return RedirectToPage();
        }
    }
}
