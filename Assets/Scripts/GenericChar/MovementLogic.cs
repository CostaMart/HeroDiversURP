using System.Linq;
using CartoonFX;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using Quaternion = UnityEngine.Quaternion;
using Vector2 = UnityEngine.Vector2;
using Vector3 = UnityEngine.Vector3;

public class MovementLogic : MonoBehaviour
{
    private Animator anim;
    public CinemachineCamera camera;
    private Rigidbody rb;
    private Collider col;

    [SerializeField] private EffectsDispatcher dispatcher;
    [SerializeField] private ControlEventManager controlEventManager;
    [SerializeField] private PlayerInput playerInput;

    [Header("Movement Settings")]
    [SerializeField] private float maxSlopeAngle = 45f;
    [SerializeField] private float groundCheckDistance = 1.0f;
    [SerializeField] private float defaultSpeed = 5f;
    [SerializeField] private float defaultRotationSpeed = 8f;
    [SerializeField] private float acceleration = 8f;
    [SerializeField] private float deceleration = 12f;

    [Header("Strafe Settings")]
    [SerializeField] public int maxStrafes = 2;
    [SerializeField] public float strafeCooldown = 5f;
    [SerializeField] public float burstDuration = 0.2f;
    [SerializeField] public float burstSpeedMultiplier = 2.0f;
    [SerializeField] public float overHeatLimit = 100f;
    public float temperature;

    private Vector3 moveDirection = Vector3.zero;
    private bool isGrounded = true;
    private bool aiming = false;

    public int jumpsAvailable = 1;
    public int usedStrafes;
    public float strafeTimer;
    public bool isBursting = false;
    public float burstDurationTimer = 0f;
    private float smokeDuration = 2f;
    private float smokeTimer = 0f;
    private bool smokeActive = false;

    private Vector3 burstDirection = Vector3.zero;

    [SerializeField] private Transform aimTarget;

    [SerializeField] EventChannels eventChannels;
    UnityEvent burstOn = new UnityEvent();
    UnityEvent burstOff = new UnityEvent();
    UnityEvent jump = new UnityEvent();


    void Awake()

    {
        eventChannels.createEvent("BurstOn", burstOn);
        eventChannels.createEvent("BurstOff", burstOff);
        eventChannels.createEvent("Jump", jump);

        controlEventManager.AddListenerMove(Move);
        controlEventManager.AddListenerJump(Jump);
        controlEventManager.AddListenerAiming((value) => Aiming = value);
        playerInput.actions["Strafe"].performed += ctx => TryStrafe();
    }

    void Start()
    {
        jumpsAvailable = dispatcher.GetAllFeatureByType<int>(FeatureType.maxJumps).Sum();
        col = GetComponent<Collider>();
        anim = GetComponent<Animator>();
        rb = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        if (rb.isKinematic) return;

        temperature = dispatcher.GetAllFeatureByType<float>(FeatureType.heat).DefaultIfEmpty(10).Sum();

        HandleMovement();
        HandleStrafeCooldown();
        HandleBurstTimer();
    }


    private void HandleMovement()
    {
        Vector3 direction = isBursting
            ? burstDirection
            : camera.transform.forward * moveDirection.y + camera.transform.right * moveDirection.x;

        direction.y = 0;
        direction.Normalize();

        bool allowMovement = true;

        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, groundCheckDistance))
        {
            float angle = Vector3.Angle(hit.normal, Vector3.up);
            if (angle > maxSlopeAngle && hit.normal.y > 0)
            {
                allowMovement = false;
            }
        }

        Quaternion targetRotation = transform.rotation;

        if (aiming && aimTarget != null)
        {
            Vector3 targetDirection = aimTarget.position - transform.position;
            targetDirection.y = 0f;
            if (targetDirection != Vector3.zero)
            {
                targetRotation = Quaternion.LookRotation(targetDirection);
                rb.MoveRotation(targetRotation);
            }
        }
        else if (direction != Vector3.zero)
        {
            float rotSpeed = dispatcher.GetAllFeatureByType<float>(FeatureType.rotationSpeed)
                .DefaultIfEmpty(defaultRotationSpeed).Sum();

            targetRotation = Quaternion.LookRotation(direction);
            rb.MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, rotSpeed * Time.fixedDeltaTime));
        }

        if (allowMovement)
        {
            float moveSpeed = dispatcher.GetAllFeatureByType<float>(FeatureType.speed).DefaultIfEmpty(defaultSpeed).Sum();
            float speedMultiplier = 1f;

            if (isBursting)
            {
                moveSpeed = dispatcher.GetAllFeatureByType<float>(FeatureType.strafePower).Sum();
                speedMultiplier = 10f;
                isGrounded = false;
            }

            Vector3 targetVelocity;

            if (direction != Vector3.zero)
            {
                targetVelocity = direction * moveSpeed;
                targetVelocity.y = rb.linearVelocity.y;
                rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, targetVelocity, speedMultiplier * acceleration * Time.fixedDeltaTime);
            }
            else
            {
                Vector3 stopVelocity = new Vector3(0f, rb.linearVelocity.y, 0f);
                rb.linearVelocity = Vector3.Lerp(rb.linearVelocity, stopVelocity, deceleration * Time.fixedDeltaTime);
            }
        }
    }
    public void Jump()
    {
        if (jumpsAvailable <= 0) return;

        Vector3 direction = camera.transform.forward * moveDirection.y + camera.transform.right * moveDirection.x;
        direction.y = 0;
        direction.Normalize();

        float jumpForceVertical = dispatcher.GetAllFeatureByType<float>(FeatureType.jumpSpeedy)
            .DefaultIfEmpty(5f).Sum();
        float jumpForceHorizontal = dispatcher.GetAllFeatureByType<float>(FeatureType.jumpSpeedx)
            .DefaultIfEmpty(0f).Sum();

        rb.linearVelocity = new Vector3(
            rb.linearVelocity.x + direction.x * jumpForceHorizontal,
            jumpForceVertical,
            rb.linearVelocity.z + direction.z * jumpForceHorizontal
        );


        jump.Invoke();


        jumpsAvailable--;
    }



    private void HandleStrafeCooldown()
    {
        var strafeCooldownFeat = dispatcher.GetAllFeatureByType<float>(FeatureType.strafeCooldown)
            .DefaultIfEmpty(strafeCooldown).Sum();

        if (usedStrafes > 0)
        {
            strafeTimer += Time.fixedDeltaTime;
            if (strafeTimer >= strafeCooldownFeat)
            {
                usedStrafes--;
                strafeTimer = 0f;
            }
        }
    }

    private void HandleBurstTimer()
    {
        var burstDurationFeat = dispatcher.GetAllFeatureByType<float>(FeatureType.strafeBurstDuration)
            .DefaultIfEmpty(burstDuration).Sum();

        if (isBursting)
        {
            burstDurationTimer += Time.fixedDeltaTime;
            if (burstDurationTimer >= burstDurationFeat)
            {
                isBursting = false;
                burstDurationTimer = 0f;

                burstOff.Invoke();
                smokeTimer = 0f;
                smokeActive = true;
            }

            isGrounded = false;
        }

        if (smokeActive)
        {
            smokeTimer += Time.fixedDeltaTime;
            if (smokeTimer >= smokeDuration)
            {
                smokeActive = false;
            }
        }
    }

    public void TryStrafe()
    {

        if (moveDirection == Vector3.zero) return; // non puoi strafeare se non indichi una direzione

        if (isBursting)
        {
            Debug.Log("Non puoi strafeare, uno strafe è già in corso!");
            return;
        }

        if (usedStrafes < dispatcher.GetAllFeatureByType<int>(FeatureType.maxStrafes).DefaultIfEmpty(maxStrafes).Sum())
        {

            isBursting = true;

            // effects
            burstOn.Invoke();
            burstDurationTimer = 0f;
            usedStrafes++;

            // Blocca la direzione iniziale dello strafe
            burstDirection = camera.transform.forward * moveDirection.y + camera.transform.right * moveDirection.x;
            burstDirection.y = 0;
            burstDirection.Normalize();
        }
        else
        {
            Debug.Log("Non puoi strafeare adesso, devi aspettare il cooldown!");
        }
    }

    public void Move(Vector2 direction)
    {
        moveDirection = new Vector3(direction.x, direction.y, 0);
    }


    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("terrain"))
        {
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

    public bool Aiming
    {
        get { return aiming; }
        set { aiming = value; }
    }
}
