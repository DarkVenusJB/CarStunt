using System;
using System.Collections.Generic;
using KimicuUtility;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;using Views;
using Database;

namespace Views
{
    public class ShopElementView : MonoBehaviour
    {
        [SerializeField] private Button _button;
        [SerializeField] private Image _elementPreview;
        [SerializeField] private TMP_Text _price;
        [SerializeField] private Image _currencyIcon;

        [SerializeField] private List<RectTransform> _layouts;
        [SerializeField] private CurrencyDictionary _currencies;

        private int _priceValue;
        private int _solvedVideoValue;
        private CurrencyType _currencyType;

        public void Initialize(Sprite sprite, CurrencyType currencyType, int price, SkinData data, Action onClick)
        {
            _priceValue = price;
            _solvedVideoValue = data.SolvedVideo;
            _currencyType = currencyType;
            
            _currencies.TryGetValue(currencyType, out Sprite currencyIcon);
            _currencyIcon.sprite = currencyIcon;
            _elementPreview.sprite = sprite;
            
            SetOpenState(data.Purchased);
            _button.AddListener(onClick.Invoke);
            foreach (RectTransform layout in _layouts) LayoutRebuilder.ForceRebuildLayoutImmediate(layout);
        }

        public void SetOpenState(bool opened)
        {
            if (opened)
            {
                _currencyIcon.gameObject.SetActive(false);
                _price.text = "OPENED";
            }
            else
            {
                _currencyIcon.gameObject.SetActive(true);
                _price.text = _currencyType == CurrencyType.Dollars
                    ? $"{_priceValue}"
                    : $"{_solvedVideoValue}/{_priceValue}";
            }
        }
    }
}

[Serializable] public class CurrencyDictionary : SerializableDictionary<CurrencyType, Sprite> { } 
#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(CurrencyDictionary))]
public class CurrencyDictionaryDrawer : DictionaryDrawer<CurrencyType, Sprite> { }
#endif