using UnityEngine;

public class ShipApproachAndOrbit : MonoBehaviour
{
    public Transform targetPlanet;
    public float approachSpeed = 5f;
    public float approachDuration = 5f; // Secondi per l'approccio

    private float timer = 0f;
    private bool isOrbiting = false;
    private Vector3 orbitBasePosition;

    void Update()
    {
        timer += Time.deltaTime;

        if (timer < approachDuration)
        {
            // Calcola quanto "lento" dovrebbe essere il movimento man mano che si avvicina
            float t = timer / approachDuration; // da 0 a 1
            float currentSpeed = Mathf.Lerp(approachSpeed, 0f, t); // rallenta gradualmente

            // Direzione verso il pianeta
            Vector3 direction = (targetPlanet.position - transform.position).normalized;
            transform.position += direction * currentSpeed * Time.deltaTime;
        }
        else
        {
            if (!isOrbiting)
            {
                orbitBasePosition = transform.position;
                isOrbiting = true;
            }

            // Movimento oscillatorio (orbita)
            float offsetX = Mathf.Sin(Time.time * 0.7f) * 0.2f;
            float offsetY = Mathf.Cos(Time.time * 1.1f) * 0.15f;
            float offsetZ = Mathf.Sin(Time.time * 1.3f + Mathf.PI / 4) * 0.1f;

            transform.position = orbitBasePosition + new Vector3(offsetX, offsetY, offsetZ);
        }
    }
}
