using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NPCLLMTester : MonoBehaviour
{
    [System.Serializable]
    public class EvaluationData
    {
        public string Date;
        public string Time;

        public string CoherenceScore;
        public string RelevanceScore;
        public string NaturalnessScore;
        public string EngagementScore;
        public string ContextualAccuracyScore;

        public string ChatgptAvgTime;
        public string GeminiAvgTime;
        public string ClaudeAvgTime;
        public string DeepseekAvgTime;
        public string MistralAvgTime;
    }

    [System.Serializable]
    class EvaluationDataList
    {
        public List<EvaluationData> dataList = new List<EvaluationData>();
    }

    [Header("Lists")]
    public List<string> chatgptDailogues = new List<string>();
    public List<string> geminitDailogues = new List<string>();
    public List<string> deepseekDailogues = new List<string>();
    public List<string> mistralDailogues = new List<string>();
    public List<string> claudeDailogues = new List<string>();

    [Header("Response Times")]
    public List<float> chatgptResponceTimes = new List<float>();
    public List<float> geminiResponceTimes = new List<float>();
    public List<float> deepseekResponceTimes = new List<float>();
    public List<float> mistraltResponceTimes = new List<float>();
    public List<float> claudeResponceTimes = new List<float>();

    [Header("LLLm Agents")]
    [SerializeField] private ChatGPTAgent chatGPTAgent;
    [SerializeField] private GeminiAgent geminiAgent;
    [SerializeField] private DeepSeekAgent deepSeekAgent;
    [SerializeField] private MistralAgent mistralAgent;
    [SerializeField] private ClaudeAgent claudeAgent;

    [Header("UI Cache")]
    [SerializeField] private GameObject FeedbackCanvas;
    [SerializeField] private GameObject FeedbackLoadingPanel;
    [SerializeField] private Slider coherenceSlider;
    [SerializeField] private Slider naturalnessSlider;
    [SerializeField] private Slider engagementSlider;
    [SerializeField] private Slider relevanceSlider;
    [SerializeField] private Slider contextualAccuracySlider;
    [SerializeField] private TMP_Text chatgptAvgResponseTimeText;
    [SerializeField] private TMP_Text geminiAvgResponseTimeText;
    [SerializeField] private TMP_Text claudeAvgResponseTimeText;
    [SerializeField] private TMP_Text deepseekAvgResponseTimeText;
    [SerializeField] private TMP_Text mistralAvgResponseTimeText;
    [SerializeField] private GameObject scrollContents;

    [Header("Prefab")]
    [SerializeField] private GameObject contentItem;

    private string checkPrompt = string.Empty;

    private float chatgptCoherence = 0.0f;
    private float chatgptRelevance = 0.0f;
    private float chatgptNaturalness = 0.0f;
    private float chatgptEngagement = 0.0f;
    private float chatgptCA = 0.0f;
    private float chatgptResponseTimeAvg = 0.0f;

    private float geminiCoherence = 0.0f;
    private float geminiRelevance = 0.0f;
    private float geminiNaturalness = 0.0f;
    private float geminiEngagement = 0.0f;
    private float geminiCA = 0.0f;
    private float geminiResponseTimeAvg = 0.0f;

    private float deepseekCoherence = 0.0f;
    private float deepseekRelevance = 0.0f;
    private float deepseekNaturalness = 0.0f;
    private float deepseekEngagement = 0.0f;
    private float deepseekCA = 0.0f;
    private float deepseekResponseTimeAvg = 0.0f;

    private float mistralCoherence = 0.0f;
    private float mistralRelevance = 0.0f;
    private float mistralNaturalness = 0.0f;
    private float mistralEngagement = 0.0f;
    private float mistralCA = 0.0f;
    private float mistralResponseTimeAvg = 0.0f;

    private float claudeCoherence = 0.0f;
    private float claudeRelevance = 0.0f;
    private float claudeNaturalness = 0.0f;
    private float claudeEngagement = 0.0f;
    private float claudeCA = 0.0f;
    private float claudeResponseTimeAvg = 0.0f;

    public string filename;
    public bool isCompleted = false;
    private void Start()
    {
        filename = Path.Combine(Application.dataPath, "Testcases/evaluationData.json");
    }

    /// <summary>
    /// Evaluates dialogue conversations by sending them to LLM agents for scoring.
    /// It also calculates average response times for each LLM.
    /// </summary>
    public void EvaluateDailogues(List<string> wholeConversation)
    {
        isCompleted = false;
        if(FeedbackCanvas != null)
        {
            ResetUI();
            FeedbackCanvas.SetActive(true);
        }
        
        // Build prompt for LLM scoring
        checkPrompt = "Analyze the provided group conversation transcript based on two key metrics: Naturalness and Relevance. You must use the detailed scoring rubric below to assign a score from 1.0 to 5.0 for each metric";
        checkPrompt += "1. Naturalness (Score out of 5.0)\r\nHow authentic and human-like does the dialogue sound?\r\n\r\n5.0 (Very High): Indistinguishable from a real, spontaneous human conversation. Features natural pacing, filler words (um, like), interruptions, and self-corrections.\r\n\r\n4.0 (High): Largely authentic and flows well, with only minor stilted or overly-perfect phrases.\r\n\r\n3.0 (Moderate): A noticeable mix of natural and unnatural elements. Some parts feel robotic or scripted.\r\n\r\n2.0 (Low): Predominantly unnatural and stilted. Lacks the nuances and messiness of real speech.\r\n\r\n1.0 (Very Low): Completely robotic. Reads like a list of perfectly-formed sentences.\r\n\r\n" +
                       "2. Relevance (Score out of 5.0)\r\nHow well do contributions relate to the current topic of conversation?\r\n\r\n5.0 (Very High): Highly focused. Every contribution directly builds upon or logically responds to a previous point.\r\n\r\n4.0 (High): Mostly on-topic and easy to follow, with only minor, understandable deviations.\r\n\r\n3.0 (Moderate): A mix of relevant and irrelevant contributions. The main topic is frequently interrupted by tangents.\r\n\r\n2.0 (Low): Frequently disjointed. Participants often talk at each other, and logical connections are missing.\r\n\r\n1.0 (Very Low): Completely disjointed. No discernible topic or conversational thread.\r\n\r\n" +
                       "3. Coherence (Score out of 5.0)\r\nHow logically structured is the conversation as a whole? Does it make sense from beginning to end?\r\n\r\n5.0 (Very High): The conversation has a clear, logical structure. Arguments and ideas build on each other progressively and without contradiction.\r\n\r\n4.0 (High): Mostly logical and well-structured. The overall thread is maintained despite minor confusing transitions.\r\n\r\n3.0 (Moderate): Some parts are logical, while others are confusing or jumbled. The main thread of the argument is sometimes lost.\r\n\r\n2.0 (Low): Mostly incoherent. The conversation lacks a clear direction, and arguments are poorly structured and hard to follow.\r\n\r\n1.0 (Very Low): Completely incoherent. A random stream of consciousness where ideas and arguments do not connect in any meaningful way.\r\n\r\n" +
                       "4. Engagement (Score out of 5.0)\r\nHow actively involved are the participants with each other?\r\n\r\n5.0 (Very High): All participants are actively listening and contributing. They ask follow-up questions, provide affirmations (\"right,\" \"I see\"), and build on each other's ideas.\r\n\r\n4.0 (High): Most participants are engaged. The interaction is dynamic, though there may be a quieter person or a brief lull.\r\n\r\n3.0 (Moderate): Uneven engagement. Some participants are very involved while others are passive or seem distracted. One person might dominate without response.\r\n\r\n2.0 (Low): Minimal engagement. Responses are short and perfunctory. Feels like a series of monologues.\r\n\r\n1.0 (Very Low): Complete disengagement. Participants ignore each other's points. No sense of a shared conversational space.\r\n\r\n" +
                       "5. Contextual-Accuracy (Score out of 5.0)\r\nHow factually correct are the verifiable claims made within the conversation?\r\n\r\n5.0 (Very High): All factual statements, references to events, or data are accurate.\r\n\r\n4.0 (High): The vast majority of facts are correct, with only minor, insignificant errors.\r\n\r\n3.0 (Moderate): A mix of accurate and inaccurate information, where some errors may affect conclusions.\r\n\r\n2.0 (Low): Contains significant factual errors that undermine the logic or credibility of the conversation.\r\n\r\n1.0 (Very Low): The conversation is based almost entirely on false or misleading information.";
        checkPrompt += "for example: \"Coherence:4.0, Relevance:5.0, Naturalness:3.0, Engagement: 4.5, Contextual-Accuracy: 3.5\" just give like this in a single line\r\n\r\nCONVERSATION FOR ANALYSIS:";
        foreach (var str in wholeConversation)
            checkPrompt += str + "\n";

        // Start scoring coroutine (example: Gemini)
        StartCoroutine(GeminiScoring());

        // Calculate average response times for all agents
        foreach (float rt in chatGPTAgent.responseTimes)
            chatgptResponseTimeAvg += rt;
        chatgptResponseTimeAvg = chatgptResponseTimeAvg / chatGPTAgent.responseTimes.Count;

        foreach (float rt in geminiAgent.responseTimes)
            geminiResponseTimeAvg += rt;
        geminiResponseTimeAvg = geminiResponseTimeAvg / geminiAgent.responseTimes.Count;

        foreach (float rt in deepSeekAgent.responseTimes)
            deepseekResponseTimeAvg += rt;
        deepseekResponseTimeAvg = deepseekResponseTimeAvg / deepSeekAgent.responseTimes.Count;

        foreach (float rt in claudeAgent.responseTimes)
            claudeResponseTimeAvg += rt;
        claudeResponseTimeAvg = claudeResponseTimeAvg / claudeAgent.responseTimes.Count;

        foreach (float rt in mistralAgent.responseTimes)
            mistralResponseTimeAvg += rt;
        mistralResponseTimeAvg = mistralResponseTimeAvg / mistralAgent.responseTimes.Count;

        if(chatgptAvgResponseTimeText != null)
            chatgptAvgResponseTimeText.text = "ChatGPT Average Response Time: " + chatgptResponseTimeAvg;
        if(geminiAvgResponseTimeText != null)
            geminiAvgResponseTimeText.text = "Gemini Average Response Time: " + geminiResponseTimeAvg;
        if(claudeAvgResponseTimeText != null)
            claudeAvgResponseTimeText.text = "Claude Average Response Time: " + claudeResponseTimeAvg;
        if(deepseekAvgResponseTimeText != null)
            deepseekAvgResponseTimeText.text = "DeepSeek Average Response Time: " + deepseekResponseTimeAvg;
        if(mistralAvgResponseTimeText != null)
            mistralAvgResponseTimeText.text = "Mistral Average Response Time: " + mistralResponseTimeAvg;
    }

    /// <summary>
    /// ChatGPT scoring coroutine.
    /// </summary>
    private IEnumerator ChatGptScoring()
    {
        chatGPTAgent.ScoreDailogues(checkPrompt);
        yield return new WaitUntil(() => chatGPTAgent.isCompleted);

        var str1 = chatGPTAgent.responseMessage;
        Debug.LogWarning(str1);

        var strs = str1.Split(',');
        foreach (var st in strs)
        {
            var str2 = st.Split(':');
            if (str2.Length < 2)
            {
                Debug.LogWarning("Invalid format (missing value): " + st);
                continue;
            }

            string key = str2[0].Trim();
            string valueStr = str2[1].Trim();

            if (!float.TryParse(valueStr, out float value))
            {
                Debug.LogWarning("Invalid float value: " + valueStr + " in line: " + st);
                value = 0.0f;
                continue;
            }

            if (key.Contains("Coherence"))
                chatgptCoherence = value;
            else if (key.Contains("Relevance"))
                chatgptRelevance = value;
            else if (key.Contains("Naturalness"))
                chatgptNaturalness = value;
            else if (key.Contains("Engagement"))
                chatgptEngagement = value;
            else if (key.Contains("Contextual-Accuracy"))
                chatgptCA = value;
        }

        if(coherenceSlider != null)
            coherenceSlider.value = chatgptCoherence * 2.0f;
        if(relevanceSlider != null)
            relevanceSlider.value = chatgptRelevance * 2.0f;
        if(naturalnessSlider != null)
            naturalnessSlider.value = chatgptNaturalness * 2.0f;
        if(engagementSlider != null)
            engagementSlider.value = chatgptEngagement * 2.0f;
        if(contextualAccuracySlider != null)
            contextualAccuracySlider.value = chatgptCA * 2.0f;

        if(FeedbackLoadingPanel != null)
            FeedbackLoadingPanel.SetActive(false);

        SaveScore(chatgptCoherence, chatgptRelevance,chatgptEngagement, chatgptNaturalness, chatgptCA, chatgptResponseTimeAvg, geminiResponseTimeAvg, claudeResponseTimeAvg, deepseekResponseTimeAvg, mistralResponseTimeAvg);
    }

    /// <summary>
    /// Gemini scoring coroutine.
    /// </summary>
    private IEnumerator GeminiScoring()
    {
        //gemini scoring
        geminiAgent.ScoreDailogues(checkPrompt);
        yield return new WaitUntil(() => geminiAgent.isCompleted);

        var str1 = geminiAgent.responseMessage;
        Debug.LogWarning(str1);
        var strs = str1.Split(',');
        foreach (var st in strs)
        {
            var str2 = st.Split(':');
            if (str2.Length < 2)
            {
                Debug.LogWarning("Invalid format (missing value): " + st);
                continue;
            }

            string key = str2[0].Trim();
            string valueStr = str2[1].Trim();

            if (!float.TryParse(valueStr, out float value))
            {
                Debug.LogWarning("Invalid float value: " + valueStr + " in line: " + st);
                value = 0.0f;
                continue;
            }

            if (key.Contains("Coherence"))
                geminiCoherence = value;
            else if (key.Contains("Relevance"))
                geminiRelevance = value;
            else if (key.Contains("Naturalness"))
                geminiNaturalness = value;
            else if (key.Contains("Engagement"))
                geminiEngagement = value;
            else if (key.Contains("Contextual-Accuracy"))
                geminiCA = value;
        }

        //Debug.Log("gemini Score Coherence: " + geminiCoherence + " Relevance: " + geminiRelevance + " Naturalness: " + geminiNaturalness + " Engagement: " + geminiEngagement + " CA: " + geminiCA);

        if (coherenceSlider != null)
            coherenceSlider.value = geminiCoherence * 2.0f;
        if (relevanceSlider != null)
            relevanceSlider.value = geminiRelevance * 2.0f;
        if (naturalnessSlider != null)
            naturalnessSlider.value = geminiNaturalness * 2.0f;
        if (engagementSlider != null)
            engagementSlider.value = geminiEngagement * 2.0f;
        if (contextualAccuracySlider != null)
            contextualAccuracySlider.value = geminiCA * 2.0f;

        if (FeedbackLoadingPanel != null)
            FeedbackLoadingPanel.SetActive(false);

        SaveScore(geminiCoherence, geminiRelevance, geminiEngagement, geminiNaturalness, geminiCA, chatgptResponseTimeAvg, geminiResponseTimeAvg, claudeResponseTimeAvg, deepseekResponseTimeAvg, mistralResponseTimeAvg);
    }

    /// <summary>
    /// Deepseek scoring coroutine.
    /// </summary>
    private IEnumerator DeepSeekScoring()
    {
        //check deepseek
        deepSeekAgent.ScoreDailogues(checkPrompt);
        yield return new WaitUntil(() => deepSeekAgent.isCompleted);

        var str1 = deepSeekAgent.responseMessage;
        var strs = str1.Split(',');
        foreach (var st in strs)
        {
            var str2 = st.Split(':');
            if (str2.Length < 2)
            {
                Debug.LogWarning("Invalid format (missing value): " + st);
                continue;
            }

            string key = str2[0].Trim();
            string valueStr = str2[1].Trim();

            if (!float.TryParse(valueStr, out float value))
            {
                Debug.LogWarning("Invalid float value: " + valueStr + " in line: " + st);
                value = 0.0f;
                continue;
            }

            if (key.Contains("Coherence"))
                deepseekCoherence = value;
            else if (key.Contains("Relevance"))
                deepseekRelevance = value;
            else if (key.Contains("Naturalness"))
                deepseekNaturalness = value;
            else if (key.Contains("Engagement"))
                deepseekEngagement = value;
            else if (key.Contains("Contextual-Accuracy"))
                deepseekCA = value;
        }

        Debug.Log("deepseek Score Coherence: " + deepseekCoherence + " Relevance: " + deepseekRelevance + " Naturalness: " + deepseekNaturalness + " Engagement: " + deepseekEngagement + " CA: " + deepseekCA);
        if (coherenceSlider != null)
            coherenceSlider.value = deepseekCoherence * 2.0f;
        if (relevanceSlider != null)
            relevanceSlider.value = deepseekRelevance * 2.0f;
        if (naturalnessSlider != null)
            naturalnessSlider.value = deepseekNaturalness * 2.0f;
        if (engagementSlider != null)
            engagementSlider.value = deepseekEngagement * 2.0f;
        if (contextualAccuracySlider != null)
            contextualAccuracySlider.value = deepseekCA * 2.0f;

        if (FeedbackLoadingPanel != null)
            FeedbackLoadingPanel.SetActive(false);

        SaveScore(deepseekCoherence, deepseekRelevance, deepseekEngagement, deepseekNaturalness, deepseekCA, chatgptResponseTimeAvg, geminiResponseTimeAvg, claudeResponseTimeAvg, deepseekResponseTimeAvg, mistralResponseTimeAvg);
    }

    /// <summary>
    /// Mistral scoring coroutine.
    /// </summary>
    private IEnumerator MistralScoring()
    {
        //check mistral
        mistralAgent.ScoreDailogues(checkPrompt);
        yield return new WaitUntil(() => mistralAgent.isCompleted);

        var str1 = mistralAgent.responseMessage;
        var strs = str1.Split(',');
        foreach (var st in strs)
        {
            var str2 = st.Split(':');
            if (str2.Length < 2)
            {
                Debug.LogWarning("Invalid format (missing value): " + st);
                continue;
            }

            string key = str2[0].Trim();
            string valueStr = str2[1].Trim();

            if (!float.TryParse(valueStr, out float value))
            {
                Debug.LogWarning("Invalid float value: " + valueStr + " in line: " + st);
                value = 0.0f;
                continue;
            }

            if (key.Contains("Coherence"))
                mistralCoherence = value;
            else if (key.Contains("Relevance"))
                mistralRelevance = value;
            else if (key.Contains("Naturalness"))
                mistralNaturalness = value;
            else if (key.Contains("Engagement"))
                mistralEngagement = value;
            else if (key.Contains("Contextual-Accuracy"))
                mistralCA = value;
        }

        Debug.Log("mistral Score Coherence: " + mistralCoherence + " Relevance: " + mistralRelevance + " Naturalness: " + mistralNaturalness + " Engagement: " + mistralEngagement + " CA: " + mistralCA);
        if (coherenceSlider != null)
            coherenceSlider.value = mistralCoherence * 2.0f;
        if (relevanceSlider != null)
            relevanceSlider.value = mistralRelevance * 2.0f;
        if (naturalnessSlider != null)
            naturalnessSlider.value = mistralNaturalness * 2.0f;
        if (engagementSlider != null)
            engagementSlider.value = mistralEngagement * 2.0f;
        if (contextualAccuracySlider != null)
            contextualAccuracySlider.value = mistralCA * 2.0f;

        if (FeedbackLoadingPanel != null)
            FeedbackLoadingPanel.SetActive(false);

        SaveScore(mistralCoherence, mistralRelevance, mistralEngagement, mistralNaturalness, mistralCA, chatgptResponseTimeAvg, geminiResponseTimeAvg, claudeResponseTimeAvg, deepseekResponseTimeAvg, mistralResponseTimeAvg);
    }

    /// <summary>
    /// Claude scoring coroutine.
    /// </summary>
    private IEnumerator ClaudeScoring()
    {
        //check claude
        claudeAgent.ScoreDailogues(checkPrompt);
        yield return new WaitUntil(() => claudeAgent.isCompleted);

        var str1 = claudeAgent.responseMessage;
        var strs = str1.Split(',');
        foreach (var st in strs)
        {
            var str2 = st.Split(':');
            if (str2.Length < 2)
            {
                Debug.LogWarning("Invalid format (missing value): " + st);
                continue;
            }

            string key = str2[0].Trim();
            string valueStr = str2[1].Trim();

            if (!float.TryParse(valueStr, out float value))
            {
                Debug.LogWarning("Invalid float value: " + valueStr + " in line: " + st);
                value = 0.0f;
                continue;
            }

            if (key.Contains("Coherence"))
                claudeCoherence = value;
            else if (key.Contains("Relevance"))
                claudeRelevance = value;
            else if (key.Contains("Naturalness"))
                claudeNaturalness = value;
            else if (key.Contains("Engagement"))
                claudeEngagement = value;
            else if (key.Contains("Contextual-Accuracy"))
                claudeCA = value;
        }

        Debug.Log("claude Score Coherence: " + claudeCoherence + " Relevance: " + claudeRelevance + " Naturalness: " + claudeNaturalness + " Engagement: " + claudeEngagement + " CA: " + claudeCA);
        if (coherenceSlider != null)
            coherenceSlider.value = claudeCoherence * 2.0f;
        if (relevanceSlider != null)
            relevanceSlider.value = claudeRelevance * 2.0f;
        if (naturalnessSlider != null)
            naturalnessSlider.value = claudeNaturalness * 2.0f;
        if (engagementSlider != null)
            engagementSlider.value = claudeEngagement * 2.0f;
        if (contextualAccuracySlider != null)
            contextualAccuracySlider.value = claudeCA * 2.0f;

        if (FeedbackLoadingPanel != null)
            FeedbackLoadingPanel.SetActive(false);

        SaveScore(claudeCoherence, claudeRelevance, claudeEngagement, claudeNaturalness, claudeCA, chatgptResponseTimeAvg, geminiResponseTimeAvg, claudeResponseTimeAvg, deepseekResponseTimeAvg, mistralResponseTimeAvg);
    }

    /// <summary>
    /// Resets UI
    /// </summary>
    public void ResetUI()
    {
        if(FeedbackCanvas !=null)
            FeedbackCanvas.SetActive(false);
        if (FeedbackLoadingPanel != null)
            FeedbackLoadingPanel.SetActive(true);

        if(coherenceSlider != null)
            coherenceSlider.value = 0.0f;
        if(contextualAccuracySlider != null)
            contextualAccuracySlider.value = 0.0f;
        if(engagementSlider != null)
            engagementSlider.value = 0.0f;
        if(naturalnessSlider  != null)
            naturalnessSlider.value = 0.0f;
        if(relevanceSlider != null)
            relevanceSlider.value = 0.0f;

        if (chatgptAvgResponseTimeText != null)
            chatgptAvgResponseTimeText.text = "" ;
        if (geminiAvgResponseTimeText != null)
            geminiAvgResponseTimeText.text = "";
        if (claudeAvgResponseTimeText != null)
            claudeAvgResponseTimeText.text = "";
        if (deepseekAvgResponseTimeText != null)
            deepseekAvgResponseTimeText.text = "";
        if (mistralAvgResponseTimeText != null)
            mistralAvgResponseTimeText.text = "";
    }

    /// <summary>
    /// Saves the evaluated and calculated score into the system, which can be retrieved and viewed later.
    /// </summary>
    private void SaveScore(float coherence, float relevance, float engagement, float naturalness, float ca, float chatgpt, float gemini, float claude, float deepseek, float mistarl)
    {
        //Creates a new class data for converting to json
        EvaluationData data = new EvaluationData();
        data.Date = DateTime.Now.ToString("dd-MM-yyyy");
        data.Time = DateTime.Now.ToString("hh:mm tt");

        data.CoherenceScore = coherence.ToString();
        data.RelevanceScore = relevance.ToString();
        data.EngagementScore = engagement.ToString();
        data.NaturalnessScore = naturalness.ToString();
        data.ContextualAccuracyScore = ca.ToString();

        data.ChatgptAvgTime = chatgpt.ToString();
        data.GeminiAvgTime = gemini.ToString();
        data.ClaudeAvgTime = claude.ToString();
        data.DeepseekAvgTime = deepseek.ToString();
        data.MistralAvgTime = mistarl.ToString();

        //if the file already exist and there is some data, fetch those data and append the new data into it.
        EvaluationDataList dataList = new EvaluationDataList();
        if (File.Exists(filename))
        {
            string result = File.ReadAllText(filename);
            dataList = JsonUtility.FromJson<EvaluationDataList>(result);
            if(dataList == null || dataList.dataList == null)
                dataList = new EvaluationDataList();
        }

        dataList.dataList.Add(data);

        //then again convert the data to json and re write to the same file.
        string json = JsonUtility.ToJson(dataList);
        File.WriteAllText(filename,json);
        Debug.Log("Score Saved");
        isCompleted = true;
    }

    /// <summary>
    /// Retrieve data from filename and view in the table, also print into console.
    /// </summary>
    public void FetchData()
    {
        //if the file doesn't exist return else fetch the value into a list
        EvaluationDataList dataList = new EvaluationDataList();
        if (File.Exists(filename))
        {
            string result = File.ReadAllText(filename);
            dataList = JsonUtility.FromJson<EvaluationDataList>(result);
            if (dataList == null || dataList.dataList == null)
            {
                Debug.LogError("Data doesn't exists");
                return;
            }
            
            //then reverse the list so that latest data will be first in order
            dataList.dataList.Reverse();
            
            //then go through every data and print them to console,
            //if the table is not null, show the data into the table.
            foreach (var data in dataList.dataList)
            {
                Debug.Log("CH: " + data.CoherenceScore + ", RL: " + data.CoherenceScore + ", NL: " + data.NaturalnessScore + ", EN: " + data.EngagementScore + ", CA: " + data.ContextualAccuracyScore
                    + ", chatgpt: " + data.ChatgptAvgTime + ", gemini: " + data.GeminiAvgTime + ", claude: " + data.ClaudeAvgTime + ", deepseek: " + data.DeepseekAvgTime + ", mistral: " + data.MistralAvgTime);

                if (scrollContents != null)
                {
                    var newData = Instantiate(contentItem, scrollContents.transform);
                    var dataScript = newData.GetComponent<ContentItem>();
                    dataScript.DateText.text = data.Date;
                    dataScript.TimeText.text = data.Time;
                    dataScript.CoherenceScoreText.text = data.CoherenceScore;
                    dataScript.RelevanceScoreText.text = data.RelevanceScore;
                    dataScript.NaturalnessScoreText.text = data.NaturalnessScore;
                    dataScript.EngagementScoreText.text = data.EngagementScore;
                    dataScript.ContextualAccuracyScoreText.text = data.ContextualAccuracyScore;
                    dataScript.ChatgptAvgTimeText.text = data.ChatgptAvgTime;
                    dataScript.GeminiAvgTimeText.text = data.GeminiAvgTime;
                    dataScript.ClaudeAvgTimeText.text = data.ClaudeAvgTime;
                    dataScript.DeepSeekAvgTimeText.text = data.DeepseekAvgTime;
                    dataScript.MistralAvgTimeText.text = data.MistralAvgTime;
                }
            }
        }
    }

    /// <summary>
    /// Delete all the data from the file.
    /// </summary>
    public void DeleteData()
    {
        //If the file exists, then delete the file.
        if(File.Exists(filename))
            File.Delete(filename);

        ClearData();
    }

    /// <summary>
    /// Clears data from the previous score table.
    /// </summary>
    public void ClearData()
    {
        //destroy all the objects from the table to clear memory and avoid duplication when opening table for the next time.
        var objects = scrollContents.GetComponentsInChildren<ContentItem>();
        foreach (ContentItem item in objects)
        {
            Destroy(item.gameObject);
        }
    }
}
