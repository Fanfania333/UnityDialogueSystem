using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogueNode", menuName = "Dialogue Manager/Dialogue Node")]
public class DialogueNodeSO : ScriptableObject
{
    [SerializeField] private SpeakerSO speaker;
    public SpeakerSO Speaker => speaker;

    [Tooltip("This is the text that appears on the dialogue option that leads to this node. " +
             "Can leave this empty if this node is not reached by a choice.")]
    [SerializeField] private string optionText;
    public string OptionText => optionText;
    
    [SerializeField] [TextArea] private string text;
    public string Text => text;
    
    [SerializeField] private List<DialogueNodeSO> nextNodes;
    public List<DialogueNodeSO> NextNodes => nextNodes;
}
