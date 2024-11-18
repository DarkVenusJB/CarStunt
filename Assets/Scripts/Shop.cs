using System;
using System.Collections.Generic;
using System.Linq;
using KimicuUtility;
using UnityEngine;
using Views;
using Database;
using NaughtyAttributes;

public class Shop : MonoBehaviour
{
    public static Shop Instance { get; private set; }
    
    [SerializeField] private Transform _pointCar;
    [SerializeField] private Transform _pointCharacter;

    private List<ShopElementView> _spawnedShopElement = new();

    private ShopElementObject _spawnedCar;
    private ShopElementObject _spawnedCharacter;
    private int _currentIndex;

    public bool IsCarShop;

    public event Action ShopElementChanged;

    private void Awake()
    {
        Instance = this;
    }

    public void InstantiateElements()
    {
        ShopManager.Instance.ShopItemsRemove(_spawnedShopElement);
        _spawnedShopElement.Clear();
        _spawnedShopElement = ShopManager.Instance.ShopItemsInstantiate(GameDataController.ShopElementsData);
    }

    public void UpdateElements()
    {
        ShopManager.Instance.ShopItemsUpdate(GameDataController.ShopElementsData.ToList(), _spawnedShopElement);
    }

    public void SelectShopElement(int index, bool isCarShop)
    {
        SoundsManager.Instance.CarSelecSounds();
        _currentIndex = index;
        
        if (_spawnedCar != null && isCarShop) _spawnedCar.gameObject.Destroy();
        if (_spawnedCharacter != null && !isCarShop) _spawnedCharacter.gameObject.Destroy();

        Transform point = isCarShop ? _pointCar : _pointCharacter;
        var shopElements = GameDataController.ShopElementsData;
        
        if (isCarShop)
        {
            _spawnedCar = Instantiate(shopElements[index].PrefabPreview, point);
            SkinData car = GameDataController.Cars.Find(p => p.Id == shopElements[index].Id);
            _spawnedCar.LockActive(!(car.Purchased || shopElements[index].Price == 0));

            //GameDataController.PreviewCar = _shopElements[index].Id; 
            GameDataController.SelectedCar = shopElements[index].Id;
        }
        else
        {
            _spawnedCharacter = Instantiate(shopElements[index].PrefabPreview, point);
            SkinData character = GameDataController.Characters.Find(p => p.Id == shopElements[index].Id);
            _spawnedCharacter.LockActive(!(character.Purchased || shopElements[index].Price == 0));
            //GameDataController.PreviewCharacter = _shopElements[index].Id;
            GameDataController.SelectedCharacter = shopElements[index].Id;
        }
        ShopElementChanged?.Invoke();
    }

    public void PreviewShopElement(int index, bool isCarShop)
    {
        SoundsManager.Instance.CarSelecSounds();
        _currentIndex = index;
        
        if (_spawnedCar != null && isCarShop) _spawnedCar.gameObject.Destroy();
        if (_spawnedCharacter != null && !isCarShop) _spawnedCharacter.gameObject.Destroy();

        Transform point = isCarShop ? _pointCar : _pointCharacter;
        var shopElements = GameDataController.ShopElementsData;

        if (isCarShop)
        {
            _spawnedCar = Instantiate(shopElements[index].PrefabPreview, point);
            SkinData car = GameDataController.Cars.Find(p => p.Id == shopElements[index].Id);
            _spawnedCar.LockActive(!(car.Purchased || shopElements[index].Price == 0));

            GameDataController.PreviewCar = shopElements[index].Id; 
            //GameDataController.SelectedCar = _shopElements[index].Id;
        }
        else
        {
            _spawnedCharacter = Instantiate(shopElements[index].PrefabPreview, point);
            SkinData character = GameDataController.Characters.Find(p => p.Id == shopElements[index].Id);
            _spawnedCharacter.LockActive(!(character.Purchased || shopElements[index].Price == 0));
            GameDataController.PreviewCharacter = shopElements[index].Id;
            //GameDataController.SelectedCharacter = _shopElements[index].Id;
        }
        ShopElementChanged?.Invoke();
    }

    public bool TryBuyShopElement()
    {
        var elementData = GameDataController.ShopElementsData[_currentIndex];
        var elementView = _spawnedShopElement[_currentIndex];

        if (elementData is CarData
                ? GameDataController.Cars.Find(p => p.Id == elementData.Id).Purchased
                : GameDataController.Characters.Find(p => p.Id == elementData.Id).Purchased) return false;
        
        if ((elementData.CurrencyType == CurrencyType.Dollars && elementData.Price <= GameDataController.Dollars) ||
            (elementData.CurrencyType == CurrencyType.Video && elementData.Price <= (elementData is CarData 
                ? GameDataController.Cars.Find(p=>p.Id == elementData.Id).SolvedVideo 
                : GameDataController.Characters.Find(p=>p.Id == elementData.Id).SolvedVideo)))
        {
            ChangeCurrencyValue(elementData.Price, elementData.CurrencyType);
            elementView.SetOpenState(true);
            elementData.PrefabPreview.LockActive(false);

            if (elementData is CarData) GameDataController.SelectedCar = elementData.Id;
            else GameDataController.SelectedCharacter = elementData.Id;

            if (elementData is CarData) GameDataController.Cars.Find(p => p.Id == elementData.Id).Purchased = true;
            else GameDataController.Characters.Find(p => p.Id == elementData.Id).Purchased = true;
            PlayerPrefs.SetInt(elementData.Id, 1);
            UpdateElements();
            return true;
        }
        
        return false;
    }

    public void ChangeCurrencyValue(int value, CurrencyType currencyType)
    {
        switch (currencyType)
        {
            case CurrencyType.Dollars:
                GameDataController.Dollars += value;
                break;
            case CurrencyType.Video:
                if (IsCarShop) GameDataController.Cars.Find(p => p.Id == GameDataController.PreviewCar).SolvedVideo += value;
                else GameDataController.Characters.Find(p => p.Id == GameDataController.PreviewCharacter).SolvedVideo += value;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(currencyType), currencyType, null);
        }
    }
}