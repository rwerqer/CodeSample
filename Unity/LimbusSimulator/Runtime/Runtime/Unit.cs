// File: Runtime/Runtime/Unit.cs
using UnityEngine;

namespace PM.Tactics {
  public sealed class Unit : MonoBehaviour {
    public string unitName;
    public int MaxHP = 100;
    public int HP = 100;

    [Header("Speed (per-turn roll)")]
    public int MinSpeed = 8;                     // 🔹 최소 속도
    public int MaxSpeed = 12;                    // 🔹 최대 속도
    [SerializeField] int _rolledSpeed = 10;      // 🔹 이번 턴 굴려진 속도(인스펙터 확인용)
    public int Speed => _rolledSpeed;            // ✅ 외부는 항상 이 값을 사용

    [Header("Skill & Mind")]
    public ClashSkillData skill;                 // 유닛 고유 스킬(비면 CombatManager의 default 사용)
    [Range(-45,45)] public int Sanity = 0;       // 유닛 정신력(앞면 확률 보정)

    public StatusController Status { get; private set; } = new();
    public bool IsAlive => HP > 0;

    /// <summary>
    /// 매 턴 시작에 호출: [MinSpeed..MaxSpeed] 내 정수로 속도 굴림
    /// </summary>
    public void RollTurnSpeed(System.Random rng) {
      int lo = Mathf.Min(MinSpeed, MaxSpeed);
      int hi = Mathf.Max(MinSpeed, MaxSpeed);
      if (rng != null) {
        // System.Random.Next는 상한 제외 → +1
        _rolledSpeed = rng.Next(lo, hi + 1);
      } else {
        // 폴백(에디터/테스트)
        _rolledSpeed = UnityEngine.Random.Range(lo, hi + 1);
      }
      Debug.Log($"[Speed] {unitName} rolled {Speed} (range {lo}-{hi})");
    }

    public void TakeDamage(int dmg) {
      HP = Mathf.Max(0, HP - dmg);
      Debug.Log($"{unitName} took {dmg}, HP={HP}");
      if (HP == 0) OnDeath();
    }

    public void Revive(int hp){ HP = Mathf.Clamp(hp,1,MaxHP); }

    void OnDeath(){
      var mgr = FindFirstObjectByType<CombatManager>();
      mgr?.Bus.Publish(
        BattleEvent.OnDeath,
        new EventCtx {
          Effect = new EffectCtx {
            Owner=this, Source=this, Caster=this,
            Bus=mgr.Bus, Scheduler=mgr.Scheduler, Policy=mgr.Policy
          }
        }
      );
    }
  }
}
