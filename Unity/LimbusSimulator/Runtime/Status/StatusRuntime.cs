// File: Runtime/Status/StatusRuntime.cs
using System.Collections.Generic;

namespace PM.Tactics {
  /// <summary>
  /// 단일 상태 인스턴스(유닛이 보유 중인 버프/디버프 하나)
  /// - Data  : 상태의 정적 정의(ScriptableObject)
  /// - Source: 상태를 부여한 주체(시전자/공격자)
  /// - Owner : 상태를 보유한 유닛(피시전자/대상)
  /// - Stacks: 현재 스택 수(1 이상)
  /// </summary>
  public sealed class StatusInstance {
    public StatusData Data;
    public Unit Source;
    public Unit Owner;
    public int Stacks = 1;
  }

  /// <summary>
  /// 한 유닛의 상태 목록을 관리하고, 게임 이벤트에 반응해 상태 훅을 실행하는 컨트롤러.
  /// 책임:
  /// - Apply : 상태 부여/스택 병합 및 적용 이벤트 발행
  /// - Remove: 상태 제거 및 제거 이벤트 발행
  /// - Broadcast: 특정 이벤트 발생 시 훅(effect)을 라우터로 전달해 실행
  ///
  /// 사용법:
  ///  - 유닛이 이벤트 경계(턴 시작/종료, 피격, 행동 전/후 등)에서
  ///    controller.Broadcast(evt, baseCtx, router) 호출
  /// </summary>
  public sealed class StatusController {
    // 소유한 상태들의 선형 컨테이너.
    // 훅 실행 중 수정될 수 있으므로 브로드캐스트 시 스냅샷을 사용한다(list.ToArray()).
    readonly List<StatusInstance> list = new();

    /// <summary>
    /// 상태를 부여(또는 스택 갱신)한다.
    /// - 동일 StatusData가 이미 있으면 stackPolicy에 따라 스택 계산만 수행
    /// - 없으면 새 StatusInstance 추가
    /// - 완료 후 OnStatusApplied 이벤트 발행(외부 UI/로그/사운드 등 반응 지점)
    /// </summary>
    public void Apply(StatusData data, int stacks, Unit source, Unit owner, EventBus bus){
      var exist = list.Find(s => s.Data == data);
      if (exist != null) {
        // 스택 정책에 따라 갱신
        switch (data.stackPolicy) {
          case StackPolicy.Additive:
            // 기존 스택 + 신규 스택, 최대 maxStacks 제한
            exist.Stacks = System.Math.Min(data.maxStacks, exist.Stacks + stacks);
            break;
          case StackPolicy.Replace:
            // 신규 스택으로 교체(상한 고려 X, 데이터 정의대로)
            exist.Stacks = stacks;
            break;
          case StackPolicy.Cap:
            // 신규 스택을 maxStacks로 캡핑해서 설정
            exist.Stacks = System.Math.Min(data.maxStacks, stacks);
            break;
        }
      } else {
        // 새로운 인스턴스 추가
        list.Add(new StatusInstance {
          Data   = data,
          Stacks = stacks,
          Source = source,
          Owner  = owner
        });
      }

      // 적용 알림 이벤트 발행
      // 주: EffectCtx는 최소 필드만 세팅(필요 시 호출부에서 Clone/채워넣기 고려)
      bus?.Publish(BattleEvent.OnStatusApplied,
        new EventCtx { Effect = new EffectCtx { Source = source, Owner = owner, Bus = bus } });
    }

    /// <summary>
    /// 특정 StatusData를 모두 제거한다.
    /// - 제거 후 OnStatusRemoved 이벤트 발행
    /// - owner 파라미터는 이벤트 리스너가 UI/로그 표시에 사용할 수 있는 정보
    /// </summary>
    public void Remove(StatusData data, EventBus bus, Unit owner){
      list.RemoveAll(s => s.Data == data);
      bus?.Publish(BattleEvent.OnStatusRemoved,
        new EventCtx { Effect = new EffectCtx { Owner = owner, Bus = bus } });
    }

    /// <summary>
    /// 외부에서 어떤 전투 이벤트(evt)가 발생했을 때 호출.
    /// 보유 중인 각 상태의 hooks를 검사하여,
    ///   - hook.trigger == evt 인 항목에 대해
    ///   - baseCtx를 Clone한 후 Source/Owner/Event를 인스턴스 값으로 덮어쓰고
    ///   - router.Dispatch(hook.effect, ctx)로 효과 실행
    ///
    /// 중요:
    /// - list.ToArray()로 스냅샷을 만들어 순회: 훅 실행 중 Apply/Remove로 리스트가 변경되어도 안전
    /// - baseCtx에는 Policy/Scheduler/Tags/Meta 등 필요한 정보가 담겨 있어야 폴리시 체인 정상 동작
    /// </summary>
    public void Broadcast(BattleEvent evt, EffectCtx baseCtx, EffectRouter router){
      foreach (var s in list.ToArray()) {
        if (s.Data.hooks == null) continue;

        foreach (var h in s.Data.hooks) {
          // 훅이 없거나 트리거가 다르면 스킵
          if (h.effect == null || h.trigger != evt) continue;

          // 이 상태 인스턴스의 문맥으로 실행(시전자/오너/이벤트 정보 주입)
          var ctx = baseCtx.Clone();
          ctx.Source = s.Source;
          ctx.Owner  = s.Owner;
          ctx.Event  = evt;

          router.Dispatch(h.effect, ctx);
        }
      }
    }

    public bool TryGet(StatusData data, out StatusInstance inst) {
      inst = list.Find(s => s.Data == data);
      return inst != null;
    }
  }
}
