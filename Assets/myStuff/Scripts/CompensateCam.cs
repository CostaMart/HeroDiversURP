using Unity.Cinemachine;
using UnityEngine;

public class ImpulseCompensator : MonoBehaviour
{
    void LateUpdate()
    {
        Vector3 impulsePos;
        Quaternion impulseRot;

        CinemachineImpulseManager.Instance.GetImpulseAt(
            transform.position,   // world position
            false,                // is 2D?
            1,                    // channel mask (default channel)
            out impulsePos,
            out impulseRot
        );

        // Compensa l'impulso sulla posizione
        transform.position -= impulsePos;
    }
}