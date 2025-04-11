using Unity.Cinemachine;
using UnityEditor.EditorTools;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.InputSystem;

public class WeapnYAimControl : MonoBehaviour
{

    private float rotationX = 0f;
    private float rotationY = 0f;

    private Vector2 delta;

    public Transform parentTransform;

    [SerializeField] private ControlEventManager controlEventManager;
    [SerializeField] private CameraSettings cameraSettings;
    private bool aiming = false;


    void Awake()
    {
        controlEventManager.AddMouseControlListener(OnMouseMove);
        controlEventManager.AddListenerAiming(OnAiming);
    }

    /// <summary>
    /// By doing this operation in lateupdate i let the followed object move first!
    /// otherwise there is a strange flickering effect
    /// </summary>
    void LateUpdate()
    {

        // Gestisci la rotazione verticale (X) con il movimento del mouse
        rotationX -= delta.y * cameraSettings.Sensitivity; // Ruota sull'asse X (verticale)
        rotationX = Mathf.Clamp(rotationX, cameraSettings.LowerBoundYrotation, cameraSettings.UpperBoundYrotation);

        // Gestisci la rotazione orizzontale (Y) prendendo la rotazione del parent
        rotationY = parentTransform.eulerAngles.y;

        var rotationz = parentTransform.eulerAngles.z;

        // Applica la rotazione finale
        transform.rotation = Quaternion.Euler(rotationX, rotationY, rotationz);
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



