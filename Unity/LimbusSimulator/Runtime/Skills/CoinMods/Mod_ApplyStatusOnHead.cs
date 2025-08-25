// File: Runtime/Skills/CoinMods/Mod_ApplyStatusOnHead.cs
using UnityEngine;

namespace PM.Tactics {
  [CreateAssetMenu(menuName="Combat/CoinMod/ApplyStatusOnHead")]
  public class Mod_ApplyStatusOnHead : CoinModAsset {
    public StatusData status;
    public int stacks = 1;

    public override void OnOutcome(
      CombatManager mgr,
      Unit winner, Unit loser,
      ClashSkillData skill,
      int coinIndex, bool head,
      int finalDamage)
    {
      if (!head || status == null || loser == null || !loser.IsAlive) return;

      loser.Status.Apply(status, stacks, winner, loser, mgr.Bus);
      Debug.Log($"[CoinMod] HEAD coin#{coinIndex} → {loser.unitName} +{status.id}x{stacks} (applier={winner?.unitName})");
    }
  }
}
