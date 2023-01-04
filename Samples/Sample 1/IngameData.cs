using System;

namespace Mixin.Save.Samples
{
    [Serializable]
    public class IngameData
    {
        public int Highscore = 100;
        public int LastScore = 20;
        public FileType FileType = FileType.JSON;
    }
}