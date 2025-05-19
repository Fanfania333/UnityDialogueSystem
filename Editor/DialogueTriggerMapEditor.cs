using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

[CustomEditor(typeof(DialogueTriggerMapSO))]
public class DialogueTriggerMapEditor : Editor
{
    SerializedProperty triggerMapProp;

    private void OnEnable()
    {
        triggerMapProp = serializedObject.FindProperty("triggerMap");
    }
    
    public override void OnInspectorGUI()
    {
        serializedObject.Update();
        
        DrawDefaultInspector();
        
        ValidateTriggers();

        if (GUILayout.Button("Sort Triggers"))
        {
            SortTriggers();
        }
        
        serializedObject.ApplyModifiedProperties();
    }

    private void ValidateTriggers()
    {
        HashSet<string> seenTrigger = new HashSet<string>();
        HashSet<string> repeatedTriggers = new HashSet<string>();
        bool hasDuplicates = false;

        for (int i = 0; i < triggerMapProp.arraySize; i++)
        {
            string trigger = triggerMapProp.GetArrayElementAtIndex(i).FindPropertyRelative("trigger").stringValue;
            
            // Delete white space
            triggerMapProp.GetArrayElementAtIndex(i).FindPropertyRelative("trigger").stringValue = trigger.Trim();
            trigger = trigger.Trim();
            
            // Check for empty triggers
            if (string.IsNullOrWhiteSpace(trigger))
            {
                EditorGUILayout.HelpBox("There are empty triggers in map.", MessageType.Error);
            }
            
            // Check for duplicates
            if (!seenTrigger.Add(trigger))
            {
                hasDuplicates = true;
                repeatedTriggers.Add(trigger);
            }
            
            // Check for deceptive characters
            Regex suspiciousCharsRegex = new Regex(
                @"[\u200B\u200C\u200D\uFEFF\u00A0\u202F\u2060\u180E\u00AD]|[\u0000-\u001F\u007F]", 
                RegexOptions.Compiled);
            if (suspiciousCharsRegex.IsMatch(trigger))
            {
                EditorGUILayout.HelpBox($"The trigger {trigger}, has deceptive characters.", MessageType.Warning);
            }
        }

        if (hasDuplicates)
        {
            string errorMessage = "The following triggers are duplicated: ";

            foreach (var trigger in repeatedTriggers)
            {
                errorMessage += trigger + ", ";
            }
            errorMessage = errorMessage.Remove(errorMessage.Length - 2);
            
            EditorGUILayout.HelpBox(errorMessage, MessageType.Error);
        }
    }

    private void SortTriggers()
    {
        List<(string trigger, Object value)> sortedTriggers = new List<(string trigger, Object value)>();
        for (int i = 0; i < triggerMapProp.arraySize; i++)
        {
            var element = triggerMapProp.GetArrayElementAtIndex(i);
            string key = element.FindPropertyRelative("trigger").stringValue;
            Object value = element.FindPropertyRelative("dialogueNode").objectReferenceValue;
            sortedTriggers.Add((key, value));
        }
        
        sortedTriggers.Sort((x, y) => 
            string.Compare(x.trigger, y.trigger, StringComparison.Ordinal));
        
        triggerMapProp.ClearArray();
        
        for (int i = 0; i < sortedTriggers.Count; i++)
        {
            triggerMapProp.InsertArrayElementAtIndex(i);
            var element = triggerMapProp.GetArrayElementAtIndex(i);
            element.FindPropertyRelative("trigger").stringValue = sortedTriggers[i].trigger;
            element.FindPropertyRelative("dialogueNode").objectReferenceValue = sortedTriggers[i].value;
        }
        
        serializedObject.ApplyModifiedProperties();
    }
}
