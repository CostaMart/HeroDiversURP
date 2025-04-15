using System.Linq;
using TMPro;
using UnityEngine;

public class Money : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField] TMP_Text text;
    [SerializeField] EffectsDispatcher dispatcher;

    void Update()
    {
        text.text = dispatcher.GetAllFeatureByType<int>(FeatureType.money).Aggregate((a, b) => a + b).ToString();
    }
}
