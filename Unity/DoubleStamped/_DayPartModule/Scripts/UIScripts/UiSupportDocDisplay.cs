//====================================================
// 파일: UiSupportDocDisplay.cs
// 역할: 보조문서(Support Docs) 패널 캐러셀 표시(이전/다음 전환)
// 패턴: Presenter(뷰 토글), Carousel
// 협력자: UIDocumentDisplayManager(초기화 호출), DocumentData
//====================================================

using UnityEngine;

public class UiSupportDocDisplay : MonoBehaviour
{
    [SerializeField] private GameObject[] supportDocPanels; // 인덱스 = 보조문서 순서
    [SerializeField] private DocumentData currentDocument;

    private int currentIndex = 0;

    /// <summary>
    /// 다음 보조문서 패널을 표시합니다(순환).
    /// </summary>
    public void OnNextButtonPressed()
    {
        if (currentDocument.supportDocs.Count == 0) return;
        currentIndex = (currentIndex + 1) % currentDocument.supportDocs.Count;
        ShowOnly(currentIndex);
    }

    /// <summary>
    /// 이전 보조문서 패널을 표시합니다(순환).
    /// </summary>
    public void OnPrevButtonPressed()
    {
        if (currentDocument.supportDocs.Count == 0) return;
        currentIndex = (currentIndex - 1 + currentDocument.supportDocs.Count) % currentDocument.supportDocs.Count;
        ShowOnly(currentIndex);
    }

    /// <summary>
    /// 지정한 인덱스의 패널만 활성화하고 나머지를 비활성화합니다.
    /// </summary>
    /// <param name="index">표시할 보조문서 인덱스</param>
    private void ShowOnly(int index)
    {
        for (int i = 0; i < currentDocument.supportDocs.Count; i++)
        {
            supportDocPanels[i].SetActive(i == index);
        }
    }

    /// <summary>
    /// 보조문서 캐러셀을 초기화하고 첫 항목을 표시합니다.
    /// </summary>
    /// <param name="doc">보조문서 목록을 가진 문서</param>
    public void Initialize(DocumentData doc)
    {
        currentDocument = doc;
        currentIndex = 0;
        ShowOnly(currentIndex);
    }
}
