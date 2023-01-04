using System;
using UnityEngine;

namespace Mixin.Save.Samples
{
    [Serializable]
    public class UserSettingsData
    {
        public int MusicVolume = 90;
        public int SoundVolume = 100;
        public SystemLanguage systemLanguage = SystemLanguage.English;
    }
}