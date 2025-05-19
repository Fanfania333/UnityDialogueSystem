using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.InputSystem;

public class AutoSetup : EditorWindow
{
    [SerializeField]
    private VisualTreeAsset m_VisualTreeAsset = default;

    private Button button;
    private ProgressBar progressBar;

    [MenuItem("Dialogue System/Auto Setup")]
    public static void AutoSetupWindow()
    {
        AutoSetup wnd = GetWindow<AutoSetup>();
        wnd.titleContent = new GUIContent("AutoSetup");
    }

    public void CreateGUI()
    {
        // Each editor window contains a root VisualElement object
        VisualElement root = rootVisualElement;

        // Instantiate UXML
        VisualElement labelFromUXML = m_VisualTreeAsset.Instantiate();
        root.Add(labelFromUXML);
        
        button = root.Query<Button>("button").First();
        progressBar = root.Query<ProgressBar>("progressbar").First();

        button.clicked += StartSetup;
    }

    private void StartSetup()
    {
        // Instantiate canvas and manager into scene
        string canvasPrefabPath = "Packages/com.fanfania.dialogue-system/Resources/DialogueCanvas.prefab";
        string managerPrefabPath = "Packages/com.fanfania.dialogue-system/Resources/DialogueManager.prefab";
        
        GameObject canvas = AssetDatabase.LoadAssetAtPath<GameObject>(canvasPrefabPath);
        GameObject manager = AssetDatabase.LoadAssetAtPath<GameObject>(managerPrefabPath);

        if (canvas == null || manager == null)
        {
            Debug.LogError("Error in finding the prefabs in package");
        }
        
        GameObject canvasInstance = PrefabUtility.InstantiatePrefab(canvas) as GameObject;
        GameObject managerInstance = PrefabUtility.InstantiatePrefab(manager) as GameObject;

        // Create Dialogue Trigger Map
        string path = "Assets/DialogueSystem";

        if (!Directory.Exists(path))
        {
            Directory.CreateDirectory(path);
            AssetDatabase.Refresh();
        }

        var asset = ScriptableObject.CreateInstance<DialogueTriggerMapSO>();
        string assetPath = $"{path}/DialogueTriggerMap.asset";
        assetPath = AssetDatabase.GenerateUniqueAssetPath(assetPath);
        
        AssetDatabase.CreateAsset(asset, assetPath);
        AssetDatabase.SaveAssets();
        
        // Copy the InputActions
        string inputSourcePath = "Packages/com.fanfania.dialogue-system/Resources/DialogueInput.inputactions";
        string inputTargetPath = Path.Combine(path, "DialogueInput.inputactions");
        inputTargetPath = AssetDatabase.GenerateUniqueAssetPath(inputTargetPath);

        if (File.Exists(inputSourcePath))
        {
            File.Copy(inputSourcePath, inputTargetPath, false);
            AssetDatabase.ImportAsset(inputTargetPath);
        }
        else
        {
            Debug.LogError("Error in finding the input actions file");
        }
        
        // Copy the dialogue choice button prefab
        string choiceSourcePath = "Packages/com.fanfania.dialogue-system/Resources/DialogueChoiceButton.prefab";
        string choiceTargetPath = Path.Combine(path, "DialogueChoiceButton.prefab");
        choiceTargetPath = AssetDatabase.GenerateUniqueAssetPath(choiceTargetPath);

        if (File.Exists(choiceSourcePath))
        {
            File.Copy(choiceSourcePath, choiceTargetPath, false);
            AssetDatabase.ImportAsset(choiceTargetPath);
        }
        else
        {
            Debug.LogError("Error in finding the choice button prefab");
        }
        
        AssetDatabase.Refresh();
        
        if (canvasInstance != null)
        {
            Undo.RegisterCreatedObjectUndo(canvasInstance, "Create " + canvasInstance.name);
            canvasInstance.name = canvas.name;
        }
        
        if (managerInstance != null)
        {
            Undo.RegisterCreatedObjectUndo(managerInstance, "Create " + managerInstance.name);
            managerInstance.name = manager.name;

            DialogueManager dm = managerInstance.GetComponent<DialogueManager>();
            dm.dialogueCanvas = canvasInstance;
            dm.triggers = asset;
            dm.inputActions = AssetDatabase.LoadAssetAtPath<InputActionAsset>(inputTargetPath);
            dm.choiceButtonPrefab = AssetDatabase.LoadAssetAtPath<GameObject>(choiceTargetPath);
        }
    }
}
