using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class DocumentData
{
    public ArtifactTypeData artifactOfDoc;

    public string documentId;
    public string serialNumber;
    public string ownerName;


    public bool isForged;
    public List<string> forgedAttribute;
    public string artifactNameInDoc;
    public int estimatedValue;
    public int issueDate;
    public Sprite seal;
    public string signature;
    public Sprite artifactIconInDoc;

    
    public bool requiresSupportDoc;
    public bool supportDocsForged;
    public List<SupportDocumentData> supportDocs;

    // artifact 정보 등록 필요 , 필요 정보는 쿼리로 구하거나 직접 접근
    // Value, Name, Date, SupportDoc, artifactIcon, seal, signatue이 Modifiable
}


[System.Serializable]
public class DocumentResult {
    public bool wasPassed;
    public bool wasCorrectlyJudged;
}

public enum DocumentValidity { Genuine, Forged }

