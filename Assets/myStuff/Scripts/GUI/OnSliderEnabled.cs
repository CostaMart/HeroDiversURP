using UnityEngine;
using UnityEngine.UI;

public class OnSliderEnabled : MonoBehaviour
{
    [SerializeField] string dataToUse;
    [SerializeField] float defaultValue = 0.5f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        this.GetComponent<Slider>().value = PlayerPrefs.GetFloat(dataToUse, 0.5f);
    }

}
