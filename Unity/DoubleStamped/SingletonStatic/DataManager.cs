//====================================================
// 파일: DataManager.cs
// 역할: 게임 데이터 조회/정책 허브(아티팩트/시그니처/블랙리스트)
//       - 아티팩트 타입/범위 값/연도 반환
//       - 시그니처↔직인 매핑, 블랙리스트 생성/검사
// 패턴: Singleton(Service), Repository/Facade
// 협력자: ArtifactDatabase(SO), ArtifactTypeData, SupportDocumentData 소비측
//====================================================

using System.Collections.Generic;
using UnityEngine;

// todo : 시그니처 관리랑 쿼리 더 추가 필요 일단은 임시
public class DataManager : MonoBehaviour, IDataManager
{
    /// <summary>
    /// 전역 접근용 싱글톤 인스턴스입니다.
    /// </summary>
    public static DataManager Instance { get; private set; }

    public ArtifactDatabase artifactDatabase;
    [SerializeField] private List<string> sessionBlacklist = new();

    // ---------- Lifetime ----------

    private void Awake()
    {
        // 싱글톤 보장(중복 인스턴스 제거)
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitializeSessionBlacklist();
    }

    // ---------- Artifact Queries ----------

    /// <summary>
    /// 난이도 조건을 만족하는 아티팩트 타입 중 무작위로 하나를 반환합니다.
    /// </summary>
    /// <param name="difficulty">현재 난이도</param>
    /// <returns>조건을 만족하는 아티팩트 또는 <c>null</c></returns>
    public ArtifactTypeData GetRandomArtifactType(int difficulty)
    {
        var filtered = artifactDatabase.artifactTypes.FindAll(a =>
            a.leastDifficulty <= difficulty
        );
        if (filtered.Count == 0) return null;

        return filtered[Random.Range(0, filtered.Count)];
    }

    /// <summary>
    /// 이름으로 아티팩트 타입을 조회합니다.
    /// </summary>
    public ArtifactTypeData GetArtifactTypeByName(string name)
    {
        return artifactDatabase.artifactTypes.Find(a => a.artifactName == name);
    }

    /// <summary>
    /// 아티팩트 이름 기준의 가치 범위에서 무작위 값을 반환합니다.
    /// </summary>
    public int GetEstimatedValueRange(string artifactName)
    {
        var artifact = GetArtifactTypeByName(artifactName);
        if (artifact == null) return 0;
        return Random.Range(artifact.minValue, artifact.maxValue + 1);
    }

    /// <summary>
    /// 아티팩트 객체 기준의 가치 범위에서 무작위 값을 반환합니다.
    /// </summary>
    public int GetEstimatedValueRange(ArtifactTypeData artifact)
    {
        if (artifact == null) return 0;
        return Random.Range(artifact.minValue, artifact.maxValue + 1);
    }

    /// <summary>
    /// 아티팩트 이름 기준의 연도 범위에서 무작위 연도를 반환합니다.
    /// </summary>
    public int GetDateRange(string artifactName)
    {
        var artifact = GetArtifactTypeByName(artifactName);
        if (artifact == null) return 0;
        return Random.Range(artifact.minYear, artifact.maxYear + 1);
    }

    /// <summary>
    /// 아티팩트 객체 기준의 연도 범위에서 무작위 연도를 반환합니다.
    /// </summary>
    public int GetDateRange(ArtifactTypeData artifact)
    {
        if (artifact == null) return 0;
        return Random.Range(artifact.minYear, artifact.maxYear + 1);
    }

    // ---------- Signature Blacklist ----------

    /// <summary>
    /// 현재 세션에서 사용할 시그니처 블랙리스트를 무작위로 초기화합니다.
    /// </summary>
    /// <remarks>
    /// - 중복 없이 5~8개를 선택합니다.<br/>
    /// - 난수 정책은 UnityEngine.Random에 따릅니다.
    /// </remarks>
    public void InitializeSessionBlacklist()
    {
        sessionBlacklist.Clear();
        int count = Random.Range(5, 9);
        var all = artifactDatabase.signatureList;
        for (int i = 0; i < count && all.Count > 0; i++)
        {
            string sig;
            do
            {
                sig = all[Random.Range(0, all.Count)];
            } while (sessionBlacklist.Contains(sig));
            sessionBlacklist.Add(sig);
        }
    }

    /// <summary>
    /// 주어진 시그니처가 현재 세션에서 블랙리스트에 포함되는지 반환합니다.
    /// </summary>
    public bool IsSignatureBlacklisted(string signature)
    {
        return sessionBlacklist.Contains(signature);
    }

    /// <summary>
    /// 블랙리스트에 포함되지 않은 무작위 유효 시그니처를 반환합니다.
    /// </summary>
    /// <returns>유효 시그니처 문자열</returns>
    public string GetRandomValidSignature()
    {
        string candidate;
        var all = artifactDatabase.signatureList;
        do
        {
            candidate = all[Random.Range(0, all.Count)];
        } while (IsSignatureBlacklisted(candidate));
        return candidate;
    }

    /// <summary>
    /// 시그니처에 대응하는 직인(Seal) 스프라이트를 반환합니다.
    /// </summary>
    /// <param name="signature">시그니처 문자열</param>
    /// <returns>직인 스프라이트 또는 <c>null</c></returns>
    public Sprite GetSealBySignature(string signature)
    {
        var list = artifactDatabase.signatureList;
        int index = list.IndexOf(signature);
        if (index >= 0 && index < artifactDatabase.sealList.Count)
        {
            return artifactDatabase.sealList[index];
        }
        return null;
    }

    /// <summary>
    /// 현재 세션의 블랙리스트 사본을 반환합니다.
    /// </summary>
    public HashSet<string> GetCurrentSessionBlacklist()
    {
        return new HashSet<string>(sessionBlacklist);
    }

    /// <summary>
    /// 블랙리스트에서 무작위 시그니처를 반환합니다.
    /// </summary>
    /// <returns>블랙리스트 시그니처 또는 <c>null</c>(목록 비었을 때)</returns>
    public string GetRandomBlacklistedSignature()
    {
        if (sessionBlacklist == null || sessionBlacklist.Count == 0)
            return null;

        return sessionBlacklist[Random.Range(0, sessionBlacklist.Count)];
    }

    /// <summary>
    /// 특정 시그니처에 해당하는 직인을 제외하고 무작위 직인을 반환합니다.
    /// </summary>
    /// <param name="signature">제외할 시그니처</param>
    /// <returns>직인 스프라이트 또는 <c>null</c>(데이터 불일치/원소 부족)</returns>
    /// <remarks>
    /// - 시그니처/직인 리스트 길이가 다르면 <c>null</c>을 반환합니다.<br/>
    /// - 제외 대상이 없거나 후보가 1개뿐일 때도 <c>null</c>.
    /// </remarks>
    public Sprite GetRandomSealExcluding(string signature)
    {
        var signatureList = artifactDatabase.signatureList;
        var sealList = artifactDatabase.sealList;

        if (signatureList == null || sealList == null || signatureList.Count != sealList.Count)
            return null;

        int excludeIndex = signatureList.IndexOf(signature);
        if (excludeIndex == -1 || sealList.Count <= 1)
            return null;

        int randomIndex;
        do
        {
            randomIndex = Random.Range(0, sealList.Count);
        } while (randomIndex == excludeIndex);

        return sealList[randomIndex];
    }

    /// <summary>
    /// 지정한 이름을 제외한 아티팩트 타입을 무작위로 반환합니다.
    /// </summary>
    public ArtifactTypeData GetArtifactTypeExceptName(string name)
    {
        var filtered = artifactDatabase.artifactTypes.FindAll(a =>
            a.artifactName != name
        );

        if (filtered.Count == 0) return null;

        return filtered[Random.Range(0, filtered.Count)];
    }
}
