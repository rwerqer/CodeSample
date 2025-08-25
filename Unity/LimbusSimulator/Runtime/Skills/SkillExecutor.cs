// File: Runtime/Skills/SkillExecutor.cs
using System.Linq;
namespace PM.Tactics {
  public interface ISkillExecutor { void ExecuteSkill(SkillRuntime skill, Unit caster, Unit[] targets, int actionId); }

  public sealed class SkillExecutor : ISkillExecutor {
    readonly EffectRouter router; readonly EventBus bus; readonly ActionScheduler sched; readonly PolicyCtx pctx;
    public SkillExecutor(EffectRouter r, EventBus b, ActionScheduler s, PolicyCtx p){ router=r; bus=b; sched=s; pctx=p; }

    public void ExecuteSkill(SkillRuntime skill, Unit caster, Unit[] targets, int actionId){
      var ctx = new EffectCtx{ Caster=caster, Source=caster, Targets=targets, Bus=bus, Scheduler=sched, Policy=pctx };
      Run(skill, BattleEvent.OnActionDeclared, ctx, actionId);
      Run(skill, BattleEvent.OnPreCast, ctx, actionId);
      Run(skill, BattleEvent.OnCast, ctx, actionId);
      // 간단 적중 고정
      ctx.Hit = new HitInfo{ IsHit=true };
      Run(skill, BattleEvent.OnPostAction, ctx, actionId);
      // 즉시 큐 처리 (추가공격 등)
      sched.DrainImmediate((aid, req)=> ExecuteSkill(req.skill, req.caster, req.targets, aid), actionId);
    }

    void Run(SkillRuntime skill, BattleEvent evt, EffectCtx baseCtx, int actionId){
      baseCtx.Event=evt; baseCtx.Tags=new TagSet(); if(skill.Data.tags!=null) baseCtx.Tags.AddRange(skill.Data.tags); baseCtx.Meta["actionId"]=actionId;
      // 이벤트 브로드캐스트 + 상태 훅 호출 (시전자/타깃 모두)
      bus.Publish(evt, new EventCtx{ Effect=baseCtx });
      casterBroadcast(baseCtx.Caster, baseCtx);
      if(baseCtx.Targets!=null) foreach(var t in baseCtx.Targets) targetBroadcast(t, baseCtx);

      if(skill.Data.effects==null) return;
      foreach(var b in skill.Data.effects.Where(e=>e.trigger==evt && e.effect!=null)) router.Dispatch(b.effect, baseCtx);

      void casterBroadcast(Unit u, EffectCtx ctx){ u?.Status?.Broadcast(evt, ctx, router); }
      void targetBroadcast(Unit u, EffectCtx ctx){ var c=ctx.Clone(); c.Owner=u; u?.Status?.Broadcast(evt, c, router); }
    }
  }
}
