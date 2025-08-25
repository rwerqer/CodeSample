// File: Runtime/Core/EventBus.cs
using System; using System.Collections.Generic;
namespace PM.Tactics {
  public sealed class EventBus {
    readonly Dictionary<BattleEvent,List<Action<EventCtx>>> _subs = new();
    public void Subscribe(BattleEvent evt, Action<EventCtx> h){ if(!_subs.TryGetValue(evt, out var l)) _subs[evt]=l=new(); if(!l.Contains(h)) l.Add(h); }
    public void Unsubscribe(BattleEvent evt, Action<EventCtx> h){ if(_subs.TryGetValue(evt, out var l)) l.Remove(h); }
    public void Publish(BattleEvent evt, EventCtx ctx){ if(_subs.TryGetValue(evt, out var l)){ var copy=l.ToArray(); foreach(var h in copy) h?.Invoke(ctx);} }
  }
  public sealed class EventCtx {
    public EffectCtx Effect; public Unit Source => Effect.Source; public Unit Owner => Effect.Owner;
    public TagSet Tags => Effect.Tags;
  }
}
