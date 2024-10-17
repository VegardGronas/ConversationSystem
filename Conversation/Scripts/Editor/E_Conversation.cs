using UnityEditor;
using UnityEngine;

namespace Narrator.New
{
    [CustomEditor(typeof(Conversation))]
    public class E_Conversation : Editor
    {
        SerializedProperty dialogues;

        private void OnEnable()
        {
            // Find the property for the dialogues list
            dialogues = serializedObject.FindProperty("dialogues");
        }

        public override void OnInspectorGUI()
        {
            Conversation conversation = (Conversation)target;

            if (Application.isPlaying)
            {
                // Add a button to the inspector
                if (GUILayout.Button("Start Conversation"))
                {
                    // Call the method to start the conversation
                    conversation.StartConversation();
                }
            }

            for (int i = 0; i < dialogues.arraySize; i++)
            {
                SerializedProperty dialogue = dialogues.GetArrayElementAtIndex(i);

                EditorGUILayout.PropertyField(dialogue.FindPropertyRelative("text"), new GUIContent("Dialogue Text"));
                EditorGUILayout.PropertyField(dialogue.FindPropertyRelative("narratorAudioClip"), new GUIContent("Narrator Audio"));
                EditorGUILayout.PropertyField(dialogue.FindPropertyRelative("duration"), new GUIContent("Duration"));
                SerializedProperty hasOptions = dialogue.FindPropertyRelative("hasOptions");
                EditorGUILayout.PropertyField(hasOptions, new GUIContent("Has Options"));

                // Conditionally show the options list if hasOptions is true
                if (hasOptions.boolValue)
                {
                    EditorGUILayout.PropertyField(dialogue.FindPropertyRelative("options"), new GUIContent("Dialogue Options"), true);
                }
                else
                {
                    EditorGUILayout.HelpBox("Options are hidden because 'Has Options' is disabled.", MessageType.Info);
                }

                EditorGUILayout.Space();
            }

            // Apply any changes made to the serialized object
            serializedObject.ApplyModifiedProperties();
        }
    }
}