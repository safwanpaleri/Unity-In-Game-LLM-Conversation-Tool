using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;


public class NPCAgentScript : MonoBehaviour
{
    #region Gemeni Api Helper classes

    [System.Serializable]
    public class RequestBody
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

    [Header("Cache")]
    [HideInInspector] public string textToSpeak = "Say cheerfully: Have a wonderful day!";
    [SerializeField] private string VoiceName = "Kore";

    public AudioSource audioSource;
    [HideInInspector] public bool isCompleted = false;
    [SerializeField] public GameObject capsule;
    [SerializeField] public Animator animator;

    /// <summary>
    /// Function for calling the coroutine which is responsible for converting Text to speech.
    /// Made it public so that other scripts can call this.
    /// </summary>
    public void TextToSpeech()
    {
        StartCoroutine(SpeakWithGemini());
    }

    /// <summary>
    /// Coroutine function which will convert text to audio speech.
    /// </summary>
    IEnumerator SpeakWithGemini()
    {
        isCompleted = false;

        string url = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash-preview-tts:generateContent?key=" + EditorPrefs.GetString("GeminiApiKey");

        //Create request body json to send to the api with the text to convert.
        //wait until we receive a response.
        //if we successfully completed the request, convert the recieved data into a audio file.
        //assign the converted audio file into the audio source, ready to be played.
        RequestBody requestBody = new RequestBody();
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
        Debug.Log(jsonData);
        using (UnityWebRequest www = UnityWebRequest.PostWwwForm(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
            www.uploadHandler = new UploadHandlerRaw(bodyRaw);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError($"Gemini TTS error: {www.error}");
                yield break;
            }

            string jsonResponse = www.downloadHandler.text;
            var response = JsonUtility.FromJson<GeminiTTSResponse>(jsonResponse);

            string base64Audio = response.candidates[0].content.parts[0].inlineData.data;

            byte[] pcmData = Convert.FromBase64String(base64Audio);
            AudioClip clip = CreateAudioClipFromPCM(pcmData, 24000, 1);

            if (clip != null)
            {
                audioSource.clip = clip;
                isCompleted = true;
            }
        }
    }

    /// <summary>
    /// Converts PCM byte data to Unity AudioClip.
    /// </summary>
    AudioClip CreateAudioClipFromPCM(byte[] pcmData, int sampleRate, int channels)
    {
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
}
