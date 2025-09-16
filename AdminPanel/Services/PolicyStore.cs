using System;
using System.Collections.Concurrent;

namespace AdminPanel.Services
{
    public interface IPolicyStore
    {
        AgentPolicy GetPolicy(string machine);
        void AddTime(string machine, int minutes);
        void SetTime(string machine, int minutes);
        void SetVolume(string machine, int volumePercent);
    }

    public class AgentPolicy
    {
        public DateTimeOffset AllowedUntil { get; set; } = DateTimeOffset.UtcNow;
        public bool RequireLock { get; set; } = false;

        public int ManualUnlockGraceMinutes { get; set; } = 60;

        // пароль, який ми також віддаємо в policy
        public string UnlockPassword { get; set; } = "7789Saurex";

        // ДЕФОЛТ: 75% (було 50)
        public int VolumePercent { get; set; } = 75;
    }

    public class PolicyStore : IPolicyStore
    {
        private readonly ConcurrentDictionary<string, AgentPolicy> _policies = new();

        private AgentPolicy Ensure(string machine) =>
            _policies.GetOrAdd(machine, _ => new AgentPolicy());

        public AgentPolicy GetPolicy(string machine) => Ensure(machine);

        public void AddTime(string machine, int minutes)
        {
            var p = Ensure(machine);
            if (minutes < 0) minutes = 0;
            var now = DateTimeOffset.UtcNow;
            if (p.AllowedUntil < now) p.AllowedUntil = now;
            p.AllowedUntil = p.AllowedUntil.AddMinutes(minutes);
        }

        public void SetTime(string machine, int minutes)
        {
            var p = Ensure(machine);
            if (minutes < 0) minutes = 0;
            p.AllowedUntil = DateTimeOffset.UtcNow.AddMinutes(minutes);
        }

        public void SetVolume(string machine, int volumePercent)
        {
            var p = Ensure(machine);
            if (volumePercent < 0) volumePercent = 0;
            if (volumePercent > 100) volumePercent = 100;
            p.VolumePercent = volumePercent;
        }
    }
}