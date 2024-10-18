using System;
using System.Collections.Generic;
using System.IO; // Add this for file handling
using System.Linq; // Add this for LINQ
using UnityEngine;

namespace Narrator.New
{
    public class Conversation : MonoBehaviour
    {
        public List<DialogueData> dialogues = new();  // List of dialogues
        private int currentDialogueIndex = 0;  // Tracks the current dialogue
        float m_CurrentDisplayTime = 0;
        bool m_ConversationStarted;
        bool m_HasOptions;

        DialogueData m_CurrentDialogue;

        private void Start()
        {
            if (ConversationManager.Instance == null)
            {
                Debug.LogError("Missing conversation manager");
                enabled = false;
            }
        }

        public void AddDialogue()
        {
            if (dialogues == null || dialogues.Count == 0)
            {
                dialogues = new List<DialogueData> { new DialogueData() };
            }
        }

        public void AddNewConversation()
        {
            DialogueData data = new();
            dialogues.Add(data);
        }

        // New method to load formatted text from a file
        public void LoadFormattedText(string filePath)
        {
            if (File.Exists(filePath))
            {
                string fullText = File.ReadAllText(filePath);
                string[] sentences = fullText.Split(new[] { '.', '?' }, StringSplitOptions.RemoveEmptyEntries);
                dialogues.Clear(); // Clear existing dialogues

                foreach (string sentence in sentences)
                {
                    // Trim whitespace and create new DialogueData for each sentence
                    string trimmedSentence = sentence.Trim();
                    if (!string.IsNullOrEmpty(trimmedSentence))
                    {
                        DialogueData dialogueData = new DialogueData { text = trimmedSentence };
                        dialogues.Add(dialogueData);
                    }
                }
                Debug.Log("Formatted text loaded successfully.");
            }
            else
            {
                Debug.LogError($"File not found: {filePath}");
            }
        }

        public void StartConversation()
        {
            Debug.Log("Conversation Started");
            ShowDialogue();
            m_ConversationStarted = true;
        }

        public void RemoveConversation(int index)
        {
            dialogues.RemoveAt(index);
        }

        public void MoveConversation(DialogueData dialogueToMove, int direction)
        {
            // Check if there are dialogues to move
            if (dialogues.Count == 0 || dialogueToMove == null) return;

            // Get the current index of the dialogue to move
            int currentIndex = dialogues.IndexOf(dialogueToMove);

            // Make sure the current index is within the list bounds
            if (currentIndex < 0 || currentIndex >= dialogues.Count) return;

            // Calculate the new index
            int newIndex = currentIndex + direction;

            // Ensure the new index is within the list bounds
            if (newIndex < 0 || newIndex >= dialogues.Count) return;

            // Remove the dialogue from its current position
            dialogues.RemoveAt(currentIndex);

            // Insert it at the new position
            dialogues.Insert(newIndex, dialogueToMove);
        }

        public void ShowDialogue()
        {
            if (currentDialogueIndex < dialogues.Count)
            {
                m_CurrentDialogue = dialogues[currentDialogueIndex];

                m_CurrentDisplayTime = m_CurrentDialogue.duration;

                m_HasOptions = m_CurrentDialogue.hasOptions;

                ShowOptions(m_CurrentDialogue.options);

                if (m_CurrentDialogue.narratorAudioClip)
                {
                    ConversationManager.Instance.AudioSource.clip = m_CurrentDialogue.narratorAudioClip;
                    ConversationManager.Instance.AudioSource.Play();
                }

                ConversationManager.Instance.StartConversation(m_CurrentDialogue);
            }
            else
            {
                EndConversation();
            }
        }

        private void ContinueConversation()
        {
            currentDialogueIndex++;

            if (currentDialogueIndex < dialogues.Count)
            {
                StartConversation();  // Start the next dialogue
            }
            else
            {
                EndConversation();  // End the conversation if there are no more dialogues
            }
        }

        private void EndConversation()
        {
            Debug.Log("Conversation ended.");
            m_ConversationStarted = false;
            ConversationManager.Instance.EndConversation();
        }

        private void ShowOptions(List<DialogueOption> options)
        {
            for (int i = 0; i < options.Count; i++)
            {
                Debug.Log($"{i + 1}: {options[i].optionText}");
            }
        }

        private void Update()
        {
            if (!m_HasOptions && m_ConversationStarted)
            {
                m_CurrentDisplayTime -= Time.deltaTime;

                if (m_CurrentDisplayTime < 0)
                {
                    m_ConversationStarted = false;
                    ContinueConversation(); // Call ContinueConversation instead of directly incrementing
                }
            }
            else if(m_HasOptions && m_ConversationStarted)
            {
                foreach(DialogueOption option in m_CurrentDialogue.options)
                {
                    if(option.continueConversation)
                    {
                        ContinueConversation();
                        break;
                    }
                }
            }
        }
    }

    [Serializable]
    public class DialogueData
    {
        public string text = "Write here:D";  // Dialogue text

        public AudioClip narratorAudioClip;

        [Tooltip("Time in seconds to show the dialogue")]
        public float duration = 5;

        public bool hasOptions;

        public List<DialogueOption> options;  // Multiple options for this dialogue

        // Method to move an option up in the list
        public void MoveOptionUp(DialogueOption optionToMove)
        {
            int currentIndex = options.IndexOf(optionToMove);
            if (currentIndex <= 0 || currentIndex >= options.Count)
                return;

            options.RemoveAt(currentIndex);
            options.Insert(currentIndex - 1, optionToMove);
        }

        // Method to move an option down in the list
        public void MoveOptionDown(DialogueOption optionToMove)
        {
            int currentIndex = options.IndexOf(optionToMove);
            if (currentIndex < 0 || currentIndex >= options.Count - 1)
                return;

            options.RemoveAt(currentIndex);
            options.Insert(currentIndex + 1, optionToMove);
        }
    }

    [Serializable]
    public class DialogueOption
    {
        public string optionText;  // Text shown for this option
        public Conversation conversationOnSelect;  // Optional next conversation

        public bool continueConversation = false;

        public void RunOption()
        {
            if(conversationOnSelect != null)
            {
                conversationOnSelect.StartConversation();
            }
            else
            {
                continueConversation = true;
            }
        }
    }
}
