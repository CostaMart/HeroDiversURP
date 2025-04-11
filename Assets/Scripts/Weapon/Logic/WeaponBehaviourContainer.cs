using UnityEngine;

public class WeaponBehaviourContainer : MonoBehaviour
{

    public AbstractWeaponLogic activeLogic;


    public void Update()
    {
        activeLogic.Updating();
    }

    void OnEnable()
    {
        activeLogic.Enable();
    }

    public void ODisable()
    {
        activeLogic.Disable();
    }

}