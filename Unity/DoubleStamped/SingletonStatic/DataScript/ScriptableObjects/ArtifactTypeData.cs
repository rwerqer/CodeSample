using UnityEngine;

[CreateAssetMenu(fileName = "ArtifactTypeData", menuName = "Data/Artifact Type")]
[System.Serializable]
public class ArtifactTypeData : ScriptableObject
{
    public string artifactName;
    public enum artifactType{
        HOLY, CULT, CURSED, BLESSED, ARTIFACT, HISTORICAL,
        ACCESSORY, PAINT, JEWELS, EXTRATERRESTRIAL, SUNDRIES,
        BOOKS
    };
    public artifactType type;

    public int minValue;
    public int maxValue;
    
    public int minYear;
    public int maxYear;
    public int leastDifficulty;
    public bool requiresSupportDoc1;
    public bool requiresSupportDoc2;
    public bool requiresSupportDoc3;
    
    public float forgedChance;
    [TextArea] public string description;

    public Sprite artifactIcon;

    // Value, Name, Date, SupportDoc, artifactIcon, seal, signatue이 Modifiable
}