//====================================================
// 파일: IDayGameLoopManager.cs
// 역할: 낮(주간) 파트 게임 루프의 외부 계약 정의
// 패턴: Facade, Coordinator
// 협력자: DataManager/ResourceManager, DocumentManager, UI(Presenter)
//====================================================

public interface IDayGameLoopManager
{
    /// <summary>
    /// 플레이어/리소스를 로드하고 UI/난이도를 초기화한 뒤
    /// 낮 파트 루프를 시작합니다.
    /// </summary>
    void InitScene();

    /// <summary>
    /// 현재 문서에 대한 판정 결과를 전달합니다.
    /// </summary>
    /// <param name="isAccepted">true=승인, false=거절</param>
    void OnJudgmentSubmitted(bool isAccepted);

    /// <summary>
    /// 당일 루프를 종료하고 결과 요약을 반환합니다.
    /// </summary>
    /// <returns>일일 결과 데이터(누계 집계)</returns>
    DayResultData EndDay();
}
