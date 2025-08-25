// File: Runtime/Runtime/ActionPlanning.cs
using System;
using System.Collections.Generic;

namespace PM.Tactics {
  // 한 유닛의 이번 턴 공격 의도(스냅샷)
  public sealed class ActionIntent {
    public Unit actor;
    public Unit target;
    public ClashSkillData skill;
    public int speedSnapshot;
    public int commitOrder; 
    public bool consumed;
  }

  // 합(Clash) 작업
  public sealed class ClashJob {
    public Unit a; public ClashSkillData skillA;
    public Unit b; public ClashSkillData skillB;
    public int initiative;       // 처리 우선순위 (속도 기반)
    public bool mutual;          // 상호 타게팅 여부(규칙 1)
  }

  // 일방 공격 작업
  public sealed class OneSidedJob {
    public Unit attacker; public Unit defender; public ClashSkillData skill;
    public int initiative;       // 처리 우선순위
  }

  // 실행 큐에 올릴 스케줄 항목(이니셔티브 정렬용)
  public struct ScheduledAction : IComparable<ScheduledAction> {
    public int initiative;
    public int tieOrder;
    public Action exec;
    public int CompareTo(ScheduledAction other) {
    int c = other.initiative.CompareTo(initiative); // 속도 우선
    if (c != 0) return c;
    return tieOrder.CompareTo(other.tieOrder);      // 동률 시 왼→오
  }
  }

}
