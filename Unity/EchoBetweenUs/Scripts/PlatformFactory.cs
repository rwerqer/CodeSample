//====================================================
// 파일: PlatformFactory.cs
// 역할: 플랫폼 프리팹 생성 및 재사용(Object Pool) 관리
// 패턴: Factory, Object Pool
// 협력자: PlatformInitializer(상태 리셋)
// 주의: 메인 스레드 전용, 프리팹/컨테이너 미지정 시 NRE 주의
//====================================================

using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 플랫폼 생성/회수를 담당하는 팩토리.
/// 풀을 통해 런타임 할당과 Instantiate 스파이크를 최소화한다.
/// </summary>
public class PlatformFactory : MonoBehaviour, IPlatformFactory
{
    // ---------- Tunables (Inspector) ----------
    [SerializeField] private GameObject platformPrefab;
    [SerializeField] private Transform container;

    // ---------- State ----------
    private readonly Queue<GameObject> pool = new();

    /// <summary>생성된 플랫폼을 수용하는 부모 트랜스폼(계층 정리/일괄 해제용).</summary>
    public Transform Container => container;

    /// <summary>
    /// 플랫폼을 생성(혹은 풀에서 꺼내 재활성화)합니다.
    /// </summary>
    /// <param name="position">스폰 월드 위치</param>
    /// <returns>활성화된 플랫폼 GameObject</returns>
    /// <remarks>
    /// - 풀에 여분이 없으면 <c>Instantiate(platformPrefab)</c>로 새로 할당됩니다. <br/>
    /// - 부모를 <see cref="container"/>로 설정하여 계층을 정리합니다. <br/>
    /// - <see cref="PlatformInitializer"/>가 존재하면 상태 초기화를 호출합니다. <br/>
    /// - 스레드: 메인 스레드 전용. 성능: 풀 사용 시 추가 GC 없음.
    /// </remarks>
    public GameObject CreatePlatform(Vector3 position)
    {
        GameObject platform;
        if (pool.Count > 0)
        {
            platform = pool.Dequeue();
            platform.SetActive(true);
        }
        else
        {
            platform = Instantiate(platformPrefab);
        }

        // 계층 정리: 컨테이너 하위로 편입(일괄 해제/이동 편의)
        platform.transform.SetParent(container);
        platform.transform.position = position;

        // 초기화 훅: 각 컴포넌트의 내부 상태/비주얼을 리셋
        var initializer = platform.GetComponent<PlatformInitializer>();
        initializer?.Initialize();

        return platform;
    }

    /// <summary>
    /// 플랫폼을 풀로 되돌립니다(비활성화 후 큐에 적재).
    /// </summary>
    /// <param name="platform">회수할 플랫폼</param>
    /// <remarks>
    /// - 중복 회수 금지(호출자 보장 필요). 
    /// </remarks>
    public void Recycle(GameObject platform)
    {
        platform.SetActive(false);
        pool.Enqueue(platform);
    }
}
