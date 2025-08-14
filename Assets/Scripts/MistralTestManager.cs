using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class MistralTestManager : MonoBehaviour
{
    #region Mistral API Helper Classes

    [System.Serializable]
    public class RequestBody
    {
        public string model;
        public RequestContent[] messages;
        public float temperature;
        public int max_tokens;
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
        public ResponseContent[] choices;
    }

    [System.Serializable]
    public class ResponseContent
    {
        public RequestContent message;
    }

    #endregion

    [HideInInspector] public string apiKey = string.Empty;
    private string apiUrl = "https://api.mistral.ai/v1/chat/completions";

    /// <summary>
    /// A public function to send a request to Mistral API.
    /// This function triggers a corotuine function.
    /// </summary>
    public void SendMessageToMistral()
    {
        apiKey = EditorPrefs.GetString("MistralApiKey");
        StartCoroutine(SendRequest());
    }

    /// <summary>
    /// The coroutine send a chat message to Mistral API and logs the reply.
    /// </summary>
    private IEnumerator SendRequest()
    {
        //Create request body json to send to the api along with prompt.
        //wait until we receive a response.
        RequestBody requestBody = new RequestBody();
        requestBody.model = "mistral-medium";
        requestBody.messages = new RequestContent[1];
        requestBody.messages[0] = new RequestContent { role = "user", content = "[You are a NPC cahracter in Sekiro], why there is ash in winds" };
        requestBody.temperature = 0.7f;
        requestBody.max_tokens = 500;
        
        string jsonData = JsonUtility.ToJson(requestBody);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        yield return request.SendWebRequest();

        if (request.result == UnityWebRequest.Result.Success)
        {
            ResponseBody response = JsonUtility.FromJson<ResponseBody>(request.downloadHandler.text);
            if (response != null && response.choices.Length > 0)
            {
                string reply = response.choices[0].message.content;
                Debug.Log("Mistral Reply: " + reply);
            }
        }
        else
        {
            Debug.LogError("Mistral API Error: " + request.error);
        }
    }
}
