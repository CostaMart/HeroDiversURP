using UnityEngine;

public class WeapnYAimControl : MonoBehaviour
{
    private float rotationX = 0f;
    private float rotationY = 0f;

    private Vector2 delta;

    public Transform parentTransform;

    [SerializeField] private ControlEventManager controlEventManager;
    [SerializeField] private CameraSettings cameraSettings;

    [SerializeField] private float rotationSmoothSpeed = 20f; // maggiore = più reattivo, minore = più morbido

    private bool aiming = false;
    private Quaternion targetRotation;

    void Awake()
    {
        controlEventManager.AddMouseControlListener(OnMouseMove);
        controlEventManager.AddListenerAiming(OnAiming);
    }

    void Update()
    {
        // Calcola la rotazione target basata sul mouse
        rotationX -= delta.y * cameraSettings.Sensitivity;
        rotationX = Mathf.Clamp(rotationX, cameraSettings.LowerBoundYrotation, cameraSettings.UpperBoundYrotation);
        rotationY = parentTransform.eulerAngles.y;
        var rotationZ = parentTransform.eulerAngles.z;

        // Imposta la rotazione target
        targetRotation = Quaternion.Euler(rotationX, rotationY, rotationZ);

        // Interpola verso la rotazione target per ottenere un movimento fluido
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, 5000);
    }

    void OnAiming(bool value)
    {
        aiming = value;
    }

    void OnMouseMove(Vector2 value)
    {
        this.delta = value;

    }
}



