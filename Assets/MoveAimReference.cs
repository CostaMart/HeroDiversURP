using UnityEngine;
using UnityEngine.UIElements;

public class MoveAimReference : MonoBehaviour
{
    [SerializeField] private LayerMask layermask;
    [SerializeField] private float maxDistance = 100f;

    void Update()
    {
        Ray ray = Camera.main.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0f));
        if (Physics.Raycast(ray, out RaycastHit hit, maxDistance, layermask))
        {
            transform.position = hit.point;
        }
    }
}
