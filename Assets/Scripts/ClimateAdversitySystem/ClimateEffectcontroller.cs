using UnityEngine;
using UnityEngine.Rendering;

public class ClimateEffectController : MonoBehaviour
{
    Adversity adversity;
    private bool notSetupped = false;
    [SerializeField] private EffectsDispatcher playerEffectsDispatcher;
    [SerializeField] private GameObject player;
    private Volume volume;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        player = GameObject.Find("Player");
        playerEffectsDispatcher = player.GetComponent<EffectsDispatcher>();
        volume = this.gameObject.GetComponent<Volume>();

    }

    // Update is called once per frame
    void Update()
    {
        if (notSetupped)
        {
            adversity.SetupAdversity(this.gameObject, player, playerEffectsDispatcher, volume);
            notSetupped = false;
        }

        if (adversity != null)
        {
            adversity.DoEffect();
        }


    }

    public void InjectAdversity(Adversity newAdversity)
    {
        if (adversity != null)
        {
            adversity.Disable();
            DestroyImmediate(adversity);
        }

        adversity = newAdversity;
        notSetupped = true;
    }
}