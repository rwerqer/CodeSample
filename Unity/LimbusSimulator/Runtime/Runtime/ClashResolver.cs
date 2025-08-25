// File: Runtime/Clash/ClashResolver.cs
using UnityEngine;
using System.Collections.Generic;

namespace PM.Tactics {
  public class ClashResolver {
    public bool DebugLog = true;
    void D(string fmt, params object[] args){ if (DebugLog) Debug.Log(string.Format(fmt, args)); }

    // ⬇︎ 유닛 Sanity 반영: 남은 코인 전체를 굴려 "총합 위력" 반환
    // 총합 = skill.basePower(한번) + Σ(각 코인: HEAD면 coinPower, TAIL이면 0)
    int RollTotalPowerRemaining(System.Random rng, Unit owner, ClashSkillData skill, int fromIndex) {
      int s = Mathf.Clamp(owner.Sanity, -45, 45);
      float headChance = Mathf.Clamp01(0.5f + (s / 100f));
      int total = skill.basePower;

      for (int i = fromIndex; i < skill.coins.Count; i++) {
        bool head = rng.NextDouble() < headChance;
        int add = head ? skill.coins[i].coinPower : 0;
        total += add;
        D("[ClashRoll] {0} {1} coin#{2} → {3} (+{4})",
          owner.unitName, skill?.id, i, head ? "HEAD" : "TAIL", add);
      }
      D("[ClashTotal] {0} {1} remainFrom={2} total={3}", owner.unitName, skill?.id, fromIndex, total);
      return total;
    }

    // (안전장치) 남은 코인들의 최대 잠재치(전부 HEAD일 때)를 구해 0-0 교착 탐지용
    int MaxRemainingCoinSum(ClashSkillData s, int fromIndex) {
      int sum = 0;
      for (int i = fromIndex; i < s.coins.Count; i++) sum += s.coins[i].coinPower;
      return sum;
    }

    public ClashResult Resolve(System.Random rng, Unit attacker, ClashSkillData atkSkill,
                               Unit defender, ClashSkillData defSkill)
    {
      int ai = 0, di = 0;

      while (ai < atkSkill.coins.Count && di < defSkill.coins.Count) {

        // ⟳ 동률이면 같은 "남은 코인 전체"로 재굴림(코인 파괴 X)
        int rerolls = 0;
        while (true) {
          int ap = RollTotalPowerRemaining(rng, attacker, atkSkill, ai);
          int dp = RollTotalPowerRemaining(rng, defender, defSkill, di);

          D("[Clash] Compare total A(rem#{0})={1}  vs  B(rem#{2})={3}", atkSkill.coins.Count - ai, ap, defSkill.coins.Count - di, dp);

          if (ap > dp) {           // A가 이김 → 수비(B)의 코인 1개 파괴
            di++;
            D("[Clash] → {0} wins this round. Defender coin breaks. (defIdx→{1})", attacker.unitName, di);
            break;
          }
          if (dp > ap) {           // B가 이김 → 공격(A)의 코인 1개 파괴
            ai++;
            D("[Clash] → {0} wins this round. Attacker coin breaks. (atkIdx→{1})", defender.unitName, ai);
            break;
          }

          // 동률: 재굴림. 무한루프 방지 안전장치
          rerolls++;
          if (rerolls > 64) {
            // 둘 다 남은 코인의 최대 잠재치가 0이면 영구 동률 → 의미 없는 코인 동시 소모
            if (MaxRemainingCoinSum(atkSkill, ai) == 0 && MaxRemainingCoinSum(defSkill, di) == 0) {
              D("[Clash] stalemate (all remaining coinPower=0) → skip both coins (A#{0}, B#{1})", ai, di);
              ai++; di++;
              break;
            }
            // 그 외 매우 드문 케이스: 임시로 둘 다 재시도 종료(한 라운드만 버리고 다음 라운드로)
            D("[Clash] too many rerolls → fail-safe skip this roll (no coin broken)");
            break;
          }
        }
      }

      if (ai < atkSkill.coins.Count && di >= defSkill.coins.Count) {
        D("[Clash] RESULT: Winner={0}, Loser={1}, winnerRemainingFromIndex={2}", attacker.unitName, defender.unitName, ai);
        return new ClashResult(attacker, defender, atkSkill, ai, true);
      }
      if (di < defSkill.coins.Count && ai >= atkSkill.coins.Count) {
        D("[Clash] RESULT: Winner={0}, Loser={1}, winnerRemainingFromIndex={2}", defender.unitName, attacker.unitName, di);
        return new ClashResult(defender, attacker, defSkill, di, true);
      }
      D("[Clash] RESULT: Full tie. No winner.");
      return new ClashResult(null, null, null, 0, false);
    }

    // ✅ 총합 + 코인별 앞/뒤 결과를 함께 산출
    public SeriesOutcome ComputeSeries(System.Random rng, Unit owner, ClashSkillData skill, int fromIndex) {
      int s = Mathf.Clamp(owner.Sanity, -45, 45);
      float headChance = Mathf.Clamp01(0.5f + (s / 100f));

      var so = new SeriesOutcome { total = skill.basePower };
      for (int i = fromIndex; i < skill.coins.Count; i++) {
        bool head = rng.NextDouble() < headChance;
        int add = head ? skill.coins[i].coinPower : 0;
        so.coins.Add(new CoinOutcome{ index=i, head=head, add=add });
        so.total += add;
      }
      return so;
    }

    // (호환용) 이전 시그니처 유지
    public int RollSeriesDamage(System.Random rng, Unit owner, ClashSkillData skill, int fromIndex) {
      return ComputeSeries(rng, owner, skill, fromIndex).total;
    }
  }

  public class ClashResult {
    public readonly Unit Winner, Loser;
    public readonly ClashSkillData Skill;
    public readonly int RemainingIndex;
    public readonly bool HasWinner;
    public ClashResult(Unit w, Unit l, ClashSkillData s, int idx, bool hasWinner){
      Winner=w; Loser=l; Skill=s; RemainingIndex=idx; HasWinner=hasWinner;
    }
  }
    public class SeriesOutcome {
    public int total;           // basePower 1회 + 남은 코인들의 합
    public List<CoinOutcome> coins = new(); // 코인별 결과 목록
  }
  public struct CoinOutcome {
    public int index;  // 코인 인덱스
    public bool head;  // 앞면 여부
    public int add;    // 이 코인이 더한 위력(HEAD면 coinPower, 아니면 0)
  }

}
