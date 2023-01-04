using Mixin.Utils;
using UnityEngine;

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
                "data.json",
                FileType.JSON,
                _salt);

            _userSettingsData = new DataFileManager<UserSettingsData>(
                "settings.json",
                FileType.JSON);

            _ingameData.Data = new IngameData();
            _userSettingsData.Data = new UserSettingsData();

            LoadAllData();
        }

        private void Update()
        {
            // Save the file with key S
            if (Input.GetKeyDown(KeyCode.S))
            {
                _ingameData.Save();
                _userSettingsData.Save();
            }
        }

        private void LoadAllData()
        {
            _ingameData.Load();
            _userSettingsData.Load();
        }
    }
}