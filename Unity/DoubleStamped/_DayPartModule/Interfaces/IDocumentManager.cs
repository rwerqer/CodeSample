//====================================================
// 파일: IDocumentManager.cs
// 역할: 낮 파트 문서 생성/상태 보관/정리 계약 정의
// 패턴: Factory, Facade
// 협력자: IDataManager, DocumentData/SupportDocumentData
//====================================================

public interface IDocumentManager
{
    /// <summary>
    /// 문서 생성 난이도를 설정합니다.
    /// </summary>
    /// <param name="difficulty">난이도 레벨</param>
    void SetDifficulty(int difficulty);

    /// <summary>
    /// 난이도/룰에 따라 새 문서를 생성하고 내부에 캐시합니다.
    /// </summary>
    /// <returns>생성된 <see cref="DocumentData"/></returns>
    /// <remarks>
    /// - 기존 문서는 생성 전에 <c>ClearDocument()</c>로 정리됩니다. <br/>
    /// - 반환값은 이후 <c>GetCurrentDocument()</c>로 재조회 가능합니다.
    /// </remarks>
    DocumentData GenerateRandomDocument();

    /// <summary>
    /// 최근 생성된 현재 문서를 반환합니다.
    /// </summary>
    /// <returns>현재 문서, 없으면 <c>null</c></returns>
    DocumentData GetCurrentDocument();

    /// <summary>
    /// 현재 문서를 해제하고 관련 리소스를 정리합니다.
    /// </summary>
    /// <remarks>
    /// - 보조 문서 인스턴스가 생성되어 있다면 함께 파괴/초기화해야 합니다. <br/>
    /// - 호출 후 <c>GetCurrentDocument()</c>는 <c>null</c>을 반환합니다.
    /// </remarks>
    void ClearDocument();
}
