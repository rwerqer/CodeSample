// File: Runtime/Effects/DelayToTurnSlot.cs
using UnityEngine;
namespace PM.Tactics {
  [CreateAssetMenu(menuName="Combat/Effects/DelayToTurnSlot")]
  public class DelayToTurnSlot : EffectAsset {
    public TurnSlot slot = TurnSlot.TurnEnd; public int turnsLater=0; [SerializeReference] public EffectAsset payload;
    public override void Execute(EffectCtx ctx){
      if(payload==null) return;
      var mgr = Object.FindObjectOfType<CombatManager>(); if(mgr==null) return;
      var ticket = new PendingTicket {
        key=$"{ctx.Owner?.name}_{payload.name}_{mgr.CurrentTurn+turnsLater}",
        slot=slot, dueTurn=mgr.CurrentTurn+turnsLater, payload=payload,
        source=ctx.Source, owner=ctx.Owner, targets=ctx.Targets, meta=new System.Collections.Generic.Dictionary<string,object>(ctx.Meta)
      };
      mgr.Pending.Enqueue(ticket);
    }
  }
}
