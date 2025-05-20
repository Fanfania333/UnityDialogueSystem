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
    
    [SerializeField] private List<DialogueChoiceSO> nextChoices;
    public List<DialogueChoiceSO> NextChoices => nextChoices;
}
