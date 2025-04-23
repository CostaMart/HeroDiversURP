using UnityEngine;

public class RotateToAngle : MonoBehaviour
{
    public enum RotationAxis { X, Y, Z }

    [Header("Configurazione Rotazione")]
    public RotationAxis axis = RotationAxis.Y;
    public float targetAngle = 180f;
    public float durationInSeconds = 900f; // default 15 min

    private Quaternion initialRotation;
    private Quaternion finalRotation;
    private float elapsedTime = 0f;
    private bool isRotating = true;

    void Start()
    {
        initialRotation = transform.rotation;
        Vector3 finalEuler = transform.eulerAngles;

        switch (axis)
        {
            case RotationAxis.X:
                finalEuler.x = targetAngle;
                break;
            case RotationAxis.Y:
                finalEuler.y = targetAngle;
                break;
            case RotationAxis.Z:
                finalEuler.z = targetAngle;
                break;
        }

        finalRotation = Quaternion.Euler(finalEuler);
    }

    void Update()
    {
        if (isRotating)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / durationInSeconds);
            transform.rotation = Quaternion.Slerp(initialRotation, finalRotation, t);

            if (t >= 1f)
            {
                isRotating = false;
            }
        }
    }
}
