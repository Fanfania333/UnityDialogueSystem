using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

#if USE_LOCALIZATION
using UnityEngine.Localization;
using UnityEngine.Localization.Components;
#endif

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;
    
    [Header("Settings")]
    [SerializeField] private float typeSpeed;
    [SerializeField] private bool useLocalization;
    
    [Header("References")]
    public DialogueTriggerMapSO triggers;
    public InputActionAsset inputActions;
    public GameObject choiceButtonPrefab;
    
    // Dialogue Box
    public GameObject dialogueCanvas;
    private GameObject dialogueBox;
    private GameObject choiceMenu;
    private Image characterPortrait;
    private TMP_Text characterName;
    private TMP_Text dialogueText;

    private bool isTyping;
    private bool isChoosing;
    private DialogueNodeSO currentNode;
    
    // Input
    private InputAction continueAction;

    private List<DialogueChoiceSO> currentVisibleChoices;
    
    #if USE_LOCALIZATION
    LocalizedString currentLocalizedString;
    #endif

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        
        Instance = this;
        
        if (triggers == null)
        {
            Debug.LogError("No trigger map was assigned to DialogueManager. " +
                           "Please create a DialogueTriggerMap ScriptableObject and assign it in the Inspector.");
        }

        if (dialogueCanvas == null)
        {
            Debug.LogError("No dialogue canvas is assigned to DialogueManager. Please import the one from the package.");
        }
        else
        {
            dialogueBox = dialogueCanvas.transform.Find("DialogueBox").gameObject;
            choiceMenu = dialogueCanvas.transform.Find("DialogueChoices").gameObject;
            
            if (choiceMenu == null)
            {
                Debug.LogError("Couldn't find the references to choice menu.");
            }
            
            if (dialogueBox == null)
            {
                Debug.LogError("Couldn't find the references to dialogue Box ");
            }
            else
            {
                characterPortrait = dialogueBox.transform.Find("PortraitFrame").Find("PortraitImage").GetComponent<Image>();
                characterName = dialogueBox.transform.Find("SpeakerName").GetComponent<TMP_Text>();
                dialogueText = dialogueBox.transform.Find("Text").GetComponent<TMP_Text>();

                string errorMessage = "Couldn't find the references to: ";
                if (characterPortrait == null) errorMessage += "Character portrait ";
                if (characterName == null) errorMessage += "Character name ";
                if (dialogueText == null) errorMessage += "Dialogue text ";

                if (errorMessage != "Couldn't find the references to: ")
                {
                    Debug.LogError(errorMessage + ". Please check the dialogue box prefab and ensure the children's names have not been changed.");
                }
            }
        }
        
        dialogueBox.SetActive(false);
        choiceMenu.SetActive(false);

        if (inputActions == null)
        {
            Debug.LogError("No InputActions assigned to DialogueManager.");
        }
        else
        {
            continueAction = inputActions.FindActionMap("Dialogue").FindAction("Continue");
            continueAction.Enable();
            
            continueAction.performed += ctx => AdvanceDialogue();
        }
    }
    

    public void StartDialogue(string trigger)
    {
        if (currentNode != null)
        {
            Debug.LogWarning("Dialogue is already open.");
            return;
        }
        
        currentNode = triggers.GetDialogueStart(trigger);

        if (currentNode == null)
        {
            Debug.LogError($"Dialogue Trigger '{trigger}' was not found in the triggers list or is not associated with a Dialogue Node.");
            return;
        }
        
        ShowDialogue();
    }
    
    private void AdvanceDialogue()
    {
        if (isChoosing)
        {
            return;
        }
        
        if (currentNode != null)
        {
            ShowDialogue();
        }
        else
        {
            dialogueBox.SetActive(false);
        }
    }

    private void ShowDialogue()
    {
        dialogueBox.SetActive(true);

        if (isTyping)
        {
            StopAllCoroutines();
            dialogueText.maxVisibleCharacters = dialogueText.textInfo.characterCount;
            isTyping = false;
            GetNextNode();
            return;
        }
        
        characterPortrait.sprite = currentNode.Speaker.Portrait;
        string characterNameLocal = currentNode.Speaker.Name;
        
        #if USE_LOCALIZATION
            if (useLocalization)
            {
                currentLocalizedString = new LocalizedString();
                currentLocalizedString.TableReference = currentNode.Speaker.TableReference;
                currentLocalizedString.TableEntryReference = characterNameLocal;

                try
                {
                    characterNameLocal = currentLocalizedString.GetLocalizedString();
                }
                catch
                {
                    Debug.LogError($"Table reference {currentLocalizedString.TableReference} could not be found. Please check your localization.");
                }
            }
        #endif
        
        characterName.text = characterNameLocal;
        StartCoroutine(TypeText(currentNode.Text, currentNode.TableReference));
    }

    private IEnumerator TypeText(string fullText, string localizationTableReference = "")
    {
        isTyping = true;
        
        #if USE_LOCALIZATION
        if (useLocalization)
        {
            currentLocalizedString = new LocalizedString();
            currentLocalizedString.TableReference = localizationTableReference;
            currentLocalizedString.TableEntryReference = fullText;
            try
            {
                fullText = currentLocalizedString.GetLocalizedString();
            }
            catch
            {
                Debug.LogError($"Table reference {currentLocalizedString.TableReference} could not be found. Please check your localization.");
            }
        }
        #endif
        
        dialogueText.text = fullText;
        dialogueText.ForceMeshUpdate();
        int totalChars = dialogueText.textInfo.characterCount;

        dialogueText.maxVisibleCharacters = 0;

        for (int i = 0; i < totalChars; i++)
        {
            dialogueText.maxVisibleCharacters = i + 1;
            yield return new WaitForSeconds(typeSpeed);
        }
        
        isTyping = false;
        GetNextNode();
    }

    private void GetNextNode()
    {
        currentVisibleChoices = new List<DialogueChoiceSO>();
        foreach (var choice in currentNode.NextChoices)
        {
            if (FlagManager.Instance.EvaluateCondition(choice.VisibleConditions))
            {
                currentVisibleChoices.Add(choice);
            }
        }
        
        if (currentVisibleChoices.Count == 0)
        {
            currentNode = null;
        }
        else if (currentVisibleChoices.Count == 1)
        {
            currentNode = currentVisibleChoices[0].NextNode;
        }
        else
        {
            DisplayChoiceMenu();
        }
    }

    private void DisplayChoiceMenu()
    {
        isChoosing = true;
        choiceMenu.SetActive(true);

        for (int i = 0; i < currentVisibleChoices.Count; i++)
        {
            GameObject choiceButton = Instantiate(choiceButtonPrefab, choiceMenu.transform);
            string choiceText = currentVisibleChoices[i].ChoiceDescription;
            
            #if USE_LOCALIZATION
                if (useLocalization)
                {
                    currentLocalizedString = new LocalizedString();
                    currentLocalizedString.TableReference = currentVisibleChoices[i].TableReference;
                    currentLocalizedString.TableEntryReference = choiceText;
                    try
                    {
                        choiceText = currentLocalizedString.GetLocalizedString();
                    }
                    catch
                    {
                        Debug.LogError($"Table reference {currentLocalizedString.TableReference} could not be found. Please check your localization.");
                    }
                }
            #endif
            
            choiceButton.GetComponentInChildren<TMP_Text>().text = choiceText;
            
            int choiceIndex = i;
            Button button = choiceButton.GetComponent<Button>();
            button.onClick.AddListener(() => OnChoiceSelected(choiceIndex));

            if (!FlagManager.Instance.EvaluateCondition(currentVisibleChoices[i].UnlockedConditions))
            {
                button.interactable = false;
            }
        }
    }

    private void OnChoiceSelected(int index)
    {
        currentNode = currentVisibleChoices[index].NextNode;

        foreach (var result in currentVisibleChoices[index].Results)
        {
            switch (result.flagType)
            {
                case FlagType.Bool:
                    FlagManager.Instance.AddFlag(result.key);
                    break;
                case FlagType.Int:
                    if (result.resultType == DialogueChoiceSO.DialogueResult.ResultType.SET)
                    {
                        FlagManager.Instance.SetIntFlag(result.key, result.intValue);
                    }
                    else if (result.resultType == DialogueChoiceSO.DialogueResult.ResultType.ADD)
                    {
                        FlagManager.Instance.AddToIntFlag(result.key, result.intValue);
                    }
                    break;
                case FlagType.Float:
                    if (result.resultType == DialogueChoiceSO.DialogueResult.ResultType.SET)
                    {
                        FlagManager.Instance.SetFloatFlag(result.key, result.floatValue);
                    }
                    else if (result.resultType == DialogueChoiceSO.DialogueResult.ResultType.ADD)
                    {
                        FlagManager.Instance.AddToFloatFlag(result.key, result.floatValue);
                    }
                    break;
                case FlagType.String:
                    FlagManager.Instance.SetStringFlag(result.key, result.stringValue);
                    break;
            }
        }

        for (int i = choiceMenu.transform.childCount - 1; i >= 0; i--)
        {
            Destroy(choiceMenu.transform.GetChild(i).gameObject);
        }
        
        isChoosing = false;
        AdvanceDialogue();
        choiceMenu.SetActive(false);
        currentVisibleChoices = null;
    }
}
