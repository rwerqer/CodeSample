//====================================================
// 파일: ICameraMover.cs
// 역할: 카메라 추적/스크롤 보조 및 벽·게임오버선 생성 계약 정의
// 패턴: Strategy(카메라 이동 정책), Factory(보조 콜라이더 생성)
// 협력자: MapManager(제어권 토글/기준 y 동기), DeadLineTrigger
// 주의: Inject* 선행 필요, 정사영 카메라 가정, 생성 자원 해제 정책 명시 필요
//====================================================

using UnityEngine;

public interface ICameraMover
{
    /// <summary>
    /// 카메라를 수직으로 스크롤합니다(상대 이동).
    /// </summary>
    /// <param name="delta">증가시킬 y(월드 유닛)</param>
    void MoveCameraByScroll(float delta);

    /// <summary>
    /// 제어 대상 카메라를 주입합니다.
    /// </summary>
    /// <param name="camera">정사영 카메라(널 아님)</param>
    /// <remarks>선행 조건: 이후 메서드는 주입 완료를 가정합니다.</remarks>
    void InjectCamera(Camera camera);

    /// <summary>
    /// 추적 대상 플레이어 트랜스폼을 주입합니다.
    /// </summary>
    /// <param name="player">플레이어 트랜스폼(널 아님)</param>
    void InjectPlayer(Transform player);

    /// <summary>
    /// 카메라 좌우에 벽 콜라이더를 생성/배치합니다(카메라 폭+버퍼 기준).
    /// </summary>
    /// <remarks>카메라 이동에 따라 부모-자식으로 따라가야 합니다.</remarks>
    void CreateWallColliders();

    /// <summary>
    /// 카메라 하단에 게임오버 트리거 라인을 생성합니다.
    /// </summary>
    /// <remarks>카메라 트랜스폼 하위로 부착하여 하단에 정렬되도록 합니다.</remarks>
    void CreateGameOverLine();
}
