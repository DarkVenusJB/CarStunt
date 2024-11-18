using System;
using System.Collections.Generic;
using Database;

namespace Database
{
    [Serializable]
    public class GameData
    {
        public int Dollars;
        
        public List<SkinData> Cars = new();
        public List<SkinData> Characters = new();

        public string SelectedCarId;
        public string SelectedCharacterId;

        public string PreviewCarId;
        public string PreviewCharacterId;
        
        public List<LevelData> Levels = new();
    }
}