using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundsManager : MonoBehaviour
{
  public static SoundsManager Instance { get; set; }


  public AudioSource PressBtnSounds,SelectSounds,InGameSound,GrageSound,LevelFail, LevelWin;



        void Awake()
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

    public void BtnSounds()
    {
        PressBtnSounds.Play();

    }

    public void CarSelecSounds()
    {
       SelectSounds.Play();

    }

    public void PlayGameSound()
    {
        InGameSound.Play();

    }

    public void PlayGrageSound()
    {
        GrageSound.Play();

    }

    public void PlayLevelFail()
    {
        LevelFail.Play();
    }

    public void PlayLevelWin()
    {
        LevelWin.Play();
    }

}
