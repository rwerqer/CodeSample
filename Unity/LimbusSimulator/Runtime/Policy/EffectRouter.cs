// File: Runtime/Policy/EffectRouter.cs
using System.Linq;
using System.Collections.Generic;

namespace PM.Tactics {
  public sealed class EffectRouter {
    readonly List<IEffectPolicy> _policies;
    public EffectRouter(IEnumerable<IEffectPolicy> policies){
      _policies = policies.OrderBy(p => p.Priority).ToList();
    }

    public void Dispatch(EffectAsset effect, EffectCtx baseCtx){
      var pkt = new EffectPacket{
        Effect  = effect,
        Source  = baseCtx.Source ?? baseCtx.Caster,
        Owner   = baseCtx.Owner,
        Targets = baseCtx.Targets,
        Event   = baseCtx.Event,
        // ⬇⬇⬇ 중요: Meta/Tags를 baseCtx로부터 가져와 초기화
        Meta    = baseCtx.Meta != null ? new Dictionary<string, object>(baseCtx.Meta)
                                       : new Dictionary<string, object>(),
        Tags    = new TagSet()
      };

      // 효과 자체의 태그 + 호출 컨텍스트의 태그 모두 합치기
      if (effect != null) pkt.Tags.AddRange(effect.Tags);
      if (baseCtx.Tags != null) pkt.Tags.AddRange(baseCtx.Tags);

      // 정책 적용
      foreach (var p in _policies){
        p.Apply(pkt, baseCtx.Policy);
        if (pkt.IsCanceled){
          baseCtx.Policy?.Log?.Info($"Canceled by {p.GetType().Name}");
          return;
        }
      }

      // 최종 exec 컨텍스트로 반영
      var exec = baseCtx.Clone();
      exec.Source  = pkt.Source;
      exec.Owner   = pkt.Owner;
      exec.Targets = pkt.Targets;
      exec.Tags    = pkt.Tags;   // ← 호출 태그 유지
      exec.Meta    = pkt.Meta;   // ← overrideDamage 등 전달

      if (pkt.Effect != null && pkt.Effect.CanExecute(exec))
        pkt.Effect.Execute(exec);
    }
  }
}
