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
                // Add a button to the inspector
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
                    conversation.MoveConversation(dialogue, -1); // Pass the dialogue to move
                    Repaint();
                }

                // Button to move the dialogue down
                if (GUILayout.Button("Move Down"))
                {
                    conversation.MoveConversation(dialogue, 1); // Pass the dialogue to move
                    Repaint();
                }

                // End the horizontal group
                EditorGUILayout.EndHorizontal();

                // Use direct access to properties
                dialogue.text = EditorGUILayout.TextField("Dialogue Text", dialogue.text);
                dialogue.narratorAudioClip = (AudioClip)EditorGUILayout.ObjectField("Narrator Audio", dialogue.narratorAudioClip, typeof(AudioClip), false);
                dialogue.duration = EditorGUILayout.FloatField("Duration", dialogue.duration);
                dialogue.hasOptions = EditorGUILayout.Toggle("Has Options", dialogue.hasOptions);

                // Conditionally show the options list if hasOptions is true
                if (dialogue.hasOptions)
                {
                    // Use a simple loop to display each option
                    for (int j = 0; j < dialogue.options.Count; j++)
                    {
                        DialogueOption option = dialogue.options[j];
                        option.optionText = EditorGUILayout.TextField($"Option {j + 1} Text", option.optionText);
                        option.conversationOnSelect = (Conversation)EditorGUILayout.ObjectField($"Option {j + 1} Next Conversation", option.conversationOnSelect, typeof(Conversation), true);

                        // Add buttons to remove or move options
                        if (GUILayout.Button($"Remove Option {j + 1}"))
                        {
                            dialogue.options.RemoveAt(j);
                            Repaint();
                            break; // Exit the loop to avoid index issues
                        }
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
                if (GUILayout.Button("Add new conversation"))
                {
                    conversation.AddNewConversation();
                }
            }
        }
    }
}