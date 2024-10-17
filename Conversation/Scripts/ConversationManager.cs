using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Narrator.New
{
    [RequireComponent(typeof(AudioSource))]
    public class ConversationManager : MonoBehaviour
    {
        public static ConversationManager Instance { get; private set; }

        [SerializeField]
        Transform m_DialogueContainer;

        [SerializeField]
        TextMeshProUGUI m_DialogueLabel;

        [SerializeField]
        Button m_OptionsBtn;

        [SerializeField]
        Transform m_OptionsContainer;

        [SerializeField]
        AudioSource m_AudioSource;

        public AudioSource AudioSource => m_AudioSource;

        private void Awake()
        {
            // Singleton pattern
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
            }
            else
            {
                Instance = this;
                DontDestroyOnLoad(gameObject); // Optional: if you want it to persist across scenes
            }

            m_AudioSource = GetComponent<AudioSource>();
            m_AudioSource.playOnAwake = false;
        }

        public void StartConversation(DialogueData dialogueData)
        {
            ClearOptions();

            m_DialogueLabel.text = dialogueData.text;
        
            if(dialogueData.options.Count > 0)
            {
                foreach (DialogueOption option in dialogueData.options)
                {
                    AddOptions(option);
                }
            }
        }

        private void AddOptions(DialogueOption option)
        {
            m_OptionsContainer.gameObject.SetActive(true);
            m_DialogueContainer.gameObject.SetActive(true);

            Button btn = Instantiate(m_OptionsBtn, m_OptionsContainer);
            btn.GetComponentInChildren<TextMeshProUGUI>().text = option.optionText;

            btn.onClick.AddListener(() => option.RunOption());
        }

        private void ClearOptions()
        {
            m_OptionsContainer.gameObject.SetActive(false);

            foreach(Transform child in m_OptionsContainer)
            {
                Destroy(child.gameObject);
            }
        }

        public void EndConversation()
        {
            m_DialogueContainer.gameObject.SetActive(false);
            m_OptionsContainer.gameObject.SetActive(false);
        }
    }
}