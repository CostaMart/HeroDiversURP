using UnityEngine;

public class OverlayMessageController : MonoBehaviour
{
    [SerializeField] private Canvas messageCanvas;
    public void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("NPC"))
            messageCanvas.gameObject.SetActive(true);
    }
    public void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("NPC"))
            messageCanvas.gameObject.SetActive(false);
    }
}
