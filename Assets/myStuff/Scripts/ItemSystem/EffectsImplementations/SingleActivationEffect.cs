using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// simply sets effect once
/// </summary>
public class SingleActivationEffect : AbstractEffect
{

  public SingleActivationEffect(Dictionary<string, string> data, int itemID, int effectID, bool inABullet) : base(data,
    itemID, inABullet)
  {
  }

  public override object Activate(AbstractStatsClass target)
  {
    var result = base.DoEffect();
    this.DetachEffect();
    target.RemoveEffect(this);
    return result;
  }
  public override void Attach(Dictionary<int, AbstractStatsClass> target, EffectsDispatcher dispatcher)
  {
    this.dispatcher = dispatcher;

    if (localTargetClassID != -1)
    {
      target[this.localTargetClassID].AttachEffect(this);
      return;
    }
    target[this.externalTargetClassID].AttachEffect(this);
  }

}
