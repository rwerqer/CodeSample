// File: Runtime/Effects/ApplyStatusEffect.cs
using UnityEngine;

namespace PM.Tactics {
  [CreateAssetMenu(menuName="Combat/Effects/ApplyStatus")]
  public class ApplyStatusEffect : EffectAsset {
    public StatusData status;
    public int stacks = 1;
    public bool toSelf = false; // true면 Owner(자기자신)에게

    public override void Execute(EffectCtx ctx) {
      if (status == null) return;

      if (toSelf) {
        var owner = ctx.Owner ?? ctx.Caster;
        if (owner != null)
          owner.Status.Apply(status, stacks, ctx.Source ?? ctx.Caster, owner, ctx.Bus);
        return;
      }

      if (ctx.Targets == null) return;
      foreach (var t in ctx.Targets) {
        if (t == null) continue;
        t.Status.Apply(status, stacks, ctx.Source ?? ctx.Caster, t, ctx.Bus);
      }
    }
  }
}
