//====================================================
// 파일: DocumentManager.cs
// 역할: 낮 파트용 문서 생성/상태 보관
//       - 난이도 기반 아티팩트 타입 선정
//       - 위조 여부/속성 결정, 보조문서(Support) 구성
//       - 현재 문서 캐시 및 클리어
// 패턴: Factory, Coordinator
// 협력자: IDataManager(Data/룰 조회), DocumentData/SupportDocumentData
//====================================================

using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class DocumentManager : MonoBehaviour, IDocumentManager
{
    public IDataManager dataManager;

    [SerializeField] private int currentDifficulty = 1;
    [SerializeField] private DocumentData currentDocument;
    private ArtifactTypeData currentArtifact;
    private bool isForged;

    [SerializeField] private SupportDocumentData supDoc1;
    [SerializeField] private SupportDocumentData supDoc2;
    [SerializeField] private SupportDocumentData supDoc3;

    /// <summary>
    /// 현재 세션의 문서 생성 난이도를 설정합니다.
    /// </summary>
    public void SetDifficulty(int difficulty)
    {
        currentDifficulty = difficulty;
    }

    /// <summary>
    /// 최근 생성된 현재 문서를 반환합니다.
    /// </summary>
    public DocumentData GetCurrentDocument()
    {
        return currentDocument;
    }

    /// <summary>
    /// 난이도에 맞는 아티팩트 타입을 선택하고,
    /// 위조 확률/규칙에 따라 속성을 구성한 새 문서를 생성합니다.
    /// </summary>
    /// <remarks>
    /// - 기존 문서는 <see cref="ClearDocument"/>로 정리 후 생성합니다. <br/>
    /// - 위조 분기:
    ///   1) 본문만 위조
    ///   2) 보조문서만 위조
    ///   3) 본문+보조문서 동시 위조(확률 가중) <br/>
    /// - 반환값을 <see cref="currentDocument"/>에 캐시합니다.
    /// </remarks>
    public DocumentData GenerateRandomDocument()
    {
        ClearDocument();
        if (dataManager == null) dataManager ??= DataManager.Instance;

        // 1) 아티팩트 타입 선택(난이도 기반)
        currentArtifact = dataManager.GetRandomArtifactType(currentDifficulty);

        var doc = new DocumentData
        {
            artifactOfDoc = currentArtifact,
            documentId = System.Guid.NewGuid().ToString(),
            ownerName = GenerateRandomName(),
            serialNumber = GenerateSerialNumber(),
            forgedAttribute = new List<string>(),
            supportDocs = new List<SupportDocumentData>()
        };

        // 2) 위조 여부 결정(기본 확률)
        float randomValue = Random.value;
        isForged = randomValue < currentArtifact.forgedChance;
        doc.isForged = isForged;

        // 3) 보조문서 필요 여부
        doc.requiresSupportDoc =
            doc.artifactOfDoc.requiresSupportDoc1 ||
            doc.artifactOfDoc.requiresSupportDoc2 ||
            doc.artifactOfDoc.requiresSupportDoc3;

        doc.supportDocsForged = false;

        // 4) 본문/보조 위조 분기
        if (isForged)
        {
            if (!doc.requiresSupportDoc)
            {
                ApplyForgery(ref doc);
            }
            else if (randomValue < currentArtifact.forgedChance * 0.65f)
            {
                ApplyForgery(ref doc);
            }
            else if (randomValue < currentArtifact.forgedChance * 0.9f)
            {
                ApplySupportForgery(ref doc);
                doc.supportDocsForged = true;

                // 보조만 위조 시 본문은 정상 값으로 채움
                doc.artifactNameInDoc = doc.artifactOfDoc.artifactName;
                doc.estimatedValue = Random.Range(doc.artifactOfDoc.minValue, doc.artifactOfDoc.maxValue + 1);
                doc.issueDate = Random.Range(doc.artifactOfDoc.minYear, doc.artifactOfDoc.maxYear + 1);
                doc.signature = dataManager.GetRandomValidSignature();
                doc.seal = dataManager.GetSealBySignature(doc.signature);
                doc.artifactIconInDoc = doc.artifactOfDoc.artifactIcon;
            }
            else
            {
                ApplyForgery(ref doc);
                ApplySupportForgery(ref doc);
                doc.supportDocsForged = true;
            }
        }
        else
        {
            // 정상 문서
            doc.artifactNameInDoc = doc.artifactOfDoc.artifactName;
            doc.estimatedValue = Random.Range(doc.artifactOfDoc.minValue, doc.artifactOfDoc.maxValue + 1);
            doc.issueDate = Random.Range(doc.artifactOfDoc.minYear, doc.artifactOfDoc.maxYear + 1);
            doc.signature = dataManager.GetRandomValidSignature();
            doc.seal = dataManager.GetSealBySignature(doc.signature);
            doc.artifactIconInDoc = doc.artifactOfDoc.artifactIcon;
        }

        // 5) 보조문서 필요 & 미위조 시, 정상 보조문서 채움
        if (doc.requiresSupportDoc && !doc.supportDocsForged)
        {
            if (currentArtifact.requiresSupportDoc1) doc.supportDocs.Add(GenerateSupportDoc(supDoc1, false));
            if (currentArtifact.requiresSupportDoc2) doc.supportDocs.Add(GenerateSupportDoc(supDoc2, false));
            if (currentArtifact.requiresSupportDoc3) doc.supportDocs.Add(GenerateSupportDoc(supDoc3, false));
        }

        // 6) 최종 위조 플래그 재평가(속성 리스트 기반)
        doc.isForged = doc.forgedAttribute.Count != 0;

        currentDocument = doc;
        return doc;
    }

    // --- 위조 적용(본문) ---
    private void ApplyForgery(ref DocumentData doc)
    {
        int choice = Random.Range(1, 4);
        List<string> possibleAttributes = new()
        {
            "artifactNameInDoc", "estimatedValue", "issueDate",
            "signature", "seal", "artifactIcon"
        };
        List<string> forgedAttributes = possibleAttributes
            .OrderBy(x => Random.value).Take(choice).ToList();

        // signature 조작
        doc.signature = forgedAttributes.Contains("signature")
            ? dataManager.GetRandomBlacklistedSignature()
            : dataManager.GetRandomValidSignature();

        // seal 조작 (signature와의 상호 의존 처리)
        if (forgedAttributes.Contains("seal"))
        {
            if (forgedAttributes.Contains("signature"))
                doc.seal = dataManager.GetSealBySignature(doc.signature);
            else
                doc.seal = dataManager.GetRandomSealExcluding(doc.signature);
        }
        else
        {
            doc.seal = dataManager.GetSealBySignature(doc.signature);
        }

        // artifact Icon 조작
        doc.artifactIconInDoc = forgedAttributes.Contains("artifactIcon")
            ? dataManager.GetArtifactTypeExceptName(doc.artifactOfDoc.artifactName).artifactIcon
            : doc.artifactOfDoc.artifactIcon;

        // IssueDate 조작
        if (forgedAttributes.Contains("issueDate"))
        {
            int midValue = doc.artifactOfDoc.minYear + doc.artifactOfDoc.maxYear / 2;
            int halfWindow = doc.artifactOfDoc.maxYear - midValue + 1;
            if (Random.value < 0.5f)
                doc.issueDate = Random.Range(midValue - 4 * halfWindow, midValue - halfWindow);
            else
                doc.issueDate = Random.Range(midValue + halfWindow, midValue + 4 * halfWindow);
        }
        else
        {
            doc.issueDate = Random.Range(doc.artifactOfDoc.minYear, doc.artifactOfDoc.maxYear + 1);
        }

        // estimatedValue 조작 (값이 낮으면 가품 가능성 ↑)
        if (forgedAttributes.Contains("estimatedValue"))
        {
            int midValue = doc.artifactOfDoc.minValue + doc.artifactOfDoc.maxValue / 2;
            int halfWindow = doc.artifactOfDoc.maxValue - midValue + 1;
            if (midValue - halfWindow <= 2)
                doc.estimatedValue = Random.Range(4, 4 + midValue + 4 * halfWindow);
            else
                doc.estimatedValue = Random.Range(1, midValue - halfWindow);
        }
        else
        {
            doc.estimatedValue = Random.Range(doc.artifactOfDoc.minValue, doc.artifactOfDoc.maxValue + 1);
        }

        // artifactNameInDoc 조작
        doc.artifactNameInDoc = forgedAttributes.Contains("artifactNameInDoc")
            ? dataManager.GetArtifactTypeExceptName(doc.artifactOfDoc.artifactName).artifactName
            : doc.artifactOfDoc.artifactName;

        doc.forgedAttribute ??= new List<string>();
        doc.forgedAttribute.AddRange(forgedAttributes);
    }

    // --- 위조 적용(보조문서) ---
    private void ApplySupportForgery(ref DocumentData doc)
    {
        // 필요한 보조문서 템플릿 수집
        List<(int index, SupportDocumentData template)> supportTemplates = new();

        if (currentArtifact.requiresSupportDoc1) supportTemplates.Add((1, supDoc1));
        if (currentArtifact.requiresSupportDoc2) supportTemplates.Add((2, supDoc2));
        if (currentArtifact.requiresSupportDoc3) supportTemplates.Add((3, supDoc3));

        int forgeCount = Random.Range(1, supportTemplates.Count + 1);
        var forgedIndices = supportTemplates
            .OrderBy(x => Random.value)
            .Take(forgeCount)
            .Select(x => x.index)
            .ToHashSet();

        foreach (var (index, template) in supportTemplates)
        {
            bool isForged = forgedIndices.Contains(index);
            var docInstance = GenerateSupportDoc(template, isForged);
            doc.supportDocs.Add(docInstance);

            if (isForged) doc.forgedAttribute.Add($"supportDoc{index}");
        }
    }

    // --- 유틸: 이름/시리얼 ---
    private string GenerateRandomName()
    {
        string[] names = {
            "Ashen One","Godfrey","HollowKnight","Smith",
            "David","Red","Steve","God",
            "Mario","Lucy","Cathady","Kirby",
            "Doctor pepper","King","Lyla","Joker",
            "OhmyGosh","Kanye","Isaac","Remilia",
            "2B","Toriel","SAnS","\"AnA\"",
            ":>","YJ_Jung","YB_Kim","NY_Yim","JW_Seul"
        };
        return names[Random.Range(0, names.Length)];
    }

    private string GenerateSerialNumber()
    {
        int protoSerialNumber = Random.Range(0, 99999999 + 1);
        string protoSerialString = protoSerialNumber.ToString("D8");
        return protoSerialString;
    }

    // --- 보조문서 인스턴스 생성 ---
    private SupportDocumentData GenerateSupportDoc(SupportDocumentData template, bool isForged)
    {
        // ScriptableObject 템플릿을 런타임 인스턴스로 복제
        SupportDocumentData instance = Instantiate(template);

        instance.title = template.title;
        instance.text = template.text;
        instance.fakeText = template.fakeText;
        instance.isForged = isForged;
        instance.displayedText = isForged ? template.fakeText : template.text;

        return instance;
    }

    /// <summary>
    /// 현재 문서를 제거하고, 생성된 보조문서 인스턴스를 파괴합니다.
    /// </summary>
    public void ClearDocument()
    {
        if (currentDocument != null)
        {
            foreach (var supportDoc in currentDocument.supportDocs)
            {
                Destroy(supportDoc); // ScriptableObject 인스턴스 파괴
            }
            currentDocument.supportDocs.Clear();
            currentDocument = null;
        }
    }

    private void Start()
    {
        dataManager ??= DataManager.Instance;
        dataManager.InitializeSessionBlacklist();
    }
}
