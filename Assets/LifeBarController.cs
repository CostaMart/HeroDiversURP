using System.Linq;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.UI;

public class LifeBarController : MonoBehaviour
{
    [SerializeField] private Image image;
    [SerializeField] private Canvas canvas;
    [SerializeField] EffectsDispatcher dispatcher;

    void Update()
    {
        var health = dispatcher.GetFeatureByType<float>(FeatureType.health);
        var maxHealth = dispatcher.GetFeatureByType<float>(FeatureType.maxHealth);
        image.fillAmount = health.Sum() / maxHealth.Sum();

        image.color = Color.Lerp(Color.red, Color.green, image.fillAmount);
    }

}
