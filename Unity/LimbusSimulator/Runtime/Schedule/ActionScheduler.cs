// File: Runtime/Schedule/ActionScheduler.cs
using System; using System.Collections.Generic;
namespace PM.Tactics {
  public enum ActionPriority { Immediate=0, High=1, Normal=2, Low=3 }
  public sealed class ActionRequest { public Unit caster; public Unit[] targets; public SkillRuntime skill; public string tag; public int actionId; public ActionPriority priority=ActionPriority.Immediate; }
  public sealed class ActionScheduler {
    readonly List<(ActionRequest,int)> heap = new();
    public void Enqueue(ActionRequest r){ Push((r,(int)r.priority)); }
    public ActionRequest Dequeue(){ return Pop().Item1; }
    public int Count => heap.Count;
    public void DrainImmediate(Action<int,ActionRequest> exec, int actionId){ while(Count>0 && heap[0].Item2==(int)ActionPriority.Immediate){ var r=Dequeue(); r.actionId=actionId; exec?.Invoke(actionId,r);} }
    void Push((ActionRequest,int) item){ heap.Add(item); int i=heap.Count-1; while(i>0){ int p=(i-1)/2; if(heap[p].Item2<=heap[i].Item2) break; (heap[p],heap[i])=(heap[i],heap[p]); i=p; } }
    (ActionRequest,int) Pop(){ var ret=heap[0]; int n=heap.Count-1; heap[0]=heap[n]; heap.RemoveAt(n); int i=0; while(true){ int l=i*2+1, r=l+1; if(l>=heap.Count) break; int m=(r<heap.Count && heap[r].Item2<heap[l].Item2)? r:l; if(heap[i].Item2<=heap[m].Item2) break; (heap[i],heap[m])=(heap[m],heap[i]); i=m; } return ret; }
  }
}
