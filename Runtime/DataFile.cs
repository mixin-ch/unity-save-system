using System;

namespace Mixin.Save
{
    [Serializable]
    public abstract class DataFile
    {
        /// <summary>
        /// Important information for debugging and support
        /// </summary>
        public string GameVersion = "undefined";
        public bool TestBuild = false;
        public int SaveCounter = 0;
        public DateTime LastSave = new DateTime();

        DateTime _lastBackup;
        const string _BACKUP_PREFIX = "_backup";

        public void SetFileInformation(string gameVersion, bool inTestingEnvironment)
        {
            GameVersion = gameVersion;
            TestBuild = inTestingEnvironment;
            SaveCounter++;
            LastSave = DateTime.Now;

            //TryGenerateBackup();
        }

        public void TryGenerateBackup()
        {

        }

        private void GetLastBackup()
        {

        }
    }
}