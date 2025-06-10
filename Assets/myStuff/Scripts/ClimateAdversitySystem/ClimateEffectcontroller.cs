using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class ClimateEffectController : MonoBehaviour
{
    static ClimateEffectController instance;
    public static ClimateEffectController Instance
    {
        get
        {
            return instance;
        }
    }

    Adversity adversity;
    public bool active = false;
    private float duration;
    [SerializeField] private EffectsDispatcher playerEffectsDispatcher;
    [SerializeField] private GameObject player;
    private Volume volume;
    private Dictionary<int, string> data = new();

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        instance = this;
        player = GameObject.Find("Player");
        playerEffectsDispatcher = ItemManager.playerDispatcher;
        volume = this.gameObject.GetComponent<Volume>();
    }

    float timer = 0f;
    // Update is called once per frame
    void Update()
    {

        if (active && adversity != null)
        {
            adversity.DoEffect();

            if (timer >= duration)
            {
                adversity.Disable();
                adversity = null;
                timer = 0f;
                active = false;
                Debug.Log("Adversity ended");
                return;
            }

            timer += Time.deltaTime;
        }

    }

    public void InjectAdversity(Adversity newAdversity, float minduration, float maxduration)
    {
        duration = Random.Range(minduration, maxduration);
        MessageHelper.Instance.PostAlarm($"Adversity: {newAdversity.name}, duration: {duration}", 2f);

        if (adversity != null)
        {
            adversity.Disable();
        }

        adversity = newAdversity;
        adversity.SetupAdversity(this.gameObject, player, playerEffectsDispatcher, volume);
        active = true;
    }

}