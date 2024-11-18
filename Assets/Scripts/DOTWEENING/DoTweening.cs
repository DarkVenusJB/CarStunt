using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class DoTweening : MonoBehaviour
{
    public static DoTweening Instance { get; set; }


    void Awake()
    {
        Instance = this;
    }


    // Loop XMove // 

    public void _XMove(RectTransform _Object, float EndPoint)
    {
        _Object.DOLocalMoveX(EndPoint, 1).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo);
    }

    void _YMove(RectTransform _Object, float EndPoint)
    {
        _Object.DOLocalMoveY(EndPoint, 1).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo);
    }


    public void DoRotate(RectTransform _object, float SetDelay)
    {
        _object.DOLocalRotate(new Vector3(0, 360f + 360, 0), 0.6f).SetRelative(true).SetEase(Ease.Linear)
            .SetDelay(SetDelay);
    }


    // Squeeze Animation // 

    public void FrameAnimation(Image _Object, float DurationTime)
    {
        _Object.DOFade(0.7f, DurationTime).SetEase(Ease.Linear).OnComplete(() =>
        {
            _Object.DOFade(1, DurationTime).SetEase(Ease.Linear);
        });
    }

    // One Tiem XMove
    public void XMove(RectTransform _Object, float EndValue, float SetDelay)
    {
        _Object.DOAnchorPosX(EndValue, .2f).SetEase(Ease.Linear).SetDelay(SetDelay);
        SoundsManager.Instance.BtnSounds();
    }
}