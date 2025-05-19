using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.InputSystem;

public class DialogueManager : MonoBehaviour
{
    public static DialogueManager Instance;
    
    public DialogueTriggerMapSO triggers;
    public InputActionAsset inputActions;
    
    // Dialogue Box
    [SerializeField] private float typeSpeed;
    public GameObject dialogueCanvas;
    private GameObject dialogueBox;
    private Image characterPortrait;
    private TMP_Text characterName;
    private TMP_Text dialogueText;

    private bool isTyping;
    private DialogueNodeSO currentNode;
    
    // Input
    private InputAction continueAction;

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
            dialogueBox = dialogueCanvas.transform.GetChild(0).gameObject;
            if (dialogueBox == null)
            {
                Debug.LogError("Couldn't find the references to dialogue Box ");
            }
            else
            {
                characterPortrait = dialogueBox.transform.Find("Portrait").GetComponent<Image>();
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
        characterName.text = currentNode.Speaker.Name;
        StartCoroutine(TypeText(currentNode.Text));
    }

    private IEnumerator TypeText(string fullText)
    {
        isTyping = true;
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
        currentNode = currentNode.NextNode;
    }
}
