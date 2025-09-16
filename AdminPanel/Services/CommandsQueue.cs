using System.Collections.Concurrent;

namespace AdminPanel.Services
{
    public enum AgentCommandType
    {
        Shutdown = 1,
        SetVolume = 2
    }

    public class AgentCommand
    {
        public AgentCommandType Type { get; set; }
        public int? IntValue { get; set; } // для Volume (0..100)
    }

    public interface ICommandsQueue
    {
        void EnqueueCommand(string machine, AgentCommand cmd);
        AgentCommand[] DequeueAll(string machine);
    }

    public class CommandsQueue : ICommandsQueue
    {
        private readonly ConcurrentDictionary<string, ConcurrentQueue<AgentCommand>> _q = new();

        public void EnqueueCommand(string machine, AgentCommand cmd)
        {
            var qq = _q.GetOrAdd(machine, _ => new ConcurrentQueue<AgentCommand>());
            qq.Enqueue(cmd);
        }

        public AgentCommand[] DequeueAll(string machine)
        {
            if (!_q.TryGetValue(machine, out var qq)) return new AgentCommand[0];
            var list = new System.Collections.Generic.List<AgentCommand>();
            while (qq.TryDequeue(out var c)) list.Add(c);
            return list.ToArray();
        }
    }
}