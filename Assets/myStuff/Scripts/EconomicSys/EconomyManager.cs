using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ItemManager;

public class EconomyManager : MonoBehaviour
{
    [SerializeField] private EffectsDispatcher dispatcher;
    [SerializeField] private Modifier transactionModifier;

    // only exectued in trasnaction with vendors
    private bool ModifyMoney(int amount, bool isAstroCreditsTransaction)
    {
        var stat = 100;
        if (isAstroCreditsTransaction)
            stat = 102;

        Dictionary<string, string> data = new Dictionary<string, string> {
            {"targetType", "ext"},
            { "target", $"@CharStats.{stat}" },
            { "expr", $"@CharStats.{stat} + {amount}" }
        };


        AbstractEffect ef = new SingleActivationEffect(data, 100, 0, false);
        Modifier mod = new Modifier();
        mod.effects = new List<AbstractEffect>
        {
            ef
        };

        dispatcher.modifierDispatch(mod);

        return true;
    }


    public bool TryBuyItem(int cost, bool isKeyTransaction)
    {
        if (dispatcher.GetFeatureByType<int>(FeatureType.money).Sum() < cost)
            return false;

        ModifyMoney(-cost, isKeyTransaction);
        return true;
    }

    public float GetTotalAstroCredits()
    {
        return dispatcher.GetFeatureByType<float>(FeatureType.astroCredits).Sum();
    }

}
