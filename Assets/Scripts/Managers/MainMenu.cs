using System;
using System.Linq;
using Code;
using Database;
using DG.Tweening;
using Interfaces;
using KimicuUtility;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class MainMenu : MonoBehaviour, IManager
{
    public static MainMenu Instance { get; private set; }

    [SerializeField] private Windows _windows;

    [Header("Main Window Rects")] [SerializeField]
    private RectTransform _walletHolderInMenu;

    [SerializeField] private RectTransform _playButton;
    [SerializeField] private RectTransform _buttonGroup;
    [SerializeField] private RectTransform _settingsButton;

    [Header("Shop Window Rects")] [SerializeField]
    private RectTransform _walletHolderInShop;

    [SerializeField] private RectTransform _readyButton;
    [SerializeField] private RectTransform _shopTitle;

    [Header("Levels Window Rects")] [SerializeField]
    private RectTransform _walletHolderInLevels;

    [SerializeField] private RectTransform _levelTitle;

    [Header("Buttons")] [SerializeField] private Button _openLevelsButton;
    [SerializeField] private Button _openCarShopButton;
    [SerializeField] private Button _openCharacterShopButton;
    [SerializeField] private Button _backButtonInLevel;
    [SerializeField] private Button _backButtonInShop;
    [SerializeField] private Button _readyButtonInShop;
    [SerializeField] private Button _playButtonInMenu;

    private void Awake() => Instance = this;

    private void Start()
    {
        SelectWindow(WindowType.MainWindow);

        Debug.Log($"{GameDataController.SelectedCar}");
        Debug.Log($"{GameDataController.SelectedCharacter}");

        int indexCar = GameDataController.ShopElementsData.ToList()
            .FindIndex(c => c.Id == GameDataController.SelectedCar);
        int indexCharacter = GameDataController.ShopElementsData.ToList()
            .FindIndex(c => c.Id == GameDataController.SelectedCharacter);

        Shop.Instance.PreviewShopElement(indexCar, true);
        Shop.Instance.PreviewShopElement(indexCharacter, false);

        _openCarShopButton.AddListener(() => { ShopManager.Instance.SelectCarShop(); });
        _openCharacterShopButton.AddListener(() => { ShopManager.Instance.SelectCharacterShop(); });
        _openLevelsButton.AddListener(SelectLevelsWindow);
        _readyButtonInShop.AddListener(SelectLevelsWindow);
        _playButtonInMenu.AddListener(SelectLevelsWindow);

        _backButtonInShop.AddListener(() =>
        {
            int indexCar = GameDataController.ShopElementsData.ToList()
                .FindIndex(c => c.Id == GameDataController.SelectedCar);
            int indexCharacter = GameDataController.ShopElementsData.ToList()
                .FindIndex(c => c.Id == GameDataController.SelectedCharacter);

            Shop.Instance.PreviewShopElement(indexCar, true);
            Shop.Instance.PreviewShopElement(indexCharacter, false);
            SelectMainWindow();
        });
        _backButtonInLevel.AddListener(SelectMainWindow);
        SoundsManager.Instance.PlayGrageSound();
        CameraController.Instance.SetActiveComponents("MenuCamera");
        CanvasController.Instance.SetActiveComponents("MenuCanvas");
    }

    #region SelectWindow

    public void SelectMainWindow() => SelectWindow(WindowType.MainWindow);
    public void SelectShopWindow() => SelectWindow(WindowType.ShopWindow);
    public void SelectLevelsWindow() => SelectWindow(WindowType.LevelsWindow);
    public void HideAllWindows() => SelectWindow(WindowType.None);

    private void SelectWindow(WindowType windowType)
    {
        foreach (var windowPair in _windows)
        {
            windowPair.Value.gameObject.SetActive(windowPair.Key == windowType);
        }
    }

    #endregion

    public void Load()
    {
    }
}

[Serializable]
public class Windows : SerializableDictionary<WindowType, RectTransform>
{
}
#if UNITY_EDITOR
[CustomPropertyDrawer(typeof(Windows))]
public class WindowsDrawer : DictionaryDrawer<WindowType, RectTransform>
{
}
#endif

public enum WindowType
{
    None,
    MainWindow,
    ShopWindow,
    LevelsWindow,
}