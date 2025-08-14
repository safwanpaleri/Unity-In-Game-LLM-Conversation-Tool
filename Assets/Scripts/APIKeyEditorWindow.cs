using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using static APIKeyEditorWindow;

/// <summary>
/// Editor window for storing and managing API keys locally using Unity's EditorPrefs.
/// </summary>
public class APIKeyEditorWindow : EditorWindow
{
    // API Keys (editable in the editor window)
    public string OpenAiApiKey = string.Empty;
    public string DeepSeekApiKey = string.Empty;
    public string GeminiApiKey = string.Empty;
    public string ClaudeApiKey = string.Empty;
    public string MistralApiKey = string.Empty;

    [MenuItem("LLM Testing Tool/API Keys")]
    public static void ShowWindow()
    {
        GetWindow<APIKeyEditorWindow>("Save API Keys");
    }

    // Load stored keys automatically when the window opens
    private void OnEnable()
    {
        LoadAPIKeys();
    }

    private void OnGUI()
    {
        GUILayout.Label("Save API Keys Locally", EditorStyles.boldLabel);

        // Editable text fields for each API key
        OpenAiApiKey = EditorGUILayout.TextField("OpenAi API Key", OpenAiApiKey);
        DeepSeekApiKey = EditorGUILayout.TextField("DeepSeek API Key", DeepSeekApiKey);
        GeminiApiKey = EditorGUILayout.TextField("Gemini API Key", GeminiApiKey);
        ClaudeApiKey = EditorGUILayout.TextField("Claude API Key", ClaudeApiKey);
        MistralApiKey = EditorGUILayout.TextField("Mistral API Key", MistralApiKey);

        // Buttons for saving, loading, and clearing keys
        if (GUILayout.Button("Save Data"))
            SaveAPIKeys();

        if (GUILayout.Button("Load Data"))
            LoadAPIKeys();

        if (GUILayout.Button("Clear Data")) 
            ClearAPIKeys();

        GUILayout.Label("* The API keys are stored locally and will not be pushed to source control", EditorStyles.miniBoldLabel);
    }

    /// <summary>
    /// Saves all API keys to Unity EditorPrefs (local storage).
    /// </summary>
    private void SaveAPIKeys()
    {
        EditorPrefs.SetString("OpenAiApiKey", OpenAiApiKey);
        EditorPrefs.SetString("DeepSeekApiKey", DeepSeekApiKey);
        EditorPrefs.SetString("GeminiApiKey", GeminiApiKey);
        EditorPrefs.SetString("ClaudeApiKey", ClaudeApiKey);
        EditorPrefs.SetString("MistralApiKey", MistralApiKey);

        Debug.Log("The API keys are saved locally");
    }

    /// <summary>
    /// Loads API keys from Unity EditorPrefs (local storage).
    /// </summary>
    public void LoadAPIKeys()
    {
        OpenAiApiKey = EditorPrefs.GetString("OpenAiApiKey");
        DeepSeekApiKey = EditorPrefs.GetString("DeepSeekApiKey");
        GeminiApiKey = EditorPrefs.GetString("GeminiApiKey");
        ClaudeApiKey = EditorPrefs.GetString("ClaudeApiKey");
        MistralApiKey = EditorPrefs.GetString("MistralApiKey");
    }

    /// <summary>
    /// Clears all saved API keys from Unity EditorPrefs.
    /// </summary>
    public void ClearAPIKeys()
    {
        EditorPrefs.DeleteKey("OpenAiApiKey");
        EditorPrefs.DeleteKey("DeepSeekApiKey");
        EditorPrefs.DeleteKey("GeminiApiKey");
        EditorPrefs.DeleteKey("ClaudeApiKey");
        EditorPrefs.DeleteKey("MistralApiKey");

        Debug.Log("The API keys have been cleared");
    }
}
