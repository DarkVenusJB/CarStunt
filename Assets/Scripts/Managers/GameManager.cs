using System.Collections;
using System.Linq;
using Database;
using StarterAssets;
using Unity.AI.Navigation;
using UnityEngine;
using Views;

public class GameManager : MonoBehaviour
{
    [SerializeField] private GameObject _userInterfacePreviewSpawnPoint;
    [SerializeField] private ThirdPersonController _playerBody;

    public static bool OnCrash;
    public static bool OnCrashEnemy;
    public static bool IsLevelFinished = false; 

    public NavMeshSurface LevelsSurface;
    public Transform PlayeResPos;
    public Transform EnemyResPos;
    public Transform LevelsSpawnPos;
    public RCC_CarControllerV3 EnemyPrefab;
    public RCC_CarControllerV3 PlayerPrefab;

    private int _adsCount;
    private GameObject _level;
    private RCC_CarControllerV3 _player;
    private RCC_CarControllerV3 _enemy;
    private string _currentLevelId;
    private bool canSpawn = true;
    

    private CarData[] _carData;
    
    public static GameManager Instance { get; private set; }

    private void Awake() => Instance = this;

    private void Start() => _carData = Resources.LoadAll<CarData>("Configurations/Shop/Cars");

    public void StartGame(string id)
    {
        IsLevelFinished = false;
        _userInterfacePreviewSpawnPoint.SetActive(false);
        var car = _carData.First(c => c.Id == GameDataController.SelectedCar);
        PlayerPrefab = car.RccCarController;
        LoadLevel(id);
         
        OnCrash = false;
        OnCrashEnemy = false;

        if (SoundsManager.Instance != null)
        {
            SoundsManager.Instance.GrageSound.Stop();
            SoundsManager.Instance.PlayGameSound();
        }
    }

    private void Update()
    {
        if (OnCrashEnemy)
        {
            EnemyRespawn();
            OnCrashEnemy = false;
        }

        if (OnCrash && !IsLevelFinished)
        {
            PlayerRespawn();
            OnCrash = false;
        }
    }

    #region LevelSpawn

    public void LoadCurrentLevel()
    {
        ClearPreviousLevel();
        LoadLevel(_currentLevelId);
    }

    private void LoadLevel(string id)
    {
        IsLevelFinished = false;
        var config = GameDataController.LevelDataConfig.First(l => l.Id == id);
        _level = Instantiate(config.LevelPrefab, LevelsSpawnPos.position, Quaternion.identity);

        _currentLevelId = config.Id;

        LevelsSurface.BuildNavMesh();
        _level.transform.parent = LevelsSpawnPos;

        EnemyRespawn();
        PlayerRespawn();
    }

    public void LoadNextLevel()
    {
        ClearPreviousLevel();
        LoadLevel(GameDataController.FirstNotPassedId);
    }

    public void ClearPreviousLevel()
    {
        if (_player != null)
        {
            Destroy(_player.gameObject);
        }
        _player = null;
        Destroy(_playerBody.gameObject);
        Destroy(_level);
        if (_enemy!= null)
        {
            Destroy(_enemy.gameObject);
        }

        _enemy = null;
    }

    public void OnGameEnded() => _userInterfacePreviewSpawnPoint.SetActive(true);

    #endregion

    #region ReSpawn

    public void PlayerRespawn()
    {
        if (canSpawn)
        {
            if (_player != null)
            {
                Destroy(_player.gameObject);
                _player = null;
            }
            
            _player = RCC.SpawnRCC(PlayerPrefab, PlayeResPos.position, PlayeResPos.rotation, true, true, true);
            _playerBody = _player.GetComponentInChildren<ThirdPersonController>();
            StartCoroutine(RespawnDelay());
        }
    }

    private void EnemyRespawn()
    {
        _enemy = RCC.SpawnRCC(EnemyPrefab, EnemyResPos.position, EnemyResPos.rotation, false, true, true);
    }

    private IEnumerator RespawnDelay()
    {
        canSpawn = false;
        yield return new WaitForSeconds(1f);
        canSpawn = true;
    }

#endregion

    #region Finish

    public void CompleteLevel(bool isTargetLevel, int multiplayer)
    {
        IsLevelFinished = true;
        GameDataController.LevelData.First(l => l.Id == _currentLevelId).Passed = true;
        GameDataController.SaveData();
        Debug.Log($"Win level: [{_currentLevelId}]");

        UiManager.Instance.LevelComplete(isTargetLevel, multiplayer);

        CarsStop();
        SoundsManager.Instance.PlayLevelWin();
    }

    public void LevelFailed()
    {
        IsLevelFinished = true;
        UiManager.Instance.LevelFailed();

        if (_playerBody.MoveAvailable) _playerBody.MoveAvailable = false;

        CarsStop();
        
        if (SoundsManager.Instance != null) SoundsManager.Instance.PlayLevelFail();
    }

    private void CarsStop()
    {
        _player.GetComponent<RCC_CarControllerV3>().canControl = false;
        _enemy.GetComponent<RCC_CarControllerV3>().canControl = false;
        _enemy.GetComponent<RCC_CarControllerV3>().speed = 0;
    }

    #endregion
}