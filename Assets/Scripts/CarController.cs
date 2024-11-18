using System.Collections;
using System.Linq;
using Components;
using Database;
using DG.Tweening;
using StarterAssets;
using UnityEngine;
using UnityEngine.Serialization;
using CharacterInfo = Components.CharacterInfo;

public class CarController : MonoBehaviour
{
    [SerializeField] private PlayerSkinController _skinController;
    [SerializeField] private Transform _pointOutCar;
    [SerializeField] private Transform _jumpPoint;
    [SerializeField] private GameObject _playerObject;
    [SerializeField] private ThirdPersonController _playerController;
    [SerializeField] private Collider _carCollider;

    [FormerlySerializedAs("_ragdollCameraTarget")]
    [Header("Jump Camera")]
    [SerializeField] private PlayerSkinController _character;
    [SerializeField] private float _maxCameraSpeed= 15;
    [SerializeField] private float _cameraSmooth = 0.7f;
    [SerializeField] private float _cameraMoveTime = 8f;

    [Header("Walking UI")] 
    [SerializeField] private GameObject _thirdPersonUI;
    

    private int AdsCount;
    private Vector3 velocity = Vector3.zero;
    private bool canFollow = false;

    private static readonly int EnterCar = Animator.StringToHash("EnterCar");
    private static readonly int JumpingOutCar = Animator.StringToHash("JumpingOutCar");
    private static readonly int Landing = Animator.StringToHash("Landing");

    private RCC_CarControllerV3 _carController;
    private Rigidbody _carRigidbody;
    private Transform _fixedCamera;
    private Transform _cameraTarget;

    public ThirdPersonController Player => _playerController;

    private CharacterInfo SelectedCharacter =>
        _skinController.Characters.First(c => c.CharacterId == GameDataController.SelectedCharacter);

    public static CarController Instance { get; private set; }

    private void Awake() => Instance = this;

    private void Start()
    {
        _carController = transform.GetComponent<RCC_CarControllerV3>();
        _carRigidbody = GetComponent<Rigidbody>();
        _carRigidbody.isKinematic = true;
        Player.PhysicsActive(false);
        
        RCC_SceneManager.Instance.activePlayerCamera.SetTarget(Player.transform, _carController, true);
        RCC_SceneManager.Instance.activePlayerCamera.cameraMode = RCC_Camera.CameraMode.TPS;
    }

    public void GetInCar()
    {
        RCC_SceneManager.Instance.activePlayerCamera.SetTarget(Player.transform, _carController);
        Player.MoveAvailable = false;

        _playerObject.transform.DOLocalRotate(new Vector3(0, 90, 0), 0.3f);
        _playerObject.transform.DOMove(_pointOutCar.position, 0.5f)
            .OnComplete(() =>
            {
                SelectedCharacter.Animator.SetTrigger(EnterCar);
                StartCoroutine(DeactivatePlayerObjectDelay());
            });
    }
    
    private void LateUpdate()
    {
        if (canFollow)
        {
            _fixedCamera.position = Vector3.SmoothDamp(_fixedCamera.position, _cameraTarget.position,
                ref velocity, _cameraSmooth,_maxCameraSpeed);
            _fixedCamera.LookAt(_cameraTarget.position);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Jump")) StartCoroutine(JumpRoutine());

        if (other.gameObject.CompareTag("Crash") && !GameManager.IsLevelFinished)
        {
            if (Implementation.Instance != null) Implementation.Instance.ShowInterstitial();

            GameManager.OnCrash = true;

            Destroy(gameObject);
        }
    }

    public Vector3 GetPlayerPosition() => transform.position;
    

    private IEnumerator DeactivatePlayerObjectDelay()
    {
        yield return new WaitForSecondsRealtime(2.5f);
        _thirdPersonUI.SetActive(false);
        _carController.canControl = true;
        _carRigidbody.isKinematic = false;
    }
    
    private IEnumerator JumpRoutine()
    {
        _playerObject.transform.position = _pointOutCar.position;
        SelectedCharacter.Animator.SetTrigger(JumpingOutCar);
        RCC_SceneManager.Instance.activePlayerCamera.SetTarget(Player.transform, _carController, true);
        Vector3 newCameraPosition = transform.position + new Vector3(0, 45, 0);
        _playerObject.transform.DORotate(new Vector3(0, 0, 0), 1f);
        
        yield return new WaitForSeconds(1f);
        
        StartCoroutine(SetCameraPosition(newCameraPosition));

        yield return new WaitForSeconds(0.5f);
        
        Player.PhysicsActive(true);
        Player.MoveAvailable = true;
        RCC_SceneManager.Instance.activePlayerCamera.cameraTarget.Player = SelectedCharacter.CameraTarget;
        Player.transform.parent = null;
        SelectedCharacter.Animator.SetTrigger(Landing);
        yield return new WaitUntil(() => _playerController.Grounded);
    }

    private IEnumerator SetCameraPosition(Vector3 cameraPosition)
    {
        _cameraTarget = SelectedCharacter.CameraTarget;
        RCC_SceneManager.Instance.activePlayerCamera.cameraMode = RCC_Camera.CameraMode.FIXED;
        RCC_FixedCamera.Instance.canTrackNow = false;
        RCC_SceneManager.Instance.activePlayerCamera.cameraTarget.Player = _cameraTarget;
        _fixedCamera = RCC_FixedCamera.Instance.transform;
        _fixedCamera.position= cameraPosition;

        canFollow = true;
        yield return new WaitForSeconds(_cameraMoveTime);
        canFollow = false;
        
        SelectedCharacter.Animator.SetTrigger(Landing);
        yield return new WaitUntil(() => Player.Grounded);
    }
}