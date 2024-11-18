using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Obstacle_Tweening : MonoBehaviour
{
    public bool IsRotatinRamp;
    public bool IsHammerRamp;

    public Vector3 RampValue;
    public float Duration;


    public Transform _Ramphammer;
    
    void Start()
    {
        
    }

   
    void Update()
    {
       

        if (IsRotatinRamp)
        RotatingRamps();
        if (IsHammerRamp)
            RampHammer();
        
    }


    public void RotatingRamps( )
    {

        transform.DORotate(RampValue, Duration, RotateMode.WorldAxisAdd)
            .SetLoops(-1, LoopType.Yoyo) // -1 means loop indefinitely
            .SetEase(Ease.Linear);
    }


    public void RampHammer()
    {
        _Ramphammer.DORotate(new Vector3(0, -90, 0), .7f).SetEase(Ease.Linear).SetLoops(1, LoopType.Yoyo);




    }
}
