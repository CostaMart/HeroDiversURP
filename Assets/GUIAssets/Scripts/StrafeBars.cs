using System.Linq;
using TMPro;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using static UnityEngine.InputSystem.InputAction;

public class StrafeBars : MonoBehaviour
{
    [SerializeField] bool toggleMode = false; // 🔁 Toggle ON/OFF

    [SerializeField] Image img;
    [SerializeField] EffectsDispatcher dispatcher;
    [SerializeField] MovementLogic movementLogic;
    [SerializeField] GameObject indicator;
    [SerializeField] GameObject indicatorContainer;
    [SerializeField] Image burstIndicator;
    [SerializeField] TMPro.TMP_Text text;
    [SerializeField] Transform restPosition;
    [SerializeField] Transform fightModePosition;
    [SerializeField] TMP_Text magCount;

    Color original;
    float cooldown;
    int maxStrafes;
    int indicatorIndex;
    bool isInFightMode = false; // 🟢 Tracciamento stato in toggle
    RectTransform rectParent;
    RectTransform thisRect;
    Vector3 originalScale;
    PlayerInput playerInput = null;

    void Start()
    {
        var strafes = dispatcher.GetAllFeatureByType<int>(FeatureType.maxStrafes)
                .DefaultIfEmpty(movementLogic.maxStrafes).Sum();

        for (int i = 0; i <= strafes * 2; i++)
        {
            Instantiate(indicator, indicatorContainer.transform);
        }


        text.text = "OVERHEAT";
        original = img.color;
        originalScale = thisRect.localScale;
    }

    void OnEnable()
    {

        rectParent = this.transform.parent.GetComponent<RectTransform>();
        thisRect = this.transform.GetComponent<RectTransform>();
        playerInput = GameObject.Find("Player").GetComponent<PlayerInput>();

        if (toggleMode)
        {
            // Toggle con un singolo evento
            playerInput.actions["Aim"].performed += ctx => TogglePosition();
        }
        else
        {
            // Comportamento originale
            playerInput.actions["Aim"].performed += ctx => MoveTo(fightModePosition);
            playerInput.actions["Aim"].canceled += ctx => MoveTo(restPosition);
        }
    }

    void OnDisable()
    {
        if (toggleMode)
        {
            playerInput.actions["Aim"].performed -= ctx => TogglePosition();
        }
        else
        {
            playerInput.actions["Aim"].performed -= ctx => MoveTo(fightModePosition);
            playerInput.actions["Aim"].canceled -= ctx => MoveTo(restPosition);
        }
    }

    void TogglePosition()
    {
        isInFightMode = !isInFightMode;
        MoveTo(isInFightMode ? fightModePosition : restPosition);
    }

    void MoveTo(Transform target)
    {
        rectParent.SetParent(target);
        rectParent.localPosition = Vector3.zero;
        rectParent.localRotation = Quaternion.identity;

        var rectSelf = transform.GetComponent<RectTransform>();
        rectSelf.localPosition = Vector3.zero;
        rectSelf.localRotation = Quaternion.identity;

        // Cambia la scala del rectParent in base alla posizione
        transform.GetComponent<RectTransform>().localScale = (target == restPosition)
            ? originalScale * 2
            : originalScale * 2;  // Più piccola in fight
    }

    void Update()
    {
        magCount.text = $"{dispatcher.GetAllFeatureByType<int>(FeatureType.magCount).DefaultIfEmpty(0).Sum()}";

        img.color = original;
        burstIndicator.color = original;
        text.gameObject.SetActive(false);


        // se siamo surriscaldati, mostra il cooldown rosso
        if (movementLogic.temperature >= movementLogic.overHeatLimit)
        {
            img.color = Color.red;
            text.color = Color.red;
            text.gameObject.SetActive(true);
            burstIndicator.color = Color.red;
        }


        maxStrafes = dispatcher.GetAllFeatureByType<int>(FeatureType.maxStrafes)
            .DefaultIfEmpty(movementLogic.maxStrafes).Sum();

        if (maxStrafes > indicatorContainer.transform.childCount)
        {
            for (int i = 0; i < (maxStrafes - indicatorContainer.transform.childCount) * 2; i++)
            {
                var bar = Instantiate(indicator, indicatorContainer.transform);
            }
        }

        indicatorIndex = 0;
        foreach (Transform indicator in indicatorContainer.transform)
        {
            indicator.gameObject.SetActive(indicatorIndex <= maxStrafes - movementLogic.usedStrafes - 1);
            indicatorIndex++;
        }

        if (movementLogic.usedStrafes > 0)
        {
            cooldown = dispatcher.GetAllFeatureByType<float>(FeatureType.strafeCooldown)
                .DefaultIfEmpty(movementLogic.strafeCooldown).Sum();

            img.fillAmount = movementLogic.strafeTimer / cooldown;
            return;
        }

        img.fillAmount = 1;
    }
}