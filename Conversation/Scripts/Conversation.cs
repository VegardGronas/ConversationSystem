using System;
using System.Collections.Generic;
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

        private void Start()
        {
            if(ConversationManager.Instance == null)
            {
                Debug.LogError("Missing conversationmanager");
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
                var currentDialogue = dialogues[currentDialogueIndex];

                m_CurrentDisplayTime = currentDialogue.duration;

                m_HasOptions = currentDialogue.hasOptions;

                ShowOptions(currentDialogue.options);

                if(currentDialogue.narratorAudioClip)
                {
                    ConversationManager.Instance.AudioSource.clip = currentDialogue.narratorAudioClip;
                    ConversationManager.Instance.AudioSource.Play();
                }

                ConversationManager.Instance.StartConversation(currentDialogue);
            }
            else
            {
                EndConversation();
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
            if(!m_HasOptions && m_ConversationStarted)
            {
                m_CurrentDisplayTime -= Time.deltaTime;

                if(m_CurrentDisplayTime < 0 )
                {
                    m_ConversationStarted = false;
                    currentDialogueIndex++;

                    if (currentDialogueIndex < dialogues.Count)
                    {
                        StartConversation();
                    }
                    else
                    {
                        EndConversation();   
                    }
                }
            }
        }
    }

    [Serializable]
    public class DialogueData
    {
        public string text;  // Dialogue text

        public AudioClip narratorAudioClip;

        public float duration;

        public bool hasOptions;

        public List<DialogueOption> options;  // Multiple options for this dialogue
    }

    [Serializable]
    public class DialogueOption
    {
        public string optionText;  // Text shown for this option
        public Conversation conversationOnSelect;  // Optional next conversation

        public void RunOption()
        {
            conversationOnSelect.StartConversation();
        }
    }
}
