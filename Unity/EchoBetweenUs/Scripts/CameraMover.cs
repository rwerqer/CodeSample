//====================================================
// 파일: CameraMover.cs
// 역할: 플레이어 기준 카메라 추적/해제 전환, 수직 스크롤 보조,
//       게임오버선·좌우 벽 콜라이더 구성
// 패턴: Strategy(ICameraMover), Threshold Trigger, Coordinator
// 협력자: MapManager(카메라 제어권 토글/기준 y 동기), DeadLineTrigger, startSceneManager(스코어)
// 주의: 정사영 카메라 가정, 제어권 충돌 방지(MapManager.Override/SetCameraOverride와 상호작용)
//====================================================

using System;
using UnityEngine;

public class CameraMover : MonoBehaviour, ICameraMover
{
    // ---------- References ----------
    [SerializeField] private Transform playerTransform;
    [SerializeField] private Camera targetCamera;
    [SerializeField] private MapManager mapManager;

    // ---------- Tunables (Inspector) ----------
    [SerializeField] private float followSpeed = 6.5f;          // 추적 속도(월드 유닛/초)
    [SerializeField] private float viewportThresholdY = 0.75f;  // 플레이어가 이 이상일 때 추적 시작
    [SerializeField] private float releaseThresholdY = 0.6f;    // 이 이하로 내려가면 추적 해제

    // 게임오버선(카메라 하단 기준), 연출 프리팹
    private GameObject deadLine;
    [SerializeField] private float deadLineOffset = -2f;
    [SerializeField] private GameObject deadLineParticlePrefab;

    // 좌우 벽(카메라 폭 + 버퍼)
    private GameObject leftWall;
    private GameObject rightWall;
    [SerializeField] private float wallThickness = 1f;
    [SerializeField] private float wallHeightBuffer = 20f; // 카메라 높이보다 여유
    [SerializeField] private LayerMask wallLayer;          // 단일 레이어 가정(하단 주석 참조)

    // ---------- Events ----------
    /// <summary>
    /// 플레이어가 상단 임계치를 초과해 카메라 추적이 시작될 때,
    /// 플레이어-카메라 y 차이를 점수로 신호합니다.
    /// </summary>
    public static event Action<float> OnScoreSignal;

    // ---------- State ----------
    [SerializeField] private bool isPlayerHigh = false; // 임계 초과 상태(추적 중)

    /// <summary>외부에서 대상 카메라를 주입합니다.</summary>
    public void InjectCamera(Camera camera) => targetCamera = camera;

    /// <summary>외부에서 플레이어 트랜스폼을 주입합니다.</summary>
    public void InjectPlayer(Transform p) => playerTransform = p;

    /// <summary>
    /// 카메라를 스크롤 양만큼 이동시킵니다(수직 스크롤 보조).
    /// </summary>
    /// <param name="delta">y 증가량(월드 유닛)</param>
    public void MoveCameraByScroll(float delta)
    {
        targetCamera.transform.position += new Vector3(0, delta, 0);
    }

    private void Update()
    {
        if (playerTransform == null || targetCamera == null) return;

        // 플레이어의 뷰포트 y(0~1)를 기반으로 추적/해제 트리거
        Vector3 viewportPos = targetCamera.WorldToViewportPoint(playerTransform.position);
        float playerYInViewport = viewportPos.y;

        // 추적 시작: 상단 임계치 초과 시 MapManager의 오버라이드 해제 + 점수 신호
        if (!isPlayerHigh && playerYInViewport > viewportThresholdY)
        {
            isPlayerHigh = true;
            mapManager.SetCameraOverride(false);

            float scoreDelta = playerTransform.position.y - targetCamera.transform.position.y;
            OnScoreSignal?.Invoke(scoreDelta);
            startSceneManager.Instance.AddScore((int)(scoreDelta) * 10);
        }

        if (isPlayerHigh)
        {
            // 추적 중: 카메라 y를 플레이어 y로 MoveTowards
            float targetY = playerTransform.position.y;
            Vector3 current = targetCamera.transform.position;
            float newY = Mathf.MoveTowards(current.y, targetY, followSpeed * Time.deltaTime);
            targetCamera.transform.position = new Vector3(current.x, newY, current.z);

            // 해제: 하강하여 하단 임계치 미만으로 떨어지면 추적 종료 + MapManager 기준 y 동기
            if (playerYInViewport < releaseThresholdY)
            {
                isPlayerHigh = false;
                mapManager.OverrideCameraY(targetCamera.transform.position.y);
                mapManager.SetCameraOverride(true);
            }
        }
    }

    /// <summary>
    /// 카메라 하단에 게임오버선을 생성합니다(트리거 + 파티클). 카메라에 부모로 붙습니다.
    /// </summary>
    public void CreateGameOverLine()
    {
        if (targetCamera == null) return;

        float camHeight = 2f * targetCamera.orthographicSize;
        float camWidth  = camHeight * targetCamera.aspect;
        Vector3 camPos  = targetCamera.transform.position;

        deadLine = new GameObject("DeadLine");
        BoxCollider2D deadCol = deadLine.AddComponent<BoxCollider2D>();
        deadCol.isTrigger = true;
        deadCol.size = new Vector2(camWidth + 2f, 1f);
        deadLine.layer = LayerMask.NameToLayer("Default");
        deadLine.transform.position = new Vector3(
            camPos.x,
            camPos.y - camHeight / 2f + deadLineOffset,
            0f
        );
        deadLine.transform.parent = targetCamera.transform;

        if (deadLineParticlePrefab != null)
        {
            float camBottomY = targetCamera.transform.position.y - targetCamera.orthographicSize - 1.8f;
            Vector3 spawnPos = new Vector3(targetCamera.transform.position.x, camBottomY, 0f);
            GameObject particles = Instantiate(deadLineParticlePrefab, spawnPos, Quaternion.identity);
            particles.transform.SetParent(targetCamera.transform);
            Debug.Log("Particle Instantiated: " + particles.name);
        }

        deadLine.AddComponent<DeadLineTrigger>();
    }

    /// <summary>
    /// 카메라 좌우에 반영구 벽(Static Rigidbodies + BoxCollider2D)을 생성합니다.
    /// </summary>
    public void CreateWallColliders()
    {
        if (targetCamera == null) return;

        float camHeight = 2f * targetCamera.orthographicSize;
        float camWidth  = camHeight * targetCamera.aspect;
        float wallHeight = camHeight + wallHeightBuffer;
        Vector3 camPos = targetCamera.transform.position;

        // 단일 레이어 마스크에서 레이어 인덱스를 추출
        int layerIndex = Mathf.RoundToInt(Mathf.Log(wallLayer.value, 2));

        leftWall = new GameObject("LeftWall");
        var leftCol = leftWall.AddComponent<BoxCollider2D>();
        leftCol.size = new Vector2(wallThickness, wallHeight);
        leftWall.layer = layerIndex;
        leftWall.tag = "reflectable";
        leftWall.transform.position = new Vector3(
            camPos.x - camWidth / 2f - wallThickness - 1f,
            camPos.y,
            0f
        );
        leftWall.transform.parent = targetCamera.transform;

        rightWall = new GameObject("RightWall");
        var rightCol = rightWall.AddComponent<BoxCollider2D>();
        rightCol.size = new Vector2(wallThickness, wallHeight);
        rightWall.layer = layerIndex;
        rightWall.tag = "reflectable";
        rightWall.transform.position = new Vector3(
            camPos.x + camWidth / 2f + wallThickness + 1f,
            camPos.y,
            0f
        );
        rightWall.transform.parent = targetCamera.transform;

        var leftRb = leftWall.AddComponent<Rigidbody2D>();  leftRb.bodyType  = RigidbodyType2D.Static;
        var rightRb = rightWall.AddComponent<Rigidbody2D>(); rightRb.bodyType = RigidbodyType2D.Static;
    }

    private void LateUpdate()
    {
        // 카메라 이동 후 게임오버선 y를 최신 하단에 정렬
        float camY = targetCamera.transform.position.y;
        float camHeight = 2f * targetCamera.orthographicSize;
        deadLine.transform.position = new Vector3(deadLine.transform.position.x, camY - camHeight / 2f + deadLineOffset, 0f);
    }
}
