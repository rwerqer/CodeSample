//====================================================
// 파일: JudgmentManager.cs
// 역할: 판정 UI 버튼 이벤트 → 낮 게임 루프에 위임(승인/거절 전달)
// 패턴: Presenter(입력 라우팅), Command(간접 호출)
// 협력자: DayGameLoopManager, UI(Button)
//====================================================

using UnityEngine;

public class JudgmentManager : MonoBehaviour
{
    // ---------- References ----------
    public DayGameLoopManager gameLoop; 

    /// <summary>
    /// 승인 버튼 콜백. 현재 문서에 대해 승인 판정을 게임 루프에 위임합니다.
    /// </summary>
    public void OnAcceptPressed()
    {
        gameLoop.OnJudgmentSubmitted(true);
    }

    /// <summary>
    /// 거절 버튼 콜백. 현재 문서에 대해 거절 판정을 게임 루프에 위임합니다.
    /// </summary>
    public void OnDeclinePressed()
    {
        gameLoop.OnJudgmentSubmitted(false);
    }
}
