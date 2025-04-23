using UnityEngine;

public class CompassBehaviour : MonoBehaviour
{
    GameObject terrain;
    void Start()
    {
        terrain = GameObject.Find("Terrain");

    }

    // Update is called once per frame
    void Update()
    {
        Vector3 direction = terrain.transform.position - transform.position;
        direction.y = 0; // ignora l'altezza per la rotazione orizzontale

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction, Vector3.up);
            transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, targetRotation.eulerAngles.y, transform.rotation.eulerAngles.z);
        }
    }
}
