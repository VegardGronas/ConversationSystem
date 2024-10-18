using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Narrator.New
{
    [CustomEditor(typeof(Conversation))]
    public class E_Conversation : Editor
    {
        public override void OnInspectorGUI()
        {
            Conversation conversation = (Conversation)target;

            if (Application.isPlaying)
            {
                // Add a button to start the conversation
                if (GUILayout.Button("Start Conversation"))
                {
                    conversation.StartConversation();
                }
            }

            // Ensure there is at least one dialogue
            if (conversation.dialogues.Count <= 0)
            {
                conversation.AddDialogue();
            }

            for (int i = 0; i < conversation.dialogues.Count; i++)
            {
                DialogueData dialogue = conversation.dialogues[i];

                // Start a horizontal group
                EditorGUILayout.BeginHorizontal();

                // Button to remove the conversation
                if (GUILayout.Button("Remove Conversation"))
                {
                    conversation.RemoveConversation(i);
                }

                // Button to move the dialogue up
                if (GUILayout.Button("Move Up"))
                {
                    conversation.MoveConversation(dialogue, -1);
                    Repaint();
                }

                // Button to move the dialogue down
                if (GUILayout.Button("Move Down"))
                {
                    conversation.MoveConversation(dialogue, 1);
                    Repaint();
                }

                // End the horizontal group
                EditorGUILayout.EndHorizontal();

                // Use direct access to properties
                dialogue.text = EditorGUILayout.TextArea(dialogue.text, GUILayout.Height(60));
                dialogue.narratorAudioClip = (AudioClip)EditorGUILayout.ObjectField("Narrator Audio", dialogue.narratorAudioClip, typeof(AudioClip), false);
                dialogue.duration = EditorGUILayout.FloatField("Duration", dialogue.duration);
                dialogue.hasOptions = EditorGUILayout.Toggle("Has Options", dialogue.hasOptions);

                // Initialize options if null
                if (dialogue.options == null)
                {
                    dialogue.options = new List<DialogueOption>();
                }

                // Conditionally show the options list if hasOptions is true
                if (dialogue.hasOptions)
                {
                    for (int j = 0; j < dialogue.options.Count; j++)
                    {
                        DialogueOption option = dialogue.options[j];

                        // Ensure option is not null
                        if (option == null)
                        {
                            option = new DialogueOption();
                            dialogue.options[j] = option;
                        }

                        option.optionText = EditorGUILayout.TextField($"Option {j + 1} Text", option.optionText);
                        option.conversationOnSelect = (Conversation)EditorGUILayout.ObjectField($"Option {j + 1} Next Conversation", option.conversationOnSelect, typeof(Conversation), true);

                        EditorGUILayout.BeginHorizontal();

                        if (GUILayout.Button($"Remove Option {j + 1}"))
                        {
                            dialogue.options.RemoveAt(j);
                            Repaint();
                            break; // Exit the loop to avoid index issues
                        }

                        if (GUILayout.Button("Move up"))
                        {
                            dialogue.MoveOptionUp(option);
                        }

                        if (GUILayout.Button("Move down"))
                        {
                            dialogue.MoveOptionDown(option);
                        }

                        EditorGUILayout.EndHorizontal();
                    }

                    // Button to add a new option
                    if (GUILayout.Button("Add New Option"))
                    {
                        dialogue.options.Add(new DialogueOption());
                    }
                }
                else
                {
                    EditorGUILayout.HelpBox("Options are hidden because 'Has Options' is disabled.", MessageType.Info);
                }

                EditorGUILayout.Space();
            }

            if (!Application.isPlaying)
            {
                if (GUILayout.Button("Load Formatted Text"))
                {
                    // Open a file selection dialog
                    string path = EditorUtility.OpenFilePanel("Load Formatted Text", "", "txt");
                    if (!string.IsNullOrEmpty(path))
                    {
                        conversation.LoadFormattedText(path);
                    }
                }

                if (GUILayout.Button("Add new conversation"))
                {
                    conversation.AddNewConversation();
                }
            }
        }
    }
}
