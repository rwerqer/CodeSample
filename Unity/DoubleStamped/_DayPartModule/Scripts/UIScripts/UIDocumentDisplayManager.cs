//====================================================
// 파일: UIDocumentDisplayManager.cs
// 역할: 메인/보조 문서 데이터 → UI 바인딩 및 표시 토글
// 패턴: Presenter(데이터 바인딩)
// 협력자: DocumentData, TextMeshProUGUI, Image
//====================================================

using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIDocumentDisplayManager : MonoBehaviour
{
    // ---------- Roots ----------
    [SerializeField] private GameObject mainDocUI;
    [SerializeField] private GameObject supportDocUI;
    [SerializeField] private GameObject[] supportDoc_1_UI; // 보조문서 슬롯(고정 개수)

    // ---------- Main Document Widgets ----------
    public TextMeshProUGUI serialText;
    public TextMeshProUGUI DocumentCode;

    public TextMeshProUGUI artifactName;
    public TextMeshProUGUI kind;
    public TextMeshProUGUI issueDateText;
    public TextMeshProUGUI valueText;
    public TextMeshProUGUI owner;

    public TextMeshProUGUI signature;
    public Image          sealImage;
    public Image          artifactImage;

    public TextMeshProUGUI mainText;
    public TextMeshProUGUI neededDocument;

    /// <summary>
    /// 전달된 문서 데이터를 UI에 바인딩하고 표시 상태를 초기화합니다.
    /// </summary>
    /// <param name="doc">전시할 문서 데이터</param>
    public void Initialize(DocumentData doc)
    {
        // 보조문서 슬롯 OFF → 클린 상태로 시작
        foreach (var go in supportDoc_1_UI)
        {
            go.SetActive(false);
        }
        mainDocUI.SetActive(false);
        supportDocUI.SetActive(false);

        Debug.Log("Initializing document display...");

        // --- 본문 필드 바인딩 ---
        artifactName.text   = doc.artifactNameInDoc;
        serialText.text     = doc.serialNumber;
        valueText.text      = $"{doc.estimatedValue} $";
        signature.text      = doc.signature;
        sealImage.sprite    = doc.seal;
        artifactImage.sprite= doc.artifactIconInDoc;
        DocumentCode.text   = doc.documentId;
        owner.text          = doc.ownerName;

        mainText.text       = doc.artifactOfDoc.description;
        kind.text           = doc.artifactOfDoc.type.ToString();

        // 연-표기(BC/AD)
        int year = doc.issueDate;
        issueDateText.text = (year < 0) ? $"{Mathf.Abs(year)} B.C" : $"A.D {year}";

        // --- 보조문서 필요 여부/목록 텍스트 ---
        if (!doc.requiresSupportDoc)
        {
            neededDocument.text = "";
        }
        else
        {
            if (doc.supportDocs == null || doc.supportDocs.Count == 0)
            {
                neededDocument.text = "No support documents listed.";
            }
            else
            {
                // 보조문서 제목 리스트 구성 + 슬롯 내용 텍스트 바인딩
                System.Text.StringBuilder sb = new System.Text.StringBuilder();

                for (int i = 0; i < doc.supportDocs.Count; i++)
                {
                    GameObject ui = supportDoc_1_UI[i];
                    // 제목
                    var titleText = ui.transform.Find("SupTitle")?.GetComponent<TextMeshProUGUI>();
                    if (titleText != null) titleText.text = doc.supportDocs[i].title;

                    // 본문
                    var mainText = ui.transform.Find("SupMainText")?.GetComponent<TextMeshProUGUI>();
                    if (mainText != null) mainText.text = doc.supportDocs[i].displayedText;

                    // 목록 표시용 문자열
                    if (i != doc.supportDocs.Count - 1) sb.Append($"{doc.supportDocs[i].title}, ");
                    else                                 sb.Append($"{doc.supportDocs[i].title}");
                }
                neededDocument.text = sb.ToString();
            }
        }

        Debug.Log("mainDocUI is null? " + (mainDocUI == null));
        mainDocUI.SetActive(true);
        ShowDocument(doc);
    }

    /// <summary>
    /// 메인/보조 문서 UI의 표시 상태를 토글합니다.
    /// </summary>
    /// <param name="doc">표시 기준이 되는 문서 데이터</param>
    public void ShowDocument(DocumentData doc)
    {
        // 메인 문서 표시
        mainDocUI.SetActive(true);

        // 보조문서 존재 시 2열 레이아웃 활성화(현재 슬롯 1개만 가시화)
        if (doc.requiresSupportDoc && doc.supportDocs.Count > 0)
        {
            supportDocUI.SetActive(true);
            supportDoc_1_UI[0].SetActive(true);
        }
        else
        {
            supportDocUI.SetActive(false);
        }
    }
}
