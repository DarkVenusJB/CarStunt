using System;
using System.Collections.Generic;
using System.Linq;
using Code;
using Database;
using Interfaces;
using Levels.Data;
using UnityEngine;

namespace Views.Managers
{
    public class LevelManager : MonoBehaviour, IManager
    {
        [SerializeField] private LevelView _viewTemplate;
        [SerializeField] private Transform _content;

        private readonly List<GameObject> _spawned = new();

        private void Start() => LoadLevelViews();

        public void LoadLevelViews()
        {
            foreach (var view in _spawned) Destroy(view);
            _spawned.Clear();
            
            Debug.Log("Load Level Views");

            var levels = GameDataController.LevelData;

            for (int i = 0; i < levels.Count; i++)
            {
                LevelData data = levels[i];
                LevelConfig config = GameDataController.LevelDataConfig.First(l => l.Id == data.Id);

                bool opened = GameDataController.LevelData.Find(l => l.Id == data.Id).Passed;
                if (i != 0 && !opened) opened = GameDataController.LevelData.Find(l => l.Id == levels[i - 1].Id).Passed;
                else if (i == 0 && !opened) opened = true;
                LevelView view = Instantiate(_viewTemplate, _content);
                view.Initialize(data.Id, config.Preview, config.LockPreview, i + 1, data.Reward, opened, LoadLevel);
                _spawned.Add(view.gameObject);
            }
        }

        private void LoadLevel(string id)
        {
            GameManager.Instance.StartGame(id);

            CameraController.Instance.SetActiveComponents("GameCamera");
            CanvasController.Instance.SetActiveComponents("GameCanvas");

            MainMenu.Instance.HideAllWindows();
        }

        public void Load()
        {
            LoadLevelViews();
        }
    }
}