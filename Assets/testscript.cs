using UnityEngine;

public class testscript : MonoBehaviour
{
    [SerializeField] private GameObject prefab;
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            var container = other.gameObject.transform.Find("Container");
            container.transform.GetChild(0).SetParent(null);
            var obj = Instantiate(prefab, container.position, Quaternion.identity);
            obj.transform.SetParent(container);


        }

    }
}
