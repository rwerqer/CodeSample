using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ArtifactDetailUI : MonoBehaviour
{
    [SerializeField] private Image artifactImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text yearText;
    [SerializeField] private TMP_Text valueText;
    [SerializeField] private TMP_Text descText;
    [SerializeField] private TMP_Text leastDifficulty;

    public void Display(ArtifactTypeData data)
    {
        if (data == null)
        {
            Debug.LogWarning("ArtifactDetailUI: data is null");
            return;
        }

        artifactImage.sprite = data.artifactIcon;
        nameText.text = $"{data.artifactName}";
        yearText.text = $"Year: {data.minYear}  ~  {data.maxYear + 1}";
        valueText.text = $"Value: {data.minValue}  ~  {data.maxValue + 1}";
        descText.text = data.description;
        leastDifficulty.text = $"Least Difficulty : {data.leastDifficulty}";
    }
}
