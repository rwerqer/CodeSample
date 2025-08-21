//====================================================
// 파일: IPlatformFactory.cs
// 역할: 플랫폼 생성/재사용(풀) 계약
// 패턴: Factory + Object Pool
// 협력자: PlatformSpawner(스폰/클린업)
// 주의: 메인 스레드 전용, 외부 오브젝트 재활용 방지
//====================================================

using System.Collections.Generic;
using UnityEngine;

public interface IPlatformFactory
{
    /// <summary>
    /// 생성된 플랫폼이 귀속되는 부모 트랜스폼(계층 정리/일괄 해제용).
    /// </summary>
    Transform Container { get; }

    /// <summary>
    /// 지정 위치(및 선택적 회전)로 플랫폼을 제공합니다.
    /// 풀에 여분이 있으면 재활성화, 없으면 인스턴스화합니다.
    /// </summary>
    GameObject CreatePlatform(Vector3 position);

    /// <summary>
    /// 플랫폼을 풀로 회수합니다.
    /// </summary>
    void Recycle(GameObject platform);
}
