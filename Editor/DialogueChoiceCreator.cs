using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;

public class DialogueChoiceCreator : EditorWindow
{
    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;

    private Button createButton;
    private TextField nameField;
    private Toggle toggle;

    [MenuItem("Dialogue System/Create Dialogue Choice")]
    public static void ShowExample()
    {
        DialogueChoiceCreator wnd = GetWindow<DialogueChoiceCreator>();
        wnd.titleContent = new GUIContent("DialogueChoiceCreator");
    }

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // Instantiate UXML
        VisualElement labelFromUXML = m_VisualTreeAsset.Instantiate();
        root.Add(labelFromUXML);
        
        createButton = root.Query<Button>("create").First();
        nameField = root.Query<TextField>("name").First();
        toggle = root.Query<Toggle>("autoadd").First();
        
        toggle.value = true;

        createButton.clicked += CreateChoice;
    }

    private void CreateChoice()
    {
        string path = "Assets/DialogueSystem";
        string choicePath = path + "/Choices";
        string dialoguePath = path + "/Dialogue";
        DialogueNodeSO parent = null;

        if (toggle.value && Selection.activeObject != null && Selection.activeObject is DialogueNodeSO)
        {
            parent = Selection.activeObject as DialogueNodeSO;
        }
        
        // Create the directories.
        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
        }

        if (!Directory.Exists(dialoguePath))
        {
            Directory.CreateDirectory(dialoguePath);
        }
        
        if (!Directory.Exists(choicePath))
        {
            Directory.CreateDirectory(choicePath);
        }
        
        // Create DialogueChoice
        var choiceTargetPath = choicePath + "/C_" + nameField.value + ".asset";
        choiceTargetPath = AssetDatabase.GenerateUniqueAssetPath(choiceTargetPath);
        
        DialogueChoiceSO choice = ScriptableObject.CreateInstance<DialogueChoiceSO>();
        
        // Create Dialogue Node
        var nodeTargetPath = dialoguePath + "/D_" + nameField.value + ".asset";
        nodeTargetPath = AssetDatabase.GenerateUniqueAssetPath(nodeTargetPath);
        
        DialogueNodeSO node = ScriptableObject.CreateInstance<DialogueNodeSO>();
        
        // Set the links between objects.
        choice.nextNode = node;
        
        AssetDatabase.CreateAsset(choice, choiceTargetPath);
        AssetDatabase.CreateAsset(node, nodeTargetPath);
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        if (parent != null)
        {
            if (parent.NextChoices == null)
            {
                parent.nextChoices = new List<DialogueChoiceSO>();
            }
            
            parent.nextChoices.Add(choice);
            EditorUtility.SetDirty(parent);
        }
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
}
