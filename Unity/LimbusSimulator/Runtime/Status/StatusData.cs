// File: Runtime/Status/StatusData.cs
using UnityEngine;
using System.Collections.Generic;

namespace PM.Tactics {
  /// <summary>
  /// 스택 정책(동일 상태가 다시 걸릴 때 스택을 어떻게 처리할지)
  /// - Additive : 기존 스택 + 신규 스택 (최대 maxStacks까지)
  /// - Replace  : 기존 스택을 신규 스택으로 교체
  /// - Cap      : 신규 스택을 maxStacks로 캡핑하여 설정(기존과 합산하지 않음)
  /// </summary>
  public enum StackPolicy { Additive, Replace, Cap }

  /// <summary>
  /// 상태(Status)의 정적 정의를 보관하는 ScriptableObject.
  /// 인스턴스는 StatusController/StatusInstance에서 관리한다.
  ///
  /// 필드:
  /// - id          : 상태 식별자(디버깅/툴링용 문자열)
  /// - stackPolicy : 동일 상태 재부여 시 스택 처리 방식
  /// - maxStacks   : Additive/Cap에서 스택 상한
  /// - hooks       : "어떤 전투 이벤트가 발생하면 → 어떤 Effect를 실행할지"를 정의한 훅 목록
  ///
  /// 사용 예:
  ///   - 화상(DoT): OnTurnEnd → DamageEffect
  ///   - 스턴     : OnTurnStart → SkipActionEffect
  ///   - 보호막   : OnPreDamage → DamageReducePolicyEffect
  /// </summary>
  [CreateAssetMenu(menuName="Combat/Status")]
  public class StatusData : ScriptableObject {
    [Header("Identity & Stack")]
    public string id;
    public StackPolicy stackPolicy = StackPolicy.Additive;
    public int maxStacks = 99;

    [Header("Hooks (event → effect)")]
    // SerializeReference를 사용하여 추상/폴리모픽 EffectAsset 인스턴스를 훅에 직렬화
    // 이 목록의 각 항목은 "trigger 이벤트가 발생했을 때 실행할 Effect"를 지정
    public List<EffectBinding> hooks;
  }

  /// <summary>
  /// 하나의 훅 정의: 특정 전투 이벤트 발생 시 실행할 Effect를 바인딩
  /// - trigger : BattleEvent (예: OnTurnStart, OnTurnEnd, OnDeath, OnPreDamage ...)
  /// - effect  : 실행할 EffectAsset (정책 라우터를 통해 실행됨)
  ///
  /// 주의:
  /// - effect는 [SerializeReference]로 직렬화(클래스 계층 구조/추상 타입 지원)
  /// - StatusController.Broadcast(evt, ...)가 evt와 일치하는 훅만 찾아
  ///   baseCtx.Clone()에 Source/Owner/Event를 채워 라우터로 Dispatch
  /// </summary>
  [System.Serializable]
  public struct EffectBinding {
    public BattleEvent trigger;

    // 훅이 가리킬 효과. 예) DamageEffect, ApplyStatusEffect, ShieldPolicyEffect 등
    public EffectAsset effect;
  }
}
