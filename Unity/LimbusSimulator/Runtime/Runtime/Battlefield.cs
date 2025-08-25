// File: Runtime/Runtime/Battlefield.cs
using System.Collections.Generic;
using System.Linq;

namespace PM.Tactics {
  public sealed class Battlefield {
    public List<Unit> TeamA = new();
    public List<Unit> TeamB = new();

    public IEnumerable<Unit> AllUnits => TeamA.Concat(TeamB);

    public bool IsTeamAlive(List<Unit> team) => team.Any(u => u.IsAlive);
    public bool IsBattleOver() => !IsTeamAlive(TeamA) || !IsTeamAlive(TeamB);
  }
}
