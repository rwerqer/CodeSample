//====================================================
// 파일: PlatformInitializer.cs
// 역할: 플랫폼 초기 폭(random) 설정 및 물리/콜라이더 동기화 트리거
// 패턴: Initializer, Configurator
// 협력자: PlatformPhysics(콜라이더/에코 트리거 동기화)
// 주의: 스케일 변경 시 콜라이더/이펙터 재설정 순서 보장 필요
//====================================================

using UnityEngine;

public class PlatformInitializer : MonoBehaviour
{
    // ---------- Tunables (Inspector) ----------
    //[SerializeField] private float minWidth = 1f;
    //[SerializeField] private float maxWidth = 6f;

    /// <summary>
    /// 플랫폼의 초기 파라미터를 설정하고 물리 구성을 동기화합니다.
    /// </summary>
    public void Initialize()
    {
        // 의도: 시작 시 플랫폼의 가로 폭을 랜덤 지정(게임 플레이 변주)
        //float width = Random.Range(minWidth, maxWidth);
        // transform.localScale = new Vector3(width, 1f, 1f);

        // 물리/에코 콜라이더 동기화 (스프라이트/스케일과 일치)
        var physics = GetComponent<PlatformPhysics>();
        if (physics != null)
            physics.SyncAll();
    }
}
