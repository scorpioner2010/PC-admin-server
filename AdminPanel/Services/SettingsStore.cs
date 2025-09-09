namespace AdminPanel.Services
{
    /// <summary>
    /// Global admin settings shared by all machines.
    /// Holds a single UnlockPassword used by clients.
    /// </summary>
    public interface ISettingsStore
    {
        string UnlockPassword { get; set; }
    }

    public sealed class SettingsStore : ISettingsStore
    {
        private readonly object _sync = new();
        private string _unlockPassword = "admin123"; // default; editable via Admin UI later

        public string UnlockPassword
        {
            get { lock (_sync) return _unlockPassword; }
            set { lock (_sync) _unlockPassword = value ?? string.Empty; }
        }
    }
}