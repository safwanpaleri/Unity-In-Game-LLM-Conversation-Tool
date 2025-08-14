using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ClaudeTestManager : MonoBehaviour
{
    #region Claude API Helper Classes

    [System.Serializable]
    public class RequestBody
    {
        public string model;
        public int max_tokens;
        public RequestContent[] messages;
    }

    [System.Serializable]
    public class RequestContent
    {
        public string role;
        public string content;
    }

    [System.Serializable]
    public class ResponseBody
    {
        public ResponseContent[] content;
    }

    [System.Serializable]
    public class ResponseContent
    {
        public string type;
        public string text;
    }

    #endregion

    [HideInInspector] public string apiKey = string.Empty;
    private string apiUrl = "https://api.anthropic.com/v1/messages";

    /// <summary>
    /// A public function to send a request to Claude API.
    /// This function triggers a corotuine function.
    /// </summary>
    public void SendMessageToClaude()
    {
        apiKey = EditorPrefs.GetString("ClaudeApiKey");
        StartCoroutine(SendRequest());
    }

    /// <summary>
    /// The coroutine send a chat message to Claude API and logs the reply.
    /// </summary>
    private IEnumerator SendRequest()
    {
        //Create request body json to send to the api along with prompt.
        //wait until we receive a response.
        RequestBody requestBody = new RequestBody();
        RequestContent requestContent = new RequestContent();
        requestContent.role = "user";
        requestContent.content = "[You are a NPC cahracter in Sekiro], why there is ash in winds";
        requestBody.model = "claude-sonnet-4-20250514";
        requestBody.max_tokens = 1000;
        requestBody.messages = new RequestContent[1];
        requestBody.messages[0] = requestContent;

        string jsonData = JsonUtility.ToJson(requestBody);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("x-api-key", apiKey);
        request.SetRequestHeader("anthropic-version", "2023-06-01");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            ResponseBody response = JsonUtility.FromJson<ResponseBody>(request.downloadHandler.text);
            if (response != null && response.content.Length > 0)
            {
                Debug.Log("Claude says: " + response.content[0].text);
            }
        }
        else
        {
            Debug.LogError("Claude API Error: " + request.error);
        }
    }
    
}
