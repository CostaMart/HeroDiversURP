using System.Collections;
using UnityEngine;

public class MoveUpAnimation : SpawnAnimation
{
    [Tooltip("The vertical offset from terrain height to start the animation.")]
    public float yOffset = -1f;

    float terrainHeight;

    public GameObject toDisable;

    void OnEnable()
    {
        terrainHeight = Terrain.activeTerrain != null ? Terrain.activeTerrain.SampleHeight(transform.position) : 0f;
        toDisable.SetActive(false); // Disable the object at the start of the animation
    }

    public override void Play()
    {
        StartCoroutine(PlayCoroutine());
    }

    IEnumerator PlayCoroutine()
    {
        Vector3 end = transform.position;
        Vector3 start = new(transform.position.x, terrainHeight + yOffset, transform.position.z);

        transform.position = start;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            transform.position = Vector3.Lerp(start, end, t);
            yield return null;
        }

        transform.position = end;
        toDisable.SetActive(true); // Enable the object at the end of the animation
    }
}
