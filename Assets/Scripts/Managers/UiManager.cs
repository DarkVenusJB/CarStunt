using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Code;
using Database;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using DG.Tweening;
using Views;

public class UiManager : MonoBehaviour
{
    public static UiManager Instance { set; get; }

    public GameObject LevelCompletePanal;
    public GameObject LevelFailedPanal;
    public GameObject LevelPausePanal;
    public GameObject DoubleCoinsPanal;

    [Header("TweenWinBtn")] public RectTransform T_NextBtn;
    public RectTransform T_WinHomeBtn;
    public RectTransform T_RewardBtn;

    [Header("TweenLoseBtn")] public RectTransform T_RestartBtn;
    public RectTransform T_LoseHomeBtn;
    public RectTransform T_LoseRewardBtn;

    public Image FadeImg;
    public float FadeImgDuration;

    [SerializeField] private GameObject _multiplaerText;
    [SerializeField] private Text _dollarsRewardText;
    [SerializeField] private Text _failedText;

    bool isLevelComplete;

    void Start()
    {
        Instance = this;
        Time.timeScale = 1;
        LevelCompletePanal.SetActive(false);
        LevelFailedPanal.SetActive(false);
        LevelPausePanal.SetActive(false);
        DoubleCoinsPanal.SetActive(false);

        FadeImg.gameObject.SetActive(true);
        ImgFadeOut();

        if (Implementation.Instance != null) Implementation.Instance.ShawBanner();
    }

#region Fade

    public void ImgFadeIn()
    {
        FadeImg.DOFade(1, FadeImgDuration);
    }

    public void ImgFadeOut()
    {
        FadeImg.DOFade(0, FadeImgDuration);
    }

#endregion

#region Buttons

    public void Home()
    {
      if(LevelPausePanal.activeSelf)
        LevelPausePanal.SetActive(false);
      else if(LevelCompletePanal.activeSelf)
          LevelCompletePanal.SetActive(false);
      else if (LevelFailedPanal.activeSelf)
          LevelFailedPanal.SetActive(false);

      Time.timeScale = 1;
      
      GameManager.Instance.ClearPreviousLevel();
      GameManager.Instance.OnGameEnded();
      int indexCar = GameDataController.ShopElementsData.ToList().FindIndex(c => c.Id == GameDataController.SelectedCar);
      int indexCharacter = GameDataController.ShopElementsData.ToList().FindIndex(c => c.Id == GameDataController.SelectedCharacter);
        
      Shop.Instance.PreviewShopElement(indexCar, true);
      Shop.Instance.PreviewShopElement(indexCharacter, false);
      
      MainMenu.Instance.SelectMainWindow();

      CameraController.Instance.SetActiveComponents("MenuCamera");
      CanvasController.Instance.SetActiveComponents("MenuCanvas");
    }

    public void Pause()
    {
        Time.timeScale = 0;
        OnLevelPause();
    }

    public void Resume()
    {
        Time.timeScale = 1;
        LevelPausePanal.SetActive(false);
    }

    public void Restart()
    {
        GameManager.Instance.LoadCurrentLevel();

        Time.timeScale = 1;
        
        if (LevelPausePanal.activeSelf)
            LevelPausePanal.SetActive(false);
        
        else if (LevelFailedPanal.activeSelf)
        {
            LevelFailedPanal.SetActive(false);
            SoundsManager.Instance.LevelFail.Stop();
        }
    }

#endregion

    public void RewardOkay()
    {
        DoubleCoinsPanal.SetActive(false);
        
        int activeSceneIndex = SceneManager.GetActiveScene().buildIndex;
    }


    public void OnLevelPause()
    {
        LevelPausePanal.SetActive(true);
    }

    public void ShowDoubleCoinsPanal()
    {
        DoubleCoinsPanal.SetActive(true);
        LevelCompletePanal.SetActive(false);
        LevelFailedPanal.SetActive(false);
    }


#region LevelComplete

    public void Next()
    {
        LevelCompletePanal.SetActive(false);
        GameManager.Instance.LoadNextLevel();
    }


    public void LevelComplete(bool isTargetLevel, int multiplayer)
    {
        StartCoroutine(OnLevelComplete(isTargetLevel, multiplayer));
    }

    private IEnumerator OnLevelComplete(bool isTargetLevel, int multiplayer)
    {
        isLevelComplete = true;
        yield return new WaitForSeconds(1);
        
        if(LevelPausePanal.activeSelf) LevelPausePanal.SetActive(false);
        LevelCompletePanal.SetActive(true);
        
        DoTweening.Instance.XMove(T_WinHomeBtn, 400f, .3f);
        DoTweening.Instance.XMove(T_NextBtn, 0f, .4f);
        DoTweening.Instance.XMove(T_RewardBtn, -400f, .5f);

        if (isTargetLevel)
        {
            _multiplaerText.SetActive(true);
            _multiplaerText.GetComponent<Text>().text = string.Format("X{0}", multiplayer);
            _dollarsRewardText.text = (200*multiplayer).ToString();
            Shop.Instance.ChangeCurrencyValue(200*multiplayer, CurrencyType.Dollars);
        }
        else
        {
            _multiplaerText.SetActive(false);
            Shop.Instance.ChangeCurrencyValue(200, CurrencyType.Dollars);
        }
    }

#endregion

#region LevelFailed

    public void LevelFailed()
    {
        StartCoroutine(OnLevelFailed("The opponent has taken the lead."));
    }

    public void MissTarget()
    {
        StartCoroutine(OnLevelFailed(""));
    }

    IEnumerator OnLevelFailed(string failedText)
    {
        yield return new WaitForSeconds(1);

        if (!LevelFailedPanal.activeSelf && !LevelCompletePanal.activeSelf)
        {
            LevelFailedPanal.SetActive(true);
            _failedText.text = failedText;
            DoTweening.Instance.XMove(T_LoseHomeBtn, 400f, .3f);
            DoTweening.Instance.XMove(T_RestartBtn, 0f, .4f);
            DoTweening.Instance.XMove(T_LoseRewardBtn, -400f, .5f);
        }
    }

#endregion
}