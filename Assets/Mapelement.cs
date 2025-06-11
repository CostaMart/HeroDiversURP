using UnityEngine;

public class MapElement : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        MapManager.Instance.mapElements.Add(gameObject);
        this.gameObject.SetActive(false);
    }

    // Update is called once per frame
    public bool rotate = true;
    void Update()
    {
        if (rotate)
            this.transform.rotation = MapManager.Instance.main.transform.rotation;
    }
}
