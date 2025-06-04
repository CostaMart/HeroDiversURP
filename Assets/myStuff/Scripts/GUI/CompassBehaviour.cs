using UnityEngine;

public class CompassBehaviour : MonoBehaviour
{
    [SerializeField] Transform compassRef;       // il riferimento al "nord"
    [SerializeField] Transform playerTransform;  // la camera o il player

    void Update()
    {
        // Calcola la direzione nord proiettata sull'asse orizzontale
        Vector3 northDirection = Vector3.ProjectOnPlane(compassRef.forward, Vector3.up).normalized;
        Vector3 playerForward = Vector3.ProjectOnPlane(playerTransform.forward, Vector3.up).normalized;

        // Calcola l'angolo tra il forward del player e il nord
        float angle = Vector3.SignedAngle(playerForward, northDirection, Vector3.up);

        transform.localEulerAngles = new Vector3(0, 0, -angle);
    }
}
