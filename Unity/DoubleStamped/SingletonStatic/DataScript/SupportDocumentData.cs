using UnityEngine;

[CreateAssetMenu(fileName = "SupportDoc", menuName = "Data/Support Document")]
public class SupportDocumentData :ScriptableObject
{
    public string title;
    public string displayedText;
    public string text;
    public string fakeText;
    public bool isForged;
}