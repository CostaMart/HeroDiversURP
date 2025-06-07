using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static ItemManager;

public class ShopItemPanel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
    public EconomyManager economyManager;
    private Image image;
    private Color originalColor;
    public Color highlightColor = Color.yellow;
    public EnrichedModifier item;
    private bool bought = false;
    public EffectsDispatcher dispatcher;
    public Seller seller;

    private bool isPressing = false;
    private float pressDuration = 0f;
    public float requiredHoldTime = 2f; // seconds

    public Image holdProgressImage;
    public GameObject holdProgressPanel;

    private void Awake()
    {
        image = GetComponent<Image>();

        if (image != null)
        {
            originalColor = image.color;
        }

        if (holdProgressImage != null)
        {
            holdProgressImage.fillAmount = 0f;
            holdProgressImage.gameObject.SetActive(false);
        }
    }

    public void SetupProgressPanel(GameObject panel)
    {
        holdProgressPanel = panel;
        holdProgressImage = panel.GetComponentInChildren<Image>();

        if (holdProgressImage != null)
        {
            holdProgressImage.fillAmount = 0f;
            holdProgressPanel.SetActive(false);
        }

    }

    private void Update()
    {
        if (isPressing && !bought)
        {
            pressDuration += Time.deltaTime;

            if (holdProgressImage != null)
            {
                holdProgressImage.fillAmount = pressDuration / requiredHoldTime;
            }

            if (pressDuration >= requiredHoldTime)
            {
                TryPurchase();
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (image != null)
            image.color = highlightColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (image != null)
            image.color = originalColor;
        ResetPressing();
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (!bought)
        {
            isPressing = true;
            pressDuration = 0f;

            holdProgressPanel.SetActive(true);
            holdProgressImage.fillAmount = 0f;
            holdProgressPanel.GetComponent<RectTransform>().position = this.transform.position;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        ResetPressing();
    }

    private void TryPurchase()
    {
        isPressing = false;
        pressDuration = 0f;

        if (holdProgressImage != null)
        {
            holdProgressImage.fillAmount = 0f;
            holdProgressImage.gameObject.SetActive(false);
        }

        var transactionSuccess = economyManager.TryBuyItem(item.inGamePrice, false);

        if (!transactionSuccess)
        {
            Debug.Log("Not enough money");
            return;
        }

        dispatcher.modifierDispatch(item.modifier);
        seller.Sold(item);
        bought = true;
        Destroy(gameObject);
    }

    private void ResetPressing()
    {
        isPressing = false;
        pressDuration = 0f;

        if (holdProgressImage != null)
        {
            holdProgressImage.fillAmount = 0f;
            holdProgressPanel.SetActive(false);
        }
    }
}
