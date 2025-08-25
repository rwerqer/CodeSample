// File: Runtime/Core/TagSet.cs
using System.Collections.Generic;
namespace PM.Tactics {
  public sealed class TagSet : HashSet<string> {
    public bool Has(string t) => Contains(t);
    public void AddRange(IEnumerable<string> tags) { if (tags==null) return; foreach (var t in tags) Add(t); }
  }
}
