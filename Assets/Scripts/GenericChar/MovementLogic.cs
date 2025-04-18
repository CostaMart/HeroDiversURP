using System.Linq;
using Unity.Cinemachine;
using UnityEngine;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class MovementLogic : MonoBehaviour
{
    private Animator anim;
    public CinemachineCamera camera;
    private int jumpsAvailable = 1;
    private bool aiming = false;

    private Rigidbody rb;
    private Collider col;

    [SerializeField] private EffectsDispatcher dispatcher;
    [SerializeField] private ControlEventManager controlEventManager;
    [SerializeField] private float maxSlopeAngle = 45f;
    [SerializeField] private float groundCheckDistance = 1.0f;
    [SerializeField] private float defaultSpeed = 5f;
    [SerializeField] private float defaultRotationSpeed = 8f;
    [SerializeField] private float acceleration = 8f;
    [SerializeField] private float deceleration = 12f; // <-- nuova variabile!

    private Vector3 moveDirection = Vector3.zero;
    private bool isGrounded = true;

    void Awake()
    {
        controlEventManager.AddListenerMove(Move);
        controlEventManager.AddListenerJump(Jump);
        controlEventManager.AddListenerAiming((value) => Aiming = value);
    }

    void Start()
    {
        jumpsAvailable = dispatcher.GetAllFeatureByType<int>(FeatureType.maxJumps).Sum();
        col = GetComponent<Collider>();
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();

        Application.targetFrameRate = 120;
    }

    private void FixedUpdate()
    {
        if (rb.isKinematic)
            return;

        Vector3 direction = Vector3.zero;
        direction += camera.transform.forward * moveDirection.y;
        direction += camera.transform.right * moveDirection.x;
        direction.y = 0;

        bool allowMovement = true;

        RaycastHit hit;
        Debug.DrawRay(transform.position, transform.forward * 5, Color.red);
        if (Physics.Raycast(transform.position, transform.forward, out hit, groundCheckDistance))
        {
            float angle = Vector3.Angle(hit.normal, Vector3.up);
            if (angle > maxSlopeAngle && hit.normal.y > 0)
            {
                allowMovement = false;
            }
        }

        // Gestiamo la rotazione in modo separato
        Quaternion targetRotation = Quaternion.LookRotation(transform.forward);

        if (aiming)
        {
            Vector3 aimForward = camera.transform.forward;
            aimForward.y = 0;
            targetRotation = Quaternion.LookRotation(aimForward);
        }
        else if (direction != Vector3.zero)
        {
            // Solo quando ci si sta muovendo, ruotiamo il personaggio
            targetRotation = Quaternion.LookRotation(direction);
        }

        float rotSpeed = dispatcher.GetAllFeatureByType<float>(
            aiming ? FeatureType.aimRotationSpeed : FeatureType.rotationSpeed
        ).DefaultIfEmpty(defaultRotationSpeed).Sum();

        if (allowMovement && isGrounded)
        {
            float moveSpeed = dispatcher.GetAllFeatureByType<float>(FeatureType.speed)
                .DefaultIfEmpty(defaultSpeed).Sum();

            Vector3 targetVelocity = Vector3.zero;

            if (direction != Vector3.zero)
            {
                // Muovendosi → target velocity è nella direzione dell'input
                targetVelocity = direction.normalized * moveSpeed;
                targetVelocity.y = rb.linearVelocity.y;

                // Applichiamo accelerazione
                rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, targetVelocity, acceleration);
            }
            else
            {
                // Nessun input → target velocity verso zero
                targetVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);

                // Applichiamo decelerazione
                rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, targetVelocity, deceleration);
            }
        }

        // Gestiamo la rotazione
        rb.MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, rotSpeed));
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("terrain"))
        {
            anim.SetBool("jump", true);
            isGrounded = false;
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("terrain"))
        {
            jumpsAvailable = dispatcher.GetAllFeatureByType<int>(FeatureType.maxJumps).Sum();
            isGrounded = true;
        }
    }

    public void Jump()
    {
        if (jumpsAvailable <= 0) return;

        Vector3 direction = Vector3.zero;
        direction += camera.transform.forward * moveDirection.y;
        direction += camera.transform.right * moveDirection.x;
        direction.y = 0;
        direction = direction.normalized;

        float jumpForceVertical = dispatcher.GetAllFeatureByType<float>(FeatureType.jumpSpeedy)
            .DefaultIfEmpty(5f).Sum();
        float jumpForceHorizontal = dispatcher.GetAllFeatureByType<float>(FeatureType.jumpSpeedx)
            .DefaultIfEmpty(0f).Sum();

        rb.linearVelocity = new Vector3(
            rb.linearVelocity.x + direction.x * jumpForceHorizontal,
            jumpForceVertical,
            rb.linearVelocity.z + direction.z * jumpForceHorizontal);

        jumpsAvailable--;
    }

    public void Move(Vector2 direction)
    {
        moveDirection = new Vector3(direction.x, direction.y, 0);
    }

    public bool Aiming
    {
        get { return aiming; }
        set { aiming = value; }
    }
}

