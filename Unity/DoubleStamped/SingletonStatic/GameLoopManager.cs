//====================================================
// 파일: GameLoopManager.cs
// 역할: 전역 게임 루프 상태머신(씬 전환/타임스케일/일자 카운트/결과 수집)
// 패턴: Singleton, State Machine, Coordinator
// 협력자: TimeManager, ResourceManager, DayGameLoopManager, NightGameLoopManager,
//         SceneManager(Unity), GameSignals(이벤트)
//====================================================

using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameState
{
    GameStart,
    Playing_Day,
    Playing_Night,
    GameClear,
    GameOver,
    DayClear
}

public class GameLoopManager : MonoBehaviour
{
    /// <summary>전역 접근용 싱글톤 인스턴스.</summary>
    public static GameLoopManager Instance { get; private set; }

    /// <summary>현재 게임 상태(State Machine).</summary>
    public GameState CurrentState { get; private set; } = GameState.GameStart;

    /// <summary>당일(낮) 결과 스냅샷.</summary>
    public DayResultData CurrentDayResult { get; private set; }
    /// <summary>당야(밤) 결과 스냅샷.</summary>
    public NightResultData CurrentNightResult { get; private set; }

    [SerializeField] private int currentDay;

    /// <summary>현재 일자를 1로 리셋합니다.</summary>
    public void ResetCurrentDay() { currentDay = 1; }
    /// <summary>현재 일자를 1 증가시킵니다.</summary>
    public void AddCurrentDay() { currentDay++; }
    /// <summary>현재 일자를 반환합니다.</summary>
    public int GetCurrentDay() { return currentDay; }

    private void Awake()
    {
        // 싱글톤 보장
        if (Instance != null && Instance != this)
        {
            Debug.Log("Duplicate GameLoopManager Destroyed");
            Destroy(gameObject);
            return;
        }
        Screen.SetResolution(1920, 1080, false); // false=창모드, true=전체화면
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        // 디버그: N키로 밤으로 스킵(낮 플레이 중일 때만)
        if (Input.GetKeyDown(KeyCode.N))
        {
            if (CurrentState == GameState.Playing_Day)
            {
                Debug.Log("디버그: N 키 입력 - NightScene으로 스킵");
                SwitchToNight();
            }
        }
    }

    private void Start()
    {
        Time.timeScale = 0f;
        ResetCurrentDay();
        SetState(GameState.GameStart);
    }

    /// <summary>
    /// 상태머신 전이: 상태를 변경하고 해당 핸들러를 실행합니다.
    /// </summary>
    /// <param name="newState">목표 상태</param>
    public void SetState(GameState newState)
    {
        if (CurrentState == newState) return;

        CurrentState = newState;

        switch (CurrentState)
        {
            case GameState.GameStart:
                HandleGameStart();
                break;
            case GameState.Playing_Day:
                HandleDayPlaying();
                break;
            case GameState.Playing_Night:
                HandleNightPlaying();
                break;
            case GameState.GameClear:
                HandleGameClear();
                break;
            case GameState.GameOver:
                HandleGameOver();
                break;
            case GameState.DayClear:
                HandleDayClear();
                break;
        }
    }

    // --- 상태 핸들러: GameStart ---
    private void HandleGameStart()
    {
        Debug.Log("게임 시작 씬 로드");
        SceneManager.LoadScene("StartScene");
        Time.timeScale = 0f;
        ResetCurrentDay();

        SceneManager.sceneLoaded += OnStartSceneLoaded;
    }

    private void OnStartSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "StartScene")
        {
            SceneManager.sceneLoaded -= OnStartSceneLoaded;

            ResourceManager.Instance.ResetResource();

            SetState(GameState.GameStart);
        }
    }

    // --- 상태 핸들러: Playing_Day ---
    private void HandleDayPlaying()
    {
        Debug.Log("Day 플레이");
        if (SceneManager.GetActiveScene().name != "DayScene")
        {
            SceneManager.LoadScene("DayScene");
            SceneManager.sceneLoaded += OnDaySceneLoaded;
        }
    }

    private void OnDaySceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "DayScene")
        {
            SceneManager.sceneLoaded -= OnDaySceneLoaded;
            Time.timeScale = 1f;

            AddCurrentDay();
            Debug.Log(currentDay);
            SetState(GameState.Playing_Day);
        }
    }

    // --- 상태 핸들러: Playing_Night ---
    private void HandleNightPlaying()
    {
        Debug.Log("Night 게임 플레이 ");
        SceneManager.LoadScene("NightScene");
        SceneManager.sceneLoaded += OnNightSceneLoaded;
    }

    private void OnNightSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "NightScene")
        {
            SceneManager.sceneLoaded -= OnNightSceneLoaded;
            Time.timeScale = 1f;
            // TODO: NightScene 초기화 로직 추가 위치
        }
    }

    // --- 상태 핸들러: GameClear ---
    private void HandleGameClear()
    {
        Debug.Log("게임 클리어 - GameClear씬 로드");

        GameObject nightManagerObj = GameObject.Find("NightGameLoopManager");
        NightGameLoopManager nightManager = nightManagerObj.GetComponent<NightGameLoopManager>();
        CurrentNightResult = nightManager.EndDay();

        SceneManager.LoadScene("GameClear");
    }

    // --- 상태 핸들러: DayClear ---
    private void HandleDayClear()
    {
        Debug.Log("낮 클리어 - DayClear씬 로드");

        GameObject dayManagerObj = GameObject.Find("DayGameLoopManager");
        DayGameLoopManager dayManager = dayManagerObj.GetComponent<DayGameLoopManager>();
        CurrentDayResult = dayManager.EndDay();

        SceneManager.LoadScene("DayClear");
    }

    // --- GameOver 이벤트 구독 ---
    private void OnEnable()
    {
        GameSignals.OnGameOver += HandleGameOver;
    }
    private void OnDisable()
    {
        GameSignals.OnGameOver -= HandleGameOver;
    }

    private void HandleGameOver()
    {
        Debug.Log("게임 오버 - GameOver씬 로드");
        Time.timeScale = 0f;
        SceneManager.LoadScene("GameOver");
    }

    /// <summary>
    /// 밤 파트로 전환합니다. (타임매니저 야간 강제 후 상태 전이)
    /// </summary>
    public void SwitchToNight()
    {
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.ForceNightMode(); // 디버깅 시 타이머까지 동기
        }
        SetState(GameState.Playing_Night);
    }

    /// <summary>
    /// 낮 파트로 전환합니다. (타임매니저 낮 리셋 후 상태 전이)
    /// </summary>
    public void SwitchToDay()
    {
        if (TimeManager.Instance != null)
        {
            TimeManager.Instance.ResetTimerToDay();
        }
        SetState(GameState.Playing_Day);
    }
}
