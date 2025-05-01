using Unity.Cinemachine;
using UnityEngine;

public class MouseRotateCamera : MonoBehaviour
{
    private float rotationX = 0f;
    private float rotationY = 0f;
    private Vector2 delta;

    private Vector3 initialPos;
    private Vector3 zoomVector;

    private bool aiming = false;
    private bool zoomed = false;
    private bool ragdolling = false;

    [SerializeField] private GameObject ragdollRef;
    private Vector3 initialLocalPos;

    [Header("Camera Settings")]
    public CinemachineThirdPersonFollow followCamera;
    [SerializeField] private CinemachineCamera cineCam;
    private float initialCameraDistance;

    [SerializeField] private CameraSettings settings;
    [SerializeField] private ControlEventManager ControlEventManager;

    [Header("Zoom Settings")]
    public float ZoomSpeed = 5.0f;

    [Header("Recoil Settings")]
    [SerializeField] private float recoilRecoverySpeed = 10f;

    private float currentVerticalRecoil = 0f;
    private float currentHorizontalRecoil = 0f;

    void Awake()
    {
        if (settings == null)
            Debug.LogError("CameraSettings not found");

        ControlEventManager.AddMouseControlListener(OnMouseRotation);
        ControlEventManager.AddListenerAiming((value) => aiming = value);
        ControlEventManager.AddRagdollListener(OnRagdolling);

        initialLocalPos = transform.localPosition;
    }

    void Start()
    {
        initialCameraDistance = followCamera.CameraDistance;
        transform.localPosition = initialLocalPos;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        initialPos = transform.localPosition;
        zoomVector = Vector3.one * settings.Zoom;
        zoomVector.y = 0;
        zoomVector.x = 0;
    }

    void Update()
    {
        float targetFov = aiming ? settings.AimingFov : settings.DefaultFov;
        float targetDistance = aiming ? initialCameraDistance - settings.Zoom : initialCameraDistance;

        cineCam.Lens.FieldOfView = Mathf.Lerp(cineCam.Lens.FieldOfView, targetFov, Time.deltaTime * settings.ZoomSpeed);
        followCamera.CameraDistance = Mathf.Lerp(followCamera.CameraDistance, targetDistance, Time.deltaTime * settings.ZoomSpeed);

        // Input del mouse
        rotationY += delta.x * settings.Sensitivity;
        rotationX -= delta.y * settings.Sensitivity;

        // Applica rinculo
        rotationX -= currentVerticalRecoil;
        rotationY += currentHorizontalRecoil;

        // Recupera rinculo
        currentVerticalRecoil = Mathf.Lerp(currentVerticalRecoil, 0f, Time.deltaTime * recoilRecoverySpeed);
        currentHorizontalRecoil = Mathf.Lerp(currentHorizontalRecoil, 0f, Time.deltaTime * recoilRecoverySpeed);

        // Clamp verticale
        rotationX = Mathf.Clamp(rotationX, settings.LowerBoundYrotation, settings.UpperBoundYrotation);

        if (!ragdolling)
        {
            transform.rotation = Quaternion.Euler(rotationX, rotationY, 0f);
        }
        else
        {
            transform.rotation = Quaternion.Euler(0f, rotationY, 0f);
            transform.position = ragdollRef.transform.position;
        }
    }

    public void OnRagdolling(bool isRagdolling)
    {
        if (!isRagdolling)
            transform.localPosition = initialLocalPos;

        ragdolling = isRagdolling;
    }

    public void OnMouseRotation(Vector2 rotation)
    {
        delta = rotation;
    }

    /// <summary>
    /// Applica un rinculo verticale e una deviazione orizzontale deterministica.
    /// </summary>
    /// <param name="verticalIntensity">Quanto spinge in alto</param>
    /// <param name="horizontalDeviation">Quanto devia lateralmente (+ destra, - sinistra)</param>
    public void ApplyRecoil(float verticalIntensity, float horizontalDeviation)
    {
        currentVerticalRecoil += verticalIntensity;
        currentHorizontalRecoil += horizontalDeviation;
    }
}

