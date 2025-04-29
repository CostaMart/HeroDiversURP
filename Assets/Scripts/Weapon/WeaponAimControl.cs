using UnityEngine;

public class WeapnAimControl : MonoBehaviour
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
    [SerializeField] private Transform aimTarget;


    private bool aiming = false;
    private Quaternion targetRotation;
    Transform torotate;
    private Quaternion initialLocalPos;

    void Awake()
    {
        controlEventManager.AddMouseControlListener(OnMouseMove);

        rb = parentTransform.GetComponent<Rigidbody>();
        controlEventManager.AddListenerAiming(OnAiming);
        torotate = transform.GetChild(0);

        torotate.localRotation = Quaternion.identity;

    }

    void LateUpdate()
    {
        // Calcola la direzione verso il target nello spazio locale
        Vector3 directionToTarget = (aimTarget.position - torotate.position).normalized;
        directionToTarget = transform.InverseTransformDirection(directionToTarget);

        // Adatta la direzione in base agli assi (Y è il nuovo forward)
        directionToTarget = new Vector3(-directionToTarget.x, -directionToTarget.z, directionToTarget.y);

        // Calcola la rotazione desiderata
        Quaternion lookRotation = Quaternion.LookRotation(directionToTarget);

        // Estrai gli angoli euler
        Vector3 euler = lookRotation.eulerAngles;

        // Converte da 0–360 a -180–180 per clamp più intuitivo
        if (euler.y > 180f) euler.y -= 360f;

        // Clamp sull'asse X
        euler.y = Mathf.Clamp(euler.y, -30f, 30f);

        // Ricostruisce la rotazione con l’X clampata
        lookRotation = Quaternion.Euler(euler);

        // Applica la rotazione interpolata
        torotate.localRotation = Quaternion.Slerp(
            torotate.localRotation,
            lookRotation,
           1
        );
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



