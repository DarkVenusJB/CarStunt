using System.Collections.Generic;
using System.Linq;
using Database;
using Interfaces;
using KimicuUtility;
using UnityEngine;
using UnityEngine.UI;
using Views;

public class ShopManager : MonoBehaviour, IManager
{
    public static ShopManager Instance { get; private set; }

    [SerializeField] private Button _selectShopElementButton;

    [SerializeField] private Image _selectCarShopImage;
    [SerializeField] private Button _selectCarShopButton;
    [SerializeField] private Sprite[] _selectCarShopStateIcons;

    [SerializeField] private Image _selectCharacterShopImage;
    [SerializeField] private Button _selectCharacterShopButton;
    [SerializeField] private Sprite[] _selectCharacterShopStateIcons;

    [SerializeField] private ShopElementView _elementPrefab;
    [SerializeField] private Transform _contentShop;

    private void Awake() => Instance = this;

    private void Start()
    {
        _selectCarShopButton.AddListener(SelectCarShop);
        _selectCharacterShopButton.AddListener(SelectCharacterShop);
        _selectShopElementButton.AddListener(SelectShopElement);

        Shop.Instance.ShopElementChanged += OnShopElementChanged;
    }

    private void OnDestroy() => Shop.Instance.ShopElementChanged -= OnShopElementChanged;

    private void OnShopElementChanged()
    {
        SkinData preview;
        SkinData selected;
        ShopElementData data;
        if (Shop.Instance.IsCarShop)
        {
            preview = GameDataController.Cars.First(c => c.Id == GameDataController.PreviewCar);
            selected = GameDataController.Cars.First(c => c.Id == GameDataController.SelectedCar);
            data = GameDataController.ShopElementsData.First(c => c.Id == GameDataController.PreviewCar);
        }
        else
        {
            preview = GameDataController.Characters.First(c => c.Id == GameDataController.PreviewCharacter);
            selected = GameDataController.Characters.First(c => c.Id == GameDataController.SelectedCharacter);
            data = GameDataController.ShopElementsData.First(c => c.Id == GameDataController.PreviewCharacter);
        }

        bool previewPurchased = preview.Purchased;
        bool currentSelected = preview.Id == selected.Id;
        _selectShopElementButton.gameObject.SetActive(!currentSelected && (previewPurchased || data.Price == 0));
    }

    private void SelectShopElement()
    {
        int index = Shop.Instance.IsCarShop
            ? GameDataController.ShopElementsData.ToList().FindIndex(c => c.Id == GameDataController.PreviewCar)
            : GameDataController.ShopElementsData.ToList().FindIndex(c => c.Id == GameDataController.PreviewCharacter);
        Shop.Instance.SelectShopElement(index, Shop.Instance.IsCarShop);
    }

    public List<ShopElementView> ShopItemsInstantiate(ShopElementData[] shopElements)
    {
        var sorted = shopElements.Where(e =>
            (e is CarData && Shop.Instance.IsCarShop) || (e is CharacterData && !Shop.Instance.IsCarShop)).ToList();

        var products = GameDataController.GetAllSkins();

        List<ShopElementView> spawnedShopElement = new();
        var shopElementsList = shopElements.ToList();
        for (int i = 0; i < sorted.Count; i++)
        {
            ShopElementData data = sorted[i];
            var product = products.Find(p => p.Id == data.Id);

            ShopElementView element = Instantiate(_elementPrefab, _contentShop);
            int index = shopElementsList.FindIndex(e => e == data);

            element.Initialize(data.Icon, data.CurrencyType, data.Price, product,
                () => { Shop.Instance.PreviewShopElement(index, Shop.Instance.IsCarShop); });
            spawnedShopElement.Add(element);
        }

        return spawnedShopElement;
    }

    public void ShopItemsUpdate(List<ShopElementData> elementData, List<ShopElementView> spawnedElements)
    {
        var products = GameDataController.GetAllSkins();
        for (int i = 0; i < elementData.Count; i++)
        {
            ShopElementData data = elementData[i];
            var product = products.Find(p => p.Id == data.Id);

            int index = elementData.FindIndex(e => e == data);
            spawnedElements[i].Initialize(data.Icon, data.CurrencyType, data.Price, product,
                () => { Shop.Instance.PreviewShopElement(index, Shop.Instance.IsCarShop); });
        }
    }

    public void ShopItemsRemove(List<ShopElementView> spawnedShopElement)
    {
        foreach (ShopElementView view in spawnedShopElement) view.gameObject.Destroy();
    }

    public void SelectCarShop()
    {
        Shop.Instance.IsCarShop = true;
        MainMenu.Instance.SelectShopWindow();
        _selectCarShopImage.sprite = _selectCarShopStateIcons[1];
        _selectCharacterShopImage.sprite = _selectCharacterShopStateIcons[0];
        Shop.Instance.InstantiateElements();
        _selectShopElementButton.gameObject.SetActive(false);
    }

    public void SelectCharacterShop()
    {
        Shop.Instance.IsCarShop = false;
        MainMenu.Instance.SelectShopWindow();
        _selectCarShopImage.sprite = _selectCarShopStateIcons[0];
        _selectCharacterShopImage.sprite = _selectCharacterShopStateIcons[1];
        Shop.Instance.InstantiateElements();
        _selectShopElementButton.gameObject.SetActive(false);
    }

    public void Load()
    {
    }
}