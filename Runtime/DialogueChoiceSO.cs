using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogueChoice", menuName = "Dialogue Manager/Dialogue Choice")]
public class DialogueChoiceSO : ScriptableObject
{
    [SerializeField] private string choiceDescription;
    public string ChoiceDescription => choiceDescription;
    
    [SerializeField] private DialogueNodeSO nextNode;
    public DialogueNodeSO NextNode => nextNode;

    [SerializeField] private ConditionGroup visibleConditions;
    public ConditionGroup VisibleConditions => visibleConditions;
    
    [SerializeField] private ConditionGroup unlockedConditions;
    public ConditionGroup UnlockedConditions => unlockedConditions;
}
