using UnityEngine;

public class testExplosion : MonoBehaviour
{
    public float explosionRadius = 5;
    public float exStrenght = 1000;
    public float verticalFactor = 6;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (Input.GetKeyDown(KeyCode.U))
        {
            Collider[] c = Physics.OverlapSphere(transform.position, explosionRadius);
            foreach (Collider item in c)
            {
                if (gameObject != item.gameObject)
                {
                    Rigidbody b = item.GetComponent<Rigidbody>();

                    if (b)
                    {
                        b.AddExplosionForce(exStrenght, transform.position, explosionRadius, verticalFactor, ForceMode.Impulse);
                    }
                }
            }

        }

    }
    void OnDrawGizmos()
    {
        Gizmos.color = Color.red;

        // Disegna una sfera al centro dell'oggetto con il raggio specificato
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }
}