using UnityEngine;

public class Rotator : MonoBehaviour
{
    [SerializeField] float x;
    [SerializeField] float y;
    [SerializeField] float z;// Start is called once before the first execution of Update after the MonoBehaviour is created

    // Update is called once per frame
    void Update()
    {
        this.transform.localRotation = Quaternion.Euler(x, y, z);

    }
}
