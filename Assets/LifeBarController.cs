using System.Linq;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.UI;

public class LifeBarController : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private Canvas canvas;
    [SerializeField] EffectsDispatcher dispatcher;

    void OnEnable()
    {
        canvas.worldCamera = GameObject.Find("GUICam").GetComponent<Camera>();
    }
    void Update()
    {
        image.fillAmount = dispatcher.GetAllFeatureByType<float>(FeatureType.health).Sum() /
            dispatcher.GetAllFeatureByType<float>(FeatureType.maxHealth).Sum();

        image.color = Color.Lerp(Color.red, Color.green, image.fillAmount);
    }

}
