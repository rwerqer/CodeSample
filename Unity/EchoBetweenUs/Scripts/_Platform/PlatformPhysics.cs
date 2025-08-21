//====================================================
// 파일: PlatformPhysics.cs
// 역할: 플랫폼의 물리 구성(메인 BoxCollider2D + PlatformEffector2D)과
//       스프라이트 크기 동기화, 에코 트리거 콜라이더 생성/설정
// 패턴: Configurator/Initializer
// 협력자: SpriteRenderer, PlatformEffector2D, Physics2D, Player(Tag="Player")
// 주의: sprite.bounds(월드) ↔ BoxCollider2D.size(로컬) 단위 차이, 트리거-플레이어 충돌 무시 일관성
//====================================================

using UnityEngine;

[RequireComponent(typeof(BoxCollider2D), typeof(PlatformEffector2D))]
public class PlatformPhysics : MonoBehaviour
{
    private BoxCollider2D mainCollider;
    private BoxCollider2D echoCollider;

    private void Awake()
    {
        InitializeMainCollider();
        SyncMainColliderWithSprite();
        SetupPlatformEffector();
        SetupEchoCollider();
    }

    private void Reset()
    {
        InitializeMainCollider();
        SyncMainColliderWithSprite();
        SetupPlatformEffector();
        SetupEchoCollider();
    }

    /// <summary>
    /// 스프라이트-콜라이더 동기화와 에코 콜라이더 구성을 다시 수행합니다.
    /// </summary>
    public void SyncAll()
    {
        SyncMainColliderWithSprite();
        SetupEchoCollider(); // 이미 중복 생성을 방지함
    }

    // --- Main Collider ---

    private void InitializeMainCollider()
    {
        mainCollider = GetComponent<BoxCollider2D>();
        mainCollider.isTrigger = false;
        mainCollider.usedByEffector = true; // Effector가 이 콜라이더를 사용하도록
    }

    private void SyncMainColliderWithSprite()
    {
        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        if (sprite != null && mainCollider != null)
        {
            mainCollider.size = sprite.bounds.size;
            mainCollider.offset = Vector2.zero;
        }
    }

    // --- Effector ---

    private void SetupPlatformEffector()
    {
        PlatformEffector2D effector = GetComponent<PlatformEffector2D>();
        if (effector != null)
        {
            effector.useOneWay = true;   // 위에서만 설 수 있게 통과 허용
            effector.surfaceArc = 155f;  // 통과 허용 각도 범위
        }
    }

    // --- Echo Collider (Trigger Sensor) ---

    private void SetupEchoCollider()
    {
        Transform existing = transform.Find("EchoCollider");
        if (existing != null)
        {
            echoCollider = existing.GetComponent<BoxCollider2D>();
            IgnorePlayerCollisionWithEcho();
            return;
        }

        GameObject echoObj = new GameObject("EchoCollider");
        echoObj.transform.SetParent(transform);
        echoObj.transform.localPosition = Vector3.zero;

        echoCollider = echoObj.AddComponent<BoxCollider2D>();
        echoCollider.isTrigger = true;
        echoObj.tag = "EchoSensor";

        SpriteRenderer sprite = GetComponent<SpriteRenderer>();
        if (sprite != null)
        {
            Vector2 spriteSize = sprite.bounds.size;
            echoCollider.size = spriteSize * 1.08f;
            echoCollider.offset = Vector2.zero;
        }

        IgnorePlayerCollisionWithEcho();
    }

    private void IgnorePlayerCollisionWithEcho()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && echoCollider != null)
        {
            Collider2D playerCollider = player.GetComponent<Collider2D>();
            if (playerCollider != null)
            {
                Physics2D.IgnoreCollision(playerCollider, echoCollider);
            }
        }
    }
}
