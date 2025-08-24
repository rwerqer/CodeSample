using System.Collections.Generic;
using UnityEngine;

public interface IDataManager
{
    public void InitializeSessionBlacklist();

    ArtifactTypeData GetRandomArtifactType(int difficulty);
    ArtifactTypeData GetArtifactTypeByName(string name);

    public bool IsSignatureBlacklisted(string signature);
    public int GetEstimatedValueRange(string artifactName);
    public int GetEstimatedValueRange(ArtifactTypeData artifact);

    public int GetDateRange(string artifactName);
    public int GetDateRange(ArtifactTypeData artifact);


    public string GetRandomValidSignature();
    public string GetRandomBlacklistedSignature();


    // DayScene 사용
    public Sprite GetSealBySignature(string signature);
    public Sprite GetRandomSealExcluding(string signature);
    public ArtifactTypeData GetArtifactTypeExceptName(string name);
}