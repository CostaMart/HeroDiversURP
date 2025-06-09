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

    [SerializeField] private EventChannels channel;


    private float currentVerticalRecoil = 0f;
    private float currentHorizontalRecoil = 0f;

    private bool isBursting = false;

    void Awake()
    {

        channel.Subscribe("BurstOn", CameraBurstReaction);
        channel.Subscribe("BurstOff", CameraBurstEndReaction);


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

        //Cursor.lockState = CursorLockMode.Locked;
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

        if (isBursting)
        {
            targetFov += settings.BurstFov;
            targetDistance += settings.BurstDistance;
        }

        cineCam.Lens.FieldOfView = Mathf.Lerp(cineCam.Lens.FieldOfView, targetFov, Time.deltaTime * settings.ZoomSpeed);
        followCamera.CameraDistance = Mathf.Lerp(followCamera.CameraDistance, targetDistance, Time.deltaTime * settings.ZoomSpeed);

        rotationY += delta.x * settings.Sensitivity;
        rotationX -= delta.y * settings.Sensitivity;

        rotationX -= currentVerticalRecoil;
        rotationY += currentHorizontalRecoil;

        currentVerticalRecoil = Mathf.Lerp(currentVerticalRecoil, 0f, Time.deltaTime * recoilRecoverySpeed);
        currentHorizontalRecoil = Mathf.Lerp(currentHorizontalRecoil, 0f, Time.deltaTime * recoilRecoverySpeed);

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

    public void ApplyRecoil(float verticalIntensity, float horizontalDeviation, float recoilMax, float reciolRecovery)
    {
        recoilRecoverySpeed = reciolRecovery;
        currentVerticalRecoil += verticalIntensity;
        currentHorizontalRecoil += horizontalDeviation;

        currentVerticalRecoil = Mathf.Clamp(currentVerticalRecoil, -recoilMax, recoilMax);
        currentHorizontalRecoil = Mathf.Clamp(currentHorizontalRecoil, -recoilMax, recoilMax);
    }

    public void ResetRecoil()
    {
        currentVerticalRecoil = 0f;
        currentHorizontalRecoil = 0f;
    }

    public void CameraBurstReaction()
    {
        if (!aiming)
            isBursting = true;
    }

    public void CameraBurstEndReaction()
    {
        isBursting = false;
    }
}
