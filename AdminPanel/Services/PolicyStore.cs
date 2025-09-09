using System.Collections.Concurrent;

namespace AdminPanel.Services
{
    public sealed class AgentPolicy
    {
        public DateTimeOffset AllowedUntil { get; set; } = DateTimeOffset.Now.AddHours(1);
        public bool RequireLock { get; set; } = false;

        /// <summary>
        /// Minutes granted when user unlocks with the server password.
        /// </summary>
        public int ManualUnlockGraceMinutes { get; set; } = 60;

        /// <summary>
        /// One-shot command for agent: "sleep" | "shutdown" (cleared after delivery).
        /// </summary>
        public string? PendingCommand { get; set; } = null;

        public string Message { get; set; } = "";
    }

    public interface IPolicyStore
    {
        AgentPolicy GetPolicy(string machine);
        void SetPolicy(string machine, AgentPolicy policy);
    }

    public sealed class PolicyStore : IPolicyStore
    {
        private readonly ConcurrentDictionary<string, AgentPolicy> _policies =
            new(StringComparer.OrdinalIgnoreCase);

        public AgentPolicy GetPolicy(string machine)
        {
            if (string.IsNullOrWhiteSpace(machine)) machine = "unknown";
            return _policies.GetOrAdd(machine, _ => new AgentPolicy
            {
                AllowedUntil = DateTimeOffset.Now.AddHours(1),
                RequireLock = false,
                ManualUnlockGraceMinutes = 60,
                PendingCommand = null,
                Message = "Default policy"
            });
        }

        public void SetPolicy(string machine, AgentPolicy policy)
        {
            if (string.IsNullOrWhiteSpace(machine)) return;
            _policies[machine] = policy;
        }
    }
}