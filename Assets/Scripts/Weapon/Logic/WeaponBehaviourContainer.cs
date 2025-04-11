using UnityEngine;

public class WeaponBehaviourContainer : MonoBehaviour
{

    public AbstractWeaponLogic activeLogic;


    public void Update()
    {
        if (activeLogic == null)
            return;
        activeLogic.Updating();
    }

    void OnEnable()
    {

        if (activeLogic == null)
            return;
        activeLogic.Enable();
    }

    public void ODisable()
    {
        if (activeLogic == null)
            return;
        activeLogic.Disable();
    }

}