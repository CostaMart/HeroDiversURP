using UnityEngine;

public class NoVisibleMagBehaviour : MonoBehaviour
{
    [SerializeField] private float lifetime = 10f;

    void OnEnable()
    {
        Invoke(nameof(DisableMag), lifetime);

    }

    private void DisableMag()
    {
        Debug.Log("lifetime expired, disabling mag");
        gameObject.SetActive(false);
    }

}