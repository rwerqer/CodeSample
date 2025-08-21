//====================================================
// 파일: PlatformSpawner.cs
// 역할: 상단 근처에서 플랫폼(및 장애물/광원) 스폰, 하단 디스폰 트리거
// 패턴: Spawner, Coordinator
// 협력자: IMapManager(경계/진행·카메라 질의), IPlatformFactory(생성/풀)
// 주의: Initialize 선행, 프리팹 미지정/널 가드 필요(에디터 설정)
//====================================================

using UnityEngine;

public class PlatformSpawner : MonoBehaviour
{
    // ---------- References ----------
    private IMapManager mapManager;
    private IPlatformFactory platformFactory;

    // ---------- Tunables (Inspector) ----------
    [SerializeField] private GameObject obstaclePrefab;
    [SerializeField] private GameObject lightPrefab;

    // ---------- State ----------
    private bool initialized = false;

    /// <summary>
    /// 맵/팩토리 의존성을 주입하고 최초 플랫폼을 배치합니다.
    /// </summary>
    public void Initialize(IMapManager mapManager, IPlatformFactory platformFactory)
    {
        this.mapManager = mapManager;
        this.platformFactory = platformFactory;

        Vector3 initialPos = mapManager.GetInitialPlatformPosition();
        GameObject platform = platformFactory.CreatePlatform(initialPos);
        mapManager.ReportPlatformSpawned(initialPos);
        initialized = true;
    }

    // 스폰 간격(수직 거리 개념). 난이도에 따라 증가하도록 설계됨.
    private float spawnInterval = 2.3f;
    // 광원 스폰 주기(플랫폼 스폰 루프 내 카운트다운)
    private int spawnLightInterval = 12;

    private void Update()
    {
        if (!initialized || !mapManager.IsGenerating) return;

        // 카메라가 상단에서 10f 이내로 접근하면 다음 플랫폼 그룹을 생성
        if (mapManager.GetCameraPosition().y > mapManager.CurrentTopY - 10f)
        {
            SpawnPlatform();
        }

        CleanupPlatforms();
    }

    /// <summary>
    /// 가로 경계를 등분해 2~3개의 플랫폼을 같은 높이에 스폰합니다.
    /// 난이도에 따라 세로 간격을 조정하고, 확률적으로 장애물/광원을 배치합니다.
    /// </summary>
    private void SpawnPlatform()
    {
        int platformCount = Random.Range(2, 4); // 2~3개
        Vector2 bounds = mapManager.GetHorizontalBounds();
        float segmentWidth = (bounds.y - bounds.x) / platformCount;

        // 난이도에 따라 수직 간격을 점진적으로 증가(최대 3.5f까지)
        spawnInterval = Mathf.Min(spawnInterval + mapManager.GetDifficultyLevel() / 3.8f, 3.5f);

        float y = mapManager.CurrentTopY + spawnInterval;
        --spawnLightInterval;

        for (int i = 0; i < platformCount; i++)
        {
            float minX = bounds.x + segmentWidth * i;
            float maxX = minX + segmentWidth;
            float x = Random.Range(minX + 0.3f, maxX - 0.3f);

            Vector3 pos = new Vector3(x, y, 0f);
            GameObject platform = platformFactory.CreatePlatform(pos);
            mapManager.ReportPlatformSpawned(pos);

            // 난이도 기반 장애물 확률(상한 22%)
            float obstacleChance = Mathf.Min(Mathf.Clamp01(0.05f + 0.02f * mapManager.GetDifficultyLevel()), 0.22f);
            if (Random.value < obstacleChance)
            {
                PlaceObstacle(platform);
            }

            // 일정 간격마다 광원 배치(난이도↑ 시 주기↓, 하한 6)
            if (spawnLightInterval == 0)
            {
                PlaceLight(platform);
                spawnLightInterval = Mathf.Max(12 - mapManager.GetDifficultyLevel() * 3, 6);
            }
        }
    }

    private void PlaceObstacle(GameObject platform)
    {
        if (obstaclePrefab == null) return;
        Vector3 pos = platform.transform.position + Vector3.up * 0.4f;
        GameObject obs = Instantiate(obstaclePrefab, pos, Quaternion.identity);
        obs.transform.SetParent(platform.transform);
    }

    private void PlaceLight(GameObject platform)
    {
        if (lightPrefab == null) return;
        Vector3 pos = platform.transform.position + Vector3.up * 1.4f;
        GameObject lightObj = Instantiate(lightPrefab, pos, Quaternion.identity);
        lightObj.transform.SetParent(platform.transform);
    }

    /// <summary>
    /// 카메라 하단 기준보다 내려간 플랫폼을 풀로 회수합니다.
    /// </summary>
    public void CleanupPlatforms()
    {
        float despawnY = mapManager.GetDespawnY();
        foreach (Transform platform in platformFactory.Container)
        {
            if (platform.position.y < despawnY && platform.gameObject.activeSelf)
            {
                platformFactory.Recycle(platform.gameObject);
            }
        }
    }
}
