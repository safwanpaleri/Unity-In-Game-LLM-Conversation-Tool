using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class ChatGptTestManager : MonoBehaviour
{
    #region Chatgpt API Helper classes

    [System.Serializable]
    public class RequestBody
    {
        public string model;
        public RequestContent[] messages;
    }

    [System.Serializable]
    public class RequestContent
    {
        public string role;
        public string content;
    }

    [System.Serializable]
    public class ResponeOutput
    {
        public ResponeContent[] choices;
    }

    [System.Serializable]
    public class ResponeContent
    {
        public RequestContent message;
    }

    #endregion

    [HideInInspector] public string apiKey = string.Empty;
    private string apiUrl = "https://api.openai.com/v1/chat/completions";

    /// <summary>
    /// A public function to send a request to Chatgpt API.
    /// This function triggers a corotuine function.
    /// </summary>
    public void SendMessageToChatGPT()
    {
        apiKey = EditorPrefs.GetString("OpenAiApiKey");
        StartCoroutine(SendRequest());
    }

    /// <summary>
    /// The coroutine send a chat message to Claude API and logs the reply.
    /// </summary>
    private IEnumerator SendRequest()
    {
        //Create request body json to send to the api along with prompt.
        //wait until we receive a response.
        string userInput = "[You are a NPC cahracter in Sekiro], why there is ash in winds"; 
        RequestBody requestBody = new RequestBody();
        requestBody.model = "gpt-4.1";
        requestBody.messages = new RequestContent[]
        {
            new RequestContent { role = "user", content = userInput }
        };
        string json = JsonUtility.ToJson(requestBody);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            //Debug.Log("Response: " + request.downloadHandler.text);
            var response = JsonUtility.FromJson<ResponeOutput>(request.downloadHandler.text);
            Debug.Log("ChatGPT: " + response.choices[0].message.content);
        }
        else
        {
            Debug.LogError("Error: " + request.error);
        }
    }
}
