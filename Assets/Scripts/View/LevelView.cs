using System;
using System.Collections.Generic;
using KimicuUtility;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Views
{
    public class LevelView : MonoBehaviour
    {
        [SerializeField] private GameObject _openedFooterParent;
        [SerializeField] private GameObject _closedFooterParent;
        [SerializeField] private GameObject _lockParent;

        [SerializeField] private Image _preview;

        [SerializeField] private Button _openForVideoButton;
        [SerializeField] private Button _selectLevelButton;

        [SerializeField] private TMP_Text _trackNumberTMP;
        [SerializeField] private TMP_Text _rewardTMP;

        [SerializeField] private List<RectTransform> _layouts = new();

        private int _reward;
        private int _trackNumber;
        private bool _isOpen;
        private (Sprite preview, Sprite lockPreview) _sprite;

        public void Initialize(string id, Sprite preview, Sprite lockPreview, int trackNumber, int reward, bool isOpen, Action <string> onClick)
        {
            _trackNumber = trackNumber;
            _reward = reward;
            _isOpen = isOpen;
            _sprite = (preview, lockPreview);

            _preview.sprite = isOpen ? preview : lockPreview;
            _rewardTMP.text = reward.ToString();
            _trackNumberTMP.text = $"TRACK {trackNumber}";

            if (isOpen)
            {
                SetOpenState(true);
                _selectLevelButton.AddListener(()=>onClick.Invoke(id));
            }
            else
            {
                SetOpenState(false);
                _openForVideoButton.AddListener(() =>
                {
                    // TODO: Вызываем рекламу

                    SetOpenState(true);
                    _selectLevelButton.AddListener(()=>onClick.Invoke(id));
                });
            }

        }

        private void SetOpenState(bool value)
        {
            _lockParent.SetActive(!value);
            _closedFooterParent.SetActive(!value);
            _openedFooterParent.SetActive(value);
            _preview.sprite = value ? _sprite.preview : _sprite.lockPreview;
            foreach (RectTransform layout in _layouts) LayoutRebuilder.ForceRebuildLayoutImmediate(layout);
        }
    }
}