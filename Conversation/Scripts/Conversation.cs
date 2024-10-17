using System;
using System.Collections.Generic;
using UnityEngine;

namespace Narrator.New
{
    public class Conversation : MonoBehaviour
    {
        [SerializeField]
        List<DialogueData> dialogues;  // List of dialogues

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

        public void StartConversation()
        {
            Debug.Log("Conversation Started");
            ShowDialogue();
            m_ConversationStarted = true;
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
