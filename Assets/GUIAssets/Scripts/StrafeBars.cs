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
    [SerializeField] Image burstIndicator;
    [SerializeField] TMPro.TMP_Text text;
    Color original;
    float cooldown;
    int maxStrafes;
    int indicatorIndex;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        var strafes = dispatcher.GetAllFeatureByType<int>(FeatureType.maxStrafes).
                DefaultIfEmpty(movementLogic.maxStrafes).Sum();

        for (int i = 0; i <= strafes * 2; i++)
        {
            Instantiate(indicator, indicatorContainer.transform);
        }


        original = img.color;


    }

    // Update is called once per frame
    void Update()
    {
        if (movementLogic.temperature > movementLogic.overHeatLimit / 2)
        {
            img.color = Color.red;
            text.text = "Overtheat risk HIGH: engines disabled";
            text.color = Color.red;
            burstIndicator.color = Color.red;
        }
        else
        {
            img.color = original;
            burstIndicator.color = original;
            text.text = "engines enabled";
            text.color = Color.cyan;
        }

        maxStrafes = dispatcher.GetAllFeatureByType<int>(FeatureType.maxStrafes).
                DefaultIfEmpty(movementLogic.maxStrafes).Sum();


        if (maxStrafes > indicatorContainer.transform.childCount)
        {
            for (int i = 0; i < (maxStrafes - indicatorContainer.transform.childCount) * 2; i++)
            {
                Instantiate(indicator, indicatorContainer.transform);
            }
        }

        indicatorIndex = 0;
        foreach (Transform indicator in indicatorContainer.transform)
        {
            if (indicatorIndex <= maxStrafes - movementLogic.usedStrafes - 1)
                indicator.gameObject.SetActive(true);
            else
                indicator.gameObject.SetActive(false);

            indicatorIndex++;
        }

        if (movementLogic.usedStrafes > 0)
        {
            cooldown = dispatcher.GetAllFeatureByType<float>(FeatureType.strafeCooldown).
            DefaultIfEmpty(movementLogic.strafeCooldown).Sum();

            img.fillAmount = movementLogic.strafeTimer / cooldown;
            return;
        }


        img.fillAmount = 1;
    }
}
