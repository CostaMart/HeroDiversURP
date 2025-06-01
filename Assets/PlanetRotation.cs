using UnityEngine;

public class PlanetRotation : MonoBehaviour
{
    public Vector3 rotationSpeed = new Vector3(0f, 0.01f, 0f); // Gradi al secondo

    void Update()
    {
        transform.Rotate(rotationSpeed * Time.deltaTime);
    }
}