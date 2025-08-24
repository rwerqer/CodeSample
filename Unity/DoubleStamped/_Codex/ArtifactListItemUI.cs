//====================================================
// 파일: ArtifactListItemUI.cs
// 역할: 코덱스 아티팩트 목록의 단일 항목 UI 바인딩 + 클릭 콜백 라우팅
// 패턴: Presenter(아이템 뷰 바인딩)
// 협력자: CodexOverlayUIManager(콜백 수신), Button, TextMeshProUGUI
//====================================================

using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ArtifactListItemUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI indexText;
    private string artifactName;

    /// <summary>
    /// 목록 아이템을 데이터로 바인딩하고 버튼 클릭 시 콜백을 호출하도록 설정합니다.
    /// </summary>
    /// <param name="name">아티팩트 표시명</param>
    /// <param name="index">리스트 내 1-base 인덱스</param>
    /// <param name="onClicked">클릭 시 호출될 콜백(매개: 아티팩트명)</param>
    /// <remarks>
    /// - 리스너 중복을 방지하기 위해 기존 리스너를 제거한 후 추가합니다.
    /// </remarks>
    public void Initialize(string name, int index, UnityAction<string> onClicked)
    {
        artifactName = name;
        indexText.text = $"{index}. {name}";

        var btn = GetComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(() =>
        {
            Debug.Log($"[Codex] Clicked on: {artifactName}");
            onClicked.Invoke(artifactName); // TODO: onClicked null 가드
        });
    }
}
