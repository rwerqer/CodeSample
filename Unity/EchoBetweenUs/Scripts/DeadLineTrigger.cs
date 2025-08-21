//====================================================
// 파일: DeadLineTrigger.cs
// 역할: 게임오버선(Trigger)에 플레이어가 진입/잔류 시 게임오버 호출
// 패턴: Trigger/Response
// 협력자: startSceneManager(게임오버 로드), Player(Tag="Player")
// 주의: 태그 의존, 중복 트리거 방지 플래그 사용
//====================================================

using UnityEngine;


public class DeadLineTrigger : MonoBehaviour
{
    private bool isGameOverTriggered = false;
    private void OnTriggerStay2D(Collider2D other)
    {
        if (!isGameOverTriggered && other.CompareTag("Player"))
        {
            isGameOverTriggered = true;
            Debug.Log("[GameOver] Player inside deadline.");
            startSceneManager.Instance.LoadGameOver();
        }
    }
}