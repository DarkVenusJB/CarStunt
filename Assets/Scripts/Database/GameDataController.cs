using System;
using System.Collections.Generic;
using System.Linq;
using Levels.Data;
using Newtonsoft.Json;
using UnityEngine;
using Views;

namespace Database
{
    public static class GameDataController
    {
        private static GameData _gameData;
        public static ShopElementData[] ShopElementsData;
        public static LevelConfig[] LevelDataConfig;

        public static bool Initialized;

        public static int Dollars
        {
            get => _gameData.Dollars;
            set
            {
                _gameData.Dollars = value;
                DollarsUpdated?.Invoke(_gameData.Dollars);
            }
        }

        public static List<SkinData> Cars => _gameData.Cars;
        public static List<SkinData> Characters => _gameData.Characters;
        public static List<LevelData> LevelData => _gameData.Levels;
        public static string LastLevelPassedId => LevelData.Last(d => d.Passed).Id;
        public static string FirstNotPassedId => LevelData.First(d => d.Passed == false).Id;

        public static string SelectedCar
        {
            get => _gameData.SelectedCarId;
            set
            {
                _gameData.SelectedCarId = value;
                SelectedCarUpdated?.Invoke(value);
            }
        }

        public static string SelectedCharacter
        {
            get => _gameData.SelectedCharacterId;
            set
            {
                _gameData.SelectedCharacterId = value;
                SelectedCharacterUpdated?.Invoke(value);
            }
        }

        public static string PreviewCar
        {
            get => _gameData.PreviewCarId;
            set
            {
                _gameData.PreviewCarId = value;
                SelectedCarUpdated?.Invoke(value);
            }
        }

        public static string PreviewCharacter
        {
            get => _gameData.PreviewCharacterId;
            set
            {
                _gameData.PreviewCharacterId = value;
                SelectedCharacterUpdated?.Invoke(value);
            }
        }

        #region Actions

        public static event Action<int> DollarsUpdated;
        public static event Action<string> SelectedCarUpdated;
        public static event Action<string> SelectedCharacterUpdated;

        #endregion

        public static List<SkinData> GetAllSkins() => _gameData.Cars.Concat(_gameData.Characters).ToList();

        public static void Initialize()
        {
            ShopElementsData = Resources.LoadAll<ShopElementData>("Configurations/Shop");
            LevelDataConfig = Resources.LoadAll<LevelConfig>("Configurations/Levels");
            
            LoadData();
            Initialized = true;
        }
        
        public static void LoadData()
        {
            var defaultData = JsonConvert.SerializeObject(GetDefaultData());
            _gameData = JsonConvert.DeserializeObject<GameData>(PlayerPrefs.GetString("game_data", defaultData));
        }

        public static void SaveData() => PlayerPrefs.SetString("game_data", JsonConvert.SerializeObject(_gameData));

        private static GameData GetDefaultData()
        {
            var gameData = new GameData();
            
            //Ппродукты в магазине
            var shopElementsSorted = ShopElementsData.Where(e => !e.IsIgnoredInGame).ToArray();
            foreach (ShopElementData elementData in shopElementsSorted)
            {
                SkinData skinData = new()
                {
                    Id = elementData.Id,
                    Purchased =  elementData.Price == 0,
                    SolvedVideo = 0
                };
                
                if (elementData as CarData) gameData.Cars.Add(skinData);
                else gameData.Characters.Add(skinData);
            }

            // Загрузка выбранных продуктов
            gameData.SelectedCarId = gameData.Cars.First(c => c.Purchased = true).Id;
            gameData.SelectedCharacterId = gameData.Characters.First(c => c.Purchased = true).Id;

            // Загрузка preview продуктов (когда игрок выбрал, но не купил еще)
            gameData.PreviewCarId = gameData.Cars.First().Id;
            gameData.PreviewCharacterId = gameData.Characters.First().Id;
            
            // Загрузка уравней
            foreach (LevelConfig config in LevelDataConfig)
            {
                gameData.Levels.Add(new LevelData()
                {
                    Id = config.Id,
                    Passed = false,
                    Reward = config.Reward
                });
            }

            gameData.Levels = gameData.Levels.OrderBy(l => int.Parse(l.Id.Replace("level_", ""))).ToList();

            // Загрузка валюты
            gameData.Dollars = 0;

            return gameData;
        }
    }
}