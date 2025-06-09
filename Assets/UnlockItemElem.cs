using System.Collections;
using TMPro;
using UnityEngine;

public class UnlockItemElem : MonoBehaviour
{
    public int id;
    public int price;
    public AudioSource source;
    public AudioClip bought;
    public AudioClip notEnoughCredits;
    public GameObject result;
    public TMP_Text creditsText;

    public void OnPressedUnlock()
    {
        var credits = PlayerPrefs.GetInt("astroCredits", 0);
        Debug.Log($"price: {price}");

        if (credits > price)
        {
            ItemManager.UnlockItem(id);
            PlayerPrefs.SetInt("astroCredits", credits - price);
            creditsText.text = PlayerPrefs.GetInt("astroCredits", 0).ToString();
            source.PlayOneShot(bought);
            this.transform.parent.gameObject.SetActive(false);
        }
        else
        {

            source.PlayOneShot(notEnoughCredits);
            result.SetActive(true);
            StartCoroutine(RemoveMessage());
        }


        Debug.Log($"hello {id}");
    }

    public IEnumerator RemoveMessage()
    {
        yield return new WaitForSeconds(1f);
        result.SetActive(false);
    }



}
