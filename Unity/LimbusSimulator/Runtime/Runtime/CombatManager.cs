// File: Runtime/Runtime/CombatManager.cs
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace PM.Tactics {
  public sealed class CombatManager : MonoBehaviour, ILogger {
    // Orchestration (기존 호환)
    public EventBus Bus { get; private set; } = new();
    public ActionScheduler Scheduler { get; private set; } = new();
    public PolicyCtx Policy { get; private set; }
    public EffectRouter Router { get; private set; }
    public PendingAgenda Pending { get; private set; } = new();
    public Battlefield Battlefield { get; private set; } = new();
    public int CurrentTurn { get; private set; } = 1;

    [Header("Policies & Router")]
    [SerializeField] List<ScriptableObject> policyAssets;   // IEffectPolicy SO들

    [Header("Teams")]
    [SerializeField] List<Unit> teamAUnits;
    [SerializeField] List<Unit> teamBUnits;

    [Header("Clash & Damage")]
    [SerializeField] ClashSkillData defaultClashSkill;  // 코인/정신력 스킬
    [SerializeField] DamageEffect damageEffect;          // Router에 태울 이펙트

    ISkillExecutor executor;

    readonly ClashResolver clash = new();

    void Awake() {
      // 1) 정책 수집 → 라우터
      var policies = new List<IEffectPolicy>();
      foreach (var a in policyAssets)
        if (a is IEffectPolicy p) policies.Add(p);

      // 2) 컨텍스트(난수 포함) 생성
      Policy = new PolicyCtx {
        Bus = Bus,
        Scheduler = Scheduler,
        Log = this,
        Rng = new System.Random(12345),
        Intents = new IntentDirector()
      };

      // 3) 라우터 생성
      Router = new EffectRouter(policies);

      // 4) 스킬 실행기 생성  ✅ 누락되면 NRE 발생
      executor = new SkillExecutor(Router, Bus, Scheduler, Policy);

      // 5) 배틀필드 팀 주입(에디터에서 비워둘 수 있으니 null 방어)
      if (teamAUnits != null) {
        Battlefield.TeamA.Clear();
        Battlefield.TeamA.AddRange(teamAUnits.Where(u => u != null));
      }
      if (teamBUnits != null) {
        Battlefield.TeamB.Clear();
        Battlefield.TeamB.AddRange(teamBUnits.Where(u => u != null));
      }

      // 6) 세팅 확인용 경고(없어도 동작은 하지만 의도치 않은 상태를 빨리 알아차리기 위함)
      if (defaultClashSkill == null)
        Debug.LogWarning("[CombatManager] defaultClashSkill is NULL. Units with no skill will be unable to act unless they have a skill.");
      if (damageEffect == null)
        Debug.LogWarning("[CombatManager] damageEffect is NULL. Damage will be routed with policies but no effect will run.");
    }

    void Start() {
      Bus.Publish(BattleEvent.OnBattleStart, new EventCtx { Effect = MakeCtx() });
      StartCoroutine(BattleLoop());
    }

    IEnumerator BattleLoop() {
      while (!Battlefield.IsBattleOver()) {
        BeginTurn();
        // ⬇️ 의도 계획 시작 알림
        Bus.Publish(BattleEvent.OnPlanIntentsStart, new EventCtx { Effect = MakeCtx() });
        foreach (var u in Battlefield.AllUnits) { // 상태 훅에도 기회 제공
          var ctx = MakeCtx(); ctx.Owner = u;
          u.Status.Broadcast(BattleEvent.OnPlanIntentsStart, ctx, Router);
        }

        // 1) 의도 수집 (간단 AI: 첫 생존 적을 타겟)
        var intents = BuildIntents(defaultClashSkill);

        // 2) 규칙에 따라 Clash/One-sided 매칭 → 이니셔티브 큐 생성
        var schedule = BuildAndPairActions(intents);

        // 3) 이니셔티브(속도) 내림차순 실행
        schedule.Sort();
        foreach (var sa in schedule) {
          sa.exec?.Invoke();
          yield return new WaitForSeconds(0.3f);
        }

        EndTurn();
        yield return new WaitForSeconds(0.5f);
      }

      Bus.Publish(BattleEvent.OnBattleEnd, new EventCtx { Effect = MakeCtx() });
      Debug.Log("Battle Finished!");
    }

    // ─────────────────────────────────────────────────────────────────────
    // (A) 의도 수집
// CombatManager.cs 내 교체
List<ActionIntent> BuildIntents(ClashSkillData fallbackSkill) {
  var intents = new List<ActionIntent>(Battlefield.TeamA.Count + Battlefield.TeamB.Count);
  int commit = 0;

  // Team A
  for (int i = 0; i < Battlefield.TeamA.Count; i++) {
    var u = Battlefield.TeamA[i];
    if (u == null || !u.IsAlive) continue;

    var t = SelectMirrorTarget(u, i, Battlefield.TeamA, Battlefield.TeamB); // A_i -> B_i
    if (t == null) t = Battlefield.TeamB.FirstOrDefault(e => e != null && e.IsAlive);
    if (t == null) continue;

    var chosen = (u.skill != null ? u.skill : fallbackSkill);
    if (Policy.Intents != null && Policy.Intents.TryGetOverrideSkill(u, out var over))
      chosen = over;
    if (chosen == null) continue;

    intents.Add(new ActionIntent {
      actor = u,
      target = t,
      skill = chosen,
      speedSnapshot = u.Speed,
      commitOrder = commit++   // 동속도 타이브레이커
    });
     Debug.Log($"[BuildIntents:T{CurrentTurn}] +A{i} {u.unitName}({u.Speed},{u.Sanity}) → {t.unitName} skill={chosen.id} commit={commit-1}");
  }

  // Team B
  for (int i = 0; i < Battlefield.TeamB.Count; i++) {
    var u = Battlefield.TeamB[i];
    if (u == null || !u.IsAlive) continue;

    var t = SelectMirrorTarget(u, i, Battlefield.TeamB, Battlefield.TeamA); // B_i -> A_i
    if (t == null) t = Battlefield.TeamA.FirstOrDefault(e => e != null && e.IsAlive);
    if (t == null) continue;

    var chosen = (u.skill != null ? u.skill : fallbackSkill);
    if (chosen == null) continue;

    intents.Add(new ActionIntent {
      actor = u,
      target = t,
      skill = chosen,
      speedSnapshot = u.Speed,
      commitOrder = commit++
    });
  }

  // 디버그: 몇 건이 모였는지 확인
  Debug.Log($"[BuildIntents] intents={intents.Count} (A:{Battlefield.TeamA.Count} / B:{Battlefield.TeamB.Count})");
  return intents;
}

// 같은 인덱스 우선 타깃 선택(죽었으면 null 반환)
Unit SelectMirrorTarget(Unit actor, int idx, List<Unit> myTeam, List<Unit> enemyTeam) {
  if (idx >= 0 && idx < enemyTeam.Count) {
    var e = enemyTeam[idx];
    if (e != null && e.IsAlive) return e;
  }
  return null;
}


    // ─────────────────────────────────────────────────────────────────────
    // (B) 규칙 3항 구현: 매칭 → 실행 큐 생성
    // 규칙 요약:
    // 1) "나를 공격하는 적을 공격" → 서로를 타게팅하면, 둘 중 높은 속도를 기준으로 '합'
    // 2) "같거나 더 빠른 대상 공격" → 대상의 공격과 별개로 내 '일방공격'
    // 3) "더 느린 대상 공격" → 그 대상이 다른 이를 공격 중이면 대상의 타겟을 나로 바꿔서 '합', 아니면 '일방공격'
    List<ScheduledAction> BuildAndPairActions(List<ActionIntent> intents) {
      var schedule = new List<ScheduledAction>(32);
      if (intents.Count == 0) return schedule;

      // 유닛→의도 맵 (빠른 조회용)
      var map = intents.ToDictionary(it => it.actor, it => it);

      // 속도 내림차순으로 의도 순회(우선권 결정)
      foreach (var it in intents.OrderByDescending(x => x.speedSnapshot)) {
        if (it.consumed || it.actor == null || it.target == null) continue;
        if (!it.actor.IsAlive || !it.target.IsAlive) { it.consumed = true; continue; }

        map.TryGetValue(it.target, out var tIntent);
        bool targetAttackingMe = (tIntent != null && !tIntent.consumed && tIntent.target == it.actor);

        // ── 규칙 1: 상호 타게팅 → 합(이니셔티브=둘 중 큰 속도)
        if (targetAttackingMe) {
          var job = new ClashJob {
            a = it.actor, skillA = it.skill,
            b = it.target, skillB = tIntent.skill,
            initiative = Mathf.Max(it.speedSnapshot, tIntent.speedSnapshot),
            mutual = true
          };
          schedule.Add(new ScheduledAction {
            initiative = job.initiative,
            tieOrder   = Mathf.Min(it.commitOrder, tIntent.commitOrder),
            exec = () => ExecuteClash(job)
          });
          it.consumed = true; tIntent.consumed = true;
          continue;
        }

        // ── 규칙 2: 같거나 더 빠른 대상 공격 → 내 일방공격 (대상의 공격과 무관)
        if (it.target.Speed >= it.actor.Speed) {
          var single = new OneSidedJob {
            attacker = it.actor, defender = it.target, skill = it.skill,
            initiative = it.speedSnapshot
          };
          schedule.Add(new ScheduledAction {
            initiative = single.initiative,
            tieOrder   = it.commitOrder,
            exec = () => ExecuteOneSided(single)
          });
          it.consumed = true;
          continue;
        }

        // ── 규칙 3: 더 느린 대상 공격
        bool targetIsAttackingSomeone = (tIntent != null && !tIntent.consumed && tIntent.target != null);

        if (targetIsAttackingSomeone) {
          // 대상의 대상 변경 → 나와 합 (이니셔티브=나의 속도)
          var job = new ClashJob {
            a = it.actor, skillA = it.skill,
            b = it.target, skillB = tIntent.skill,
            initiative = it.speedSnapshot,
            mutual = false
          };
          schedule.Add(new ScheduledAction {
            initiative = Mathf.Max(it.speedSnapshot, tIntent.speedSnapshot),
            tieOrder   = Mathf.Min(it.commitOrder, tIntent.commitOrder),
            exec       = () => ExecuteClash(job)
          });
          it.consumed = true; tIntent.consumed = true; // 대상의 원래 공격은 소모됨
        } else {
          // 대상이 공격하지 않음 → 일방공격
          var single = new OneSidedJob {
            attacker = it.actor, defender = it.target, skill = it.skill,
            initiative = it.speedSnapshot
          };
          schedule.Add(new ScheduledAction {
            initiative = it.speedSnapshot,
            tieOrder   = it.commitOrder,
            exec = () => ExecuteOneSided(single)
          });
          it.consumed = true;
        }
      }

      return schedule;
    }

    // ─────────────────────────────────────────────────────────────────────
    // (C) 실행 루틴들 — 정책/라우터 경유(DamageEffect)
    void ExecuteClash(ClashJob job) {
      if (job.a == null || job.b == null) return;
      if (!job.a.IsAlive || !job.b.IsAlive) return;

      var res = clash.Resolve(Policy.Rng, job.a, job.skillA, job.b, job.skillB);
      if (!res.HasWinner) { Debug.Log("Clash draw → no damage."); return; }

      var series = clash.ComputeSeries(Policy.Rng, res.Winner, res.Skill, res.RemainingIndex);
      DispatchDamage(res.Winner, res.Loser, series.total, isClash: true);

      // ✅ 결과론적 코인 특수효과 실행 (각 코인별)
      ApplyCoinMods(res.Winner, res.Loser, res.Skill, series);

      // (선택) 스킬 이벤트 파이프라인
      executor.ExecuteSkill(new SkillRuntime(res.Skill), res.Winner, new[] { res.Loser }, actionId: CurrentTurn);
    }

    void ExecuteOneSided(OneSidedJob job) {
      if (job.attacker == null || job.defender == null) return;
      if (!job.attacker.IsAlive || !job.defender.IsAlive) return;

      var series = clash.ComputeSeries(Policy.Rng, job.attacker, job.skill, 0);
      DispatchDamage(job.attacker, job.defender, series.total, isClash: false);

      // ✅ 결과론적 코인 특수효과
      ApplyCoinMods(job.attacker, job.defender, job.skill, series);

      executor.ExecuteSkill(new SkillRuntime(job.skill), job.attacker, new[] { job.defender }, actionId: CurrentTurn);
    }

    // 결과론적 코인 모드 실행기
    void ApplyCoinMods(Unit winner, Unit loser, ClashSkillData skill, SeriesOutcome series) {
      if (skill?.coins == null) return;
      foreach (var oc in series.coins) {
        var coin = skill.coins[oc.index];
        if (coin?.mods == null) continue;
        foreach (var mod in coin.mods) {
          try {
            mod?.OnOutcome(this, winner, loser, skill, oc.index, oc.head, series.total);
          } catch (System.Exception ex) {
            Debug.LogWarning($"[CoinMod] {mod?.name} threw: {ex.Message}");
          }
        }
      }
    }
    // 라우터로 흘려 정책 체인 개입
    void DispatchDamage(Unit src, Unit dst, int dmg, bool isClash) {
      var ctx = new EffectCtx {
        Caster = src,
        Source = src,
        Owner = null,
        Targets = new[] { dst },
        Event = 0,
        Bus = Bus,
        Scheduler = Scheduler,
        Policy = Policy,
        Tags = new TagSet(),
        Meta = new Dictionary<string, object>()
      };
      ctx.Meta["overrideDamage"] = dmg;
      ctx.Tags.Add("Damage");
      if (isClash) ctx.Tags.Add("Clash");

      Router.Dispatch(damageEffect, ctx);
          
      // 실제 적용값(정책 이후)로 출력
      int applied = ctx.Hit?.Damage ?? dmg;
      Debug.Log($"{src.unitName} {(isClash ? "wins clash and " : "")}hits {dst.unitName} for {applied}.");
    }

    // ── 턴 경계 (기존 Pending/Bus와 호환 유지)
    void BeginTurn() {
      // 0) 이번 턴 속도 굴림(생존자만)
      RollSpeedsForTurn();

      // 1) 기존 이벤트/펜딩 훅 유지
      Bus.Publish(BattleEvent.OnTurnStart, new EventCtx { Effect = MakeCtx() });
      Pending.Commit(TurnSlot.TurnStart, CurrentTurn, Router, MakeCtx());

      // (선택) 상태 훅을 유닛별로 쏘는 코드가 있다면 여기에 유지
      // foreach (var u in Battlefield.AllUnits) { ... u.Status.Broadcast(...); }
    }

    void RollSpeedsForTurn() {
      foreach (var u in Battlefield.AllUnits) {
        if (u != null && u.IsAlive) u.RollTurnSpeed(Policy?.Rng);
      }
    }
    void EndTurn() {
      Pending.Commit(TurnSlot.TurnEnd, CurrentTurn, Router, MakeCtx());
      
      foreach (var u in Battlefield.AllUnits) {
        var ctx = MakeCtx(); 
        ctx.Owner = u;
        // ⬇️ 턴 번호 태깅
        ctx.Meta["turn"] = CurrentTurn;
        u.Status.Broadcast(BattleEvent.OnTurnEnd, ctx, Router);
      }

      Bus.Publish(BattleEvent.OnTurnEnd, new EventCtx { Effect = MakeCtx() });
      CurrentTurn++;
    }

    // 컨텍스트 헬퍼 & 로거
    EffectCtx MakeCtx() => new EffectCtx {
      Bus = Bus,
      Scheduler = Scheduler,
      Policy = Policy,
      Tags = new TagSet(),
      Meta = new System.Collections.Generic.Dictionary<string, object>()
    };
    public void Info(string m) => Debug.Log(m);
    public void Warn(string m) => Debug.LogWarning(m);
  }
}
