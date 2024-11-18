using System.Collections;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] private float _startDelay = 5f;
    [SerializeField] private float _lowSpeed = 20; 
    [SerializeField] private float _distanceFromMaxSpeed = 50;

    private float _maxSpeed;
    private RCC_AICarController _enemyCarController;
    private GameObject _player;
    private void Start()
    {
        _enemyCarController = GetComponent<RCC_AICarController>();
        _player = CarController.Instance.gameObject;
        transform.GetComponent<RCC_CarControllerV3>().canControl = false;

        _maxSpeed = _enemyCarController.maximumSpeed;

        StartCoroutine(EnemyStartDelay());
    }

    private void Update()
    {
        float distance = 0;
        if (_player != null)
        {
            distance = Vector3.Distance(transform.position,CarController.Instance.GetPlayerPosition());
        }
        
        if (distance > _distanceFromMaxSpeed && IsEnemyFirst())
            _enemyCarController.maximumSpeed = _lowSpeed;
       
        else
            _enemyCarController.maximumSpeed = _maxSpeed;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Crash"))
        {
            GameManager.OnCrashEnemy = true;
            Destroy(gameObject,.1f);
        }
    }
    
    private bool IsEnemyFirst()
    {
        if (_enemyCarController.currentWaypointIndex - PlayerAIAssistance.currentWaypointIndex > 0)
            return true;
        
        else
            return false;
    }

    private IEnumerator EnemyStartDelay()
    {
        yield return new WaitForSeconds(_startDelay);
        transform.GetComponent<RCC_CarControllerV3>().canControl = true;
    }
}
