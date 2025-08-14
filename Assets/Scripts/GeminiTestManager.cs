using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class GeminiTestManager : MonoBehaviour
{
    #region Gemini API Helper classes

    [System.Serializable]
    public class RequestBody
    {
        public RequestContent[] contents;
    }

    [System.Serializable]
    public class RequestContent
    {
        public Part[] parts;
    }

    [System.Serializable]
    public class Part
    {
        public string text;
    }

    [System.Serializable]
    public class ResponseOutput
    {
        public ResponseContent[] candidates;
    }

    [System.Serializable]
    public class ResponseContent
    {
        public RequestContent content;
    }

    #endregion

    [HideInInspector] public string apiKey = string.Empty;
    private string apiUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash-preview-05-20:generateContent?key=";
    [HideInInspector] public string result;
    [HideInInspector] public bool isCompleted = false;
    [HideInInspector] public string prompt = string.Empty;

    /// <summary>
    /// A public function to send a request to Gemini API.
    /// This function triggers a corotuine function.
    /// </summary>
    public void SendMessageToGemini()
    {
        apiKey = EditorPrefs.GetString("GeminiApiKey");
        apiUrl += apiKey;
        StartCoroutine(SendRequest());
    }

    /// <summary>
    /// The coroutine send a chat message to Gemini API and logs the reply.
    /// </summary>
    private IEnumerator SendRequest()
    {
        //Create request body json to send to the api along with prompt.
        //wait until we receive a response.
        isCompleted = false;
        RequestBody requestBody = new RequestBody();
        var prompt1 = new Part { text = prompt };
        var parts = new Part[1];
        parts[0] = prompt1;
        var requestContent = new RequestContent();
        requestContent.parts = parts;
        requestBody.contents = new RequestContent[1];
        requestBody.contents[0] = requestContent;


        string jsonData = JsonUtility.ToJson(requestBody);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        request.SetRequestHeader("RequestContent-Type", "application/json");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        Debug.Log("request sent");

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            ResponseOutput response = JsonUtility.FromJson<ResponseOutput>(request.downloadHandler.text);
            if (response != null && response.candidates.Length > 0)
            {
                result = response.candidates[0].content.parts[0].text;
                Debug.Log("Gemini Reply: " + result);
                isCompleted = true;
            }
        }
        else
        {
            Debug.LogError("Gemini API Error: " + request.error);
        }
    }

}
