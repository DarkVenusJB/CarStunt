using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FrameRate : MonoBehaviour
{
    public static FrameRate Instance { set; get; }


    void Start()
    {
        DontDestroyOnLoad(this.gameObject);
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

    }
    void Update()
    {
        if(Application.targetFrameRate > 60)
        {
            Application.targetFrameRate = 60;
        }
    }
}
