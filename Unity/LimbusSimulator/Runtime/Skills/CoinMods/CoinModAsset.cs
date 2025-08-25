// File: Runtime/Skills/CoinMods/CoinModAsset.cs
using UnityEngine;

namespace PM.Tactics {
  /// 결과가 확정된 뒤(피해 산출/적용 직후)에만 호출되는 코인 특수효과 베이스
  public abstract class CoinModAsset : ScriptableObject {
    public abstract void OnOutcome(
      CombatManager mgr,
      Unit winner, Unit loser,
      ClashSkillData skill,
      int coinIndex, bool head,
      int finalDamage);
  }
}
