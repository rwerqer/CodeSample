// File: Runtime/Effects/IntentOverrideByStacks.cs
using UnityEngine;
namespace PM.Tactics {
  [CreateAssetMenu(menuName="Combat/Effects/Intent/OverrideByStacks")]
  public class IntentOverrideByStacks : EffectAsset {
    public StatusData watchStatus;
    public int threshold = 5;
    public ClashSkillData overrideSkill; // 임계치 넘으면 이 스킬로

    public override void Execute(EffectCtx ctx) {
      var owner = ctx.Owner;
      if (owner == null || watchStatus == null || overrideSkill == null) return;
      if (owner.Status.TryGet(watchStatus, out var inst) && inst.Stacks >= threshold) {
        ctx.Policy?.Intents?.SetOverrideSkill(owner, overrideSkill);
        ctx.Policy?.Log?.Info($"[IntentOverride] {owner.unitName} stacks={inst.Stacks} → next skill={overrideSkill.id}");
      }
    }
  }
}
