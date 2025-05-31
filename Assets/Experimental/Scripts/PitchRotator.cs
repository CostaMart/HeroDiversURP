using UnityEngine;

public class PitchRotator : MonoBehaviour
{
    /// <summary>
    /// Ruota lungo l'asse X verso una direzione, limitando il pitch massimo.
    /// </summary>
    /// <param name="targetDirection">La direzione verso cui ruotare.</param>
    /// <param name="maxPitchAngle">Pitch massimo in gradi.</param>
    /// <param name="angularSpeed">Velocità massima di rotazione in gradi/sec.</param>
    public void RotatePitch(Vector3 targetDirection, float maxPitchAngle, float angularSpeed)
{
    if (targetDirection == Vector3.zero)
        return;

    // Calcola l'angolo di pitch relativo alla posizione attuale
    Vector3 localDirection = transform.parent.InverseTransformDirection(targetDirection.normalized);
    float pitchAngle = Mathf.Atan2(localDirection.y, localDirection.z) * Mathf.Rad2Deg;
    pitchAngle = Mathf.Clamp(pitchAngle, -maxPitchAngle, maxPitchAngle);

    // Crea rotazione target
    Quaternion targetRotation = Quaternion.Euler(-pitchAngle, 0f, 0f);

    // Applica rotazione graduale
    transform.localRotation = Quaternion.RotateTowards(
        transform.localRotation,
        targetRotation,
        angularSpeed * Time.deltaTime
    );
}
}
