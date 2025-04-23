using System;
using System.Linq;
using Unity.Cinemachine;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEngine.InputSystem.InputAction;
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
    [SerializeField] public int maxStrafes = 2;                // Numero massimo di strafe disponibili
    [SerializeField] public float strafeCooldown = 5f;         // Tempo per ricaricare uno strafe
    [SerializeField] public float burstDuration = 0.2f;        // Durata del burst (scatto rapido)
    [SerializeField] public float burstSpeedMultiplier = 2.0f; // Moltiplicatore velocità durante il burst
    [SerializeField] public float overHeatLimit = 100f; // Limite di surriscaldamento per lo strafe
    public float temperature;

    private Vector3 moveDirection = Vector3.zero;
    private bool isGrounded = true;
    private bool aiming = false;

    public int jumpsAvailable = 1;
    public int usedStrafes;
    public float strafeTimer;
    public bool isBursting = false;
    public float burstTimer = 0f;

    [Header("Visual effect")]
    [SerializeField] ParticleSystem thrusterEffect1;
    [SerializeField] ParticleSystem thrusterEffect2;
    [SerializeField] ParticleSystem thrusterEffect3;
    [SerializeField] ParticleSystem thrusterEffect4;


    void Awake()
    {
        // Collegamenti agli eventi di input
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

        // compute temperature as sum of all sources of heat
        temperature = dispatcher.GetAllFeatureByType<float>(FeatureType.heat).DefaultIfEmpty(10).Sum();


        HandleMovement();
        HandleStrafeCooldown();
        HandleBurstTimer();
    }

    private void HandleMovement()
    {
        Vector3 direction = camera.transform.forward * moveDirection.y + camera.transform.right * moveDirection.x;
        direction.y = 0;
        direction.Normalize();

        bool allowMovement = true;

        // Controllo inclinazione terreno
        if (Physics.Raycast(transform.position, Vector3.down, out RaycastHit hit, groundCheckDistance))
        {
            float angle = Vector3.Angle(hit.normal, Vector3.up);
            if (angle > maxSlopeAngle && hit.normal.y > 0)
            {
                allowMovement = false;
            }
        }

        Quaternion targetRotation = transform.rotation;

        // Rotazione verso la direzione
        if (aiming)
        {
            Vector3 aimForward = camera.transform.forward;
            aimForward.y = 0;
            if (aimForward != Vector3.zero)
                targetRotation = Quaternion.LookRotation(aimForward);
        }
        else if (direction != Vector3.zero)
        {
            targetRotation = Quaternion.LookRotation(direction);
        }

        float rotSpeed = dispatcher.GetAllFeatureByType<float>(
            aiming ? FeatureType.aimRotationSpeed : FeatureType.rotationSpeed
        ).DefaultIfEmpty(defaultRotationSpeed).Sum();

        if (allowMovement) // 🔥 NON controlliamo più isGrounded
        {
            float moveSpeed = dispatcher.GetAllFeatureByType<float>(FeatureType.speed).DefaultIfEmpty(defaultSpeed).Sum();
            float speedMultiplier = 1f;

            if (isBursting)
            {
                moveSpeed = dispatcher.GetAllFeatureByType<float>(FeatureType.strafePower).Sum();
                speedMultiplier = 10f;
                thrusterEffect1.Play();
                thrusterEffect2.Play();
                thrusterEffect3.Play();
                thrusterEffect4.Play();

            }
            else
            {
                thrusterEffect1.Stop();
                thrusterEffect2.Stop();
                thrusterEffect3.Stop();
                thrusterEffect4.Stop();
            }

            // se non specificato multiplicatore nelle faturre ➡ defaultBurstSpeedMultiplier 

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

        rb.MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, rotSpeed * Time.fixedDeltaTime));

    }

    private void HandleStrafeCooldown()
    {
        // recupera il valore di cooldown dai dati
        var strafeCooldownFeat = dispatcher.GetAllFeatureByType<float>(FeatureType.strafeCooldown)
            .DefaultIfEmpty(strafeCooldown).Sum();

        // Se non siamo al massimo degli strafes, facciamo scorrere il tempo
        if (usedStrafes > 0)
        {
            strafeTimer += Time.fixedDeltaTime;
            if (strafeTimer >= strafeCooldownFeat)
            {
                usedStrafes--;
                strafeTimer = 0f;
                Debug.Log("Available strafes: " + (maxStrafes - usedStrafes));
            }
        }
    }

    private void HandleBurstTimer()
    {
        // recupera durata burst dai dati
        var burstDurationFeat = dispatcher.GetAllFeatureByType<float>(FeatureType.strafeBurstDuration)
            .DefaultIfEmpty(burstDuration).Sum();

        // Gestiamo la durata del burst
        if (isBursting)
        {
            burstTimer += Time.fixedDeltaTime;
            if (burstTimer >= burstDurationFeat)
            {
                isBursting = false;
                burstTimer = 0f;
            }

            isGrounded = false;
        }
    }

    public void TryStrafe()
    {
        if (temperature > overHeatLimit / 2)
        {
            Debug.Log("Non puoi strafeare, armatura surriscaldata!");
            return;
        }

        if (usedStrafes < dispatcher.GetAllFeatureByType<int>(FeatureType.maxStrafes).DefaultIfEmpty(maxStrafes).Sum())
        {
            isBursting = true;
            burstTimer = 0f;
            usedStrafes++;
            strafeTimer = 0f;
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

        jumpsAvailable--;
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

    public bool Aiming
    {
        get { return aiming; }
        set { aiming = value; }
    }
}


