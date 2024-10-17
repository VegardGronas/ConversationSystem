using UnityEditor;
using UnityEngine;
using System.Collections.Generic;

namespace Narrator.New
{
    public class EW_Conversation : EditorWindow
    {
        private Conversation[] conversations;  // Store root conversations

        [MenuItem("Tools/Conversation Finder")]
        public static void ShowWindow()
        {
            GetWindow<EW_Conversation>("Conversation Finder");
        }

        private void OnGUI()
        {
            // Button to refresh the conversation list
            if (GUILayout.Button("Refresh Conversations"))
            {
                // Find all root conversations in the scene
                conversations = FindObjectsByType<Conversation>(FindObjectsSortMode.None);
            }

            if (conversations != null)
            {
                foreach (var conversation in conversations)
                {
                    if (conversation.transform.parent == null)
                    {
                        // Display the root conversation as a button
                        if (GUILayout.Button(conversation.gameObject.name))
                        {
                            // Select the root conversation in the scene
                            Selection.activeObject = conversation.gameObject;
                        }

                        // Access the dialogues and recursively display options
                        SerializedObject serializedConversation = new SerializedObject(conversation);
                        SerializedProperty dialoguesProperty = serializedConversation.FindProperty("dialogues");

                        // Display the dialogues and options in the context of the root conversation
                        DisplayDialogueOptions(dialoguesProperty, 1, conversation);
                    }
                }
            }
            else
            {
                GUILayout.Label("No conversations found. Click 'Refresh' to search.");
            }
        }

        // Recursively display dialogue options and their linked child conversations, but avoid duplication
        private void DisplayDialogueOptions(SerializedProperty dialoguesProperty, int indentLevel, Conversation rootConversation)
        {
            if (dialoguesProperty == null || dialoguesProperty.arraySize == 0) return;

            for (int i = 0; i < dialoguesProperty.arraySize; i++)
            {
                var dialogue = dialoguesProperty.GetArrayElementAtIndex(i);

                SerializedProperty textProperty = dialogue.FindPropertyRelative("text");
                SerializedProperty hasOptionsProperty = dialogue.FindPropertyRelative("hasOptions");
                SerializedProperty optionsProperty = dialogue.FindPropertyRelative("options");

                // Display the dialogue as part of the root conversation
                GUILayout.BeginHorizontal();
                GUILayout.Space(indentLevel * 20);  // Indent for hierarchy
                GUILayout.Label("Dialogue: " + textProperty.stringValue);
                GUILayout.EndHorizontal();

                // If this dialogue has options, display them
                if (hasOptionsProperty.boolValue && optionsProperty != null)
                {
                    for (int j = 0; j < optionsProperty.arraySize; j++)
                    {
                        SerializedProperty optionProperty = optionsProperty.GetArrayElementAtIndex(j);
                        SerializedProperty optionTextProperty = optionProperty.FindPropertyRelative("optionText");
                        SerializedProperty conversationOnSelectProperty = optionProperty.FindPropertyRelative("conversationOnSelect");

                        // Display the option as part of the current root conversation
                        GUILayout.BeginHorizontal();
                        GUILayout.Space((indentLevel + 1) * 20);  // Indent for options
                        if (GUILayout.Button("Option: " + optionTextProperty.stringValue))
                        {
                            // Select the conversation triggered by this option
                            Conversation linkedConversation = (Conversation)conversationOnSelectProperty.objectReferenceValue;
                            if (linkedConversation != null)
                            {
                                Selection.activeObject = linkedConversation.gameObject;
                            }
                            else
                            {
                                Debug.LogWarning("No linked conversation found for this option.");
                            }
                        }
                        GUILayout.EndHorizontal();

                        // If the option has a linked conversation, recursively display its dialogues, but only as a child
                        if (conversationOnSelectProperty.objectReferenceValue != null)
                        {
                            Conversation linkedConversation = (Conversation)conversationOnSelectProperty.objectReferenceValue;

                            // Only display child conversations within the context of the parent (not as a new root)
                            if (linkedConversation == rootConversation)
                            {
                                // Avoid recursive loops if the option points back to the same conversation
                                Debug.LogWarning("Option links to the same root conversation, skipping.");
                            }
                            else
                            {
                                SerializedObject linkedSerializedConversation = new SerializedObject(linkedConversation);
                                SerializedProperty linkedDialoguesProperty = linkedSerializedConversation.FindProperty("dialogues");

                                // Recursively display linked conversation's dialogues as children of the root
                                DisplayDialogueOptions(linkedDialoguesProperty, indentLevel + 2, linkedConversation);
                            }
                        }
                    }
                }
            }
        }
    }
}