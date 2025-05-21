using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogueNode", menuName = "Dialogue Manager/Dialogue Node")]
public class DialogueNodeSO : ScriptableObject
{
    [SerializeField] private SpeakerSO speaker;
    public SpeakerSO Speaker => speaker;
    
    [SerializeField] [TextArea] private string text;
    public string Text => text;
    
    [Tooltip("Refers to the name of the localization table the translation for this node will be taken from. Can be left blank, if not using localization.")]
    [SerializeField] private string tableReference;
    public string TableReference => tableReference;
    
    [SerializeField] public List<DialogueChoiceSO> nextChoices;
    public List<DialogueChoiceSO> NextChoices => nextChoices;
}
