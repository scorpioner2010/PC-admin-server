using System.Collections.Concurrent;

namespace AdminPanel.Services
{
    public sealed class AgentStatus
    {
        public string Machine { get; set; } = "";
        public string OS { get; set; } = "";
        public long UptimeSec { get; set; }
        public DateTimeOffset Time { get; set; }
    }

    public sealed class AgentRecord
    {
        public AgentStatus Status { get; set; } = new AgentStatus();
        public DateTimeOffset LastSeen { get; set; }
    }

    public interface IAgentStore
    {
        void Upsert(AgentStatus status);
        IReadOnlyDictionary<string, AgentRecord> Snapshot();
        int Cleanup(TimeSpan maxAge);
    }

    public sealed class AgentStore : IAgentStore
    {
        private readonly ConcurrentDictionary<string, AgentRecord> _agents =
            new(StringComparer.OrdinalIgnoreCase);

        public void Upsert(AgentStatus status)
        {
            var rec = new AgentRecord
            {
                Status = status,
                LastSeen = DateTimeOffset.Now
            };
            _agents.AddOrUpdate(status.Machine, rec, (_, __) => rec);
        }

        public IReadOnlyDictionary<string, AgentRecord> Snapshot()
            => new Dictionary<string, AgentRecord>(_agents);

        public int Cleanup(TimeSpan maxAge)
        {
            var cutoff = DateTimeOffset.Now - maxAge;
            int removed = 0;
            foreach (var kv in _agents)
            {
                if (kv.Value.LastSeen < cutoff)
                {
                    if (_agents.TryRemove(kv.Key, out _))
                        removed++;
                }
            }
            return removed;
        }
    }
}