using UnityEngine;
using System;
using System.Collections.Generic;

public class NonPlayerCharacter : MonoBehaviour
{
    [Header("NPC Settings")]
    public string npcName = "NPC";
    public Sprite portraitSprite;
    
    [Header("Dialogue Settings")]
    [TextArea(3, 10)]
    public string[] dialogueLines;
    public float interactionRadius = 2f;
    
    [Header("Visual Feedback")]
    public GameObject interactionIndicator;
    
    private bool playerInRange = false;
    private int currentLineIndex = 0;
    private bool isInDialogue = false;
    private Transform playerTransform;

    private void Start()
    {
        if (interactionIndicator != null)
        {
            interactionIndicator.SetActive(false);
        }
    }

    private void Update()
    {
        UpdateInteractionIndicator();
    }

    private void UpdateInteractionIndicator()
    {
        if (interactionIndicator != null)
        {
            bool shouldShow = playerInRange && !isInDialogue;
            interactionIndicator.SetActive(shouldShow);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = true;
            playerTransform = other.transform;
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInRange = false;
            playerTransform = null;
            EndDialogue();
        }
    }

    // Public method called by PlayerController
    public void DisplayDialog()
    {
        Debug.Log($"DisplayDialog called on NPC: {gameObject.name}");
        
        // Check if dialogue lines exist
        if (dialogueLines == null || dialogueLines.Length == 0)
        {
            Debug.LogWarning($"No dialogue lines set for NPC: {gameObject.name}");
            return;
        }

        if (!isInDialogue)
        {
            StartDialogue();
        }
        else
        {
            AdvanceDialogue();
        }
    }

    private void StartDialogue()
    {
        if (dialogueLines == null || dialogueLines.Length == 0)
        {
            Debug.LogWarning($"No dialogue lines set for NPC: {gameObject.name}");
            return;
        }

        Debug.Log($"Starting dialogue with {dialogueLines.Length} lines");
        isInDialogue = true;
        currentLineIndex = 0;
        DisplayCurrentLine();
    }

    private void AdvanceDialogue()
    {
        if (!isInDialogue) return;

        currentLineIndex++;

        if (currentLineIndex >= dialogueLines.Length)
        {
            EndDialogue();
        }
        else
        {
            DisplayCurrentLine();
        }
    }

    private void DisplayCurrentLine()
    {
        if (!isInDialogue || dialogueLines == null || currentLineIndex >= dialogueLines.Length) 
        {
            Debug.LogWarning($"Cannot display dialogue line. isInDialogue: {isInDialogue}, dialogueLines null: {dialogueLines == null}, currentLineIndex: {currentLineIndex}");
            return;
        }

        Debug.Log($"Displaying line {currentLineIndex + 1} of {dialogueLines.Length}");
        if (UIHandler.instance != null)
        {
            Debug.Log($"UIHandler found, showing dialogue: {dialogueLines[currentLineIndex]}");
            UIHandler.instance.ShowDialogue(npcName, dialogueLines[currentLineIndex], portraitSprite);
        }
        else
        {
            Debug.LogError("UIHandler.instance is null! Make sure there's a UIHandler in the scene.");
        }
    }

    private void EndDialogue()
    {
        if (!isInDialogue) return;

        isInDialogue = false;
        if (UIHandler.instance != null)
        {
            UIHandler.instance.HideDialogue();
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, interactionRadius);
    }
}
