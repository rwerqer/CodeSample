// File: Runtime/Runtime/IntentDirector.cs
using System.Collections.Generic;
namespace PM.Tactics {
  public sealed class IntentDirector {
    readonly Dictionary<Unit, ClashSkillData> _overrideSkill = new();
    // 필요하면 강제 타깃/스킵 등도 확장 가능
    public void SetOverrideSkill(Unit u, ClashSkillData s) { if(u!=null && s!=null) _overrideSkill[u]=s; }
    public bool TryGetOverrideSkill(Unit u, out ClashSkillData s) => _overrideSkill.TryGetValue(u, out s);
    public void Clear() => _overrideSkill.Clear();
  }
}
