// File: Runtime/Effects/EffectAsset.cs
using UnityEngine;
namespace PM.Tactics {
  public abstract class EffectAsset : ScriptableObject {
    [SerializeField] string[] tags; public string[] Tags => tags ?? System.Array.Empty<string>();
    public virtual bool CanExecute(EffectCtx ctx)=> true;
    public abstract void Execute(EffectCtx ctx);
  }
}
