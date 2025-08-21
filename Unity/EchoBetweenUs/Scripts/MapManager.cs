//====================================================
// 파일: MapManager.cs
// 역할: 맵 가로 경계/카메라 상단 기준/디스폰 Y 계산 + 진행(스크롤·점수) 오케스트레이션
// 패턴: Facade, Coordinator
// 협력자: DifficultyManager, ICameraMover, startSceneManager
// 주의: Initialize 순서 보장, 정사영 카메라 가정
//====================================================

using UnityEngine;

public class MapManager : MonoBehaviour, IMapManager
{
    // ---------- Bounds ----------
    private float boundsMinX = -3f;
    private float boundsMaxX = 3f;

    /// <summary>월드 좌표 기준 가로 경계를 지정합니다.</summary>
    public void SetHorizontalBounds(float minX, float maxX)
    {
        boundsMinX = minX;
        boundsMaxX = maxX;
    }

    // ---------- Tunables ----------
    [SerializeField] private float despawnMargin = 2f;
    [SerializeField] private float initialPlatformY = -4f;

    // ---------- References ----------
    [SerializeField] private MonoBehaviour cameraMoverObject;
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private DifficultyManager difficultyManager;

    // ---------- State ----------
    [SerializeField] private float difficulty;
    [SerializeField] private bool isOverridingCamera = true;
    private float currentTopY;
    private float cameraTopY;
    private bool isGenerating = false;

    // ---------- Scoring ----------
    public float scoringCooldown = 1f;
    public float scoringCooldownValue = 1f;

    /// <summary>카메라 y를 MapManager에서 직접 제어할지 설정합니다.</summary>
    public void SetCameraOverride(bool value) => isOverridingCamera = value;

    /// <summary>카메라 상단 기준 y를 외부에서 덮어씁니다.</summary>
    public void OverrideCameraY(float y) => cameraTopY = y;

    /// <summary>맵 초기화(카메라/플레이어 주입, 경계 산출, ICameraMover 초기 생성 작업).</summary>
    public void Initialize(Camera camera, Transform player)
    {
        mainCamera = camera;
        playerTransform = player;
        cameraTopY = mainCamera.transform.position.y;

        float halfHeight = mainCamera.orthographicSize;
        float halfWidth = halfHeight * mainCamera.aspect;
        boundsMinX = mainCamera.transform.position.x - halfWidth;
        boundsMaxX = mainCamera.transform.position.x + halfWidth;

        if (cameraMoverObject is ICameraMover mover)
        {
            mover.InjectCamera(camera);
            mover.InjectPlayer(playerTransform);
            mover.CreateWallColliders();
            mover.CreateGameOverLine();
        }
    }

    private void Awake()
    {
        currentTopY = -5f;
    }

    /// <summary>현재까지 스폰된 플랫폼의 최고 y.</summary>
    public float CurrentTopY => currentTopY;

    /// <summary>맵 진행 루프(스크롤/스코어) 가동 여부.</summary>
    public bool IsGenerating => isGenerating;

    /// <summary>월드 기준 가로 경계(minX, maxX).</summary>
    public Vector2 GetHorizontalBounds() => new Vector2(boundsMinX, boundsMaxX);

    private void Update()
    {
        if (scoringCooldown > 0) scoringCooldown -= Time.deltaTime;
        if (!isGenerating) return;

        difficulty = difficultyManager.CalculateDifficulty(currentTopY);

        // 난이도 기반 카메라 상승 속도(하한·중간상한·최종상한 순서로 clamp)
        float baseSpeed = 0.2f;
        float maxSpeed = 0.7f;
        float speed = Mathf.Min(baseSpeed + difficulty * 2.4f,
                                maxSpeed  + difficulty * 0.3f,
                                1.8f);

        cameraTopY += speed * Time.deltaTime;

        if (scoringCooldown <= 0)
        {
            scoringCooldown = scoringCooldownValue;
            startSceneManager.Instance.AddScore((int)(difficulty) * 10 + 10);
        }

        if (isOverridingCamera)
        {
            mainCamera.transform.position = new Vector3(
                mainCamera.transform.position.x,
                cameraTopY,
                mainCamera.transform.position.z);
        }
    }

    /// <summary>최초 플랫폼 스폰 위치를 반환합니다.</summary>
    public Vector3 GetInitialPlatformPosition() => new Vector3(0f, initialPlatformY, 0f);

    /// <summary>스폰된 플랫폼의 위치를 보고하여 최고 y를 갱신합니다.</summary>
    public void ReportPlatformSpawned(Vector3 pos)
    {
        currentTopY = Mathf.Max(currentTopY, pos.y);
    }

    /// <summary>화면 하단보다 더 아래로 내려간 오브젝트 제거 기준 y를 반환합니다.</summary>
    public float GetDespawnY() => cameraTopY - mainCamera.orthographicSize - despawnMargin;

    /// <summary>현재 진행 높이에 기반한 난이도 레벨(정수)을 반환합니다.</summary>
    public int GetDifficultyLevel() => difficultyManager.CalculateDifficulty(currentTopY);

    /// <summary>맵 진행(스크롤/스코어링) 루프를 시작합니다.</summary>
    public void StartMapGeneration() => isGenerating = true;

    /// <summary>현재 카메라 위치(월드)를 반환합니다.</summary>
    public Vector3 GetCameraPosition() => mainCamera.transform.position;

    /// <summary>메인 카메라 참조를 반환합니다.</summary>
    public Camera GetMainCamera() => mainCamera;
}
