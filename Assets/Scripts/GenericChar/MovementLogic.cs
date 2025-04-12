
using System.ComponentModel.Design.Serialization;
using System.Linq;
using System.Numerics;
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

    // movement event manager 
    [SerializeField] private EffectsDispatcher dispatcher;
    [SerializeField] private ControlEventManager controlEventManager;
    [SerializeField] private float maxSlopeAngle = 45f;
    private Vector3 moveDirection = Vector3.zero;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        // connect to movement event manager
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



    // Dichiara una distanza per il controllo del terreno (valore da settare in base alle esigenze)
    [SerializeField] public float groundCheckDistance = 1.0f;
    private void FixedUpdate()
    {

        Vector3 direction = Vector3.zero;

        if (rb.isKinematic)
            return;

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

        Quaternion targetRotation = Quaternion.LookRotation(transform.forward);

        if (aiming)
        {
            Vector3 aimForward = camera.transform.forward;
            aimForward.y = 0;
            targetRotation = Quaternion.LookRotation(aimForward);
        }

        float rotSpeed = dispatcher.GetAllFeatureByType<float>(
            aiming ? FeatureType.aimRotationSpeed : FeatureType.rotationSpeed
        ).Sum();


        if (direction != Vector3.zero)
        {
            if (!aiming)
            {
                targetRotation = Quaternion.LookRotation(direction);

            }
            if (allowMovement)
            {

                Vector3 linear = direction.normalized * dispatcher.GetAllFeatureByType<float>(FeatureType.speed).Sum();
                linear.y = rb.linearVelocity.y;
                rb.linearVelocity = linear;
            }

        }
        else
        {
            // Solo gravità
            rb.linearVelocity = new Vector3(0, rb.linearVelocity.y, 0);
        }

        rb.MoveRotation(Quaternion.Slerp(transform.rotation, targetRotation, rotSpeed));
    }


    void OnTriggerExit(Collider other)
    {
        anim.SetBool("jump", true);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("terrain"))
            jumpsAvailable = dispatcher.GetAllFeatureByType<int>(FeatureType.maxJumps).Sum();
    }

    public void Jump()
    {
        Vector3 direction = Vector3.zero;

        direction += camera.transform.forward * moveDirection.y;
        direction += camera.transform.right * moveDirection.x;

        if (jumpsAvailable <= 0) return;


        rb.linearVelocity = new Vector3(
            rb.linearVelocity.x,
            dispatcher.GetAllFeatureByType<float>(FeatureType.jumpSpeedy).Sum(),
            rb.linearVelocity.z);
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
