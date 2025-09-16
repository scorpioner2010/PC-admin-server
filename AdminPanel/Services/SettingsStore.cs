namespace AdminPanel.Services
{
    public interface ISettingsStore
    {
        string UnlockPassword { get; }
        int ManualUnlockMinutes { get; } // якщо потрібно далі
        void SetUnlockPassword(string value);
    }

    public class SettingsStore : ISettingsStore
    {
        // дефолтний пароль
        private string _unlockPassword = "7789Saurex";
        public string UnlockPassword => _unlockPassword;

        public int ManualUnlockMinutes { get; } = 60;

        public void SetUnlockPassword(string value)
        {
            if (!string.IsNullOrWhiteSpace(value))
                _unlockPassword = value;
        }
    }
}