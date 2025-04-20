using System.Linq;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.UI;

public class StrafeBars : MonoBehaviour
{
    [SerializeField] Image img;
    [SerializeField] EffectsDispatcher dispatcher;
    [SerializeField] MovementLogic movementLogic;
    float cooldown;
    int maxStrafes;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

        if (movementLogic.usedStrafes > 0)
        {
            cooldown = dispatcher.GetAllFeatureByType<float>(FeatureType.strafeCooldown).
            DefaultIfEmpty(movementLogic.strafeCooldown).Sum();

            maxStrafes = dispatcher.GetAllFeatureByType<int>(FeatureType.maxStrafes)
            .DefaultIfEmpty(movementLogic.maxStrafes).Sum();

            img.fillAmount = movementLogic.strafeTimer / maxStrafes;
            return;
        }

        img.fillAmount = 1;
    }
}
