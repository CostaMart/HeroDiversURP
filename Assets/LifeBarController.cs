using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class LifeBarController : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private Canvas canvas;
    [SerializeField] EffectsDispatcher dispatcher;

    void Update()
    {
        image.fillAmount = dispatcher.GetFeatureByType<float>(FeatureType.health).Sum() /
            dispatcher.GetFeatureByType<float>(FeatureType.maxHealth).Sum();

        image.color = Color.Lerp(Color.red, Color.green, image.fillAmount);
    }

}
