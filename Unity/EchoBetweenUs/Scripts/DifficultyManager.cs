//====================================================
// 파일: DifficultyManager.cs
// 역할: 진행 높이(월드 y)에 따른 난이도 레벨 계산
// 협력자: MapManager(현재 높이→난이도 질의)
//====================================================

using UnityEngine;

public class DifficultyManager : MonoBehaviour
{
    public int CalculateDifficulty(float height)
    {
        return Mathf.FloorToInt(height / 20f);
    }
}