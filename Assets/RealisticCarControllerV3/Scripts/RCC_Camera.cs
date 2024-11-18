//----------------------------------------------
//            Realistic Car Controller
//
// Copyright © 2014 - 2023 BoneCracker Games
// https://www.bonecrackergames.com
// Buğra Özdoğanlar
//
//----------------------------------------------

using System;
using System.Collections;
using Components;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// Main RCC Camera controller. Includes 6 different camera modes with many customizable settings. It doesn't use different cameras on your scene like *other* assets. Simply it parents the camera to their positions that's all. No need to be Einstein.
/// Also supports collision detection.
/// </summary>
[AddComponentMenu("BoneCracker Games/Realistic Car Controller/Camera/RCC Camera")]
public class RCC_Camera : MonoBehaviour
{
    //  Main camera target, which is our player vehicle.
    [Serializable]
    public class CameraTarget
    {
        public bool IsPlayer = false; //  Player.
        public Transform Player; //  Player.
        public RCC_CarControllerV3 playerVehicle; //  Player vehicle.

        public override string ToString() =>
            $"IsPlayer - {IsPlayer} | Player - {Player} | playerVehicle - {playerVehicle}";

        private CharacterController _characterController;

        public CharacterController CharacterController
        {
            get
            {
                if (_characterController == null ||
                    (_characterController != null && Player != _characterController.transform))
                    _characterController = Player.GetComponent<CharacterController>();
                return _characterController;
            }
            set => _characterController = value;
        }

        //  Vehicle speed.
        public float Speed
        {
            get
            {
                if (!playerVehicle) return 0f;

                var velocity = CharacterController == null ? Vector3.zero : CharacterController.velocity;
                var speed = Mathf.Abs(velocity.x) + Mathf.Abs(velocity.y) + Mathf.Abs(velocity.z);
                return IsPlayer ? speed : playerVehicle.speed;
            }
        }

        //  Vehicle velocity.
        public Vector3 Velocity
        {
            get
            {
                if (!playerVehicle) return Vector3.zero;

                return IsPlayer
                    ? Player.transform.InverseTransformDirection(TargetVelocity)
                    : playerVehicle.transform.InverseTransformDirection(TargetVelocity);
            }
        }

        //  Hood camera of the vehicle.
        public RCC_HoodCamera HoodCamera
        {
            get
            {
                if (!playerVehicle) return null;

                if (!_hoodCamera)
                    _hoodCamera = playerVehicle.GetComponentInChildren<RCC_HoodCamera>();

                return _hoodCamera;
            }
        }

        private RCC_HoodCamera _hoodCamera;

        //  Wheel camera of the vehicle.
        public RCC_WheelCamera WheelCamera
        {
            get
            {
                if (!playerVehicle)
                    return null;

                if (!_wheelCamera)
                    _wheelCamera = playerVehicle.GetComponentInChildren<RCC_WheelCamera>();

                return _wheelCamera;
            }
        }

        private PlayerRigidbody _playerBody;

        public Vector3 TargetVelocity
        {
            get
            {
                if (!IsPlayer) return playerVehicle.Rigid.velocity;
                _playerBody ??= Player.GetComponent<PlayerRigidbody>();
                return _playerBody.Rigidbody == null
                    ? _playerBody.CharacterController.velocity
                    : _playerBody.Rigidbody.velocity;
            }
        }

        private RCC_WheelCamera _wheelCamera;
    }

    //  Target of the camera, which is our player vehicle with custom class. Can be assigned manually with "SetTarget" method.
    public CameraTarget cameraTarget = new CameraTarget();

    // Currently rendering?
    public bool isRendering = true;

    // Camera is not attached to this main gameobject. Camera is parented to pivot gameobject.
    // Therefore, we can apply additional position and rotation changes.
    public Camera actualCamera;

    // Pivot center of the camera. Used for making offsets and collision movements.
    public GameObject pivot;

    // Camera Modes.
    public enum CameraMode
    {
        TPS,
        FPS,
        WHEEL,
        FIXED,
        CINEMATIC,
        TOP
    }

    public CameraMode cameraMode = CameraMode.TPS;
    private CameraMode lastCameraMode = CameraMode.TPS;

    private RCC_FixedCamera FixedCamera => RCC_FixedCamera.Instance;
    private RCC_CinematicCamera CinematicCamera => RCC_CinematicCamera.Instance;

    public bool TPSLockX = true; //  Locks X angle to vehicle's X.
    public bool TPSLockY = true; //  Locks Y angle to vehicle's Y.
    public bool TPSLockZ = true; //  Locks Z angle to vehicle's Z.
    public bool TPSFreeFall = true; //  Camera rotation won't track vehicle if it's not grounded.

    //  Use dynamic distance, height, and pitch angle related to vehicle rigidbody velocity.
    public bool TPSDynamic = false;

    public bool useTopCameraMode = false; // Shall we use top camera mode?
    public bool useHoodCameraMode = true; // Shall we use hood camera mode?
    public bool useOrbitInTPSCameraMode = true; // Shall we use orbit control in TPS camera mode?
    public bool useOrbitInHoodCameraMode = true; // Shall we use orbit control in hood camera mode?
    public bool useWheelCameraMode = true; //Shall we use wheel camera mode?
    public bool useFixedCameraMode = true; // Shall we use fixed camera mode?
    public bool useCinematicCameraMode = true; // Shall we use cinematic camera mode?
    public bool useOrthoForTopCamera = false; // Shall we use ortho in top camera mode?
    public bool useOcclusion = true; // Shall we use camera occlusion?
    public LayerMask occlusionLayerMask = -1; //  Camera will be ocluded by these layers.
    private bool occluded = false; //  Currently camera is occluding?

    public bool useAutoChangeCamera = false; // Shall we change camera mode by auto? (For cinematics)
    private float autoChangeCameraTimer = 0f; //  Timer for changing the camera mode auto.

    // We will use this Vector3 angle for top camera mode if it's enabled.
    public Vector3 topCameraAngle = new Vector3(45f, 45f, 0f);

    public float topCameraDistance = 100f; // Top camera height / distance.

    // Distance offset for top camera mode. Related with vehicle speed. If vehicle speed is higher, camera will move to front of the vehicle.
    public float maximumZDistanceOffset = 10f;

    private float topCameraDistanceOffset = 0f;

    // Target position.
    private Vector3 targetPosition = Vector3.zero;

    // Used for resetting orbit values when direction of the vehicle has been changed.
    private int direction = 1;
    private int lastDirection = 1;

    [Range(0f, 20f)] public float TPSDistance = 9.5f; // The distance for TPS camera mode.

    [Range(0f, 10f)]
    public float TPSHeight = 3f; // The height we want the camera to be above the target for TPS camera mode.

    [Range(0f, 1f)] public float TPSRotationDamping = .7f; // Rotation movement damper.
    [Range(0f, 25f)] public float TPSTiltMaximum = 15f; // Maximum tilt angle related with rigidbody local velocity.
    [Range(0f, 10f)] public float TPSTiltMultiplier = 1.5f; // Tilt angle multiplier.
    [Range(-45f, 45f)] public float TPSYaw = 0f; // Yaw angle.
    [Range(-45f, 45f)] public float TPSPitch = 10f; // Pitch angle.

    // Auto focus to player vehicle. Adjusts distance and height depends on vehicle bounds.
    public bool TPSAutoFocus = true;

    public bool TPSAutoReverse = true; // Auto reverse when player vehicle is at reverse gear.
    public bool TPSCollision = true; // Collision effect when player vehicle crashes.
    public Vector3 TPSOffset = new Vector3(0f, 0f, .2f); // TPS position offset.
    public Vector3 TPSStartRotation = new Vector3(0f, 0f, 0f); // Rotation of the camera will be this when game starts.
    private Quaternion TPSLastRotation; //  Last rotation of the camera.
    private float TPSTiltAngle = 0f; // Current tilt angle.

    // Camera will adapt its field of view to this target field of view. All field of views below this line will feed this value.
    internal float targetFieldOfView = 60f;

    [Range(10f, 90f)] public float TPSMinimumFOV = 40f; // Minimum field of view related with vehicle speed.
    [Range(10f, 160f)] public float TPSMaximumFOV = 60f; // Maximum field of view related with vehicle speed.

    [Range(10f, 160f)] public float hoodCameraFOV = 60f; // Hood field of view.
    [Range(10f, 160f)] public float wheelCameraFOV = 60f; // Wheel field of view.

    public float minimumOrtSize = 10f; // Minimum ortho size related with vehicle speed.
    public float maximumOrtSize = 20f; // Maximum ortho size related with vehicle speed.

    internal int cameraSwitchCount = 0; // Used in switch case for running corresponding camera mode method.

    private float xVelocity, yVelocity, zVelocity = 0f; //  Ref values for smooth damp and rotation.
    private Vector3 accelerationVelocity = Vector3.zero;

    public Vector3 acceleration = Vector3.zero;
    public Vector3 lastVelocity = Vector3.zero;
    public Vector3 acceleration_Smoothed = Vector3.zero;

    private Vector3 collisionDirection = Vector3.zero; // Collision direction.
    private Vector3 collisionPos = Vector3.zero; // Collision position.
    private Quaternion collisionRot = Quaternion.identity; // Collision rotation.

    [Range(.5f, 10f)] public float zoomScrollMultiplier = 5f; //  Zoom scroll multiplier.
    private float zoomScroll = 0; //  Zoom scroll amount now.
    public float minimumScroll = 0f; //  Minimum distane for the zoom.
    public float maximumScroll = 5f; //  Maximum distance for the zoom.

    // Raw Orbit X and Y inputs.
    private float orbitX, orbitY = 0f;

    // Smooth Orbit X and Y inputs.
    private float orbitX_Smoothed, orbitY_Smoothed = 0f;

    // Minimum and maximum Orbit X, Y degrees.
    public float minOrbitY = -15f;
    public float maxOrbitY = 70f;

    //	Orbit X and Y speeds.
    public float orbitXSpeed = 100f;
    public float orbitYSpeed = 100f;
    public float orbitSmooth = 40f;

    //	Resetting orbits.
    public bool orbitReset = false;
    private float orbitResetTimer = 0f;
    private float oldOrbitX, oldOrbitY = 0f;
    private float timeSinceSpawn = 0f;

    public bool lookBackNow = false; //  Camera is looking back now?

    //  Event when camera spawned.
    public delegate void onBCGCameraSpawned(GameObject BCGCamera);

    public static event onBCGCameraSpawned OnBCGCameraSpawned;

    public override string ToString()
    {
        return "RCC_CAMERA VALUES:\n" +
               $"cameraMode {cameraMode}\n" +
               $"TPSLock ({TPSLockX}, {TPSLockY}, {TPSLockZ})\n" +
               $"targetPosition {targetPosition}\n" +
               $"direction {direction}\n" +
               $"TPSDistance {TPSDistance}\n" +
               $"TPSOffset {TPSOffset}\n" +
               $"TPSTiltAngle {TPSTiltAngle}\n" +
               $"targetFieldOfView {targetFieldOfView}\n" +
               $"cameraSwitchCount {cameraSwitchCount}\n" +
               $"Velocity ({xVelocity}, {yVelocity}, {zVelocity}) {accelerationVelocity}\n" +
               $"acceleration {acceleration}\n" +
               $"lastVelocity {lastVelocity}\n" +
               $"acceleration_Smoothed {acceleration_Smoothed}\n" +
               $"collisionDirection {collisionDirection}\n" +
               $"collisionPos {collisionPos}\n" +
               $"collisionRot {collisionRot}\n" +
               $"zoomScrollMultiplier {zoomScrollMultiplier}\n" +
               $"zoomScroll {zoomScroll}\n" +
               $"minimumScroll {minimumScroll}\n" +
               $"maximumScroll {maximumScroll}\n\n";
    }

    private void Awake()
    {
        // Getting Camera.
        actualCamera = GetComponentInChildren<Camera>();

        //  If pivot of the camera is not found, create it.
        if (!pivot)
        {
            pivot = transform.Find("Pivot").gameObject;
            Debug.Log($"pivot find - {pivot}");

            if (!pivot) pivot = new GameObject("Pivot");

            pivot.transform.SetParent(transform);
            actualCamera.transform.SetParent(pivot.transform, true);
            pivot.transform.localPosition = Vector3.zero;
            pivot.transform.localRotation = Quaternion.identity;
        }
    }

    private void OnEnable()
    {
        ResetCameraVariables();

        // Calling this event when BCG Camera spawned.
        if (OnBCGCameraSpawned != null) OnBCGCameraSpawned(gameObject);

        // Listening player vehicle collisions for crashing effects.
        RCC_CarControllerV3.OnRCCPlayerCollision += RCC_CarControllerV3_OnRCCPlayerCollision;

        // Listening input events for camera modes and look back.
        RCC_InputManager.OnChangeCamera += RCC_InputManager_OnChangeCamera;
        RCC_InputManager.OnLookBack += RCC_InputManager_OnLookBack;
    }

    /// <summary>
    /// Listening collision info of the target vehicle.
    /// </summary>
    /// <param name="RCC"></param>
    /// <param name="collision"></param>
    private void RCC_CarControllerV3_OnRCCPlayerCollision(RCC_CarControllerV3 RCC, Collision collision) =>
        Collision(collision);

    /// <summary>
    /// Look back with button event.
    /// </summary>
    /// <param name="state"></param>
    private void RCC_InputManager_OnLookBack(bool state) => lookBackNow = state;

    /// <summary>
    /// Change camera with button event.
    /// </summary>
    private void RCC_InputManager_OnChangeCamera() => ChangeCamera();

    /// <summary>
    /// Sets target vehicle of the camera.
    /// </summary>
    /// <param name = "player" ></ param >
    public void SetTarget(Transform player, RCC_CarControllerV3 playerCar, bool isPlayer = false)
    {
        //  Setting target vehicle.
        cameraTarget = new CameraTarget { playerVehicle = playerCar, Player = player, IsPlayer = isPlayer };

        //  If auto focus is enabled, adjust distance and height of the camera automatically.
        if (TPSAutoFocus) StartCoroutine(AutoFocus());

        //  And reset the camera modes.
        SetupForNewMode();

        TPSLastRotation = player.transform.rotation;
    }

    /// <summary>
    /// Removes target vehicle.
    /// </summary>
    public void RemoveTarget()
    {
        transform.SetParent(null);
        cameraTarget.playerVehicle = null;
        cameraTarget.IsPlayer = false;
        cameraTarget.Player = null;
    }

    private void Update()
    {
        timeSinceSpawn += Time.deltaTime;
        acceleration_Smoothed = Vector3.SmoothDamp(acceleration_Smoothed, acceleration, ref accelerationVelocity, .3f);

        // If it's active, enable the camera. If it's not, disable the camera.
        if (!isRendering)
        {
            if (actualCamera.gameObject.activeSelf)
                actualCamera.gameObject.SetActive(false);

            return;
        }
        else
        {
            if (!actualCamera.gameObject.activeSelf)
                actualCamera.gameObject.SetActive(true);
        }

        // Lerping current field of view to target field of view.
        actualCamera.fieldOfView = Mathf.Lerp(actualCamera.fieldOfView, targetFieldOfView, Time.deltaTime * 5f);

        // Early out if we don't have the player vehicle.
        if (!cameraTarget.playerVehicle)
            return;

        //  Receive inputs.
        Inputs();
    }

    private void LateUpdate()
    {
        // Early out if we don't have the player vehicle.
        if (!cameraTarget.playerVehicle)
            return;

        // Even if we have the player vehicle and it's disabled, return.
        if (!cameraTarget.playerVehicle.gameObject.activeSelf)
            return;

        if (Time.timeScale <= 0)
            return;

        // Run the corresponding method with choosen camera mode.
        switch (cameraMode)
        {
            case CameraMode.TPS:

                if (useOrbitInTPSCameraMode)
                    ORBIT();

                TPS();

                break;

            case CameraMode.FPS:

                if (useOrbitInHoodCameraMode)
                    ORBIT();

                FPS();

                break;

            case CameraMode.WHEEL:
                WHEEL();
                break;

            case CameraMode.FIXED:
                FIXED();
                break;

            case CameraMode.CINEMATIC:
                CINEMATIC();
                break;

            case CameraMode.TOP:
                TOP();
                break;
        }

        //  If camera mode has been changed in last frame, reset the modes.
        if (lastCameraMode != cameraMode)
            SetupForNewMode();

        lastCameraMode = cameraMode;

        //  If auto change camera is enabled, change the camera mode each 10 seconds.
        if (useAutoChangeCamera)
            autoChangeCameraTimer += Time.deltaTime;

        //  If auto change camera is enabled, change the camera mode each 10 seconds.
        if (useAutoChangeCamera && autoChangeCameraTimer >= 10)
        {
            autoChangeCameraTimer = 0f;
            ChangeCamera();
        }
    }

    private void FixedUpdate()
    {
        // Early out if we don't have the player vehicle.
        if (!cameraTarget.playerVehicle)
            return;

        // Even if we have the player vehicle and it's disabled, return.
        if (!cameraTarget.playerVehicle.gameObject.activeSelf)
            return;

        //  Checking if camera is occluded by some colliders.
        CheckIfOccluded();

        acceleration = cameraTarget.IsPlayer
            ? (cameraTarget.Player.InverseTransformDirection(cameraTarget.TargetVelocity) - lastVelocity) /
              Time.fixedDeltaTime
            : (cameraTarget.playerVehicle.transform.InverseTransformDirection(cameraTarget.TargetVelocity) -
               lastVelocity) / Time.fixedDeltaTime;
        lastVelocity = cameraTarget.IsPlayer
            ? cameraTarget.Player.InverseTransformDirection(cameraTarget.TargetVelocity)
            : cameraTarget.playerVehicle.transform.InverseTransformDirection(cameraTarget.TargetVelocity);

        acceleration.x = 0f;
        acceleration.y = 0f;
        acceleration = Vector3.ClampMagnitude(acceleration, 10f);
    }

    /// <summary>
    /// Receiving player inputs for orbiting the camera and zoom.
    /// </summary>
    private void Inputs()
    {
        if (timeSinceSpawn < 1f)
            return;

        //  Receiving player inputs
        RCC_Inputs inputs = RCC_InputManager.Instance.inputs;

        //  Setting orbits.
        orbitX += inputs.orbitX;
        orbitY -= inputs.orbitY;

        // Clamping orbit Y.
        orbitY = Mathf.Clamp(orbitY, minOrbitY, maxOrbitY);

        //  Smoothing orbits.
        orbitX_Smoothed = Mathf.Lerp(orbitX_Smoothed, orbitX, Time.deltaTime * orbitSmooth);
        orbitY_Smoothed = Mathf.Lerp(orbitY_Smoothed, orbitY, Time.deltaTime * orbitSmooth);

        //  Adjusting zoom scroll with player inputs.
        zoomScroll += inputs.scroll.y * zoomScrollMultiplier;
        zoomScroll = Mathf.Clamp(zoomScroll, minimumScroll, maximumScroll);
    }

    /// <summary>
    /// Change camera by increasing camera switch counter.
    /// </summary>
    public void ChangeCamera()
    {
        // Increasing camera switch counter at each camera changing.
        cameraSwitchCount++;

        // We have 6 camera modes at total. If camera switch counter is greater than maximum, set it to 0.
        if (cameraSwitchCount >= 6)
            cameraSwitchCount = 0;

        switch (cameraSwitchCount)
        {
            case 0:
                cameraMode = CameraMode.TPS;
                break;

            case 1:
                if (useHoodCameraMode && cameraTarget.HoodCamera)
                    cameraMode = CameraMode.FPS;
                else
                    ChangeCamera();
                break;

            case 2:
                if (useWheelCameraMode && cameraTarget.WheelCamera)
                    cameraMode = CameraMode.WHEEL;
                else
                    ChangeCamera();
                break;

            case 3:
                if (useFixedCameraMode && FixedCamera)
                    cameraMode = CameraMode.FIXED;
                else
                    ChangeCamera();
                break;

            case 4:
                if (useCinematicCameraMode && CinematicCamera)
                    cameraMode = CameraMode.CINEMATIC;
                else
                    ChangeCamera();
                break;

            case 5:
                if (useTopCameraMode)
                    cameraMode = CameraMode.TOP;
                else
                    ChangeCamera();
                break;
        }
    }

    /// <summary>
    /// Change camera by directly setting it to specific mode.
    /// </summary>
    /// <param name="mode"></param>
    public void ChangeCamera(CameraMode mode)
    {
        cameraMode = mode;
    }

    /// <summary>
    /// FPS mode.
    /// </summary>
    private void FPS()
    {
        // Assigning orbit rotation, and transform rotation.
        if (useOrbitInHoodCameraMode)
            transform.rotation = cameraTarget.playerVehicle.transform.rotation *
                                 Quaternion.Euler(orbitY_Smoothed, orbitX_Smoothed, 0f);
        else
            transform.rotation = cameraTarget.playerVehicle.transform.rotation;
    }

    /// <summary>
    /// Wheel mode.
    /// </summary>
    private void WHEEL()
    {
        //  If camera is occluded by some colliders, change mode to TPS.
        if (useOcclusion && occluded)
            ChangeCamera(CameraMode.TPS);
    }

    /// <summary>
    /// TPS mode.
    /// </summary>
    private void TPS()
    {
        //  Setting rotation of the camera to the latest (previos frame) rotation.
        transform.rotation = TPSLastRotation;

        // If TPS Auto Reverse is enabled and vehicle is moving backwards, reset X and Y orbits when vehicle direction is changed. Camera will look directly rear side of the vehicle.
        if (lastDirection != (cameraTarget.IsPlayer ? 1 : cameraTarget.playerVehicle.direction))
            direction = cameraTarget.IsPlayer ? 1 : cameraTarget.playerVehicle.direction;

        lastDirection = cameraTarget.IsPlayer ? 1 : cameraTarget.playerVehicle.direction;

        //	Vehicle direction angle used for back side camera angle. 0 means forwards, 180 means backwards.
        int dir = 0;

        float rotDamp = TPSRotationDamping;

        // Calculate the current rotation angles for TPS mode.
        if (TPSAutoReverse) dir = (direction == 1 ? 0 : 180);

        //  Looks back if it's enabled by player input.
        if (lookBackNow)
        {
            dir = 180;
            rotDamp = 1f;
        }

        //  Make sure camera is still checking rotation of the player vehicle.
        if (TPSFreeFall && Time.timeSinceLevelLoad >= 1f)
        {
            if (cameraTarget.IsPlayer
                    ? !cameraTarget.CharacterController.isGrounded
                    : !cameraTarget.playerVehicle.isGrounded)
                rotDamp = -500f;
        }

        //  X angle.
        float xAngle = 0f;

        //  If TPS Lock X is enabled, set X angle.
        if (TPSLockX)
            xAngle = Mathf.SmoothDampAngle(transform.eulerAngles.x,
                cameraTarget.IsPlayer
                    ? cameraTarget.Player.eulerAngles.x * (dir == 180 ? -1f : 1f)
                    : cameraTarget.playerVehicle.transform.eulerAngles.x * (dir == 180 ? -1f : 1f), ref xVelocity,
                1f - rotDamp);

        if (useOrbitInTPSCameraMode && orbitY != 0)
            xAngle = orbitY_Smoothed;

        //  Y angle.
        float yAngle = 0f;

        //  If TPS Lock Y is enabled, set Y angle.
        if (TPSLockY)
        {
            //  If orbit for TPS is enabled, process orbit Y. Otherwise process Y of the player vehicle only.
            if (!useOrbitInTPSCameraMode)
            {
                yAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, ( /*cameraTarget.IsPlayer
                    ? cameraTarget.Player.eulerAngles.y
                    : */cameraTarget.playerVehicle.transform.eulerAngles.y) + dir, ref yVelocity, 1f - rotDamp);
            }
            else
            {
                float y = /*cameraTarget.IsPlayer
                    ? cameraTarget.Player.eulerAngles.y
                    : */cameraTarget.playerVehicle.transform.eulerAngles.y;
                yAngle = orbitX != 0
                    ? Mathf.SmoothDampAngle(transform.eulerAngles.y, y + orbitX_Smoothed, ref yVelocity, .025f)
                    : Mathf.SmoothDampAngle(transform.eulerAngles.y, y + dir, ref yVelocity, 1f - rotDamp);
            }
        }
        else
        {
            if (useOrbitInTPSCameraMode && orbitX != 0)
                yAngle = Mathf.SmoothDampAngle(transform.eulerAngles.y, orbitX_Smoothed, ref yVelocity, .025f);
        }

        //  Z Angle.
        float zAngle = 0f;

        //  If TPS Lock Z is enabled, set Z angle.
        if (TPSLockZ)
            zAngle = Mathf.SmoothDampAngle(transform.eulerAngles.z,
                /*cameraTarget.IsPlayer
                    ? cameraTarget.Player.eulerAngles.z
                    :*/ cameraTarget.playerVehicle.transform.eulerAngles.z,
                ref zVelocity, 1f - rotDamp);

        if (float.IsNaN(xAngle) || float.IsNaN(yAngle) || float.IsNaN(zAngle))
            return;

        // Position at the target.
        Vector3 position = cameraTarget.IsPlayer
            ? cameraTarget.Player.position
            : cameraTarget.playerVehicle.transform.position;

        string log = "";
        log += $"1) {position.ToString("F1")}\n";
        // Rotation at the target.
        Quaternion rotation = Quaternion.Euler(xAngle, yAngle, zAngle);

        // Then offset by distance behind the new angle.
        position += rotation * (-Vector3.forward * (TPSDistance + zoomScroll));
        log += $"2) {position.ToString("F1")}\n";
        position += ( /*cameraTarget.IsPlayer
            ? cameraTarget.Player.rotation
            :*/ cameraTarget.playerVehicle.transform.rotation) * TPSOffset;
        log += $"3) {position.ToString("F1")}\n";
        position += Vector3.up * TPSHeight;
        log += $"4) {position.ToString("F1")}\n";

        //  If TPS dynamic is enabled, reduce distance and height related to the vehicle speed.
        if (TPSDynamic)
            position -= (cameraTarget.IsPlayer
                ? cameraTarget.Player.rotation
                : cameraTarget.playerVehicle.transform.rotation) * acceleration_Smoothed / 20f;

        log += $"5) {position.ToString("F1")}\n";
        //  Setting position and rotation.
        transform.rotation = rotation;
        transform.position = position;
        

        // Collision positions and rotations that affects pivot of the camera.
        collisionPos = Vector3.Lerp(collisionPos, Vector3.zero, Time.deltaTime * 5f);
        collisionRot = Quaternion.Lerp(collisionRot, Quaternion.identity, Time.deltaTime * 5f);

        // Lerping position and rotation of the pivot to collision.
        pivot.transform.localPosition = Vector3.Lerp(pivot.transform.localPosition, collisionPos, Time.deltaTime * 10f);
        pivot.transform.localRotation =
            Quaternion.Lerp(pivot.transform.localRotation, collisionRot, Time.deltaTime * 10f);

        // Lerping targetFieldOfView from TPSMinimumFOV to TPSMaximumFOV related with vehicle speed.
        targetFieldOfView = Mathf.Lerp(TPSMinimumFOV, TPSMaximumFOV, Mathf.Abs(cameraTarget.Speed) / 150f);

        // Rotates camera by Z axis for tilt effect.
        TPSTiltAngle = TPSTiltMaximum *
                       (Mathf.Clamp(cameraTarget.Velocity.x, -1f, 1f) * Mathf.Abs(cameraTarget.Velocity.x) / 250f);
        TPSTiltAngle *= TPSTiltMultiplier;

        //  Checks occlusion if it's enabled.
        if (useOcclusion)
            OccludeRay(cameraTarget.playerVehicle.transform.position);

        //  Assigning last rotation of the camera.
        TPSLastRotation = transform.rotation;

        //  Setting TPS pitch, yaw, and tilt angles.
        transform.rotation *= Quaternion.Euler(TPSPitch, 0f, TPSYaw + TPSTiltAngle);
       // Debug.Log($"use -> END TPS: {transform.position.ToString("F1")}" +
              
        
        
       
    }

    private void FIXED()
    {
        //  Parenting the camera to the fixed camera gameobject.
        if (FixedCamera.transform.parent != null)
            FixedCamera.transform.SetParent(null);

        //  If occluded by some colliders, change position.
        if (useOcclusion && occluded)
        {
            FixedCamera.ChangePosition();
            occluded = false;
        }
    }

    private void TOP()
    {
        // Setting ortho or perspective?
        //actualCamera.orthographic = useOrthoForTopCamera;

        //  Setting distance and target field of view.
        topCameraDistanceOffset = Mathf.Lerp(0f, maximumZDistanceOffset, Mathf.Abs(cameraTarget.Speed) / 100f);
        targetFieldOfView = Mathf.Lerp(minimumOrtSize, maximumOrtSize, Mathf.Abs(cameraTarget.Speed) / 100f);
        actualCamera.orthographicSize = targetFieldOfView;

        // Setting proper targetPosition for top camera mode.
        targetPosition = cameraTarget.IsPlayer
            ? cameraTarget.Player.position
            : cameraTarget.playerVehicle.transform.position;
        targetPosition +=
            (cameraTarget.IsPlayer ? cameraTarget.Player.rotation : cameraTarget.playerVehicle.transform.rotation) *
            Vector3.forward * topCameraDistanceOffset;

        // Assigning position and rotation.
        transform.position = targetPosition;
        Debug.Log($"use -> TOP {transform.position.ToString("F1")}");
        transform.rotation = Quaternion.Euler(topCameraAngle);

        // Pivot position.
        pivot.transform.localPosition = new Vector3(0f, 0f, -topCameraDistance);
    }

    /// <summary>
    /// Orbit mode.
    /// </summary>
    private void ORBIT()
    {
        //  If latest orbit value is changed, set timer to 2 seconds. We'll be using this timer to reset orbit mode if enabled.
        if (oldOrbitX != orbitX)
        {
            oldOrbitX = orbitX;
            orbitResetTimer = 2f;
        }

        //  If latest orbit value is changed, set timer to 2 seconds. We'll be using this timer to reset orbit mode if enabled.
        if (oldOrbitY != orbitY)
        {
            oldOrbitY = orbitY;
            orbitResetTimer = 2f;
        }

        //  Timer for reset orbit mode if enabled.
        if (orbitResetTimer > 0)
            orbitResetTimer -= Time.deltaTime;

        Mathf.Clamp(orbitResetTimer, 0f, 2f);

        //  If vehicle speed is above 25 km/h and reset timer is below 0, reset orbit if enabled.
        if (orbitReset && cameraTarget.Speed >= 25f && orbitResetTimer <= 0f)
        {
            orbitX = 0f;
            orbitY = 0f;
        }
    }

    /// <summary>
    /// Used with mobile UI drag panel.
    /// </summary>
    /// <param name="pointerData"></param>
    public void OnDrag(PointerEventData pointerData)
    {
        // Receiving drag input from UI.
        orbitX += pointerData.delta.x * orbitXSpeed / 1000f;
        orbitY -= pointerData.delta.y * orbitYSpeed / 1000f;

        orbitResetTimer = 0f;
    }

    /// <summary>
    /// Used with mobile UI drag panel.
    /// </summary>
    /// <param name="x"></param>
    /// <param name="y"></param>
    public void OnDrag(float x, float y)
    {
        // Receiving drag input from UI.
        orbitX += x * orbitXSpeed / 10f;
        orbitY -= y * orbitYSpeed / 10f;

        orbitResetTimer = 0f;
    }

    /// <summary>
    /// Cinematic mode.
    /// </summary>
    private void CINEMATIC()
    {
        //  Parenting the camera to the cinematic camera gameobject.
        if (CinematicCamera.transform.parent != null)
            CinematicCamera.transform.SetParent(null);

        //  Setting target field of view of the camera.
        targetFieldOfView = CinematicCamera.targetFOV;

        //  If occluded by some colliders, change mode to TPS.
        if (useOcclusion && occluded)
            ChangeCamera(CameraMode.TPS);
    }

    /// <summary>
    /// Listening collision info of the target vehicle.
    /// </summary>
    /// <param name="collision"></param>
    public void Collision(Collision collision)
    {
        // If it's not enable or camera mode is TPS, return.
        if (!enabled || !isRendering || cameraMode != CameraMode.TPS || !TPSCollision)
            return;

        // Local relative velocity.
        Vector3 colRelVel = collision.relativeVelocity;
        colRelVel *= 1f - Mathf.Abs(Vector3.Dot(transform.up, collision.GetContact(0).normal));

        float cos = Mathf.Abs(Vector3.Dot(collision.GetContact(0).normal, colRelVel.normalized));

        if (colRelVel.magnitude * cos >= 5f)
        {
            collisionDirection = transform.InverseTransformDirection(colRelVel) / (30f);

            collisionPos -= collisionDirection * 5f;
            collisionRot = Quaternion.Euler(new Vector3(-collisionDirection.z * 10f, -collisionDirection.y * 10f,
                -collisionDirection.x * 10f));
            targetFieldOfView = actualCamera.fieldOfView - Mathf.Clamp(collision.relativeVelocity.magnitude, 0f, 15f);
        }
    }

    /// <summary>
    /// Resetting camera while switching to the next mode.
    /// </summary>
    private void SetupForNewMode()
    {
        ResetCameraVariables();

        if (FixedCamera)
            FixedCamera.canTrackNow = false;

        switch (cameraMode)
        {
            case CameraMode.TPS:
                transform.SetParent(null);
                targetFieldOfView = TPSMinimumFOV;
                break;

            case CameraMode.FPS:
                transform.SetParent(cameraTarget.HoodCamera.transform, false);
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
                targetFieldOfView = hoodCameraFOV;
                cameraTarget.HoodCamera.FixShake();
                break;

            case CameraMode.WHEEL:
                transform.SetParent(cameraTarget.WheelCamera.transform, false);
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
                targetFieldOfView = wheelCameraFOV;
                break;

            case CameraMode.FIXED:
                transform.SetParent(FixedCamera.transform, false);
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
                targetFieldOfView = 60;
                FixedCamera.canTrackNow = true;
                break;

            case CameraMode.CINEMATIC:
                transform.SetParent(CinematicCamera.pivot.transform, false);
                transform.localPosition = Vector3.zero;
                transform.localRotation = Quaternion.identity;
                targetFieldOfView = 30f;
                break;

            case CameraMode.TOP:
                transform.SetParent(null);
                targetFieldOfView = minimumOrtSize;
                pivot.transform.localPosition = Vector3.zero;
                pivot.transform.localRotation = Quaternion.identity;
                targetPosition = cameraTarget.IsPlayer
                    ? cameraTarget.Player.position
                    : cameraTarget.playerVehicle.transform.position;
                targetPosition +=
                    (cameraTarget.IsPlayer
                        ? cameraTarget.Player.rotation
                        : cameraTarget.playerVehicle.transform.rotation) * Vector3.forward *
                    topCameraDistanceOffset;
                transform.position = cameraTarget.IsPlayer
                    ? cameraTarget.Player.position
                    : cameraTarget.playerVehicle.transform.position;
                Debug.Log($"use -> TOP2 {transform.position.ToString("F1")}");
                break;
        }
    }

    /// <summary>
    /// Resetting camera. Useful when switching between camera modes.
    /// </summary>
    public void ResetCameraVariables()
    {
        TPSTiltAngle = 0f;

        collisionPos = Vector3.zero;
        collisionRot = Quaternion.identity;
        collisionDirection = Vector3.zero;

        actualCamera.transform.localPosition = Vector3.zero;
        actualCamera.transform.localRotation = Quaternion.identity;

        pivot.transform.localPosition = collisionPos;
        pivot.transform.localRotation = collisionRot;

        orbitX = TPSStartRotation.y;
        orbitY = TPSStartRotation.x;

        zoomScroll = 0f;

        if (TPSStartRotation != Vector3.zero)
            TPSStartRotation = Vector3.zero;

        actualCamera.orthographic = false;
        occluded = false;

        orbitResetTimer = 0f;
        orbitX = 0f;
        orbitY = 0f;
        direction = 1;
        lastDirection = 1;
        lookBackNow = false;
        autoChangeCameraTimer = 0f;

        acceleration = Vector3.zero;
        lastVelocity = Vector3.zero;

        targetPosition = Vector3.zero;
    }

    /// <summary>
    /// Enables or disables the camera.
    /// </summary>
    /// <param name="state"></param>
    public void ToggleCamera(bool state)
    {
        // Enabling / disabling activity.
        isRendering = state;
    }

    /// <summary>
    /// Checks occlusion.
    /// </summary>
    /// <param name="targetFollow"></param>
    private void OccludeRay(Vector3 targetFollow)
    {
        //declare a new raycast hit.
        RaycastHit wallHit = new RaycastHit();

        if (Physics.Linecast(targetFollow, transform.position, out wallHit, occlusionLayerMask))
        {
            if (!wallHit.collider.isTrigger && !wallHit.transform.IsChildOf(cameraTarget.playerVehicle.transform))
            {
                //the x and z coordinates are pushed away from the wall by hit.normal.
                //the y coordinate stays the same.
                Vector3 occludedPosition = new Vector3(wallHit.point.x + wallHit.normal.x * .2f,
                    wallHit.point.y + wallHit.normal.y * .2f, wallHit.point.z + wallHit.normal.z * .2f);

                transform.position = occludedPosition;
                Debug.Log($"use -> OccludeRay {transform.position.ToString("F1")}");
            }
        }
    }

    /// <summary>
    /// Checks occlusion.
    /// </summary>
    /// <param name="targetFollow"></param>
    private void CheckIfOccluded()
    {
        //Declare a new raycast hit.
        RaycastHit wallHit = new RaycastHit();

        if (Physics.Linecast(cameraTarget.playerVehicle.transform.position, transform.position, out wallHit,
                occlusionLayerMask))
        {
            if (!wallHit.collider.isTrigger && !wallHit.transform.IsChildOf(cameraTarget.playerVehicle.transform))
                occluded = true;
        }
    }

    /// <summary>
    /// Autofocus to target vehicle. Calculates distance and height of the camera related to bounds of the target vehicle.
    /// </summary>
    /// <returns></returns>
    public IEnumerator AutoFocus()
    {
        float timer = 3f;
        float bounds = RCC_GetBounds.MaxBoundsExtent(cameraTarget.playerVehicle.transform);

        while (timer > 0f)
        {
            timer -= Time.deltaTime;
            TPSDistance = Mathf.Lerp(TPSDistance, bounds * 2.8f, Time.deltaTime * 2f);
            TPSHeight = Mathf.Lerp(TPSHeight, bounds * .65f, Time.deltaTime * 2f);
            yield return null;
        }

        TPSDistance = bounds * 2.8f;
        TPSHeight = bounds * .65f;
    }

    /// <summary>
    /// Autofocus to target vehicle. Calculates distance and height of the camera related to bounds of the target vehicle.
    /// </summary>
    /// <returns></returns>
    public IEnumerator AutoFocus(Transform transformBounds)
    {
        float timer = 3f;
        float bounds = RCC_GetBounds.MaxBoundsExtent(transformBounds);

        while (timer > 0f)
        {
            timer -= Time.deltaTime;
            TPSDistance = Mathf.Lerp(TPSDistance, bounds * 2.8f, Time.deltaTime * 2f);
            TPSHeight = Mathf.Lerp(TPSHeight, bounds * .65f, Time.deltaTime * 2f);
            yield return null;
        }

        TPSDistance = bounds * 2.8f;
        TPSHeight = bounds * .65f;
    }

    /// <summary>
    /// Autofocus to target vehicle. Calculates distance and height of the camera related to bounds of the target vehicle.
    /// </summary>
    /// <returns></returns>
    public IEnumerator AutoFocus(Transform transformBounds1, Transform transformBounds2)
    {
        float timer = 3f;
        float bounds = (RCC_GetBounds.MaxBoundsExtent(transformBounds1) +
                        RCC_GetBounds.MaxBoundsExtent(transformBounds2));

        while (timer > 0f)
        {
            timer -= Time.deltaTime;
            TPSDistance = Mathf.Lerp(TPSDistance, bounds * 2.8f, Time.deltaTime * 2f);
            TPSHeight = Mathf.Lerp(TPSHeight, bounds * .65f, Time.deltaTime * 2f);
            yield return null;
        }

        TPSDistance = bounds * 2.8f;
        TPSHeight = bounds * .65f;
    }

    /// <summary>
    /// Autofocus to target vehicle. Calculates distance and height of the camera related to bounds of the target vehicle.
    /// </summary>
    /// <returns></returns>
    public IEnumerator AutoFocus(Transform transformBounds1, Transform transformBounds2, Transform transformBounds3)
    {
        float timer = 3f;
        float bounds = (RCC_GetBounds.MaxBoundsExtent(transformBounds1) +
                        RCC_GetBounds.MaxBoundsExtent(transformBounds2) +
                        RCC_GetBounds.MaxBoundsExtent(transformBounds3));

        while (timer > 0f)
        {
            timer -= Time.deltaTime;
            TPSDistance = Mathf.Lerp(TPSDistance, bounds * 2.8f, Time.deltaTime * 2f);
            TPSHeight = Mathf.Lerp(TPSHeight, bounds * .65f, Time.deltaTime * 2f);
            yield return null;
        }

        TPSDistance = bounds * 2.8f;
        TPSHeight = bounds * .65f;
    }

    private void OnDisable()
    {
        RCC_CarControllerV3.OnRCCPlayerCollision -= RCC_CarControllerV3_OnRCCPlayerCollision;

        // Listening input events.
        RCC_InputManager.OnChangeCamera -= RCC_InputManager_OnChangeCamera;
        RCC_InputManager.OnLookBack -= RCC_InputManager_OnLookBack;
    }

    private void Reset()
    {
        //  If pivot of the camera is not found, create it.
        if (transform.Find("Pivot"))
            pivot = transform.Find("Pivot").gameObject;

        if (!pivot)
        {
            pivot = new GameObject("Pivot");

            pivot.transform.SetParent(transform);
            pivot.transform.localPosition = Vector3.zero;
            pivot.transform.localRotation = Quaternion.identity;
        }

        Camera foundCamera = GetComponentInChildren<Camera>();

        if (foundCamera)
            Destroy(foundCamera);

        GameObject newCamera = new GameObject("Camera");
        newCamera.transform.SetParent(pivot.transform);
        newCamera.transform.localPosition = Vector3.zero;
        newCamera.transform.localRotation = Quaternion.identity;
        newCamera.AddComponent<Camera>();
        newCamera.AddComponent<AudioListener>();
        newCamera.gameObject.tag = "MainCamera";
    }
}