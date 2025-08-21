//====================================================
// 파일: KnockbackSignal.cs
// 역할: 플레이어 넉백 발생 알림용 전역 신호(발행/구독)
// 패턴: Event Channel (C# static event)
// 협력자: ObstacleBehavior(발행), playerCtrl(구독)
//====================================================
using System;

public static class KnockbackSignal
{
    public static event Action OnPlayerKnockedBack;

    public static void Invoke()
    {
        OnPlayerKnockedBack?.Invoke();
    }
}