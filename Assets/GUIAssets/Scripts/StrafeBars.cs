using System.Linq;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static UnityEngine.InputSystem.InputAction;

public class StrafeBars : MonoBehaviour
{
    [SerializeField] Image img;
    [SerializeField] EffectsDispatcher dispatcher;
    [SerializeField] MovementLogic movementLogic;
    [SerializeField] GameObject indicator;
    [SerializeField] GameObject indicatorContainer;
    [SerializeField] Image burstIndicator;
    [SerializeField] TMPro.TMP_Text text;
    [SerializeField] Transform restPosition;
    [SerializeField] Transform fightModePosition;
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

        GameObject.Find("Player").GetComponent<PlayerInput>().actions["Aim"].performed += ctx => MoveTo(fightModePosition);
        GameObject.Find("Player").GetComponent<PlayerInput>().actions["Aim"].canceled += ctx => MoveTo(restPosition);


        original = img.color;


    }


    void MoveTo(Transform target)
    {
        transform.parent.GetComponent<RectTransform>().SetParent(target);
        transform.parent.GetComponent<RectTransform>().localPosition = Vector3.zero;
        transform.parent.GetComponent<RectTransform>().localRotation = Quaternion.identity;
        transform.GetComponent<RectTransform>().localPosition = Vector3.zero;
        transform.GetComponent<RectTransform>().localRotation = Quaternion.identity;
    }

    // Update is called once per frame
    void Update()
    {

        if (movementLogic.temperature > movementLogic.overHeatLimit / 2)
        {
            img.color = Color.red;
            text.text = "OVERHEAT";
            text.color = Color.red;
            burstIndicator.color = Color.red;
        }
        else
        {
            img.color = original;
            burstIndicator.color = original;
            text.text = "";
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
