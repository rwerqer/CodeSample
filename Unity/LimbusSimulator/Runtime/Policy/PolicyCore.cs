// File: Runtime/Policy/PolicyCore.cs
using System.Collections.Generic;
namespace PM.Tactics {
  public interface IEffectPolicy { int Priority { get; } void Apply(EffectPacket pkt, PolicyCtx ctx); }
  public interface ILogger { void Info(string m); void Warn(string m); }
  public sealed class PolicyCtx { 
    public EventBus Bus; 
    public ActionScheduler Scheduler; 
    public ILogger Log; public System.Random Rng = new(1); 
    public IntentDirector Intents;
  }

  public sealed class EffectPacket {
    public EffectAsset Effect; public Unit Source; public Unit Owner; public Unit[] Targets;
    public BattleEvent Event; public TagSet Tags = new(); public Dictionary<string,object> Meta = new();
    public bool IsCanceled { get; private set; }
    public void Cancel(string reason){ IsCanceled = true; Meta["cancel_reason"]=reason; }
  }
}
