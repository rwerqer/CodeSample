//====================================================
// 파일: ObstacleBehavior.cs
// 역할: 플레이어와 충돌 시 넉백 효과/사운드 트리거
// 패턴: Trigger/Response(MonoBehaviour)
// 협력자: KnockbackSignal, SoundManager, Rigidbody2D(Player)
// 주의: "Player" 태그 의존, 물리 모드는 2D(Impulse)
//====================================================

using UnityEngine;

public class ObstacleBehavior : MonoBehaviour
{
    /// <summary>
    /// 플레이어와 충돌하면 넉백 신호를 보내고 임펄스 힘을 가합니다.
    /// 좌/우 방향은 충돌 지점의 상대 위치로 결정됩니다.
    /// </summary>
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.collider.CompareTag("Player")) return;

        var rb = collision.collider.GetComponent<Rigidbody2D>();
        if (rb == null) return;

        // 전역 신호: 플레이어 컨트롤러/피격 UI 등이 구독
        KnockbackSignal.Invoke();

        // 넉백 벡터: 장애물 기준 좌우 방향(sign) + 위로 튀기기
        float direction = Mathf.Sign(collision.transform.position.x - transform.position.x);
        Vector2 knockback = new Vector2(direction * 22f, 15f);

        // 기존 운동량 제거 후 임펄스 부여
        rb.linearVelocity = Vector2.zero;
        rb.AddForce(knockback, ForceMode2D.Impulse);

        // 피격 사운드
        SoundManager.Instance.PlaySFX(10);

        Debug.Log($"[Obstacle] Player knockback {knockback}");
    }
}
