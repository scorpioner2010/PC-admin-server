namespace AdminPanel.Services
{
    public sealed class AgentPolicy
    {
        public DateTimeOffset AllowedUntil { get; set; }
        public bool RequireLock { get; set; }
        public int ManualUnlockGraceMinutes { get; set; }  // NEW: client uses this when offline manual unlock
        public string? Message { get; set; }
    }

    public interface IPolicyStore
    {
        AgentPolicy GetPolicy(string machine);
        void SetPolicy(string machine, AgentPolicy policy);
    }

    public sealed class PolicyStore : IPolicyStore
    {
        private readonly Dictionary<string, AgentPolicy> _policies =
            new(StringComparer.OrdinalIgnoreCase);

        public AgentPolicy GetPolicy(string machine)
        {
            if (!_policies.TryGetValue(machine, out var p))
            {
                p = new AgentPolicy
                {
                    AllowedUntil = DateTimeOffset.Now.AddHours(1),
                    RequireLock = false,
                    ManualUnlockGraceMinutes = 10,     // default grace
                    Message = "Default allow 60 min"
                };
                _policies[machine] = p;
            }
            return p;
        }

        public void SetPolicy(string machine, AgentPolicy policy)
        {
            _policies[machine] = policy;
        }
    }
}