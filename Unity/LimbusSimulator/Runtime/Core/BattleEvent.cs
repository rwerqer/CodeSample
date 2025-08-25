// Runtime/Core/BattleEvent.cs
namespace PM.Tactics {
  public enum BattleEvent {
    OnBattleStart, OnBattleEnd,
    OnRoundStart, OnRoundEnd,
    OnTurnStart, OnTurnEnd,
    OnActionDeclared, OnPreCast, OnCast, OnPostAction,
    OnBeingHit_PreMitigation, OnBeingHit_PostMitigation,
    OnDamageCalc_Begin, OnDamageCalc_Modify, OnDamageCalc_Finalize,
    OnStatusApplied, OnStatusRemoved, OnStackChanged,
    OnDeath, OnRevive,
    OnPlanIntentsStart
  }
}
