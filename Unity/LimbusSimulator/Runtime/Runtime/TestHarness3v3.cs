// File: Runtime/Runtime/TestHarness3v3.cs
using UnityEngine;
using System.Collections.Generic;

namespace PM.Tactics {
  public sealed class TestHarness3v3 : MonoBehaviour {
    [Header("Refs")]
    public CombatManager manager;          // 씬의 CombatManager
    public GameObject unitPrefab;          // 단순한 큐브나 빈 GO + Unit 붙인 프리팹
    public ClashSkillData skillA0, skillA1, skillA2;
    public ClashSkillData skillB0, skillB1, skillB2;

    [Header("Positions")]
    public Vector3 teamAOrigin = new Vector3(-4, 0, 0);
    public Vector3 teamBOrigin = new Vector3( 4, 0, 0);
    public float laneGap = 2.0f;

    [Header("Speed Ranges (inclusive)")]
    public Vector2Int a0Speed = new Vector2Int(11,13);
    public Vector2Int a1Speed = new Vector2Int(9,11);
    public Vector2Int a2Speed = new Vector2Int(7,9);
    public Vector2Int b0Speed = new Vector2Int(10,12);
    public Vector2Int b1Speed = new Vector2Int(8,10);
    public Vector2Int b2Speed = new Vector2Int(6,8);
    public int hp = 100;

    void Start() {
      if (manager == null) {
        manager = FindObjectOfType<CombatManager>();
        if (manager == null) { Debug.LogError("CombatManager not found."); return; }
      }
      if (unitPrefab == null) { Debug.LogError("Assign a Unit prefab."); return; }

      // Team A
      var A = new List<Unit>(3);
      A.Add(SpawnUnit("A0", teamAOrigin + new Vector3(0, 0, 0), a0Speed, skillA0));
      A.Add(SpawnUnit("A1", teamAOrigin + new Vector3(0, laneGap, 0), a1Speed, skillA1));
      A.Add(SpawnUnit("A2", teamAOrigin + new Vector3(0,-laneGap, 0), a2Speed, skillA2));

      // Team B
      var B = new List<Unit>(3);
      B.Add(SpawnUnit("B0", teamBOrigin + new Vector3(0, 0, 0), b0Speed, skillB0));
      B.Add(SpawnUnit("B1", teamBOrigin + new Vector3(0, laneGap, 0), b1Speed, skillB1));
      B.Add(SpawnUnit("B2", teamBOrigin + new Vector3(0,-laneGap, 0), b2Speed, skillB2));

      // CombatManager에 팀 등록 (배치 순서=커밋 순서)
      manager.Battlefield.TeamA.Clear();
      manager.Battlefield.TeamB.Clear();
      manager.Battlefield.TeamA.AddRange(A);
      manager.Battlefield.TeamB.AddRange(B);
    }

    Unit SpawnUnit(string name, Vector3 pos, Vector2Int speedRange, ClashSkillData skill) {
      var go = Instantiate(unitPrefab, pos, Quaternion.identity);
      go.name = name;

      var rend = go.GetComponentInChildren<Renderer>();
      if (rend != null) rend.material.color = (name.StartsWith("A") ? Color.cyan : Color.magenta);

      var u = go.GetComponent<Unit>();
      if (u == null) u = go.AddComponent<Unit>();
      u.unitName = name;
      u.MaxHP = u.HP = hp;

      // ✅ 범위 세팅(턴마다 CombatManager가 RollTurnSpeed로 굴림)
      u.MinSpeed = Mathf.Min(speedRange.x, speedRange.y);
      u.MaxSpeed = Mathf.Max(speedRange.x, speedRange.y);

      u.skill = skill;
      return u;
    }
  }
}
