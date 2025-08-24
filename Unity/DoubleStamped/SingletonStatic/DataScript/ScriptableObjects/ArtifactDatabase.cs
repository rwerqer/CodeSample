using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "ArtifactDatabase", menuName = "Data/Artifact Database")]
public class ArtifactDatabase : ScriptableObject
{
    public List<ArtifactTypeData> artifactTypes;

    [Tooltip("서명 리스트 (index가 sealList와 1:1 매칭됨)")]
    public List<string> signatureList;

    [Tooltip("도장 Sprite 리스트 (index가 signatureList와 1:1 매칭됨)")]
    public List<Sprite> sealList;
}