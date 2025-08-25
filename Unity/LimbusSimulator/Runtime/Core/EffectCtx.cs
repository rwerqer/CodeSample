// File: Runtime/Core/EffectCtx.cs
using System.Collections.Generic;
namespace PM.Tactics {
  public class HitInfo {
    public bool IsHit;
    public int Damage;
  }
  public sealed class EffectCtx {
    public Unit Caster; public Unit Source; public Unit Owner; public Unit[] Targets;
    public BattleEvent Event; public HitInfo Hit;
    
    public TagSet Tags = new(); public Dictionary<string,object> Meta = new();
    public EventBus Bus; public PolicyCtx Policy; public ActionScheduler Scheduler;
    public EffectCtx Clone() => (EffectCtx)MemberwiseClone();
  }
}
