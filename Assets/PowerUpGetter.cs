using System.Linq;
using UnityEngine;

public class PowerUpGetter : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public EffectsDispatcher dispatcher;
    public GameObject effects;
    void Start()
    {
        if (SunController.Instance.night)
        {
            dispatcher.modifierDispatch(SunController.Instance.powerUP);
            effects.SetActive(true);
            return;
        }

        SunController.Instance.SendPowerUp += InstallPowerUP;
    }

    public void InstallPowerUP(Modifier mod)
    {
        dispatcher.modifierDispatch(mod);
        if (effects != null)
            effects.SetActive(true);
    }

}