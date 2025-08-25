// File: Runtime/Skills/SkillData.cs
using UnityEngine; using System.Collections.Generic;
namespace PM.Tactics {
    public sealed class SkillRuntime { public ClashSkillData Data; public SkillRuntime(ClashSkillData d){ Data=d; } }

    [CreateAssetMenu(menuName="Combat/ClashSkill")]
    public class ClashSkillData : ScriptableObject {
        public string id;
        public List<SkillCoin> coins;   // 코인들
        public int basePower = 0; 
        public List<string> tags;
        public List<EffectBinding> effects;
    }
    [System.Serializable]
  public class SkillCoin {
    public int coinPower; // 앞면일 경우 추가
    [SerializeReference] public List<CoinModAsset> mods; // ⬅️ 결과론적 코인 특수효과
  }
}

