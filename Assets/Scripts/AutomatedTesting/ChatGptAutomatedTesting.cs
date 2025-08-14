using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XCharts.Runtime;

public class ChatGptAutomatedTesting : MonoBehaviour
{
    [System.Serializable]
    public class TestCases
    {
        public string Chatgpt;
        public string Gemini;
        public string Claude;
        public string Deepseek;
        public string Mistral;

        public string Topic;
    }

    [System.Serializable]
    public class TestCasesList
    {
        public List<TestCases> testCases = new List<TestCases>();
    }

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

    [Header("Agents")]
    [SerializeField] private ChatGPTAgent chatgptAgent;
    [SerializeField] private ChatGPTAgent geminiAgent;
    [SerializeField] private ChatGPTAgent claudeAgent;
    [SerializeField] private ChatGPTAgent deepSeekAgent;
    [SerializeField] private ChatGPTAgent mistralAgent;
    [SerializeField] private NPCLLMTester npcllmtester;

    [Header("Parameters")]
    [SerializeField] private int dailogueCount = 25; // Total number of dialogues before conversation ends

    [Header("UI Items")]
    [SerializeField] private Slider progressSilder;
    [SerializeField] private TMP_InputField tescaseInputfield;
    [SerializeField] private LineChart lineChart;
    [SerializeField] private BarChart barChart;
    [SerializeField] private CandlestickChart candlestickChart;
    [SerializeField] private GameObject scrollContents;

    [Header("Prefab")]
    [SerializeField] private GameObject contentItem;
    private string topic;

    private int agent = 0; // Current speaking agent
    private int currentDailogueCount = 0; // Current dialogue count
    [SerializeField] private List<string> conversationHistory = new List<string>(); // Stores all conversation lines
    private string moderatorContent = string.Empty; // Used to send prompts for moderator introduction/conclusion
    private bool isSpeakingStarted = false; // Checks if conversation has started
    public bool stopConversation = false; // Conversation stop flag
    private string endContent = ""; // Stores full conversation for final summary

    private string filename;
    private string evaluatedfilename;
    private TestCasesList testCasesList = new TestCasesList();
    int currentTest = 0;


    private float circleSize = 12.5f;
    private float lineWidth = 7.5f;
    EvaluationDataList evaluationdataList = new EvaluationDataList();

    private List<Line> serieLines = new List<Line>();
    private List<Bar> serieBars = new List<Bar>();
    int tescaseValue = 0;

    private bool stopTest = false;

    List<float> coherenceList = new List<float>();
    List<float> relevanceList = new List<float>();
    List<float> naturalnessList = new List<float>();
    List<float> engagementList = new List<float>();
    List<float> caList = new List<float>();
    // Start is called before the first frame update
    void Start()
    {
        evaluatedfilename = Path.Combine(Application.dataPath, "Testcases/evaluationData_Chatgpt.json");

    }

    public void StartTest()
    {
        filename = Path.Combine(Application.dataPath, "Testcases/100Testcases.json");
        tescaseValue = int.TryParse(tescaseInputfield.text, out var result) ? result : 0;
        if (File.Exists(filename))
        {
            string jsonContent = File.ReadAllText(filename);
            testCasesList = JsonUtility.FromJson<TestCasesList>(jsonContent);
            chatgptAgent.isAutomatedTesting = true;
            geminiAgent.isAutomatedTesting = true;
            deepSeekAgent.isAutomatedTesting = true;
            claudeAgent.isAutomatedTesting = true;
            mistralAgent.isAutomatedTesting = true;
            progressSilder.maxValue = testCasesList.testCases.Count;
            StartCoroutine(StartTest_Coroutine());
        }
        else
        {
            Debug.LogError("JSON file not found at path: " + filename);
        }
    }

    private IEnumerator StartTest_Coroutine()
    {
        foreach (var testCase in testCasesList.testCases)
        {
            if (stopTest)
                yield break;

            ++currentTest;
            if (currentTest < tescaseValue)
                continue;

            //Debug.LogWarning(testCase.Topic + "\n" + testCase.Chatgpt + "\n" + testCase.Gemini + "\n" + testCase.Claude + "\n" + testCase.Deepseek + "\n" + testCase.Mistral);
            chatgptAgent.characterDescription = testCase.Chatgpt;
            geminiAgent.characterDescription = testCase.Gemini;
            claudeAgent.characterDescription = testCase.Claude;
            deepSeekAgent.characterDescription = testCase.Deepseek;
            mistralAgent.characterDescription = testCase.Mistral;

            chatgptAgent.topic = testCase.Topic;
            geminiAgent.topic = testCase.Topic;
            claudeAgent.topic = testCase.Topic;
            deepSeekAgent.topic = testCase.Topic;
            mistralAgent.topic = testCase.Topic;



            isSpeakingStarted = true;
            StartConversation();
            Debug.Log("testing case: " + currentTest + "/" + testCasesList.testCases.Count);
            progressSilder.value = currentTest;
            yield return new WaitUntil(() => !isSpeakingStarted);

            yield return new WaitUntil(() => npcllmtester.isCompleted);
        }

        Debug.Log("---- Test Completed --------");
    }

    private void SetTopic()
    {
        // Set "other character names" for context when generating dialogues
        chatgptAgent.otherCharactersNames = geminiAgent.characterName + " : " + geminiAgent.characterDescription + "," + claudeAgent.characterName + " : " + claudeAgent.characterDescription + "," + deepSeekAgent.characterName + " : " + deepSeekAgent.characterDescription + "," + mistralAgent.characterName + " : " + mistralAgent.characterDescription;
        geminiAgent.otherCharactersNames = chatgptAgent.characterName + " : " + chatgptAgent.characterDescription + "," + claudeAgent.characterName + " : " + claudeAgent.characterDescription + "," + deepSeekAgent.characterName + " : " + deepSeekAgent.characterDescription + "," + mistralAgent.characterName + " : " + mistralAgent.characterDescription;
        claudeAgent.otherCharactersNames = chatgptAgent.characterName + " : " + chatgptAgent.characterDescription + "," + geminiAgent.characterName + " : " + geminiAgent.characterDescription + "," + deepSeekAgent.characterName + " : " + deepSeekAgent.characterDescription + "," + mistralAgent.characterName + " : " + mistralAgent.characterDescription;
        deepSeekAgent.otherCharactersNames = chatgptAgent.characterName + " : " + chatgptAgent.characterDescription + "," + geminiAgent.characterName + " : " + geminiAgent.characterDescription + "," + claudeAgent.characterName + " : " + claudeAgent.characterDescription + "," + mistralAgent.characterName + " : " + mistralAgent.characterDescription;
        mistralAgent.otherCharactersNames = chatgptAgent.characterName + " : " + chatgptAgent.characterDescription + "," + geminiAgent.characterName + " : " + geminiAgent.characterDescription + "," + claudeAgent.characterName + " : " + claudeAgent.characterDescription + "," + deepSeekAgent.characterName + " : " + deepSeekAgent.characterDescription;

        // Set topic for all agents
        chatgptAgent.topic = topic;
        geminiAgent.topic = topic;
        claudeAgent.topic = topic;
        deepSeekAgent.topic = topic;
        mistralAgent.topic = topic;
    }

    /// <summary>
    /// Starts the conversation sequence: initializes variables and calls moderator intro.
    /// </summary>
    public void StartConversation()
    {
        SetTopic();
        stopConversation = false;
        conversationHistory.Clear();
        currentDailogueCount = 0;
        StartCoroutine(InitialModerator_Coroutine());
    }

    /// <summary>
    /// Moderator gives the initial introduction based on which agent is set as moderator.
    /// If the moderator is player-controlled, switches to PlayerAgentMode.
    /// </summary>
    private IEnumerator InitialModerator_Coroutine()
    {
        // Checks if the agent is the moderator.
        // If player-controlled, waits for player's dictation.
        // If AI, generates introductory dialogue and plays it.
        // Adds the intro to conversation history.
        // Waits for audio to finish.
        if (chatgptAgent.isModerator)
        {
            agent = 1;
            moderatorContent = "you are a moderator named " + chatgptAgent.characterName + "  a " +
                chatgptAgent.characterDescription + " and there are other characters named , " +
                geminiAgent.characterName + " " + geminiAgent.characterDescription + ", " +
                deepSeekAgent.characterName + " " + deepSeekAgent.characterDescription + ", " +
                mistralAgent.characterName + " " + mistralAgent.characterDescription + ", " +
                claudeAgent.characterName + " " + claudeAgent.characterDescription + ", " +
                ". generate a introductory dialogue for a conversation between them about" +
                topic + ". just like the  example\r\n\"Say Happily: Welcome guys and mom, thank you for being here.\"\r\n\"Say doubtfully: let me check the internet and make it sure\"\r\n\"Say sympathathically: so thats the end\"\r\nonly generate a single dialogue for moderator";

            chatgptAgent.GenerateModeratorContent(moderatorContent);
            yield return new WaitUntil(() => chatgptAgent.isCompleted);
            var str = chatgptAgent.characterName + " : " + chatgptAgent.responseMessage;
            conversationHistory.Add(str);
            npcllmtester.chatgptDailogues.Add(str);
        }
        else if (geminiAgent.isModerator)
        {
            agent = 2;
            moderatorContent = "you are a moderator named " + geminiAgent.characterName + "  a " +
                geminiAgent.characterDescription + " and there are other characters named , " +
                chatgptAgent.characterName + " " + chatgptAgent.characterDescription + ", " +
                deepSeekAgent.characterName + " " + deepSeekAgent.characterDescription + ", " +
                mistralAgent.characterName + " " + mistralAgent.characterDescription + ", " +
                claudeAgent.characterName + " " + claudeAgent.characterDescription + ", " +
                ". generate a introductory dialogue for a conversation between them about" +
                topic + ". just like the  example\r\n\"Say Happily: Welcome guys and mom, thank you for being here.\"\r\n\"Say doubtfully: let me check the internet and make it sure\"\r\n\"Say sympathathically: so thats the end\"\r\nonly generate a single dialogue for moderator";

            geminiAgent.GenerateModeratorContent(moderatorContent);
            yield return new WaitUntil(() => geminiAgent.isCompleted);
            var str = geminiAgent.characterName + " : " + geminiAgent.responseMessage;
            conversationHistory.Add(str);
            npcllmtester.geminitDailogues.Add(str);
        }
        else if (claudeAgent.isModerator)
        {
            agent = 3;
            moderatorContent = "you are a moderator named " + claudeAgent.characterName + "  a " +
                claudeAgent.characterDescription + " and there are other characters named , " +
                geminiAgent.characterName + " " + geminiAgent.characterDescription + ", " +
                deepSeekAgent.characterName + " " + deepSeekAgent.characterDescription + ", " +
                mistralAgent.characterName + " " + mistralAgent.characterDescription + ", " +
                chatgptAgent.characterName + " " + chatgptAgent.characterDescription + ", " +
                ". generate a introductory dialogue for a conversation between them about" +
                topic + ". just like the  example\r\n\"Say Happily: Welcome guys and mom, thank you for being here.\"\r\n\"Say doubtfully: let me check the internet and make it sure\"\r\n\"Say sympathathically: so thats the end\"\r\nonly generate a single dialogue for moderator";

            claudeAgent.GenerateModeratorContent(moderatorContent);
            yield return new WaitUntil(() => claudeAgent.isCompleted);
            var str = claudeAgent.characterName + " : " + claudeAgent.responseMessage;
            conversationHistory.Add(str);
            npcllmtester.claudeDailogues.Add(str);
        }
        else if (deepSeekAgent.isModerator)
        {
            agent = 4;
            moderatorContent = "you are a moderator named " + deepSeekAgent.characterName + "  a " +
                deepSeekAgent.characterDescription + " and there are other characters named , " +
                geminiAgent.characterName + " " + geminiAgent.characterDescription + ", " +
                chatgptAgent.characterName + " " + chatgptAgent.characterDescription + ", " +
                mistralAgent.characterName + " " + mistralAgent.characterDescription + ", " +
                claudeAgent.characterName + " " + claudeAgent.characterDescription + ", " +
                ". generate a introductory dialogue for a conversation between them about" +
                topic + ". just like the  example\r\n\"Say Happily: Welcome guys and mom, thank you for being here.\"\r\n\"Say doubtfully: let me check the internet and make it sure\"\r\n\"Say sympathathically: so thats the end\"\r\nonly generate a single dialogue for moderator";

            deepSeekAgent.GenerateModeratorContent(moderatorContent);
            yield return new WaitUntil(() => deepSeekAgent.isCompleted);
            var str = deepSeekAgent.characterName + " : " + deepSeekAgent.responseMessage;
            conversationHistory.Add(str);
            npcllmtester.deepseekDailogues.Add(str);
        }
        else if (mistralAgent.isModerator)
        {
            agent = 5;
            moderatorContent = "you are a moderator named " + mistralAgent.characterName + "  a " +
                mistralAgent.characterDescription + " and there are other characters named , " +
                geminiAgent.characterName + " " + geminiAgent.characterDescription + ", " +
                deepSeekAgent.characterName + " " + deepSeekAgent.characterDescription + ", " +
                chatgptAgent.characterName + " " + chatgptAgent.characterDescription + ", " +
                claudeAgent.characterName + " " + claudeAgent.characterDescription + ", " +
                ". generate a introductory dialogue for a conversation between them about" +
                topic + ". just like the  example\r\n\"Say Happily: Welcome guys and mom, thank you for being here.\"\r\n\"Say doubtfully: let me check the internet and make it sure\"\r\n\"Say sympathathically: so thats the end\"\r\nonly generate a single dialogue for moderator";

            mistralAgent.GenerateModeratorContent(moderatorContent);
            yield return new WaitUntil(() => mistralAgent.isCompleted);
            var str = mistralAgent.characterName + " : " + mistralAgent.responseMessage;
            conversationHistory.Add(str);
            npcllmtester.mistralDailogues.Add(str);
        }

        // After moderator introduction:
        IncremeantLastSpoken(agent); // Increments "lastSpoken" value for scoring system
        currentDailogueCount++;
        StartCoroutine(Conversation_Coroutine()); // Starts main conversation loop
    }

    /// <summary>
    /// Main conversation loop where agents take turns based on scoring, emotional analysis, and interruption logic.
    /// </summary>
    private IEnumerator Conversation_Coroutine()
    {

        while (!stopConversation)
        {
            var str = string.Empty;
            agent = FindNextSpeaker(); // Chooses next agent to speak based on scores

            // Handles single-speaker turn or multiple speakers trying to interrupt.
            // Checks for emotional keywords (e.g., "angry") and triggers moderator intervention if needed.
            // Waits for speech audio to finish before continuing.
            if (agent < 10)
            {
                if (agent == 1) //activate chatgpt 
                {
                    if (chatgptAgent.isModerator)
                        chatgptAgent.GenerateNextDailogue(conversationHistory, "Ask a question related to topic based on the conversation");
                    else
                        chatgptAgent.GenerateNextDailogue(conversationHistory);

                    yield return new WaitUntil(() => chatgptAgent.isCompleted);
                    str = chatgptAgent.characterName + " : " + chatgptAgent.responseMessage;
                    conversationHistory.Add(str);
                    npcllmtester.chatgptDailogues.Add(str);
                }
                else if (agent == 2) //activate Gemini
                {
                    if (geminiAgent.isModerator)
                        geminiAgent.GenerateNextDailogue(conversationHistory, "Ask a question related to topic based on the conversation");
                    else
                        geminiAgent.GenerateNextDailogue(conversationHistory);

                    yield return new WaitUntil(() => geminiAgent.isCompleted);
                    str = geminiAgent.characterName + " : " + geminiAgent.responseMessage;
                    conversationHistory.Add(str);
                    npcllmtester.geminitDailogues.Add(str);
                }
                else if (agent == 3) //activate claude
                {
                    if (claudeAgent.isModerator)
                        claudeAgent.GenerateNextDailogue(conversationHistory, "Ask a question related to topic based on the conversation");
                    else
                        claudeAgent.GenerateNextDailogue(conversationHistory);

                    yield return new WaitUntil(() => claudeAgent.isCompleted);
                    str = claudeAgent.characterName + " : " + claudeAgent.responseMessage;
                    conversationHistory.Add(str);
                    npcllmtester.geminitDailogues.Add(str);
                }
                else if (agent == 4) //activate deepseek
                {
                    if (deepSeekAgent.isModerator)
                        deepSeekAgent.GenerateNextDailogue(conversationHistory, "Ask a question related to topic based on the conversation");
                    else
                        deepSeekAgent.GenerateNextDailogue(conversationHistory);

                    yield return new WaitUntil(() => deepSeekAgent.isCompleted);
                    str = deepSeekAgent.characterName + " : " + deepSeekAgent.responseMessage;
                    conversationHistory.Add(str);
                    npcllmtester.deepseekDailogues.Add(str);
                }
                else if (agent == 5) //activate Mistral
                {
                    if (mistralAgent.isModerator)
                        mistralAgent.GenerateNextDailogue(conversationHistory, "Ask a question related to topic based on the conversation");
                    else
                        mistralAgent.GenerateNextDailogue(conversationHistory);

                    yield return new WaitUntil(() => mistralAgent.isCompleted);
                    str = mistralAgent.characterName + " : " + mistralAgent.responseMessage;
                    conversationHistory.Add(str);
                    npcllmtester.mistralDailogues.Add(str);
                }

                IncremeantLastSpoken(agent);

                str = str.ToLower();
                if (str.Contains("frustrated") || str.Contains("angrily") || str.Contains("sadly") ||
                    str.Contains("mockingly") | str.Contains("worried") || str.Contains("worriedly"))
                {
                    if (chatgptAgent.isModerator)
                    {
                        agent = 1;
                        chatgptAgent.GenerateNextDailogue(conversationHistory, "generate a dailogue to calm the previous speaker");
                        yield return new WaitUntil(() => chatgptAgent.isCompleted);
                        str = chatgptAgent.characterName + " : " + chatgptAgent.responseMessage;
                        conversationHistory.Add(str);
                        npcllmtester.chatgptDailogues.Add(str);
                    }
                    else if (geminiAgent.isModerator)
                    {
                        agent = 2;
                        conversationHistory.Add("generate a dailogue to calm the previous speaker");
                        geminiAgent.GenerateNextDailogue(conversationHistory);
                        yield return new WaitUntil(() => geminiAgent.isCompleted);
                        str = geminiAgent.characterName + " : " + geminiAgent.responseMessage;
                        conversationHistory.Add(str);
                        npcllmtester.geminitDailogues.Add(str);
                    }
                    else if (claudeAgent.isModerator)
                    {
                        agent = 3;
                        conversationHistory.Add("generate a dailogue to calm the previous speaker");
                        claudeAgent.GenerateNextDailogue(conversationHistory);
                        yield return new WaitUntil(() => claudeAgent.isCompleted);
                        str = claudeAgent.characterName + " : " + claudeAgent.responseMessage;
                        conversationHistory.Add(str);
                        npcllmtester.claudeDailogues.Add(str);
                    }
                    else if (deepSeekAgent.isModerator)
                    {
                        agent = 4;
                        conversationHistory.Add("generate a dailogue to calm the previous speaker");
                        deepSeekAgent.GenerateNextDailogue(conversationHistory);
                        yield return new WaitUntil(() => deepSeekAgent.isCompleted);
                        str = deepSeekAgent.characterName + " : " + deepSeekAgent.responseMessage;
                        conversationHistory.Add(str);
                        npcllmtester.deepseekDailogues.Add(str);
                    }
                    else if (mistralAgent.isModerator)
                    {
                        agent = 5;
                        conversationHistory.Add("generate a dailogue to calm the previous speaker");
                        mistralAgent.GenerateNextDailogue(conversationHistory);
                        yield return new WaitUntil(() => mistralAgent.isCompleted);
                        str = mistralAgent.characterName + " : " + mistralAgent.responseMessage;
                        conversationHistory.Add(str);
                        npcllmtester.mistralDailogues.Add(str);
                    }
                    IncremeantLastSpoken(agent);
                }
            }
            else
            {
                var agents = agent;
                var a = new List<int>();
                while (agents > 0)
                {
                    a.Add(agents % 10);
                    agents = agents / 10;
                }

                var highestSpeakingCapability = 0.0f;
                var speakingagent = 0;
                var speakingAgentName = string.Empty;
                var speakingAgentDescription = string.Empty;
                foreach (var b in a)
                {
                    if (b == 1 && highestSpeakingCapability < chatgptAgent.speakingCapability && !chatgptAgent.isPlayer)
                    {
                        speakingagent = 1;
                        speakingAgentName = chatgptAgent.characterName;
                        highestSpeakingCapability = chatgptAgent.speakingCapability;
                        speakingAgentDescription = chatgptAgent.characterDescription;
                    }
                    else if (b == 2 && highestSpeakingCapability < geminiAgent.speakingCapability && !geminiAgent.isPlayer)
                    {
                        speakingagent = 2;
                        speakingAgentName = geminiAgent.characterName;
                        highestSpeakingCapability = geminiAgent.speakingCapability;
                        speakingAgentDescription = geminiAgent.characterDescription;
                    }
                    else if (b == 3 && highestSpeakingCapability < claudeAgent.speakingCapability && !claudeAgent.isPlayer)
                    {
                        speakingagent = 3;
                        speakingAgentName = claudeAgent.characterName;
                        highestSpeakingCapability = claudeAgent.speakingCapability;
                        speakingAgentDescription = claudeAgent.characterDescription;
                    }
                    else if (b == 4 && highestSpeakingCapability < deepSeekAgent.speakingCapability && !deepSeekAgent.isPlayer)
                    {
                        speakingagent = 4;
                        speakingAgentName = deepSeekAgent.characterName;
                        highestSpeakingCapability = deepSeekAgent.speakingCapability;
                        speakingAgentDescription = deepSeekAgent.characterDescription;
                    }
                    else if (b == 5 && highestSpeakingCapability < mistralAgent.speakingCapability && !mistralAgent.isPlayer)
                    {
                        speakingagent = 5;
                        speakingAgentName = mistralAgent.characterName;
                        highestSpeakingCapability = mistralAgent.speakingCapability;
                        speakingAgentDescription = mistralAgent.characterDescription;
                    }
                }

                a.Remove(speakingagent);

                if (speakingagent == 1) //activate chatgpt
                {
                    chatgptAgent.GenerateNextDailogue(conversationHistory);
                    yield return new WaitUntil(() => chatgptAgent.isCompleted);
                    str = chatgptAgent.characterName + " : " + chatgptAgent.responseMessage;
                    conversationHistory.Add(str);
                    npcllmtester.chatgptDailogues.Add(str);
                }
                else if (speakingagent == 2) //activate Gemini
                {
                    geminiAgent.GenerateNextDailogue(conversationHistory);
                    yield return new WaitUntil(() => geminiAgent.isCompleted);
                    str = geminiAgent.characterName + " : " + geminiAgent.responseMessage;
                    conversationHistory.Add(str);
                    npcllmtester.geminitDailogues.Add(str);
                }
                else if (speakingagent == 3) //activate claude
                {
                    claudeAgent.GenerateNextDailogue(conversationHistory);
                    yield return new WaitUntil(() => claudeAgent.isCompleted);
                    str = claudeAgent.characterName + " : " + claudeAgent.responseMessage;
                    conversationHistory.Add(str);
                    npcllmtester.claudeDailogues.Add(str);
                }
                else if (speakingagent == 4) //activate deepseek
                {
                    deepSeekAgent.GenerateNextDailogue(conversationHistory);
                    yield return new WaitUntil(() => deepSeekAgent.isCompleted);
                    str = deepSeekAgent.characterName + " : " + deepSeekAgent.responseMessage;
                    conversationHistory.Add(str);
                    npcllmtester.deepseekDailogues.Add(str);
                }
                else if (speakingagent == 5) //activate Mistral
                {
                    mistralAgent.GenerateNextDailogue(conversationHistory);
                    yield return new WaitUntil(() => mistralAgent.isCompleted);
                    str = mistralAgent.characterName + " : " + mistralAgent.responseMessage;
                    conversationHistory.Add(str);
                    npcllmtester.mistralDailogues.Add(str);
                }

                if (a.Contains(1) && speakingagent != 1) //activate chatgpt
                {
                    chatgptAgent.GenerateNextDailogue(conversationHistory, "just begin to say a dailogue  but you realized you interrupted another speaker so stop your point and apologize for interrupting another speaker and politely tell that person to continue talking, create very short dialogue, the interrupted person is " + speakingAgentName + ":" + speakingAgentDescription);
                    yield return new WaitUntil(() => chatgptAgent.isCompleted);
                    str = chatgptAgent.characterName + " : " + chatgptAgent.responseMessage;
                    conversationHistory.Add(str);
                    npcllmtester.chatgptDailogues.Add(str);
                }
                else if (a.Contains(2) && speakingagent != 2) //activate Gemini
                {
                    geminiAgent.GenerateNextDailogue(conversationHistory, "\"just begin to say a dailogue  but you realized you interrupted another speaker so stop your point and apologize for interrupting another speaker and politely tell that person to continue talking, create very short dialogue, the interrupted person is " + speakingAgentName + ":" + speakingAgentDescription);
                    yield return new WaitUntil(() => geminiAgent.isCompleted);
                    str = geminiAgent.characterName + " : " + geminiAgent.responseMessage;
                    conversationHistory.Add(str);
                    npcllmtester.geminitDailogues.Add(str);
                }
                else if (a.Contains(3) && speakingagent != 3) //activate claude
                {
                    claudeAgent.GenerateNextDailogue(conversationHistory, "\"just begin to say a dailogue  but you realized you interrupted another speaker so stop your point and apologize for interrupting another speaker and politely tell that person to continue talking, create very short dialogue, the interrupted person is " + speakingAgentName + ":" + speakingAgentDescription);
                    yield return new WaitUntil(() => claudeAgent.isCompleted);
                    str = claudeAgent.characterName + " : " + claudeAgent.responseMessage;
                    conversationHistory.Add(str);
                    npcllmtester.claudeDailogues.Add(str);
                }
                else if (a.Contains(4) && speakingagent != 4) //activate deepseek
                {
                    deepSeekAgent.GenerateNextDailogue(conversationHistory, "\"just begin to say a dailogue  but you realized you interrupted another speaker so stop your point and apologize for interrupting another speaker and politely tell that person to continue talking, create very short dialogue, the interrupted person is " + speakingAgentName + ":" + speakingAgentDescription);
                    yield return new WaitUntil(() => deepSeekAgent.isCompleted);
                    str = deepSeekAgent.characterName + " : " + deepSeekAgent.responseMessage;
                    conversationHistory.Add(str);
                    npcllmtester.deepseekDailogues.Add(str);
                }
                else if (a.Contains(5) && speakingagent != 5) //activate Mistral
                {
                    mistralAgent.GenerateNextDailogue(conversationHistory, "\"just begin to say a dailogue  but you realized you interrupted another speaker so stop your point and apologize for interrupting another speaker and politely tell that person to continue talking, create very short dialogue, the interrupted person is " + speakingAgentName + ":" + speakingAgentDescription);
                    yield return new WaitUntil(() => mistralAgent.isCompleted);
                    str = mistralAgent.characterName + " : " + mistralAgent.responseMessage;
                    conversationHistory.Add(str);
                    npcllmtester.mistralDailogues.Add(str);
                }

                if (speakingagent == 1) //activate chatgpt
                {
                    chatgptAgent.GenerateNextDailogue(conversationHistory);
                    yield return new WaitUntil(() => chatgptAgent.isCompleted);
                    str = chatgptAgent.characterName + " : " + chatgptAgent.responseMessage;
                    conversationHistory.Add(str);
                    npcllmtester.chatgptDailogues.Add(str);
                }
                else if (speakingagent == 2) //activate Gemini
                {
                    geminiAgent.GenerateNextDailogue(conversationHistory);
                    yield return new WaitUntil(() => geminiAgent.isCompleted);
                    str = geminiAgent.characterName + " : " + geminiAgent.responseMessage;
                    conversationHistory.Add(str);
                    npcllmtester.geminitDailogues.Add(str);
                }
                else if (speakingagent == 3) //activate claude
                {
                    claudeAgent.GenerateNextDailogue(conversationHistory);
                    yield return new WaitUntil(() => claudeAgent.isCompleted);
                    str = claudeAgent.characterName + " : " + claudeAgent.responseMessage;
                    conversationHistory.Add(str);
                    npcllmtester.claudeDailogues.Add(str);
                }
                else if (speakingagent == 4) //activate deepseek
                {
                    deepSeekAgent.GenerateNextDailogue(conversationHistory);
                    yield return new WaitUntil(() => deepSeekAgent.isCompleted);
                    str = deepSeekAgent.characterName + " : " + deepSeekAgent.responseMessage;
                    conversationHistory.Add(str);
                    npcllmtester.deepseekDailogues.Add(str);
                }
                else if (speakingagent == 5) //activate Mistral
                {
                    mistralAgent.GenerateNextDailogue(conversationHistory);
                    yield return new WaitUntil(() => mistralAgent.isCompleted);
                    str = mistralAgent.characterName + " : " + mistralAgent.responseMessage;
                    conversationHistory.Add(str);
                    npcllmtester.mistralDailogues.Add(str);
                }

                IncremeantLastSpoken(speakingagent);
            }
            currentDailogueCount++;
            if (currentDailogueCount > dailogueCount)
                stopConversation = true;
        }
        StartCoroutine(EndModerator_Coroutine());
    }

    /// <summary>
    /// Moderator summarizes the entire conversation and concludes it.
    /// </summary>
    private IEnumerator EndModerator_Coroutine()
    {
        // Combines all conversation lines into one string and asks moderator to generate conclusion.
        foreach (var str in conversationHistory)
            endContent += str + "\n";

        if (chatgptAgent.isModerator)
        {
            moderatorContent = "you are a moderator named " + chatgptAgent.characterName + "  a " +
                chatgptAgent.characterDescription + " and there are other characters named , " +
                geminiAgent.characterName + " " + geminiAgent.characterDescription + ", " +
                deepSeekAgent.characterName + " " + deepSeekAgent.characterDescription + ", " +
                mistralAgent.characterName + " " + mistralAgent.characterDescription + ", " +
                claudeAgent.characterName + " " + claudeAgent.characterDescription + ", " +
                ". generate a conclusion dialogue for the conversation " +
                topic + ". just like the  example\r\n\"Say Happily: Welcome guys and mom, thank you for being here.\"\r\n\"Say doubtfully: let me check the internet and make it sure\"\r\n\"Say sympathathically: so thats the end\"\r\nthe full dailogue is given above, and create a single dailogue concluding most of the valid points spoken";

            endContent += "\n" + moderatorContent;
            chatgptAgent.GenerateModeratorContent(endContent);
            yield return new WaitUntil(() => chatgptAgent.isCompleted);
            var str = chatgptAgent.characterName + " : " + chatgptAgent.responseMessage;
            conversationHistory.Add(str);
            npcllmtester.chatgptDailogues.Add(str);
        }
        else if (geminiAgent.isModerator)
        {
            moderatorContent = "you are a moderator named " + geminiAgent.characterName + "  a " +
                geminiAgent.characterDescription + " and there are other characters named , " +
                chatgptAgent.characterName + " " + chatgptAgent.characterDescription + ", " +
                deepSeekAgent.characterName + " " + deepSeekAgent.characterDescription + ", " +
                mistralAgent.characterName + " " + mistralAgent.characterDescription + ", " +
                claudeAgent.characterName + " " + claudeAgent.characterDescription + ", " +
                ". generate a conclusion dialogue for the conversation " +
                topic + ". just like the  example\r\n\"Say Happily: Welcome guys and mom, thank you for being here.\"\r\n\"Say doubtfully: let me check the internet and make it sure\"\r\n\"Say sympathathically: so thats the end\"\r\nthe full dailogue is given above, and create a single dailogue concluding most of the valid points spoken";

            endContent += "\n" + moderatorContent;
            geminiAgent.GenerateModeratorContent(endContent);
            yield return new WaitUntil(() => geminiAgent.isCompleted);
            var str = geminiAgent.characterName + " : " + geminiAgent.responseMessage;
            conversationHistory.Add(str);
            npcllmtester.geminitDailogues.Add(str);
        }
        else if (deepSeekAgent.isModerator)
        {
            moderatorContent = "you are a moderator named " + deepSeekAgent.characterName + "  a " +
                deepSeekAgent.characterDescription + " and there are other characters named , " +
                geminiAgent.characterName + " " + geminiAgent.characterDescription + ", " +
                chatgptAgent.characterName + " " + chatgptAgent.characterDescription + ", " +
                mistralAgent.characterName + " " + mistralAgent.characterDescription + ", " +
                claudeAgent.characterName + " " + claudeAgent.characterDescription + ", " +
                ". generate a conclusion dialogue for the conversation " +
                topic + ". just like the  example\r\n\"Say Happily: Welcome guys and mom, thank you for being here.\"\r\n\"Say doubtfully: let me check the internet and make it sure\"\r\n\"Say sympathathically: so thats the end\"\r\nthe full dailogue is given above, and create a single dailogue concluding most of the valid points spoken";

            endContent += "\n" + moderatorContent;
            deepSeekAgent.GenerateModeratorContent(endContent);
            yield return new WaitUntil(() => deepSeekAgent.isCompleted);
            var str = deepSeekAgent.characterName + " : " + deepSeekAgent.responseMessage;
            conversationHistory.Add(str);
            npcllmtester.deepseekDailogues.Add(str);
        }
        else if (mistralAgent.isModerator)
        {
            moderatorContent = "you are a moderator named " + mistralAgent.characterName + "  a " +
                mistralAgent.characterDescription + " and there are other characters named , " +
                geminiAgent.characterName + " " + geminiAgent.characterDescription + ", " +
                deepSeekAgent.characterName + " " + deepSeekAgent.characterDescription + ", " +
                chatgptAgent.characterName + " " + chatgptAgent.characterDescription + ", " +
                claudeAgent.characterName + " " + claudeAgent.characterDescription + ", " +
                ". generate a conclusion dialogue for the conversation " +
                topic + ". just like the  example\r\n\"Say Happily: Welcome guys and mom, thank you for being here.\"\r\n\"Say doubtfully: let me check the internet and make it sure\"\r\n\"Say sympathathically: so thats the end\"\r\nthe full dailogue is given above, and create a single dailogue concluding most of the valid points spoken";

            endContent += "\n" + moderatorContent;
            mistralAgent.GenerateModeratorContent(endContent);
            yield return new WaitUntil(() => mistralAgent.isCompleted);
            var str = mistralAgent.characterName + " : " + mistralAgent.responseMessage;
            conversationHistory.Add(str);
            npcllmtester.mistralDailogues.Add(str);
        }
        else if (claudeAgent.isModerator)
        {
            moderatorContent = "you are a moderator named " + claudeAgent.characterName + "  a " +
                claudeAgent.characterDescription + " and there are other characters named , " +
                geminiAgent.characterName + " " + geminiAgent.characterDescription + ", " +
                deepSeekAgent.characterName + " " + deepSeekAgent.characterDescription + ", " +
                mistralAgent.characterName + " " + mistralAgent.characterDescription + ", " +
                chatgptAgent.characterName + " " + chatgptAgent.characterDescription + ", " +
                ". generate a conclusion dialogue for the conversation " +
                topic + ". just like the  example\r\n\"Say Happily: Welcome guys and mom, thank you for being here.\"\r\n\"Say doubtfully: let me check the internet and make it sure\"\r\n\"Say sympathathically: so thats the end\"\r\nthe full dailogue is given above, and create a single dailogue concluding most of the valid points spoken";

            endContent += "\n" + moderatorContent;
            claudeAgent.GenerateModeratorContent(endContent);
            yield return new WaitUntil(() => claudeAgent.isCompleted);
            var str = claudeAgent.characterName + " : " + claudeAgent.responseMessage;
            conversationHistory.Add(str);
            npcllmtester.claudeDailogues.Add(str);
        }

        StartEvaluation();
    }

    #region Helper Functions

    /// <summary>
    /// Determines the next speaker based on emotional score, knowledge, and speaking capability.
    /// If multiple agents have the same highest score, triggers an interruption event.
    /// </summary>
    private int FindNextSpeaker()
    {
        if(conversationHistory.Count > 1)
        {
            var str = conversationHistory[conversationHistory.Count - 1];
            if (agent == 1 && str != "" && !string.IsNullOrEmpty(str))
            {
                var str1 = str.ToLower();
                if (str.Contains("angry") || str.Contains("frustrated"))
                    chatgptAgent.emotionalScore = 1.0f;
                if (str.Contains("worried"))
                    chatgptAgent.emotionalScore = 0.8f;
                if (str.Contains("confused") || str.Contains("determined"))
                    chatgptAgent.emotionalScore = 0.5f;
                if (str.Contains("happy") || str.Contains("neutral"))
                    chatgptAgent.emotionalScore = 0.2f;
            }
            else if (agent == 2 && str != "" && !string.IsNullOrEmpty(str))
            {
                var str1 = str.ToLower();
                if (str.Contains("angry") || str.Contains("frustrated"))
                    geminiAgent.emotionalScore = 1.0f;
                if (str.Contains("worried"))
                    geminiAgent.emotionalScore = 0.8f;
                if (str.Contains("confused") || str.Contains("determined"))
                    geminiAgent.emotionalScore = 0.5f;
                if (str.Contains("happy") || str.Contains("neutral"))
                    geminiAgent.emotionalScore = 0.2f;
            }
            else if (agent == 3 && str != "" && !string.IsNullOrEmpty(str))
            {
                var str1 = str.ToLower();
                if (str.Contains("angry") || str.Contains("frustrated"))
                    claudeAgent.emotionalScore = 1.0f;
                if (str.Contains("worried"))
                    claudeAgent.emotionalScore = 0.8f;
                if (str.Contains("confused") || str.Contains("determined"))
                    claudeAgent.emotionalScore = 0.5f;
                if (str.Contains("happy") || str.Contains("neutral"))
                    claudeAgent.emotionalScore = 0.2f;
            }
            else if (agent == 4 && str != "" && !string.IsNullOrEmpty(str))
            {
                var str1 = str.ToLower();
                if (str.Contains("angry") || str.Contains("frustrated"))
                    deepSeekAgent.emotionalScore = 1.0f;
                if (str.Contains("worried"))
                    deepSeekAgent.emotionalScore = 0.8f;
                if (str.Contains("confused") || str.Contains("determined"))
                    deepSeekAgent.emotionalScore = 0.5f;
                if (str.Contains("happy") || str.Contains("neutral"))
                    deepSeekAgent.emotionalScore = 0.2f;
            }
            else if (agent == 5 && str != "" && !string.IsNullOrEmpty(str))
            {
                var str1 = str.ToLower();
                if (str.Contains("angry") || str.Contains("frustrated"))
                    mistralAgent.emotionalScore = 1.0f;
                if (str.Contains("worried"))
                    mistralAgent.emotionalScore = 0.8f;
                if (str.Contains("confused") || str.Contains("determined"))
                    mistralAgent.emotionalScore = 0.5f;
                if (str.Contains("happy") || str.Contains("neutral"))
                    mistralAgent.emotionalScore = 0.2f;
            }
        }


        float score1 = (chatgptAgent.characterKnowledge + chatgptAgent.speakingCapability + chatgptAgent.emotionalScore + chatgptAgent.lastSpoken);
        float score2 = (geminiAgent.characterKnowledge + geminiAgent.speakingCapability + geminiAgent.emotionalScore + geminiAgent.lastSpoken);
        float score3 = (claudeAgent.characterKnowledge + claudeAgent.speakingCapability + claudeAgent.emotionalScore + claudeAgent.lastSpoken);
        float score4 = (deepSeekAgent.characterKnowledge + deepSeekAgent.speakingCapability + deepSeekAgent.emotionalScore + deepSeekAgent.lastSpoken);
        float score5 = (mistralAgent.characterKnowledge + mistralAgent.speakingCapability + mistralAgent.emotionalScore + mistralAgent.lastSpoken);

        float bestScore = Mathf.Max(score1, score2, score3, score4, score5);
        //Debug.Log("score 1: " + score1 + " score 2: " + score2 + " Score3: " + score3 + " Score4: " + score4 + " Score5: " + score5);

        int interruption = 0;
        int intruptionagents = 0;

        if (bestScore == score1)
        {
            interruption++;
            intruptionagents = (intruptionagents * 10) + 1;
        }
        if (bestScore == score2)
        {
            interruption++;
            intruptionagents = (intruptionagents * 10) + 2;
        }
        if (bestScore == score3)
        {
            interruption++;
            intruptionagents = (intruptionagents * 10) + 3;
        }
        if (bestScore == score4)
        {
            interruption++;
            intruptionagents = (intruptionagents * 10) + 4;
        }
        if (bestScore == score5)
        {
            interruption++;
            intruptionagents = (intruptionagents * 10) + 5;
        }

        if (interruption > 1)
            return intruptionagents;
        else
        {
            if (bestScore == score1)
                return 1;
            else if (bestScore == score2)
                return 2;
            else if (bestScore == score3)
                return 3;
            else if (bestScore == score4)
                return 4;
            else if (bestScore == score5)
                return 5;
        }

        return Random.Range(0, 5);// Fallback: random agent if no clear winner
    }

    /// <summary>
    /// Increments "lastSpoken" for current speaker and resets others.
    /// Helps in scoring system for selecting next speaker.
    /// </summary>
    private void IncremeantLastSpoken(int agent)
    {
        chatgptAgent.lastSpoken = (agent == 1) ? 0.0f : chatgptAgent.lastSpoken + 1.0f;
        geminiAgent.lastSpoken = (agent == 2) ? 0.0f : geminiAgent.lastSpoken + 1.0f;
        claudeAgent.lastSpoken = (agent == 3) ? 0.0f : claudeAgent.lastSpoken + 1.0f;
        deepSeekAgent.lastSpoken = (agent == 4) ? 0.0f : deepSeekAgent.lastSpoken + 1.0f;
        mistralAgent.lastSpoken = (agent == 5) ? 0.0f : mistralAgent.lastSpoken + 1.0f;
    }

    /// <summary>
    /// Start evaluating the conversation and the speed of LLMs.
    /// </summary>
    private void StartEvaluation()
    {
        npcllmtester.chatgptResponceTimes = chatgptAgent.responseTimes;
        npcllmtester.geminiResponceTimes = geminiAgent.responseTimes;
        npcllmtester.claudeResponceTimes = claudeAgent.responseTimes;
        npcllmtester.deepseekResponceTimes = deepSeekAgent.responseTimes;
        npcllmtester.mistraltResponceTimes = mistralAgent.responseTimes;
        var conversation = new List<string>(conversationHistory);
        npcllmtester.filename = evaluatedfilename;
        npcllmtester.EvaluateDailogues(conversation);
        isSpeakingStarted = false;
    }

    /// <summary>
    /// Retrieve data from filename and view in the table, also print into console.
    /// </summary>
    

    private void SetupLineChart()
    {
        var chatgptline = lineChart.AddSerie<Line>("Chatgpt", true);
        var gemintline = lineChart.AddSerie<Line>("Gemini", true);
        var claudeline = lineChart.AddSerie<Line>("Claude", true);
        var mistralline = lineChart.AddSerie<Line>("Mistral", true);
        var deepseekline = lineChart.AddSerie<Line>("DeepSeek", true);

        serieLines.Add(chatgptline);
        serieLines.Add(gemintline);
        serieLines.Add(claudeline);
        serieLines.Add(deepseekline);
        serieLines.Add(mistralline);

        foreach (var line in serieLines)
        {
            line.symbol.size = circleSize;
            line.lineStyle.width = lineWidth;
            line.symbol.type = SymbolType.Circle;
        }

        int i = 0;
        lineChart.AddXAxisData(i.ToString());
        foreach (var data in evaluationdataList.dataList)
        {
            i++;
            lineChart.AddXAxisData(i.ToString());
            lineChart.AddData(chatgptline.index, float.Parse(data.ChatgptAvgTime));
            lineChart.AddData(gemintline.index, float.Parse(data.GeminiAvgTime));
            lineChart.AddData(claudeline.index, float.Parse(data.ClaudeAvgTime));
            lineChart.AddData(deepseekline.index, float.Parse(data.DeepseekAvgTime));
            lineChart.AddData(mistralline.index, float.Parse(data.MistralAvgTime));
        }
    }

    private void SetupBarChart()
    {
        var bar1 = barChart.AddSerie<Bar>("Average Time", true);
        serieBars.Add(bar1);

        int i = 0;
        barChart.AddXAxisData(i.ToString());
        foreach (var data in evaluationdataList.dataList)
        {
            i++;
            barChart.AddXAxisData(i.ToString());
            float avg = float.Parse(data.ChatgptAvgTime) ;
            barChart.AddData(bar1.index, avg);
        }
    }
    public void FetchData()
    {
        //if the file doesn't exist return else fetch the value into a list

        if (File.Exists(evaluatedfilename))
        {
            string result = File.ReadAllText(evaluatedfilename);
            evaluationdataList = JsonUtility.FromJson<EvaluationDataList>(result);
            if (evaluationdataList == null || evaluationdataList.dataList == null)
            {
                Debug.LogError("Data doesn't exists");
                return;
            }
        }
        ResetChart();
        SetupLineChart();
        SetupBarChart();
        SetupCandlestickChart();
    }
    private void SetupCandlestickChart()
    {
        foreach(var data in evaluationdataList.dataList)
        {
            coherenceList.Add(float.Parse(data.CoherenceScore));
            relevanceList.Add(float.Parse(data.RelevanceScore));
            naturalnessList.Add(float.Parse(data.NaturalnessScore));
            engagementList.Add(float.Parse(data.EngagementScore));
            caList.Add(float.Parse(data.ContextualAccuracyScore));
        }
        coherenceList.Sort();
        relevanceList.Sort();
        naturalnessList.Sort();
        engagementList.Sort();
        caList.Sort();

        var coherenceStats = CalculateQuartiles(coherenceList);
        var relevanceStats = CalculateQuartiles(relevanceList);
        var naturalnessStats = CalculateQuartiles(naturalnessList);
        var engagementStats = CalculateQuartiles(engagementList);
        var caStats = CalculateQuartiles(caList);

        candlestickChart.AddXAxisData("Coherence");
        candlestickChart.AddData(0, 1, coherenceStats.q1, coherenceStats.q3, coherenceStats.min, coherenceStats.max);
        candlestickChart.AddXAxisData("Relevance");
        candlestickChart.AddData(0, 2, relevanceStats.q1, relevanceStats.q3, relevanceStats.min, relevanceStats.max);
        candlestickChart.AddXAxisData("Naturalness");
        candlestickChart.AddData(0, 3, naturalnessStats.q1, naturalnessStats.q3, naturalnessStats.min, naturalnessStats.max);
        candlestickChart.AddXAxisData("Engagement");
        candlestickChart.AddData(0, 4, engagementStats.q1, engagementStats.q3, engagementStats.min, engagementStats.max);
        candlestickChart.AddXAxisData("Contextual Accuracy");
        candlestickChart.AddData(0, 5, caStats.q1, caStats.q3, caStats.min, caStats.max);

        //var candlestick = candlestickChart.AddSerieData();
    }

    private (float min, float q1, float median, float q3, float max) CalculateQuartiles(List<float> sortedList)
    {
        int count = sortedList.Count;
        if (count == 0) return (0, 0, 0, 0, 0);

        float median = GetMedian(sortedList, 0, count);
        float q1 = GetMedian(sortedList, 0, count / 2);
        float q3 = GetMedian(sortedList, (count + 1) / 2, count);

        return (sortedList.First(), q1, median, q3, sortedList.Last());
    }

    private float GetMedian(List<float> sortedList, int start, int end)
    {
        int length = end - start;
        int mid = start + length / 2;

        if (length % 2 == 0)
            return (sortedList[mid - 1] + sortedList[mid]) / 2f;
        else
            return sortedList[mid];
    }
    public void ResetChart()
    {
        barChart.RemoveAllSerie();
        barChart.ClearData();
        lineChart.RemoveAllSerie();
        lineChart.RemoveData();
        candlestickChart.ClearData();
    }

    public void StopTest()
    {
        stopTest = true;
    }

    public void ViewTableData()
    {
        //if the file doesn't exist return else fetch the value into a list
        EvaluationDataList dataList = new EvaluationDataList();
        if (File.Exists(evaluatedfilename))
        {
            string result = File.ReadAllText(evaluatedfilename);
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
        if (File.Exists(evaluatedfilename))
            File.Delete(evaluatedfilename);

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

    #endregion
}
