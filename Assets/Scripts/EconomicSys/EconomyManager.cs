using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using static ItemManager;

public class EconomyManager : MonoBehaviour
{
    [SerializeField] private EffectsDispatcher dispatcher;
    [SerializeField] private Modifier transactionModifier;


    private bool ModifyMoney(int amount, bool isKeyTransaction)
    {
        var stat = 100;
        if (isKeyTransaction)
            stat = 101;

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
        if (dispatcher.GetAllFeatureByType<int>(FeatureType.money).Sum() < cost)
            return false;

        ModifyMoney(-cost, isKeyTransaction);
        return true;
    }

    public bool GetMoney(int cost, bool isKeyTransaction)
    {
        ModifyMoney(cost, isKeyTransaction);
        return true;
    }
}
