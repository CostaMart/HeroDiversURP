using Unity.Cinemachine;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;

public class MouseRotateCamera : MonoBehaviour
{

    private float rotationX = 0f;
    private float rotationY = 0f;
    private Vector3 initialPos;
    private Vector3 zoomVector;
    private bool aiming = false;
    private bool zoomed = false;
    private Vector2 delta;

    // ragdolling camera management
    [SerializeField] private GameObject ragdollRef;
    private Vector3 initialLocalPos;
    private bool ragdolling = false;

    [Header("Camera Settings")]
    [Tooltip("Questo componente gestisce le impostazioni della cinemachinecamera, questo componente è perciò richiesto. In particolare assume che la camera segua un empy gameObject posizionato sul personaggio")]
    public CinemachineThirdPersonFollow followCamera;
    [Tooltip("riferimento alla cinecamera")]
    [SerializeField] private CinemachineCamera cineCam;
    private float initialCameraDistance;

    [SerializeField] private CameraSettings settings;
    [SerializeField] private ControlEventManager ControlEventManager;

    void Awake()
    {
        if (settings == null)
        {
            Debug.LogError("CameraSettings not found");
        }
        ControlEventManager.AddMouseControlListener(OnMouseRotation);
        ControlEventManager.AddListenerAiming((value) => aiming = value);
        if (ControlEventManager != null)
            ControlEventManager.AddRagdollListener(OnRagdolling);
        initialLocalPos = transform.localPosition;
    }

    void Start()
    {
        initialCameraDistance = followCamera.CameraDistance;
        transform.localPosition = initialLocalPos;

        // Blocca il cursore al centro dello schermo e lo rende invisibile
        //Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        initialPos = transform.localPosition;
        zoomVector = Vector3.one * settings.Zoom;
        zoomVector.y = 0;
        zoomVector.x = 0;
    }
    public float ZoomSpeed = 5.0f;
    void Update()
    {
        float targetFov = aiming ? settings.AimingFov : settings.DefaultFov;
        float targetDistance = aiming ? initialCameraDistance - settings.Zoom : initialCameraDistance;

        // Interpolazione smooth
        cineCam.Lens.FieldOfView = Mathf.Lerp(cineCam.Lens.FieldOfView, targetFov, Time.deltaTime * settings.ZoomSpeed);
        followCamera.CameraDistance = Mathf.Lerp(followCamera.CameraDistance, targetDistance, Time.deltaTime * settings.ZoomSpeed);

        // Calcola la direzione di rotazione in base al movimento del mouse
        rotationY += delta.x * settings.Sensitivity;
        rotationX -= delta.y * settings.Sensitivity;
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
}


