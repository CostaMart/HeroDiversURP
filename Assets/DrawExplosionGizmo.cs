using UnityEngine;

public class DrawExplosionGizmo : MonoBehaviour
{
    bool start = false;
    public float radius = 0f;
    public Vector3 position;

    public void OnDrawGizmos()
    {
        if (start)
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, radius);
        }
    }

    public void StartDrawing(float radius, Vector3 position)
    {
        transform.SetParent(null);
        this.position = position;
        this.radius = radius;
        start = true;
    }
}
