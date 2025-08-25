// File: Runtime/Effects/DamageEffect.cs
using UnityEngine;

namespace PM.Tactics {
  [CreateAssetMenu(menuName="Combat/Effects/Damage")]
  public class DamageEffect : EffectAsset {
    public int baseDamage = 10;

    public override void Execute(EffectCtx ctx) {
      // 1) overrideDamage 있으면 그 값을 우선 사용
      int dmg = baseDamage;
      if (ctx.Meta != null && ctx.Meta.TryGetValue("overrideDamage", out var ov)) {
        try { dmg = System.Convert.ToInt32(ov); } catch { /* 무시 */ }
      }

      // 2) 곱/가감 보정(정책에서 넣어줄 수 있음)
      if (ctx.Meta != null && ctx.Meta.TryGetValue("dmgMul", out var mulObj)) {
        float mul = System.Convert.ToSingle(mulObj);
        dmg = Mathf.RoundToInt(dmg * mul);
      }
      if (ctx.Meta != null && ctx.Meta.TryGetValue("dmgAdd", out var addObj)) {
        dmg += System.Convert.ToInt32(addObj);
      }

      dmg = Mathf.Max(0, dmg);

      // 3) 실제 적용 + 디버그
      foreach (var t in ctx.Targets) {
        t.TakeDamage(dmg);
        Debug.Log($"[DamageEffect] {ctx.Source?.unitName} -> {t.unitName} applied={dmg}");
        
      }

      // 4) 결과 기록(있다면)
      ctx.Hit = new HitInfo { IsHit = true, Damage = dmg };
    }
  }
}
