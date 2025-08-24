//====================================================
// 파일: CodexOverlayUIManager.cs
// 역할: 코덱스 오버레이(매뉴얼/아티팩트 목록/상세/서명 목록) 패널 전환 & 목록 렌더
// 패턴: Presenter(뷰 토글/목록 바인딩)
// 협력자: DataManager(Artifact DB), SignatureSlotUI, ArtifactListItemUI, ArtifactDetailUI
//====================================================

using System.Collections.Generic;
using UnityEngine;

public class CodexOverlayUIManager : MonoBehaviour
{
    // ---------- Panels ----------
    public GameObject panelManual;
    public GameObject panelArtifactList;
    public GameObject panelArtifactDetail;
    public GameObject panelSignatureList;

    // ---------- Signature Grid ----------
    public Transform signatureGridParent;
    public GameObject signatureSlotPrefab;

    // ---------- Artifact List ----------
    public Transform artifactListParent;
    public GameObject artifactListItemPrefab;

    // ---------- Artifact Detail ----------
    public ArtifactDetailUI artifactDetailUI;

    // ---------- State ----------
    private HashSet<string> unusableSignatures;

    private void Start()
    {
        // 시작 시 모든 패널 비활성화(오버레이 닫힌 상태)
        panelManual?.SetActive(false);
        panelArtifactList?.SetActive(false);
        panelArtifactDetail?.SetActive(false);
        panelSignatureList?.SetActive(false);
    }

    /// <summary>
    /// 매뉴얼 패널을 표시합니다.
    /// </summary>
    public void manualButtonOnclick()
    {
        ShowPanel(panelManual);
    }

    /// <summary>
    /// 코덱스를 열고 블랙리스트(사용 불가 서명 집합)를 적용합니다.
    /// </summary>
    /// <param name="blacklist">사용 불가 서명 문자열 집합</param>
    public void OpenCodex(HashSet<string> blacklist)
    {
        gameObject.SetActive(true);
        unusableSignatures = blacklist;
        ShowPanel(panelManual);
    }

    /// <summary>
    /// 코덱스를 닫습니다.
    /// </summary>
    public void CloseCodex()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// 서명 목록 패널을 띄우고 슬롯을 채웁니다.
    /// </summary>
    public void ShowSignatureList()
    {
        ShowPanel(panelSignatureList);
        PopulateSignatureSlots();
    }

    // 시그니처/씰 슬롯을 그리드에 채움
    private void PopulateSignatureSlots()
    {
        foreach (Transform child in signatureGridParent)
            Destroy(child.gameObject);

        var sigList  = DataManager.Instance.artifactDatabase.signatureList;
        var sealList = DataManager.Instance.artifactDatabase.sealList;

        for (int i = 0; i < sigList.Count; i++)
        {
            GameObject go = Instantiate(signatureSlotPrefab, signatureGridParent);
            var slot = go.GetComponent<SignatureSlotUI>();
            bool isUsable = !(unusableSignatures?.Contains(sigList[i]) ?? false);
            slot.Initialize(sealList[i], sigList[i], isUsable);
        }
    }

    // 단일 패널만 활성화
    private void ShowPanel(GameObject panelToShow)
    {
        panelManual.SetActive(false);
        panelArtifactList.SetActive(false);
        panelArtifactDetail.SetActive(false);
        panelSignatureList.SetActive(false);
        panelToShow.SetActive(true);
    }

    /// <summary>
    /// 아티팩트 목록 패널을 띄우고 리스트를 구성합니다.
    /// </summary>
    public void ShowArtifactList()
    {
        ShowPanel(panelArtifactList);
        PopulateArtifactList();
    }

    // 아티팩트 목록을 프리팹 아이템으로 채움
    private void PopulateArtifactList()
    {
        foreach (Transform child in artifactListParent)
            Destroy(child.gameObject);

        var artifacts = DataManager.Instance.artifactDatabase.artifactTypes;

        for (int i = 0; i < artifacts.Count; i++)
        {
            var artifact = artifacts[i];
            GameObject go = Instantiate(artifactListItemPrefab, artifactListParent);
            var item = go.GetComponent<ArtifactListItemUI>();
            // 콜백으로 상세 열기
            item.Initialize(artifact.artifactName, i + 1, ShowArtifactDetail);
        }
    }

    // 상세 패널로 전환하고 상세 UI에 바인딩
    private void ShowArtifactDetail(string artifactName)
    {
        var artifact = DataManager.Instance.GetArtifactTypeByName(artifactName);
        if (artifact == null) return;

        ShowPanel(panelArtifactDetail);
        artifactDetailUI.Display(artifact);
    }
}
