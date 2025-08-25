// File: Runtime/Schedule/PendingAgenda.cs
using System.Collections.Generic;
namespace PM.Tactics {
  public enum TurnSlot { TurnStart, TurnEnd }
  public sealed class PendingTicket {
    public string key; public TurnSlot slot; public int dueTurn;
    public EffectAsset payload; public Unit source; public Unit owner; public Unit[] targets; public Dictionary<string,object> meta=new();
  }
  public sealed class PendingAgenda {
    readonly Dictionary<TurnSlot,List<PendingTicket>> buckets = new();
    public void Enqueue(PendingTicket t){ if(!buckets.TryGetValue(t.slot, out var l)) buckets[t.slot]=l=new(); var i=l.FindIndex(x=>x.key==t.key); if(i>=0) l[i]=t; else l.Add(t); }
    public void Commit(TurnSlot slot, int currentTurn, EffectRouter router, EffectCtx baseCtx){
      if(!buckets.TryGetValue(slot, out var l) || l.Count==0) return; var copy=l.ToArray(); l.Clear();
      foreach(var t in copy){ if(t.dueTurn>currentTurn) { l.Add(t); continue; } var ctx=baseCtx.Clone(); ctx.Source=t.source; ctx.Owner=t.owner; ctx.Targets=t.targets; ctx.Meta=t.meta; router.Dispatch(t.payload, ctx); }
    }
  }
}
