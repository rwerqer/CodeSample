// File: Runtime/Policy/Policies_Minimal.cs
using UnityEngine;
namespace PM.Tactics {
  [CreateAssetMenu(menuName="Combat/Policy/DenyByTag")]
  public class DenyByTag : ScriptableObject, IEffectPolicy {
    public int priority=-100; public string[] blocked;
    public int Priority => priority;
    public void Apply(EffectPacket pkt, PolicyCtx ctx){ if(blocked==null) return; foreach(var t in blocked) if(pkt.Tags.Has(t)){ pkt.Cancel($"Deny:{t}"); return; } }
  }

  [CreateAssetMenu(menuName="Combat/Policy/Throttle")]
  public class Throttle : ScriptableObject, IEffectPolicy {
    public int priority=-20; public string tag="ExtraAttack"; public int perAction=1;
    static readonly System.Collections.Generic.Dictionary<int,int> _count = new();
    public int Priority => priority;
    public void Apply(EffectPacket pkt, PolicyCtx ctx){
      if(!pkt.Tags.Has(tag)) return;
      int actionId = pkt.Meta.TryGetValue("actionId", out var v) ? (int)v : 0;
      _count.TryGetValue(actionId, out int cur); if(cur>=perAction){ pkt.Cancel("Throttle"); return; } _count[actionId]=cur+1;
    }
  }
}
