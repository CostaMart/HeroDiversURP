using UnityEngine;

public class WeapnYAimControl : MonoBehaviour
{
    private float rotationX = 0f;
    private float rotationY = 0f;

    private Vector2 delta;

    public Rigidbody parentTransform;

    [SerializeField] private ControlEventManager controlEventManager;
    [SerializeField] private CameraSettings cameraSettings;
    Rigidbody rb;

    [SerializeField] private float rotationSmoothSpeed = 20f; // maggiore = più reattivo, minore = più morbido
    [SerializeField] private EffectsDispatcher dispatcher;


    private bool aiming = false;
    private Quaternion targetRotation;

    void Awake()
    {
        controlEventManager.AddMouseControlListener(OnMouseMove);

        rb = parentTransform.GetComponent<Rigidbody>();
        controlEventManager.AddListenerAiming(OnAiming);
    }

    void LateUpdate()
    {
        // Calcola la rotazione target basata sul mouse
        rotationX -= delta.y * cameraSettings.Sensitivity;
        rotationX = Mathf.Clamp(rotationX, cameraSettings.LowerBoundYrotation, cameraSettings.UpperBoundYrotation);

        rotationY += delta.x * cameraSettings.Sensitivity;

        // Imposta la rotazione target
        targetRotation = Quaternion.Euler(rotationX, rotationY, 0);

        // Interpola verso la rotazione target per ottenere un movimento fluido
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 100);
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



