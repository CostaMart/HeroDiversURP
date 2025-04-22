using System.Linq;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.UI;

public class StrafeBars : MonoBehaviour
{
    [SerializeField] Image img;
    [SerializeField] EffectsDispatcher dispatcher;
    [SerializeField] MovementLogic movementLogic;
    [SerializeField] GameObject indicator;
    [SerializeField] GameObject indicatorContainer;
    float cooldown;
    int maxStrafes;
    int indicatorIndex;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var strafes = dispatcher.GetAllFeatureByType<int>(FeatureType.maxStrafes).
                DefaultIfEmpty(movementLogic.maxStrafes).Sum();

        for (int i = 0; i <= strafes; i++)
        {
            Instantiate(indicator, indicatorContainer.transform);
        }


    }

    // Update is called once per frame
    void Update()
    {
        maxStrafes = dispatcher.GetAllFeatureByType<int>(FeatureType.maxStrafes).
                DefaultIfEmpty(movementLogic.maxStrafes).Sum();

        if (movementLogic.usedStrafes > 0)
        {
            cooldown = dispatcher.GetAllFeatureByType<float>(FeatureType.strafeCooldown).
            DefaultIfEmpty(movementLogic.strafeCooldown).Sum();

            img.fillAmount = movementLogic.strafeTimer / cooldown;
            return;
        }

        indicatorIndex = 0;
        foreach (Transform indicator in indicatorContainer.transform)
        {
            if (indicatorIndex <= maxStrafes - movementLogic.usedStrafes)
                indicator.gameObject.SetActive(true);
            else
                indicator.gameObject.SetActive(false);

            indicatorIndex++;
        }

        img.fillAmount = 1;
    }
}
