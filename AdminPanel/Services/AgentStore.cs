using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace AdminPanel.Services
{
    public interface IAgentStore
    {
        /// <summary>Додати/оновити запис про ПК.</summary>
        void Upsert(string machine, string os, DateTimeOffset nowUtc, long? uptimeSec = null);

        /// <summary>Отримати ВСІ відомі ПК (онлайн і офлайн). Копія для читання.</summary>
        Dictionary<string, AgentRecord> GetAll();
    }

    public class AgentStatus
    {
        public string Machine { get; set; } = "";
        public string OS { get; set; } = "";
        public long UptimeSec { get; set; }
    }

    public class AgentRecord
    {
        private const int OnlineWindowSeconds = 15; // якщо бачили за останні 15 с — ONLINE

        public AgentStatus Status { get; set; } = new AgentStatus();
        public DateTimeOffset LastSeenUtc { get; set; }

        /// <summary>Онлайн, якщо бачили за останні OnlineWindowSeconds.</summary>
        public bool IsOnline =>
            (DateTimeOffset.UtcNow - LastSeenUtc) <= TimeSpan.FromSeconds(OnlineWindowSeconds);
    }

    public class AgentStore : IAgentStore
    {
        private readonly ConcurrentDictionary<string, AgentRecord> _agents = new();

        public void Upsert(string machine, string os, DateTimeOffset nowUtc, long? uptimeSec = null)
        {
            var rec = _agents.GetOrAdd(machine, _ => new AgentRecord
            {
                Status = new AgentStatus { Machine = machine }
            });

            rec.Status.Machine = machine;
            rec.Status.OS = os ?? "";
            if (uptimeSec.HasValue) rec.Status.UptimeSec = uptimeSec.Value;
            rec.LastSeenUtc = nowUtc;
        }

        public Dictionary<string, AgentRecord> GetAll()
        {
            // повертаємо копії, щоб зовні не змінювали наші об’єкти
            return _agents.ToDictionary(
                kv => kv.Key,
                kv => new AgentRecord
                {
                    Status = new AgentStatus
                    {
                        Machine = kv.Value.Status.Machine,
                        OS = kv.Value.Status.OS,
                        UptimeSec = kv.Value.Status.UptimeSec
                    },
                    LastSeenUtc = kv.Value.LastSeenUtc
                });
        }
    }
}
