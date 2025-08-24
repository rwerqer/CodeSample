//====================================================
// 파일: SignatureSlotUI.cs
// 역할: 서명 슬롯(아이콘/라벨/사용불가 오버레이) UI 바인딩
// 패턴: Presenter(단일 아이템 바인딩)
// 협력자: TextMeshProUGUI, Image
//====================================================

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SignatureSlotUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private GameObject xMarkOverlay;
    [SerializeField] private TextMeshProUGUI sigText;

    /// <summary>
    /// 서명 슬롯의 시각 요소를 전달된 데이터로 갱신합니다.
    /// </summary>
    /// <param name="icon">서명/직인 아이콘 스프라이트</param>
    /// <param name="sig">서명 텍스트(표시명)</param>
    /// <param name="isUsable">사용 가능 여부(true=정상, false=사용불가)</param>
    /// <remarks>
    /// - UI 위젯 참조는 유효하다고 가정합니다(인스펙터 연결 필수).<br/>
    /// - 동일 데이터로의 반복 호출도 안전해야 합니다(아이템 리사이클 대비).
    /// </remarks>
    public void Initialize(Sprite icon, string sig, bool isUsable)
    {
        // 아이콘/텍스트 바인딩 및 상태 오버레이 토글
        iconImage.sprite = icon;
        sigText.text = sig;
        xMarkOverlay.SetActive(!isUsable);
    }
}
