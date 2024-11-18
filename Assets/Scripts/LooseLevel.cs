using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LooseLevel : MonoBehaviour
{
    [SerializeField] private LayerMask _playerLayer;

    private void OnTriggerEnter(Collider other)
    {
        if (_playerLayer == (_playerLayer | (1 << other.gameObject.layer)))
        {
            GameManager.IsLevelFinished = true;
            UiManager.Instance.MissTarget();
        }
            
    }
}
