using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class DeepSeekTestManager : MonoBehaviour
{
    #region DeepSeek API Helper class

    [System.Serializable]
    public class RequestBody
    {
        public string model;
        public MessageContent[] messages;
        public bool stream;
    }

    [System.Serializable]
    public class MessageContent
    {
        public string role;
        public string content;
    }

    [System.Serializable]
    public class ResponseOutput
    {
        public OutputMessages[] choices;
    }

    [System.Serializable]
    public class OutputMessages
    {
        public MessageContent message;
    }

    #endregion

    [HideInInspector] public string apiKey = string.Empty;
    private string apiUrl = "https://api.deepseek.com/v1/chat/completions";

    /// <summary>
    /// A public function to send a request to DeepSeek API.
    /// This function triggers a corotuine function.
    /// </summary>
    public void SendMessageToDeepSeek()
    {
        apiKey = EditorPrefs.GetString("DeepSeekApiKey");
        StartCoroutine(SendRequest());
    }

    /// <summary>
    /// The coroutine send a chat message to DeepSeek API and logs the reply.
    /// </summary>
    private IEnumerator SendRequest()
    {
        //Create request body json to send to the api along with prompt.
        //wait until we receive a response.
        string userPrompt = "Why there is ash in the wind";

        RequestBody requestBody = new RequestBody();
        requestBody.model = "deepseek-chat";
        requestBody.messages = new MessageContent[]
        {
                new MessageContent { role = "system", content = "You are a NPC cahracter in Sekiro" },
                new MessageContent { role = "user", content = userPrompt}
        };
        string json = JsonUtility.ToJson(requestBody);
        byte[] byteRaw = Encoding.UTF8.GetBytes(json);

        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);
        request.uploadHandler = new UploadHandlerRaw(byteRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            ResponseOutput response = JsonUtility.FromJson<ResponseOutput>(request.downloadHandler.text);
            Debug.Log("DeepSeek Reply: " + response.choices[0].message.content);
        }
        else
        {
            Debug.LogError("Error: " + request.error);
        }
    }
}
