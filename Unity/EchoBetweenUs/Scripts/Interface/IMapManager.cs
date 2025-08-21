//====================================================
// 파일: IMapManager.cs
// 역할: 맵 진행·경계·카메라 기준·디스폰 라인 등 맵 전반의 질의/명령 계약 정의
// 패턴: Facade(읽기/질의 제공), Coordinator(진행 제어 시작 트리거)
// 협력자: DifficultyManager, ICameraMover, 스포너/스코어 시스템
// 주의: 월드 좌표계 기준, 정사영 카메라 가정
//====================================================

using UnityEngine;

public interface IMapManager
{
    /// <summary>
    /// 현재까지 스폰된 플랫폼의 최고 Y값.
    /// </summary>
    float CurrentTopY { get; }

    /// <summary>
    /// 화면 하단보다 더 아래로 내려간 오브젝트를 제거하기 위한 Y 기준값을 반환합니다. <br/>
    /// 일반적으로 <c>cameraTopY - orthographicSize - margin</c> 형태입니다.
    /// </summary>
    /// <returns>디스폰 임계 Y값</returns>
    float GetDespawnY();

    /// <summary>
    /// 새 플랫폼이 스폰되었음을 보고하여 최고 Y 기준을 갱신합니다.
    /// </summary>
    /// <param name="pos">스폰된 플랫폼의 월드 위치</param>
    void ReportPlatformSpawned(Vector3 pos);

    /// <summary>
    /// 현재 진행 높이에 기반한 난이도 레벨(정수)을 반환합니다.
    /// </summary>
    /// <returns>난이도 레벨(예: 0,1,2…)</returns>
    int GetDifficultyLevel();

    /// <summary>
    /// 맵 진행(카메라 스크롤/스코어링 등)을 시작합니다.
    /// </summary>
    void StartMapGeneration();

    /// <summary>
    /// 현재 카메라의 월드 위치를 반환합니다.
    /// </summary>
    /// <returns>카메라 Transform.position(월드 좌표)</returns>
    Vector3 GetCameraPosition();

    /// <summary>
    /// 맵 진행 루프의 가동 상태를 나타냅니다.
    /// </summary>
    bool IsGenerating { get; }

    /// <summary>
    /// 최초 플랫폼의 스폰 위치를 반환합니다.
    /// </summary>
    /// <returns>초기 플랫폼 위치(월드 좌표)</returns>
    Vector3 GetInitialPlatformPosition();

    /// <summary>
    /// 맵의 가로 경계(minX, maxX)를 반환합니다.
    /// </summary>
    /// <returns>좌우 경계(월드 좌표)</returns>
    Vector2 GetHorizontalBounds();

    /// <summary>
    /// 메인 카메라 핸들을 반환합니다. 소유권은 매니저가 유지합니다.
    /// </summary>
    /// <returns>메인 카메라 참조(널 아님, 초기화 이후 유효)</returns>
    Camera GetMainCamera();
}
