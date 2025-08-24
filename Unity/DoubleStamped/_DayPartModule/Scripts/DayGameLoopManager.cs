//====================================================
// 파일: DayGameLoopManager.cs
// 역할: 낮(주간) 파트의 게임 루프 오케스트레이션
//       - 씬 초기화(플레이어 데이터/난이도/UI 바인딩)
//       - 문서 생성/전시, 판정 결과 집계, 일일 결과 산출
// 패턴: Coordinator, Facade, State-like(진행 플래그 기반)
// 협력자: DataManager/ResourceManager, DocumentManager,
//         UIDocumentDisplayManager, UiSuoortDocDisplay, CodexOverlayUIManager
//====================================================

using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

public class DayGameLoopManager : MonoBehaviour, IDayGameLoopManager
{
    // ---------- References (Scene/UI) ----------
    public GameObject mainUICanvas;
    public DocumentManager documentManager;
    public UIDocumentDisplayManager uiDocDisplayer;
    public UiSupportDocDisplay uiSupDocDisplayer;
    public DataManager dataManager;
    public CodexOverlayUIManager codex;

    // ---------- Player/Data ----------
    public PlayerData currentPlayerData;
    [SerializeField] private DayResultData dayResult;

    // ---------- UI Text ----------
    public TextMeshProUGUI moneyText;
    public TextMeshProUGUI fameText;

    // ---------- State ----------
    private bool isRunning = false;

    private void Start()
    {
        InitScene();
        // DataManager 인스턴스 주입(미할당 시 싱글톤 폴백)
        dataManager = dataManager != null ? dataManager : DataManager.Instance;

        GenerateNextDocument();
        dayResult = new DayResultData();
    }

    /// <summary>
    /// 플레이어 데이터를 로드하여 UI/난이도를 초기화하고
    /// 낮 파트 루프를 시작합니다.
    /// </summary>
    /// <remarks>
    /// - 사전조건: ResourceManager 초기화 완료, UI 참조 유효<br/>
    /// - 효과: UI 활성화, DocumentManager 난이도 설정, 진행 플래그 on
    /// </remarks>
    public void InitScene()
    {
        ResourceManager.Instance.UpdatePlayerData();
        currentPlayerData = ResourceManager.Instance.currentPlayerData;

        mainUICanvas.SetActive(false);

        documentManager.SetDifficulty(currentPlayerData.difficulty);
        moneyText.text = $"{currentPlayerData.money:N0}";
        fameText.text  = $"{currentPlayerData.fame:N0}";

        mainUICanvas.SetActive(true);
        isRunning = true;
    }

    private void Update()
    {
        if (!isRunning) return;

        // 디버그 문서 생성
        // if (Input.GetMouseButtonDown(0)) GenerateNextDocument();
    }

    /// <summary>
    /// 사용자의 판정 결과(승인/거절)를 집계하고 다음 문서를 준비합니다.
    /// </summary>
    /// <param name="isAccepted">true=승인, false=거절</param>
    /// <remarks>
    /// - 정책: 위조 여부와의 일치 여부로 정오 판정<br/>
    /// - 효과: DayResultData 누계 갱신 → 다음 문서 전시
    /// </remarks>
    public void OnJudgmentSubmitted(bool isAccepted)
    {
        var currentDoc = documentManager.GetCurrentDocument();
        bool isForged = currentDoc.isForged;
        bool correct = (isAccepted && !isForged) || (!isAccepted && isForged);

        dayResult.totalChecked++;

        if (isAccepted)
        {
            dayResult.passedDocCount++;
            if (correct) dayResult.correctPassJudgments++;
            else         dayResult.incorrectPassJudgments++;
        }
        else
        {
            dayResult.rejectedDocCount++;
            if (correct) dayResult.correctRejectJudgments++;
            else         dayResult.incorrectRejectJudgments++;
        }

        GenerateNextDocument();
    }

    /// <summary>
    /// 당일 루프를 종료하고 결과 요약을 반환합니다.
    /// </summary>
    /// <returns>일일 결과(누계 집계)</returns>
    public DayResultData EndDay()
    {
        isRunning = false;
        return dayResult;
    }

    /// <summary>
    /// 현재 세션의 블랙리스트를 적용한 상태로 코덱스 오버레이를 엽니다.
    /// </summary>
    public void OpenCodexWithBlackList()
    {
        HashSet<string> blacklist = DataManager.Instance.GetCurrentSessionBlacklist();
        codex.OpenCodex(blacklist);
    }

    // ---------- Internals ----------
    // 다음 문서를 생성하고 UI에 바인딩한다.
    private void GenerateNextDocument()
    {
        var nextDoc = documentManager.GenerateRandomDocument();
        uiDocDisplayer.Initialize(nextDoc);
        if (nextDoc.requiresSupportDoc)
            uiSupDocDisplayer.Initialize(nextDoc);
    }
}
