using System.Collections;
using TMPro;
using UnityEngine;

public class UnlockItemElem : MonoBehaviour
{
    public int id;
    public float price;
    public AudioSource source;
    public AudioClip bought;
    public AudioClip notEnoughCredits;
    public GameObject result;

    public void OnPressedUnlock()
    {
        var credits = 0;

        if (price > credits)
        {
            ItemManager.UnlockItem(id);
            PlayerPrefs.SetFloat("credits", credits - price);
            source.PlayOneShot(bought);
            Destroy(this.gameObject);
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
