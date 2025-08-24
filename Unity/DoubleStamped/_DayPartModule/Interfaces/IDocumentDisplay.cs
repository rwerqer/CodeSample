//====================================================
// 파일: IDocumentDisplay.cs
// 역할: 문서/보조문서 UI 표시·정리·강조 표기를 위한 뷰 계약
// 패턴: Presenter/View 계약(Facade)
// 협력자: DocumentData, SupportDocumentData
//====================================================

using System.Collections.Generic;

public interface IDocumentDisplay
{
    /// <summary>
    /// 메인 문서 데이터를 화면에 바인딩하고 표시합니다.
    /// </summary>
    /// <param name="document">표시할 문서</param>
    /// <remarks>
    /// - 사전조건: UI 위젯 참조 유효.
    /// </remarks>
    void ShowDocument(DocumentData document);

    /// <summary>
    /// 보조문서 목록을 화면에 표시합니다.
    /// </summary>
    /// <param name="docs">보조문서 리스트</param>
    /// <remarks>
    /// - 순서가 UI 표시 순서를 결정합니다.<br/>
    /// - 슬롯 수 &lt; 문서 수인 경우의 페이징/캐러셀 정책 명시 필요.
    /// </remarks>
    void ShowSupportDocuments(List<SupportDocumentData> docs);

    /// <summary>
    /// 특정 필드를 시각적으로 강조합니다(예: 하이라이트/펄스 애니메이션).
    /// </summary>
    /// <param name="fieldName">강조할 필드 키(예: "signature", "seal", "artifactName", "issueDate", "estimatedValue"...)</param>
    /// <remarks>
    /// - 키 규약: UI가 인식하는 문자열 키 집합에 맞춰야 합니다.
    /// </remarks>
    void HighlightField(string fieldName);

    /// <summary>
    /// 모든 문서/보조문서 표시를 정리하고 UI를 초기 상태로 되돌립니다.
    /// </summary>
    /// <remarks>
    /// - 반복 호출에도 안전해야 합니다.<br/>
    /// - 비동기 로딩/애니메이션 취소가 있다면 함께 종료해야 합니다.
    /// </remarks>
    void Clear();
}
