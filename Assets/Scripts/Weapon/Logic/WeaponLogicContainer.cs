using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Weapon.State;

// manages weapon behaviours and provides them with access to most of necessary components
public class WeaponLogicContainer : MonoBehaviour
{
    [SerializeField] public MouseRotateCamera cameraController;
    public bool isPrimary;
    public EffectsDispatcher dispatcher;
    public WeaponStats weaponStats;
    public List<AbstractWeaponLogic> logList = new List<AbstractWeaponLogic>();
    public AbstractWeaponLogic activeLogic;
    public int lastActiveLogicId;
    public PlayerInput inputSys;

    // reference to the bullet pool gameobject
    public GameObject pool;

    // anchor for the bullets
    public Transform muzzle;

    // line renderer for the aim laser
    private LineRenderer lineRenderer;

    public int currentAmmo = 0;

    public void Update()
    {
        if (activeLogic == null)
            return;

        CheckForLogicReasignment();
        activeLogic.UpdateWeaponBehaviour();
    }

    public void LateUpdate()
    {
        DrawAimLaser();
        activeLogic.LateUpdateWeaponBehaviour();
    }

    void OnEnable()
    {
        //activate 
        dispatcher.SetActiveStatusClass(weaponStats.ID, true);

        // assign reference to this
        activeLogic.weaponContainer = this;
        activeLogic.EnableWeaponBehaviour();

        lineRenderer = muzzle.GetComponent<LineRenderer>();
    }

    public void OnDisable()
    {
        // deactivate
        dispatcher.SetActiveStatusClass(weaponStats.ID, false);

        if (activeLogic == null)
            return;

        activeLogic.DisableWeaponBehaviour();
    }

    /// <summary>
    /// this method is called to reassign new logic corresponding to the current active logic in the weapon stats
    /// </summary>
    public void CheckForLogicReasignment()
    {
        var currentActive = dispatcher.GetMostRecentFeatureValue<int>(FeatureType.activeLogicIndex);

        if (lastActiveLogicId != currentActive)
        {
            activeLogic.DisableWeaponBehaviour();

            // update current behaviour to match the value of the corresponding feature    
            activeLogic = logList[currentActive];
            lastActiveLogicId = currentActive;
            activeLogic.weaponContainer = this;
            activeLogic.EnableWeaponBehaviour();
        }
    }

    public int GetCurrentAmmoCount()
    {
        return currentAmmo;
    }

    // just draws the aim laser
    public void DrawAimLaser()
    {
        lineRenderer.SetPosition(0, muzzle.transform.position); // origin
        lineRenderer.SetPosition(1, muzzle.transform.position + muzzle.transform.forward * 100f); // direction and length
        lineRenderer.startWidth = 0.01f; // width
        lineRenderer.endWidth = 0.01f;   // width
        lineRenderer.startColor = Color.red; // color
        lineRenderer.endColor = Color.red;   // color
    }

}