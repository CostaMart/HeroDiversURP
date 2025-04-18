using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using static ItemManager;

public class ShopItemPanel : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public EconomyManager economyManager;
    private Image image;
    private Color originalColor;
    public Color highlightColor = Color.yellow;
    public Modifier item;
    private bool bought = false;
    public EffectsDispatcher dispatcher;
    public Seller seller;

    private void Awake()
    {
        image = GetComponent<Image>();
        if (image != null)
        {
            originalColor = image.color;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        Debug.Log("Pointer Entered");
        if (image != null)
            image.color = highlightColor;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (image != null)
            image.color = originalColor;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (bought)
            return;

        var transactionSuccess = economyManager.TryBuyItem(item.inGamePrice, false);

        if (!transactionSuccess)
        {
            Debug.Log("Not enough money");
            return;
        }

        dispatcher.ItemDispatch(item);
        seller.Sold(item);
        this.gameObject.SetActive(false);
    }
}
