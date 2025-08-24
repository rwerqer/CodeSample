using UnityEngine;
using System;

//시그널은 여기서 선언해서 모아두기
public static class GameSignals
{
    public static Action<int> OnScoreChanged;
    public static Action<int> OnFameChanged;
    public static Action OnGameStarted;
    public static Action OnGameOver;

    public static Action<string> OnItemCollected;
    public static Action<int> OnLevelCleared;

    // Police 이벤트 시그널들 
    public static Action OnPoliceApppeared;
    public static Action OnPoliceAlertResponded;
    public static Action OnAlertTimeout;
    public static Action OnPoliceEnd;
}