using Unity.Cinemachine;
using UnityEngine;

public class CinemachineImpluseController : MonoBehaviour
{
    [SerializeField] EventChannels eventChannels;
    [SerializeField] CinemachineImpulseSource impulseSource;
    [SerializeField] string startEvent = "VFXEvent";

    void Start()
    {
        eventChannels.Subscribe(startEvent, GenerateImpulse);
    }

    public void GenerateImpulse()
    {
        impulseSource.GenerateImpulse();
    }
}
