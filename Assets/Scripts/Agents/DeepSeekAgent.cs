using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

public class DeepSeekAgent : MonoBehaviour
{
    #region Api helper classes

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

    [System.Serializable]
    public class RequestBodyTTS
    {
        public RequestContent[] contents;
        public RequestConfig generationConfig;
        public string model;
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
    public class RequestConfig
    {
        public string[] responseModalities;
        public SpeechConfig speechConfig;
    }

    [System.Serializable]
    public class SpeechConfig
    {
        public VoiceConfig voiceConfig;
    }
    [Serializable]
    public class VoiceConfig
    {
        public PrebuiltVoiceConfig prebuiltVoiceConfig;
    }

    [Serializable]
    public class PrebuiltVoiceConfig
    {
        public string voiceName;
    }

    [System.Serializable]
    public class GeminiTTSResponse
    {
        public Candidate[] candidates;

        [System.Serializable]
        public class Candidate
        {
            public Content content;
        }

        [System.Serializable]
        public class Content
        {
            public Part[] parts;
        }

        [System.Serializable]
        public class Part
        {
            public InlineData inlineData;
        }

        [System.Serializable]
        public class InlineData
        {
            public string mimeType;
            public string data;
        }
    }

    #endregion

    [Header("Character Info")]
    public string characterName = "";
    public string characterDescription = "";
    public float characterAngle = 0.0f;
    public GameObject characterPlayerView;

    [Header("parameters")]
    public float characterKnowledge = 1.0f;
    public float lastSpoken = 0.0f;
    public float speakingCapability = 1.0f;
    public float emotionalScore = 0.0f;

    [Header("Prompt")]
    [Tooltip("Basic Prompt is hardcoded in script. Add additional changes to prompts here. \n this will help alter the prompt" +
        "\n utilize this area for making changes in to the prompts.")]
    [SerializeField] private string AdditionalPrompt = "";

    [Header("Cache")]
    [HideInInspector] public string textToSpeak = "Say cheerfully: Have a wonderful day!";
    [SerializeField] private string VoiceName = "Kore";
    [HideInInspector] public string prompt = String.Empty;

    public AudioSource audioSource;
    [HideInInspector] public bool isCompleted = false;
    [SerializeField] public GameObject capsule;
    [SerializeField] public Animator animator;

    [HideInInspector] public string apiKey = string.Empty;
    private string apiUrl = "https://api.deepseek.com/v1/chat/completions";
    [HideInInspector] public List<MessageContent> messages = new List<MessageContent>();
    [HideInInspector] public bool isDone = false;
    [HideInInspector] public string topic = string.Empty;
    [HideInInspector] public string otherCharactersNames = string.Empty;
    [HideInInspector] public string responseMessage = string.Empty;
    string initialContent = string.Empty;
    [HideInInspector] public bool isModerator = false;
    public bool isPlayer = false;

    private bool isTimer = false;
    private float seconds = 0.0f;
    public List<float> responseTimes = new List<float>();

    public bool isAutomatedTesting = false;

    private void Start()
    {
        apiKey = EditorPrefs.GetString("DeepSeekApiKey");
        // Initial instruction for deepseek to behave as a specific character in group conversation
        initialContent = "act as a " + characterDescription + ", named as " + characterName +
            " , and talk about " + topic + ". the dialogues should be like the example given \"Say Confused: the data shows people are choosing veganism, I wonder why\" \"Say happy: I have passed the test!\" \"Say imitating: Let the bygones be bygones\" only give one dailogue. This is group conversation but only give dailogues for" + characterName +
            "the other characters are " + otherCharactersNames + ". and only include emotion and dailogue no action just like the examples, also don't include the speaker name at start. Only give a single dailogue.";

    }

    private void Update()
    {
        //Timer functionality for tracking response times for each api request.
        if (isTimer)
        {
            seconds += Time.deltaTime;
        }
    }

    /// <summary>
    /// Public function to call from any script to generate dailogue using deepseek.
    /// </summary>
    /// <param name="conversationHistory">Give the whole conversation, give a null list to start a new conversation</param>
    /// <param name="additionalContent">optional string for giving additional instructions</param>
    public void GenerateNextDailogue(List<string> conversationHistory, string additionalContent = "")
    {
        /*
         * reset bool, previous conversation part
         * Added initial instruction (the name and character description).
         * then the conversation going down, if null it will start the conversation.
         * then calls the coroutine for sending this request to api.
         */
        isCompleted = false;
        messages.Clear();
        messages.Add(new MessageContent { role = "user", content = initialContent + '\n' + AdditionalPrompt });
        foreach (var conversation in conversationHistory)
        {
            messages.Add(new MessageContent { role = "user", content = conversation });
        }
        if (additionalContent != "")
            messages.Add(new MessageContent { role = "user", content = additionalContent });
        StartCoroutine(SendRequest());
    }

    /// <summary>
    /// This coroutine sends the api request and recieves the response.
    /// and starts another coroutine to convert the dailogue into a speech.
    /// Also keeps track of response times.
    /// </summary>
    /// <returns></returns>
    private IEnumerator SendRequest()
    {
        /*
         * Create a new requestBody using helper class with all the required parameters.
         * the requestBody then convereted to json and further converted into byte to send to api
         * Crates a new API Request with the url, method, apikey and data type.
         * send the request and wait until the request is completed
         * if success then the response text is send to gemini api to convert into speech.
         * else debug the error.
         * the iscomplete sets to true. and response timer is recorded
         */
        RequestBody requestBody = new RequestBody();
        requestBody.model = "deepseek-chat";
        requestBody.messages = messages.ToArray();
        string json = JsonUtility.ToJson(requestBody);
        byte[] byteRaw = Encoding.UTF8.GetBytes(json);

        UnityWebRequest request = new UnityWebRequest(apiUrl, "POST");
        request.SetRequestHeader("Content-Type", "application/json");
        request.SetRequestHeader("Authorization", "Bearer " + apiKey);
        request.uploadHandler = new UploadHandlerRaw(byteRaw);
        request.downloadHandler = new DownloadHandlerBuffer();

        isTimer = true;
        yield return request.SendWebRequest();
        isTimer = false;
        responseTimes.Add(seconds);
        seconds = 0.0f;
        if (request.result == UnityWebRequest.Result.Success)
        {
            ResponseOutput response = JsonUtility.FromJson<ResponseOutput>(request.downloadHandler.text);
            responseMessage = response.choices[0].message.content;
            isDone = true;
            textToSpeak = responseMessage;
            if (!isAutomatedTesting)
                StartCoroutine(SpeakWithGemini());
            else
                isCompleted = true;
        }
        else
        {
            Debug.LogError("Error: " + request.error);
            isCompleted = true;
        }
    }

    /// <summary>
    /// The coroutine which send the text to Gemini API to convert into a speech.
    /// the response then converted into audio and added to audio source to played when required.
    /// </summary>
    /// <returns></returns>
    IEnumerator SpeakWithGemini()
    {
        /*
         * Creates a new requestBody with the help of helper class and set required contents.
         * the requestBody then converted to json and further converted to byte to send along with request.
         * Creates a new api request with the url, method, apikey and converted byte data.
         * wait until the request is completed.
         * if the request is success a helper function is called to convert the response into audio clip.
         * else logs the error along with message.
         * the converted audio clip then assigned to audio source, ready to played whenever required.
         */
        string url = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash-preview-tts:generateContent?key=" + EditorPrefs.GetString("GeminiApiKey");

        RequestBodyTTS requestBody = new RequestBodyTTS();
        RequestContent[] requestContents = new RequestContent[1];
        requestContents[0] = new RequestContent();
        requestContents[0].parts = new Part[1];
        requestContents[0].parts[0] = new Part { text = textToSpeak };
        requestBody.contents = requestContents;
        requestBody.generationConfig = new RequestConfig
        {
            responseModalities = new[] { "AUDIO" },
            speechConfig = new SpeechConfig
            {
                voiceConfig = new VoiceConfig
                {
                    prebuiltVoiceConfig = new PrebuiltVoiceConfig
                    {
                        voiceName = VoiceName
                    }
                }
            }
        };

        requestBody.model = "gemini-2.5-flash-preview-tts";

        string jsonData = JsonUtility.ToJson(requestBody);
        byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);

        UnityWebRequest request = new UnityWebRequest(url, "POST");
        request.uploadHandler = new UploadHandlerRaw(bodyRaw);
        request.downloadHandler = new DownloadHandlerBuffer();
        request.SetRequestHeader("Content-Type", "application/json");

        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogError($"Gemini TTS error: {request.error}");
            yield break;
        }

        string jsonResponse = request.downloadHandler.text;
        var response = JsonUtility.FromJson<GeminiTTSResponse>(jsonResponse);

        string base64Audio = response.candidates[0].content.parts[0].inlineData.data;
        AudioClip clip = CreateAudioClipFromPCM(base64Audio);

        if (clip != null)
        {
            audioSource.clip = clip;
            isCompleted = true;
        }
    }

    /// <summary>
    /// A helper function to convert the byte to a unity audio clip
    /// </summary>
    /// <param name="response"></param>
    /// <returns>a unity audio clip to attached to audio source.</returns>
    private AudioClip CreateAudioClipFromPCM(string response)
    {
        /*
         * The string is converted to byte,
         * the byte then byte converted to audioData.
         * a new audioclip is created using audio data.
         * and the audio clip is returned.
         */
        byte[] pcmData = Convert.FromBase64String(response);
        int sampleRate = 24000;
        int channels = 1;
        int sampleCount = pcmData.Length / 2;
        float[] audioData = new float[sampleCount];

        for (int i = 0; i < sampleCount; i++)
        {
            short sample = BitConverter.ToInt16(pcmData, i * 2);
            audioData[i] = sample / 32768.0f;
        }

        AudioClip audioClip = AudioClip.Create("GeminiTTS", sampleCount, channels, sampleRate, false);
        audioClip.SetData(audioData, 0);
        return audioClip;
    }

    /// <summary>
    /// A public function to specifically to generate moderator dailogues.
    /// </summary>
    /// <param name="content"></param>
    public void GenerateModeratorContent(string content)
    {
        /*
         * Reset bool and previous contents
         * create a new content to send to api
         * calls the coroutine to send the request to api
         */
        isCompleted = false;
        messages.Clear();
        messages.Add(new MessageContent { role = "user", content = content });
        StartCoroutine(SendRequest());
    }

    /// <summary>
    /// A public function to score the generated conversation with the chatgpt llm.
    /// </summary>
    /// <param name="content"> The entire conversation along with the instructions to score them is passed down into this parameter</param>
    public void ScoreDailogues(string content)
    {
        /*
         * reset bool, and previous contents
         * create a new content,
         * calls the coroutine to send request to api
         */
        isCompleted = false;
        messages.Clear();
        messages.Add(new MessageContent { role = "user", content = content });
        StartCoroutine(ScoreDailogues_Coroutine());
    }

    /// <summary>
    /// Sends the request to Chatgpt with the conversation and instructions
    /// the response then saved to a public variable responseMessage to fetch whenever needed.
    /// </summary>
    /// <returns></returns>
    private IEnumerator ScoreDailogues_Coroutine()
    {
        /*
         * a requestBody is created with the help of helper class.
         * added required parameters, converted them to json, further converted to byte.
         * created a request with url, api key, converted byte data and send to api
         * wait until the request is complete.
         * if request is success the response assigned to public variable 'responseMessage', 'isComplete' sets to true
         * else logs the error along with error message.
         */
        RequestBody requestBody = new RequestBody();
        requestBody.model = "deepseek-chat";
        requestBody.messages = messages.ToArray();
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
            responseMessage = response.choices[0].message.content;
            isDone = true;
            isCompleted = true;
        }
        else
        {
            Debug.LogError("Error: " + request.error);
            isCompleted = true;
        }
    }
}
