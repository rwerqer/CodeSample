# PM Tactics — Unity Turn-Based **Clash** Prototype

> 속도 기반 **N: N 턴제 전투**에서 *합(Clash)*, **코인 기반 스킬**, **정책(Policy) 체인**, **상태(Status) 훅**을 데이터 주도적으로 확장 가능한 구조로 구현한 유니티 프로토타입입니다.  
> 림버스 컴퍼니류 전투를 **모듈화 + 결과론적 후처리**로 재현했으며, 신규 효과/룰을 **ScriptableObject 추가와 라우팅만으로** 붙일 수 있도록 설계했습니다.

---

## ✨ 핵심 특징

- **합(Clash) 규칙 재현**
  - 총합 위력 = `basePower(스킬 당 1회)` + 남은 코인의 **HEAD 가산 합**(TAIL=0)
  - 동률은 **재굴림**(코인 파괴 없음), 승패가 나면 **패자의 코인만** 1개 소모
  - 승자는 남은 코인으로 동일 규칙으로 **최종 피해 산출**
  - 각 코인의 HEAD 확률 = `0.5 ± (Unit.Sanity / 100)` (Sanity −45~+45 ⇒ 약 5%~95%)

- **매 턴 속도 범위 롤 & 행동 스케줄링**
  - 유닛마다 `MinSpeed..MaxSpeed` 범위에서 **매 턴** 속도 굴림 → 스냅샷으로 의도(Intent) 생성
  - 실행 순서: **속도 내림차순**, 동속도는 **커밋 순(배치 순)**

- **Clash 매칭 규칙**
  1) *나를 공격하는 적을 내가 공격* → 서로 타깃이면 **합**(높은 속도 기준)  
  2) *같거나 더 빠른 대상을 공격* → **상대 공격과 무관한 일방공격**  
  3) *더 느린 대상을 공격* → 그 대상이 다른 타깃을 공격 중이면 **타깃을 나로 바꿔 합**, 아니면 **일방공격**  
     - (3)에서 끌려온 대상의 **유효 처리 속도는 빠른 쪽에 맞춤**(우선순위 역전 방지)

- **코인 모드(CoinMod) — 결과론적 특수효과**
  - 코인별 **HEAD/TAIL 결과**를 바탕으로, **피해 적용 직후** 모드(예: 디버프 부여, 지원 추가타, 예약 폭발 등) 실행

- **상태(Status) 훅과 DoT**
  - `StatusData.hooks`에 `BattleEvent → Effect` 바인딩
  - 예: `DecayingDotEffect` — 턴 종료 시 **스택만큼 피해**, **스택 ⌊1/2⌋**, **1 이하면 제거**(상세 로그)

- **정책(Policy) 체인 & 라우터**
  - `EffectRouter` + `IEffectPolicy[]`로 **면역/증감/캔슬/소스·타깃 치환** 같은 규칙을 실행 전 단계에서 적용

- **스킬 이벤트 파이프라인**
  - `OnActionDeclared → OnPreCast → OnCast → OnPostAction` 타이밍에 `EffectBinding` 실행(연출/버프/예약 등)

---

---

## ⚙️ 요구사항

- Unity **2021.3+** (권장)  
- 신규 3D/URP 템플릿에서 동작, 별도 패키지 의존성 없음

---

## 🚀 빠른 시작 (3v3 하네스 데모)

1. **씬 구성**
   - 씬에 `CombatManager`와 `TestHarness3v3`를 배치
   - `CombatManager` 인스펙터:
     - `damageEffect` ← `DamageEffect` SO 지정
     - (옵션) `defaultClashSkill` 지정
   - `TestHarness3v3` 인스펙터:
     - `manager` ← CombatManager
     - `unitPrefab` ← `Unit`(+ 권장: `UnitHUD`)가 붙은 간단 프리팹
     - 팀 A/B의 스킬 SO, 속도/HP 설정

2. **재생**
   - 콘솔: 합/코인 헤드·테일/피해/DoT 로그 확인
   - 화면: 각 유닛 HP 게이지/숫자 갱신 확인

> 🧪 자동 생성(선택): 메뉴 **`Combat/Create Test Assets (Clash & Damage)`** 로 기본 `DamageEffect`와 샘플 스킬을 생성할 수 있습니다.

---

## 🧱 핵심 타입(요약)

### CombatManager (턴 오케스트레이션)
- 필드: `EventBus`, `ActionScheduler`, `PolicyCtx`, `EffectRouter`, `PendingAgenda`, `Battlefield`, `CurrentTurn`
- 주요 메서드
  - `BeginTurn()` / `EndTurn()`  
    - `OnTurnStart/End` 발행, `PendingAgenda` 커밋, 각 유닛 `Status.Broadcast(...)`로 훅 실행
  - `BuildIntents()` → `BuildAndPairActions()` → `ExecuteClash/ExecuteOneSided()`  
    - 속도 스냅샷 기반으로 의도 생성, 규칙대로 Clash/OneSided 매칭 후 실행
  - `DispatchDamage(src, dst, dmg, isClash)`  
    - `overrideDamage/dmgMul/dmgAdd` 메타 반영하여 `DamageEffect` 라우팅
  - `ApplyCoinMods(winner, loser, skill, SeriesOutcome)`  
    - **피해 적용 직후** 코인별 결과론 모드 실행

### ClashResolver (합 판정/총합 계산)
- `Resolve(rng, atk, atkSkill, def, defSkill)`  
  - 코인별 비교(동률 재굴림) → 승자·패자·남은 코인 인덱스
- `ComputeSeries(rng, owner, skill, fromIndex)`  
  - 총합 = `basePower(1회)` + HEAD 가산 합  
  - `CoinOutcome(index, head, add)` 리스트 포함

### Skill System
- `ClashSkillData` (SO): `basePower`, `List<SkillCoin{ coinPower, mods }>`
- `CoinModAsset` (SO): `OnOutcome(...)` — 코인별 결과론 트리거
- `SkillExecutor`: `OnActionDeclared → OnPreCast → OnCast → OnPostAction`에서 `EffectBinding` 실행

### Status System
- `StatusData` (SO): `id`, `stackPolicy`, `maxStacks`, `hooks: List<EffectBinding>`
- `StatusController`: `Apply/Remove/TryGet`, `Broadcast(evt, ctx, router)`
- 예: `DecayingDotEffect` — `OnTurnEnd` 훅에서 스택 피해/감소/제거 + 로그

### Policy Chain
- `EffectRouter.Dispatch(effect, ctx)` → `IEffectPolicy.Apply(...)` 순차 적용(캔슬/면역/치환/증감 등)

---

## 🪄 스킬 만들기 예시 (HEAD 시 Decaying DoT 5스택)

1) **Status & Effect**
- `Create → Combat → Status` → `Status_DecayingDot`
- `Create → Combat → Effects → DecayingDot`  
  - `status = Status_DecayingDot`
- `Status_DecayingDot.hooks`에 항목 추가  
  - `trigger = OnTurnEnd`, `effect = DecayingDotEffect`

2) **Coin Mod**
- `Create → Combat → CoinMod → ApplyStatusOnHead`  
  - `status = Status_DecayingDot`, `stacks = 5`

3) **Skill**
- `Create → Combat → ClashSkill`
  - 예: `basePower = 4`, `coins = [3,3,2]`
  - 원하는 코인(예: index 1)의 `mods`에 위 Coin Mod 추가

4) **Unit**
- `Unit.skill = (위 스킬 SO)`  
- `MinSpeed..MaxSpeed`, `Sanity` 설정

> 실행하면 해당 코인 **HEAD 시 상태 5스택**이 부여되고, **턴 종료마다** DoT 로그/피해/감소가 발생합니다.

---

## 🔁 한 턴 파이프라인
```
BeginTurn
|-- Bus.OnTurnStart
|-- Status.Broadcast(OnTurnStart)
`-- Pending(TurnStart).Commit

PlanIntents
|-- Bus.OnPlanIntentsStart (optional)
|-- Status.Broadcast(OnPlanIntentsStart) # IntentDirector
`-- BuildIntents (Speed Roll Snapshot)

Pair & Execute
|-- BuildAndPairActions (Clash / OneSided)
|-- ExecuteClash / ExecuteOneSided
|   |-- Resolve -> ComputeSeries
|   |-- DispatchDamage (Router -> DamageEffect)
|   `-- ApplyCoinMods (result-based)
`-- SkillExecutor (event pipeline)

EndTurn
|-- Pending(TurnEnd).Commit
|-- Status.Broadcast(OnTurnEnd) # e.g., DecayingDotEffect
`-- Bus.OnTurnEnd
```

---

## 로깅(디버깅)

- `[BuildIntents]` 의도 수집/속도 스냅샷  
- `[Clash]` / `[SeriesRoll]` 합 진행/코인 헤드·테일  
- `[DamageEffect]` 최종 피해 적용  
- `[CoinMod]` 코인 결과론 모드 적용  
- `[DecayingDot]` 스택/피해/감소/제거  
- `[IntentOverride]` 스택 임계치 기반 의도 오버라이드

---
