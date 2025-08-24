//====================================================
// 파일: ResourceManager.cs
// 역할: 런타임 자원 상태 저장/조회/동기화 (money, fame → difficulty)
// 패턴: Singleton, Repository/Facade
// 협력자: GameLoopManager(자원 음수 시 GameOver 전이), PlayerData 소비자들
// 주의:
//  - Unity 기본 직렬화는 Dictionary를 인스펙터에 표시/보존하지 않음(런타임용으로만 사용)
//  - GameLoopManager.Instance 접근 시 널 가능성(초기화 타이밍 주의)
//  - 난이도 정책: fame 1000당 +1 (정수 나눗셈)
//====================================================

using System.Collections.Generic;
using UnityEngine;

public class ResourceManager : MonoBehaviour
{
    // ---------- Singleton ----------
    public static ResourceManager Instance { get; private set; }

    // ---------- State ----------
    [SerializeField] private Dictionary<string, int> resources = new Dictionary<string, int>();
    public PlayerData currentPlayerData { get; private set; }

    private void Awake()
    {
        // 싱글톤 보장
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        // 초기 자원 세팅(데모 값)
        AddResource("money", 1000);
        AddResource("fame",  900);

        currentPlayerData = new PlayerData
        {
            money = 0,
            fame = 0
        };
        UpdatePlayerData();
    }

    /// <summary>
    /// 내부 자원 상태를 초기화하고 기본 자원을 재설정합니다.
    /// </summary>
    public void ResetResource()
    {
        ClearResources();
        AddResource("money", 1000);
        AddResource("fame",  900);
        UpdatePlayerData();
    }

    /// <summary>
    /// 지정한 자원에 양을 더합니다(없으면 생성). 결과가 음수면 게임 오버 전이.
    /// </summary>
    /// <param name="resourceName">자원 키(예: "money", "fame")</param>
    /// <param name="amount">증감 값(음수 허용)</param>
    public void AddResource(string resourceName, int amount)
    {
        if (!resources.ContainsKey(resourceName))
        {
            resources[resourceName] = 0;
        }
        resources[resourceName] += amount;

        // 음수 보호 정책: 즉시 GameOver 전이
        if (resources[resourceName] < 0)
            GameLoopManager.Instance.SetState(GameState.GameOver);
    }

    /// <summary>
    /// 자원 값을 조회합니다. 없으면 0을 반환합니다.
    /// </summary>
    public int GetResourceAmount(string resourceName)
    {
        return resources.TryGetValue(resourceName, out int value) ? value : 0;
    }

    /// <summary>
    /// PlayerData에 현재 자원을 반영하고 난이도를 갱신합니다.
    /// </summary>
    /// <remarks>난이도 = fame / 1000 (정수 나눗셈)</remarks>
    public void UpdatePlayerData()
    {
        currentPlayerData.fame       = GetResourceAmount("fame");
        currentPlayerData.money      = GetResourceAmount("money");
        currentPlayerData.difficulty = currentPlayerData.fame / 1000;
    }

    /// <summary>
    /// 현재 보유 자원을 디버그 로그로 출력합니다.
    /// </summary>
    public void PrintAllResources()
    {
        Debug.Log("==== 현재 자원 ====");
        foreach (var pair in resources)
        {
            Debug.Log($"{pair.Key}: {pair.Value}");
        }
    }

    /// <summary>
    /// 모든 자원을 초기화하고 PlayerData도 리셋합니다.
    /// </summary>
    public void ClearResources()
    {
        resources.Clear();
        currentPlayerData.money      = 0;
        currentPlayerData.fame       = 0;
        currentPlayerData.difficulty = 0;
    }
}
