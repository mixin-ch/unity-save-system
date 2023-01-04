using Mixin.Utils;

namespace Mixin.Save.Samples
{
    public class SampleSaveManager : Singleton<SampleSaveManager>
    {
        private const string _salt = "YOUR_SALT";
        private string GameVersion => "0.1.0";

        private DataFileManager<IngameData> _ingameData;
        private DataFileManager<UserSettingsData> _userSettingsData;

        public DataFileManager<IngameData> IngameData { get => _ingameData; set => _ingameData = value; }
        public DataFileManager<UserSettingsData> UserSettingsData { get => _userSettingsData; set => _userSettingsData = value; }

        protected override void Awake()
        {
            _ingameData = new DataFileManager<IngameData>(
                "data",
                FileType.Binary,
                GameVersion,
                _salt);

            _userSettingsData = new DataFileManager<UserSettingsData>(
                "settings",
                FileType.XML,
                GameVersion);

            _ingameData.Data = new IngameData();
            _userSettingsData.Data = new UserSettingsData();

            _ingameData.Save();
            _userSettingsData.Save();

            LoadAllData();
        }

        private void LoadAllData()
        {
            _ingameData.Load();
            _userSettingsData.Load();
        }
    }
}