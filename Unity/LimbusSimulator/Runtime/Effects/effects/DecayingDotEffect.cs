// File: Runtime/Effects/DecayingDotEffect.cs
using UnityEngine;

namespace PM.Tactics {
  [CreateAssetMenu(menuName="Combat/Effects/DecayingDot")]
  public class DecayingDotEffect : EffectAsset {
    [Tooltip("이 효과가 관리할 StatusData (자기 자신을 가리켜도 OK)")]
    public StatusData status;

    public override void Execute(EffectCtx ctx) {
      var owner = ctx.Owner;
      if (owner == null || status == null) return;

      // 현재 스택 조회
      if (!owner.Status.TryGet(status, out var inst)) {
        Debug.Log($"[DecayingDot] {owner?.unitName} has no '{status?.id}' → skip.");
        return;
      }

      int stacks = Mathf.Max(0, inst.Stacks);
        // Meta에서 안전하게 턴 번호 꺼내기 (없으면 -1)
        int turn = -1;
        if (ctx.Meta != null && ctx.Meta.TryGetValue("turn", out var tObj)) {
            try { turn = System.Convert.ToInt32(tObj); } catch {}
        }
      if (stacks <= 0) {
        owner.Status.Remove(status, ctx.Bus, owner);
        Debug.Log($"[DecayingDot] {owner.unitName} stacks<=0 → removed");
        return;
      }

      // 피해 적용 (정책/면역을 태우지 않고 직접 적용 — 필요 시 DamageEffect 경유로 바꿀 수 있음)
      owner.TakeDamage(stacks);
      Debug.Log($"[DecayingDot] APPLY DAMAGE: {owner.unitName} -{stacks}hp (after={owner.HP})");

      // 스택 절반(내림), 1 이하면 제거
      int next = stacks / 2;
      if (next <= 1) {
        owner.Status.Remove(status, ctx.Bus, owner);
        Debug.Log($"[DecayingDot] {owner.unitName} stacks halved to {next} → removed");
      } else {
        inst.Stacks = next;
        Debug.Log($"[DecayingDot] {owner.unitName} stacks → {inst.Stacks}");
      }
    }
  }
}
