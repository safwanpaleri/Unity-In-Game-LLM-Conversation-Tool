using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class LLMTestManager : MonoBehaviour
{
    [SerializeField] private ChatGptTestManager chatGptTestManager;
    [SerializeField] private DeepSeekTestManager deepSeekTestManager;
    [SerializeField] private GeminiTestManager geminiTestManager;
    [SerializeField] private ClaudeTestManager claudeTestManager;
    [SerializeField] private MistralTestManager mistralTestManager;

    void Start()
    {
        LoadAPIKeys();
    }

    /// <summary>
    /// Loads API keys for all LLM test managers from Unity Editor preferences.
    /// </summary>
    public void LoadAPIKeys()
    {
        chatGptTestManager.apiKey = EditorPrefs.GetString("OpenAiApiKey");
        deepSeekTestManager.apiKey = EditorPrefs.GetString("DeepSeekApiKey");
        geminiTestManager.apiKey = EditorPrefs.GetString("GeminiApiKey");
        claudeTestManager.apiKey = EditorPrefs.GetString("ClaudeApiKey");
        mistralTestManager.apiKey = EditorPrefs.GetString("MistralApiKey");
    }

    /// <summary>
    /// Starts the LLM tests by calling their respective send-message methods.
    /// </summary>
    public void onStartTestButtonPressed()
    {
        Debug.Log("-----Started Test-----");

        if(chatGptTestManager != null && chatGptTestManager.apiKey != string.Empty)
            chatGptTestManager.SendMessageToChatGPT();
        
        if(deepSeekTestManager != null && deepSeekTestManager.apiKey != string.Empty)
            deepSeekTestManager.SendMessageToDeepSeek();

         if(geminiTestManager != null && geminiTestManager.apiKey != string.Empty)
            geminiTestManager.SendMessageToGemini();

         if(claudeTestManager != null && claudeTestManager.apiKey != string.Empty)
            claudeTestManager.SendMessageToClaude();

         if(mistralTestManager != null && mistralTestManager.apiKey != string.Empty)
            mistralTestManager.SendMessageToMistral();

    }
}
