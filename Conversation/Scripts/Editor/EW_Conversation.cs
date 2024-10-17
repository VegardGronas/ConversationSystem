using UnityEditor;
using UnityEngine;

namespace Narrator.New
{
    public class EW_Conversation : EditorWindow
    {
        private Conversation[] conversations;  // Store root conversations
        private bool[] foldouts;  // Store foldout states for each conversation

        private Vector2 scrollPosition;

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
                foldouts = new bool[conversations.Length];  // Initialize foldout states
            }

            if (conversations != null)
            {
                scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

                for (int i = 0; i < conversations.Length; i++)
                {
                    if (conversations[i].transform.parent == null)
                    {
                        // Display the root conversation as a foldout
                        foldouts[i] = EditorGUILayout.Foldout(foldouts[i], conversations[i].gameObject.name, true);

                        if (foldouts[i])
                        {
                            if (GUILayout.Button("Select " + conversations[i].gameObject.name))
                            {
                                // Select the root conversation in the scene
                                Selection.activeObject = conversations[i].gameObject;
                            }

                            // Access the dialogues and display them if the foldout is open
                            SerializedObject serializedConversation = new SerializedObject(conversations[i]);
                            SerializedProperty dialoguesProperty = serializedConversation.FindProperty("dialogues");
                            DisplayDialogueOptions(dialoguesProperty, 1, conversations[i]);
                        }
                    }
                }

                EditorGUILayout.EndScrollView();
            }
            else
            {
                GUILayout.Label("No conversations found. Click 'Refresh' to search.");
            }
        }

        // Recursively display dialogue options and their linked child conversations
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

                        // If the option has a linked conversation, recursively display its dialogues
                        if (conversationOnSelectProperty.objectReferenceValue != null)
                        {
                            Conversation linkedConversation = (Conversation)conversationOnSelectProperty.objectReferenceValue;

                            // Only display child conversations within the context of the parent
                            if (linkedConversation != rootConversation)
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
