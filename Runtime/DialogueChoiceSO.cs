using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogueChoice", menuName = "Dialogue Manager/Dialogue Choice")]
public class DialogueChoiceSO : ScriptableObject
{
    [System.Serializable]
    public class DialogueResult
    {
        public enum ResultType
        {
            SET,
            ADD
        }
        
        public FlagType flagType;
        public ResultType resultType;
        public string key;
        public int intValue;
        public float floatValue;
        public string stringValue;
    }
    
    [SerializeField] private string choiceDescription;
    public string ChoiceDescription => choiceDescription;
    
    [SerializeField] public DialogueNodeSO nextNode;
    public DialogueNodeSO NextNode => nextNode;

    [SerializeField] private ConditionGroup visibleConditions;
    public ConditionGroup VisibleConditions => visibleConditions;
    
    [SerializeField] private ConditionGroup unlockedConditions;
    public ConditionGroup UnlockedConditions => unlockedConditions;
    
    [SerializeField] private List<DialogueResult> results;
    public List<DialogueResult> Results => results;
}
