using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DialogueTriggerMap", menuName = "Dialogue Manager/Dialogue Trigger Map")]
public class DialogueTriggerMapSO : ScriptableObject
{
    [System.Serializable]
    public class DialogueTrigger
    {
        public string trigger;
        public DialogueNodeSO dialogueNode;
    }

    [SerializeField] private List<DialogueTrigger> triggerMap;

    private Dictionary<string, DialogueNodeSO> runtimeTriggerMap;

    public void InitializeTriggerMap()
    {
        if (runtimeTriggerMap != null) return;

        runtimeTriggerMap = new Dictionary<string, DialogueNodeSO>();
        foreach (var dialogueTrigger in triggerMap)
        {
            runtimeTriggerMap.Add(dialogueTrigger.trigger, dialogueTrigger.dialogueNode);
        }
    }

    public DialogueNodeSO GetDialogueStart(string trigger)
    {
        if(runtimeTriggerMap == null) InitializeTriggerMap();
        
        return runtimeTriggerMap.GetValueOrDefault(trigger, null);
    }
}
