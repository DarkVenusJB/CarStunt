using System;
using UnityEngine;

public class FinishLevel : MonoBehaviour
{ 
    [SerializeField] private LayerMask _playerLayer; 
    [SerializeField] private LayerMask _enemyLayer;
    [SerializeField] private GameObject _crashTrigger;
    
    [Space(10f)]
    [Header("X5 - X2 Multiplayer Distance")] 
    [SerializeField] private float[] distance = new float[4];
    
    private Transform centerTarget;
    private bool isFirstTouch;
    private bool isTargetLevel;
    private int[] multiplayers = new[] { 5, 4, 3, 2, 1 };

    private void Start()
    {
        centerTarget = GetComponentInChildren<Transform>();
        isFirstTouch = true;
    }

    private void OnCollisionEnter(Collision other)
    {
        if (isFirstTouch)
        {
            isFirstTouch = false;
            isTargetLevel = true;
            
            if (_playerLayer == (_playerLayer | (1 << other.gameObject.layer)))
                GameManager.Instance.CompleteLevel(isTargetLevel,SelectMultiplayer(other));

            else if (_enemyLayer == (_enemyLayer | (1 << other.gameObject.layer)))
                GameManager.Instance.LevelFailed();
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        isTargetLevel = false;

        if (_playerLayer == (_playerLayer | (1 << other.gameObject.layer)))
            GameManager.Instance.CompleteLevel(isTargetLevel,1);

        else if (_enemyLayer == (_enemyLayer | (1 << other.gameObject.layer)))
            GameManager.Instance.LevelFailed();
    }
    
    private int SelectMultiplayer(Collision other)
    {
        Transform player = other.transform;
        float distanceToTarget = Vector3.Distance(player.position, centerTarget.position);
        Debug.Log(distanceToTarget);
        
        for (int i = 0; i < distance.Length; i++)
        {
            if (distanceToTarget < distance[i])
                return multiplayers[i];
        }

        return multiplayers[4];
    }
}