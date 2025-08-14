using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages multiple NPC agents (ChatGPT, Gemini, Claude, DeepSeek, and Mistral) 
/// that engage in a conversation with a moderator, player input, and automatic dialogue flow.
/// Handles camera switching, subtitles, emotional scoring, and conversation history.
/// </summary>
public class NPCAgentsManager : MonoBehaviour
{
    [Header("Agents")]
    [SerializeField] private ChatGPTAgent chatgptAgent;
    [SerializeField] private GeminiAgent geminiAgent;
    [SerializeField] private ClaudeAgent claudeAgent;
    [SerializeField] private DeepSeekAgent deepSeekAgent;
    [SerializeField] private MistralAgent mistralAgent;

    [Header("Cache")]
    [SerializeField] private NPCAgentCustomizationManager characterCustomization;
    [SerializeField] private Camera characterCamera;
    [SerializeField] private NPCLLMTester npcllmtester;
    [SerializeField] private DictationManager dictationManager; // For player speech input

    [Header("UI Cache")]
    [SerializeField] private TMP_Text subtitleText;
    [SerializeField] private GameObject settingsUI;
    [SerializeField] private Button startSpeakingButton;
    [SerializeField] private Button stopSpeakingButton;

    [Header("Parameters")]
    [SerializeField] private int dailogueCount = 25; // Total number of dialogues before conversation ends

    private bool isAnimate = false; // Smooth camera rotation flag
    private bool isSpeakingStarted = false; // Checks if conversation has started
    public bool stopConversation = false; // Conversation stop flag
    private Quaternion targetRot; // Target rotation for smooth camera movement
    private int agent = 0; // Current speaking agent
    private string endContent = ""; // Stores full conversation for final summary
    private int currentDailogueCount = 0; // Current dialogue count
    private Vector3 initialPos; // Initial camera position
    private Vector3 initialScale; // Initial camera scale
    private float initialFOV = 60.0f; // Default camera FOV
    [SerializeField] private List<string> conversationHistory = new List<string>(); // Stores all conversation lines
    private string moderatorContent = string.Empty; // Used to send prompts for moderator introduction/conclusion

    private void Awake()
    {
        // Set "other character names" for context when generating dialogues
        chatgptAgent.otherCharactersNames = geminiAgent.characterName + " : " + geminiAgent.characterDescription + "," + claudeAgent.characterName +" : " + claudeAgent.characterDescription + "," + deepSeekAgent.characterName + " : " + deepSeekAgent.characterDescription + "," + mistralAgent.characterName + " : " + mistralAgent.characterDescription;
        geminiAgent.otherCharactersNames = chatgptAgent.characterName + " : " + chatgptAgent.characterDescription + "," + claudeAgent.characterName + " : " + claudeAgent.characterDescription + "," + deepSeekAgent.characterName + " : " + deepSeekAgent.characterDescription + "," + mistralAgent.characterName + " : " + mistralAgent.characterDescription;
        claudeAgent.otherCharactersNames = chatgptAgent.characterName + " : " + chatgptAgent.characterDescription + "," + geminiAgent.characterName + " : " + geminiAgent.characterDescription + "," + deepSeekAgent.characterName + " : " + deepSeekAgent.characterDescription + "," + mistralAgent.characterName + " : " + mistralAgent.characterDescription;
        deepSeekAgent.otherCharactersNames = chatgptAgent.characterName + " : " + chatgptAgent.characterDescription + "," + geminiAgent.characterName + " : " + geminiAgent.characterDescription + "," + claudeAgent.characterName + " : " + claudeAgent.characterDescription + "," + mistralAgent.characterName + " : " + mistralAgent.characterDescription;
        mistralAgent.otherCharactersNames = chatgptAgent.characterName + " : " + chatgptAgent.characterDescription + "," + geminiAgent.characterName + " : " + geminiAgent.characterDescription + "," + claudeAgent.characterName + " : " + claudeAgent.characterDescription + "," + deepSeekAgent.characterName + " : " + deepSeekAgent.characterDescription;

        // Set topic for all agents
        chatgptAgent.topic = characterCustomization.discussionTopic_Inputfield.text;
        geminiAgent.topic = characterCustomization.discussionTopic_Inputfield.text;
        claudeAgent.topic = characterCustomization.discussionTopic_Inputfield.text;
        deepSeekAgent.topic = characterCustomization.discussionTopic_Inputfield.text;
        mistralAgent.topic = characterCustomization.discussionTopic_Inputfield.text;

        // Cache initial camera position and scale for resetting later
        initialPos = characterCamera.transform.localPosition;
        initialScale = characterCamera.transform.localScale;
    }

    private void Update()
    {
        // Smooth camera rotation when switching between agents
        if (isAnimate)
        {
            if (Quaternion.Angle(characterCamera.transform.rotation, targetRot) < 1.0f)
            {
                characterCamera.transform.rotation = targetRot;
                isAnimate = false;
            }
            characterCamera.gameObject.transform.rotation = Quaternion.Slerp(characterCamera.gameObject.transform.rotation, targetRot, 5.0f * Time.deltaTime);
        }
    }

    /// <summary>
    /// Starts the conversation sequence: initializes variables and calls moderator intro.
    /// </summary>
    public void StartConversation()
    {
        stopConversation = false;
        conversationHistory.Clear();
        isSpeakingStarted = false;
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
            if (chatgptAgent.isPlayer)
                yield return StartCoroutine(PlayerAgentMode(agent));
            else
            {
                moderatorContent = "you are a moderator named " + chatgptAgent.characterName + "  a " +
                chatgptAgent.characterDescription + " and there are other characters named , " +
                geminiAgent.characterName + " " + geminiAgent.characterDescription + ", " +
                deepSeekAgent.characterName + " " + deepSeekAgent.characterDescription + ", " +
                mistralAgent.characterName + " " + mistralAgent.characterDescription + ", " +
                claudeAgent.characterName + " " + claudeAgent.characterDescription + ", " +
                ". generate a introductory dialogue for a conversation between them about" +
                characterCustomization.discussionTopic_Inputfield.text + ". just like the  example\r\n\"Say Happily: Welcome guys and mom, thank you for being here.\"\r\n\"Say doubtfully: let me check the internet and make it sure\"\r\n\"Say sympathathically: so thats the end\"\r\nonly generate a single dialogue for moderator";

                chatgptAgent.GenerateModeratorContent(moderatorContent);
                yield return new WaitUntil(() => chatgptAgent.isCompleted);
                var str = chatgptAgent.characterName + " : " + chatgptAgent.responseMessage;
                conversationHistory.Add(str);
                npcllmtester.chatgptDailogues.Add(str);
                yield return StartCoroutine(WaitUntilAllAudiosArePlayed(agent));
            }
        }
        else if (geminiAgent.isModerator)
        {
            agent = 2;
            if (geminiAgent.isPlayer)
                yield return StartCoroutine(PlayerAgentMode(agent));
            else
            {
                moderatorContent = "you are a moderator named " + geminiAgent.characterName + "  a " +
                geminiAgent.characterDescription + " and there are other characters named , " +
                chatgptAgent.characterName + " " + chatgptAgent.characterDescription + ", " +
                deepSeekAgent.characterName + " " + deepSeekAgent.characterDescription + ", " +
                mistralAgent.characterName + " " + mistralAgent.characterDescription + ", " +
                claudeAgent.characterName + " " + claudeAgent.characterDescription + ", " +
                ". generate a introductory dialogue for a conversation between them about" +
                characterCustomization.discussionTopic_Inputfield.text + ". just like the  example\r\n\"Say Happily: Welcome guys and mom, thank you for being here.\"\r\n\"Say doubtfully: let me check the internet and make it sure\"\r\n\"Say sympathathically: so thats the end\"\r\nonly generate a single dialogue for moderator";

                geminiAgent.GenerateModeratorContent(moderatorContent);
                yield return new WaitUntil(() => geminiAgent.isCompleted);
                var str = geminiAgent.characterName + " : " + geminiAgent.responseMessage;
                conversationHistory.Add(str);
                npcllmtester.geminitDailogues.Add(str);
                yield return StartCoroutine(WaitUntilAllAudiosArePlayed(agent));
            }
        }
        else if (claudeAgent.isModerator)
        {
            agent = 3;
            if (claudeAgent.isPlayer)
                yield return StartCoroutine(PlayerAgentMode(agent));
            else
            {
                moderatorContent = "you are a moderator named " + claudeAgent.characterName + "  a " +
                claudeAgent.characterDescription + " and there are other characters named , " +
                geminiAgent.characterName + " " + geminiAgent.characterDescription + ", " +
                deepSeekAgent.characterName + " " + deepSeekAgent.characterDescription + ", " +
                mistralAgent.characterName + " " + mistralAgent.characterDescription + ", " +
                chatgptAgent.characterName + " " + chatgptAgent.characterDescription + ", " +
                ". generate a introductory dialogue for a conversation between them about" +
                characterCustomization.discussionTopic_Inputfield.text + ". just like the  example\r\n\"Say Happily: Welcome guys and mom, thank you for being here.\"\r\n\"Say doubtfully: let me check the internet and make it sure\"\r\n\"Say sympathathically: so thats the end\"\r\nonly generate a single dialogue for moderator";

                claudeAgent.GenerateModeratorContent(moderatorContent);
                yield return new WaitUntil(() => claudeAgent.isCompleted);
                var str = claudeAgent.characterName + " : " + claudeAgent.responseMessage;
                conversationHistory.Add(str);
                npcllmtester.claudeDailogues.Add(str);
                yield return StartCoroutine(WaitUntilAllAudiosArePlayed(agent));
            }
        }
        else if (deepSeekAgent.isModerator)
        {
            agent = 4;
            if (deepSeekAgent.isPlayer)
                yield return StartCoroutine(PlayerAgentMode(agent));
            else
            {
                moderatorContent = "you are a moderator named " + deepSeekAgent.characterName + "  a " +
                deepSeekAgent.characterDescription + " and there are other characters named , " +
                geminiAgent.characterName + " " + geminiAgent.characterDescription + ", " +
                chatgptAgent.characterName + " " + chatgptAgent.characterDescription + ", " +
                mistralAgent.characterName + " " + mistralAgent.characterDescription + ", " +
                claudeAgent.characterName + " " + claudeAgent.characterDescription + ", " +
                ". generate a introductory dialogue for a conversation between them about" +
                characterCustomization.discussionTopic_Inputfield.text + ". just like the  example\r\n\"Say Happily: Welcome guys and mom, thank you for being here.\"\r\n\"Say doubtfully: let me check the internet and make it sure\"\r\n\"Say sympathathically: so thats the end\"\r\nonly generate a single dialogue for moderator";

                deepSeekAgent.GenerateModeratorContent(moderatorContent);
                yield return new WaitUntil(() => deepSeekAgent.isCompleted);
                var str = deepSeekAgent.characterName + " : " + deepSeekAgent.responseMessage;
                conversationHistory.Add(str);
                npcllmtester.deepseekDailogues.Add(str);
                yield return StartCoroutine(WaitUntilAllAudiosArePlayed(agent));
            }
        }
        else if (mistralAgent.isModerator)
        {
            agent = 5;
            if (mistralAgent.isPlayer)
                yield return StartCoroutine(PlayerAgentMode(agent));
            else
            {
                moderatorContent = "you are a moderator named " + mistralAgent.characterName + "  a " +
                mistralAgent.characterDescription + " and there are other characters named , " +
                geminiAgent.characterName + " " + geminiAgent.characterDescription + ", " +
                deepSeekAgent.characterName + " " + deepSeekAgent.characterDescription + ", " +
                chatgptAgent.characterName + " " + chatgptAgent.characterDescription + ", " +
                claudeAgent.characterName + " " + claudeAgent.characterDescription + ", " +
                ". generate a introductory dialogue for a conversation between them about" +
                characterCustomization.discussionTopic_Inputfield.text + ". just like the  example\r\n\"Say Happily: Welcome guys and mom, thank you for being here.\"\r\n\"Say doubtfully: let me check the internet and make it sure\"\r\n\"Say sympathathically: so thats the end\"\r\nonly generate a single dialogue for moderator";

                mistralAgent.GenerateModeratorContent(moderatorContent);
                yield return new WaitUntil(() => mistralAgent.isCompleted);
                var str = mistralAgent.characterName + " : " + mistralAgent.responseMessage;
                conversationHistory.Add(str);
                npcllmtester.mistralDailogues.Add(str);
                yield return StartCoroutine(WaitUntilAllAudiosArePlayed(agent));
            }
        }

        //Debug.Log("inital end");
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
        while(!stopConversation)
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
                    if(chatgptAgent.isPlayer)
                        yield return StartCoroutine(PlayerAgentMode(agent));
                    else
                    {
                        if (chatgptAgent.isModerator)
                            chatgptAgent.GenerateNextDailogue(conversationHistory, "Ask a question related to topic based on the conversation");
                        else
                            chatgptAgent.GenerateNextDailogue(conversationHistory);

                        yield return new WaitUntil(() => chatgptAgent.isCompleted);
                        str = chatgptAgent.characterName + " : " + chatgptAgent.responseMessage;
                        conversationHistory.Add(str);
                        npcllmtester.chatgptDailogues.Add(str);
                        yield return StartCoroutine(WaitUntilAllAudiosArePlayed(agent));
                    }
                }
                else if (agent == 2) //activate Gemini
                {
                    if (geminiAgent.isPlayer)
                        yield return StartCoroutine(PlayerAgentMode(agent));
                    else
                    {
                        if (geminiAgent.isModerator)
                            geminiAgent.GenerateNextDailogue(conversationHistory, "Ask a question related to topic based on the conversation");
                        else
                            geminiAgent.GenerateNextDailogue(conversationHistory);

                        yield return new WaitUntil(() => geminiAgent.isCompleted);
                        str = geminiAgent.characterName + " : " + geminiAgent.responseMessage;
                        conversationHistory.Add(str);
                        npcllmtester.geminitDailogues.Add(str);
                        yield return StartCoroutine(WaitUntilAllAudiosArePlayed(agent));
                    }
                }
                else if (agent == 3) //activate claude
                {
                    if (claudeAgent.isPlayer)
                        yield return StartCoroutine(PlayerAgentMode(agent));
                    else
                    {
                        if (claudeAgent.isModerator)
                            claudeAgent.GenerateNextDailogue(conversationHistory, "Ask a question related to topic based on the conversation");
                        else
                            claudeAgent.GenerateNextDailogue(conversationHistory);

                        yield return new WaitUntil(() => claudeAgent.isCompleted);
                        str = claudeAgent.characterName + " : " + claudeAgent.responseMessage;
                        conversationHistory.Add(str);
                        npcllmtester.geminitDailogues.Add(str);
                        yield return StartCoroutine(WaitUntilAllAudiosArePlayed(agent));
                    }
                }
                else if (agent == 4) //activate deepseek
                {
                    if (deepSeekAgent.isPlayer)
                        yield return StartCoroutine(PlayerAgentMode(agent));
                    else
                    {
                        if (deepSeekAgent.isModerator)
                            deepSeekAgent.GenerateNextDailogue(conversationHistory, "Ask a question related to topic based on the conversation");
                        else
                            deepSeekAgent.GenerateNextDailogue(conversationHistory);

                        yield return new WaitUntil(() => deepSeekAgent.isCompleted);
                        str = deepSeekAgent.characterName + " : " + deepSeekAgent.responseMessage;
                        conversationHistory.Add(str);
                        npcllmtester.deepseekDailogues.Add(str);
                        yield return StartCoroutine(WaitUntilAllAudiosArePlayed(agent));
                    }
                }
                else if (agent == 5) //activate Mistral
                {
                    if (mistralAgent.isPlayer)
                        yield return StartCoroutine(PlayerAgentMode(agent));
                    else
                    {
                        if (mistralAgent.isModerator)
                            mistralAgent.GenerateNextDailogue(conversationHistory, "Ask a question related to topic based on the conversation");
                        else
                            mistralAgent.GenerateNextDailogue(conversationHistory);

                        yield return new WaitUntil(() => mistralAgent.isCompleted);
                        str = mistralAgent.characterName + " : " + mistralAgent.responseMessage;
                        conversationHistory.Add(str);
                        npcllmtester.mistralDailogues.Add(str);
                        yield return StartCoroutine(WaitUntilAllAudiosArePlayed(agent));
                    }
                }
                
                IncremeantLastSpoken(agent);

                str = str.ToLower();
                if(str.Contains("frustrated") || str.Contains("angrily") || str.Contains ("sadly") || 
                    str.Contains("mockingly") | str.Contains("worried") || str.Contains("worriedly"))
                {
                    Debug.Log("moderator intervene");
                    if (chatgptAgent.isModerator)
                    {
                        agent = 1;
                        if (chatgptAgent.isPlayer)
                            yield return StartCoroutine(PlayerAgentMode(agent));
                        else
                        {
                            chatgptAgent.GenerateNextDailogue(conversationHistory, "generate a dailogue to calm the previous speaker");
                            Debug.Log("generating moderator intervene dialogue");
                            yield return new WaitUntil(() => chatgptAgent.isCompleted);
                            str = chatgptAgent.characterName + " : " + chatgptAgent.responseMessage;
                            conversationHistory.Add(str);
                            npcllmtester.chatgptDailogues.Add(str);
                            yield return StartCoroutine(WaitUntilAllAudiosArePlayed(agent));
                        }
                    }
                    else if (geminiAgent.isModerator)
                    {
                        agent = 2;
                        if (geminiAgent.isPlayer)
                            yield return StartCoroutine(PlayerAgentMode(agent));
                        else
                        {
                            conversationHistory.Add("generate a dailogue to calm the previous speaker");
                            geminiAgent.GenerateNextDailogue(conversationHistory);
                            yield return new WaitUntil(() => geminiAgent.isCompleted);
                            str = geminiAgent.characterName + " : " + geminiAgent.responseMessage;
                            conversationHistory.Add(str);
                            npcllmtester.geminitDailogues.Add(str);
                            yield return StartCoroutine(WaitUntilAllAudiosArePlayed(agent));
                        }
                    }
                    else if (claudeAgent.isModerator)
                    {
                        agent = 3;
                        if (claudeAgent.isPlayer)
                            yield return StartCoroutine(PlayerAgentMode(agent));
                        else
                        {
                            conversationHistory.Add("generate a dailogue to calm the previous speaker");
                            claudeAgent.GenerateNextDailogue(conversationHistory);
                            yield return new WaitUntil(() => claudeAgent.isCompleted);
                            str = claudeAgent.characterName + " : " + claudeAgent.responseMessage;
                            conversationHistory.Add(str);
                            npcllmtester.claudeDailogues.Add(str);
                            yield return StartCoroutine(WaitUntilAllAudiosArePlayed(agent));
                        }
                    }
                    else if (deepSeekAgent.isModerator)
                    {
                        agent = 4;
                        if (deepSeekAgent.isPlayer)
                            yield return StartCoroutine(PlayerAgentMode(agent));
                        else
                        {
                            conversationHistory.Add("generate a dailogue to calm the previous speaker");
                            deepSeekAgent.GenerateNextDailogue(conversationHistory);
                            yield return new WaitUntil(() => deepSeekAgent.isCompleted);
                            str = deepSeekAgent.characterName + " : " + deepSeekAgent.responseMessage;
                            conversationHistory.Add(str);
                            npcllmtester.deepseekDailogues.Add(str);
                            yield return StartCoroutine(WaitUntilAllAudiosArePlayed(agent));
                        }
                    }
                    else if (mistralAgent.isModerator)
                    {
                        agent = 5;
                        if (mistralAgent.isPlayer)
                            yield return StartCoroutine(PlayerAgentMode(agent));
                        else
                        {
                            conversationHistory.Add("generate a dailogue to calm the previous speaker");
                            mistralAgent.GenerateNextDailogue(conversationHistory);
                            yield return new WaitUntil(() => mistralAgent.isCompleted);
                            str = mistralAgent.characterName + " : " + mistralAgent.responseMessage;
                            conversationHistory.Add(str);
                            npcllmtester.mistralDailogues.Add(str);
                            yield return StartCoroutine(WaitUntilAllAudiosArePlayed(agent));
                        }
                    }
                    IncremeantLastSpoken(agent);
                    Debug.LogWarning("moderator intervene completed");
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
                    if (chatgptAgent.isPlayer)
                        yield return StartCoroutine(PlayerAgentMode(speakingagent));
                    else
                    {
                        chatgptAgent.GenerateNextDailogue(conversationHistory);
                        yield return new WaitUntil(() => chatgptAgent.isCompleted);
                        str = chatgptAgent.characterName + " : " + chatgptAgent.responseMessage;
                        conversationHistory.Add(str);
                        npcllmtester.chatgptDailogues.Add(str);
                        yield return StartCoroutine(WaitUntilAllAudiosArePlayed(speakingagent));
                    }
                }
                else if (speakingagent == 2) //activate Gemini
                {
                    if (geminiAgent.isPlayer)
                        yield return StartCoroutine(PlayerAgentMode(speakingagent));
                    else
                    {
                        geminiAgent.GenerateNextDailogue(conversationHistory);
                        yield return new WaitUntil(() => geminiAgent.isCompleted);
                        str = geminiAgent.characterName + " : " + geminiAgent.responseMessage;
                        conversationHistory.Add(str);
                        npcllmtester.geminitDailogues.Add(str);
                        yield return StartCoroutine(WaitUntilAllAudiosArePlayed(speakingagent));
                    }
                }
                else if (speakingagent == 3) //activate claude
                {
                    if (claudeAgent.isPlayer)
                        yield return StartCoroutine(PlayerAgentMode(speakingagent));
                    else
                    {
                        claudeAgent.GenerateNextDailogue(conversationHistory);
                        yield return new WaitUntil(() => claudeAgent.isCompleted);
                        str = claudeAgent.characterName + " : " + claudeAgent.responseMessage;
                        conversationHistory.Add(str);
                        npcllmtester.claudeDailogues.Add(str);
                        yield return StartCoroutine(WaitUntilAllAudiosArePlayed(speakingagent));
                    }
                }
                else if (speakingagent == 4) //activate deepseek
                {
                    if (deepSeekAgent.isPlayer)
                        yield return StartCoroutine(PlayerAgentMode(speakingagent));
                    else
                    {
                        deepSeekAgent.GenerateNextDailogue(conversationHistory);
                        yield return new WaitUntil(() => deepSeekAgent.isCompleted);
                        str = deepSeekAgent.characterName + " : " + deepSeekAgent.responseMessage;
                        conversationHistory.Add(str);
                        npcllmtester.deepseekDailogues.Add(str);
                        yield return StartCoroutine(WaitUntilAllAudiosArePlayed(speakingagent));
                    }
                }
                else if (speakingagent == 5) //activate Mistral
                {
                    if (mistralAgent.isPlayer)
                        yield return StartCoroutine(PlayerAgentMode(speakingagent));
                    else
                    {
                        mistralAgent.GenerateNextDailogue(conversationHistory);
                        yield return new WaitUntil(() => mistralAgent.isCompleted);
                        str = mistralAgent.characterName + " : " + mistralAgent.responseMessage;
                        conversationHistory.Add(str);
                        npcllmtester.mistralDailogues.Add(str);
                        yield return StartCoroutine(WaitUntilAllAudiosArePlayed(speakingagent));
                    }
                }

                if (a.Contains(1) && speakingagent != 1) //activate chatgpt
                {
                    chatgptAgent.GenerateNextDailogue(conversationHistory, "just begin to say a dailogue  but you realized you interrupted another speaker so stop your point and apologize for interrupting another speaker and politely tell that person to continue talking, create very short dialogue, the interrupted person is " + speakingAgentName + ":" + speakingAgentDescription);
                    yield return new WaitUntil(() => chatgptAgent.isCompleted);
                    str = chatgptAgent.characterName + " : " + chatgptAgent.responseMessage;
                    conversationHistory.Add(str);
                    npcllmtester.chatgptDailogues.Add(str);

                    if (chatgptAgent.audioSource.isPlaying)
                        yield return new WaitUntil(() => !chatgptAgent.audioSource.isPlaying);

                    if (geminiAgent.audioSource.isPlaying)
                        geminiAgent.audioSource.Pause();

                    if (claudeAgent.audioSource.isPlaying)
                        claudeAgent.audioSource.Pause();

                    if (deepSeekAgent.audioSource.isPlaying)
                        deepSeekAgent.audioSource.Pause();

                    if (mistralAgent.audioSource.isPlaying)
                        mistralAgent.audioSource.Pause();

                    CameraSettingsAgent(1);
                }
                else if (a.Contains(2) && speakingagent != 2) //activate Gemini
                {
                    geminiAgent.GenerateNextDailogue(conversationHistory, "\"just begin to say a dailogue  but you realized you interrupted another speaker so stop your point and apologize for interrupting another speaker and politely tell that person to continue talking, create very short dialogue, the interrupted person is " + speakingAgentName + ":" + speakingAgentDescription);
                    yield return new WaitUntil(() => geminiAgent.isCompleted);
                    str = geminiAgent.characterName + " : " + geminiAgent.responseMessage;
                    conversationHistory.Add(str);
                    npcllmtester.geminitDailogues.Add(str);

                    if (geminiAgent.audioSource.isPlaying)
                        yield return new WaitUntil(() => !geminiAgent.audioSource.isPlaying);

                    if (chatgptAgent.audioSource.isPlaying)
                        chatgptAgent.audioSource.Pause();

                    if (claudeAgent.audioSource.isPlaying)
                        claudeAgent.audioSource.Pause();

                    if (deepSeekAgent.audioSource.isPlaying)
                        deepSeekAgent.audioSource.Pause();

                    if (mistralAgent.audioSource.isPlaying)
                        mistralAgent.audioSource.Pause();

                    CameraSettingsAgent(2);
                }
                else if (a.Contains(3) && speakingagent != 3) //activate claude
                {
                    claudeAgent.GenerateNextDailogue(conversationHistory, "\"just begin to say a dailogue  but you realized you interrupted another speaker so stop your point and apologize for interrupting another speaker and politely tell that person to continue talking, create very short dialogue, the interrupted person is " + speakingAgentName + ":" + speakingAgentDescription);
                    yield return new WaitUntil(() => claudeAgent.isCompleted);
                    str = claudeAgent.characterName + " : " + claudeAgent.responseMessage;
                    conversationHistory.Add(str);
                    npcllmtester.claudeDailogues.Add(str);

                    if (claudeAgent.audioSource.isPlaying)
                        yield return new WaitUntil(() => !claudeAgent.audioSource.isPlaying);

                    if (chatgptAgent.audioSource.isPlaying)
                        chatgptAgent.audioSource.Pause();

                    if (geminiAgent.audioSource.isPlaying)
                        geminiAgent.audioSource.Pause();

                    if (deepSeekAgent.audioSource.isPlaying)
                        deepSeekAgent.audioSource.Pause();

                    if (mistralAgent.audioSource.isPlaying)
                        mistralAgent.audioSource.Pause();

                    CameraSettingsAgent(3);
                }
                else if (a.Contains(4) && speakingagent != 4) //activate deepseek
                {
                    deepSeekAgent.GenerateNextDailogue(conversationHistory, "\"just begin to say a dailogue  but you realized you interrupted another speaker so stop your point and apologize for interrupting another speaker and politely tell that person to continue talking, create very short dialogue, the interrupted person is " + speakingAgentName + ":" + speakingAgentDescription);
                    yield return new WaitUntil(() => deepSeekAgent.isCompleted);
                    str = deepSeekAgent.characterName + " : " + deepSeekAgent.responseMessage;
                    conversationHistory.Add(str);
                    npcllmtester.deepseekDailogues.Add(str);

                    if (deepSeekAgent.audioSource.isPlaying)
                        yield return new WaitUntil(() => !deepSeekAgent.audioSource.isPlaying);

                    if (chatgptAgent.audioSource.isPlaying)
                        chatgptAgent.audioSource.Pause();

                    if (claudeAgent.audioSource.isPlaying)
                        claudeAgent.audioSource.Pause();

                    if (geminiAgent.audioSource.isPlaying)
                        geminiAgent.audioSource.Pause();

                    if (mistralAgent.audioSource.isPlaying)
                        mistralAgent.audioSource.Pause();

                    CameraSettingsAgent(4);
                }
                else if (a.Contains(5) && speakingagent != 5) //activate Mistral
                {
                    mistralAgent.GenerateNextDailogue(conversationHistory, "\"just begin to say a dailogue  but you realized you interrupted another speaker so stop your point and apologize for interrupting another speaker and politely tell that person to continue talking, create very short dialogue, the interrupted person is " + speakingAgentName + ":" + speakingAgentDescription);
                    yield return new WaitUntil(() => mistralAgent.isCompleted);
                    str = mistralAgent.characterName + " : " + mistralAgent.responseMessage;
                    conversationHistory.Add(str);
                    npcllmtester.mistralDailogues.Add(str);

                    if (mistralAgent.audioSource.isPlaying)
                        yield return new WaitUntil(() => !mistralAgent.audioSource.isPlaying);

                    if (chatgptAgent.audioSource.isPlaying)
                        chatgptAgent.audioSource.Pause();

                    if (claudeAgent.audioSource.isPlaying)
                        claudeAgent.audioSource.Pause();

                    if (deepSeekAgent.audioSource.isPlaying)
                        deepSeekAgent.audioSource.Pause();

                    if (geminiAgent.audioSource.isPlaying)
                        geminiAgent.audioSource.Pause();

                    CameraSettingsAgent(5);
                }

                if (speakingagent == 1) //activate chatgpt
                {
                    if (chatgptAgent.isPlayer)
                        yield return StartCoroutine(PlayerAgentMode(speakingagent));
                    else
                    {
                        chatgptAgent.GenerateNextDailogue(conversationHistory);
                        yield return new WaitUntil(() => chatgptAgent.isCompleted);
                        str = chatgptAgent.characterName + " : " + chatgptAgent.responseMessage;
                        conversationHistory.Add(str);
                        npcllmtester.chatgptDailogues.Add(str);
                        yield return StartCoroutine(WaitUntilAllAudiosArePlayed(speakingagent));
                    }
                }
                else if (speakingagent == 2) //activate Gemini
                {
                    if (geminiAgent.isPlayer)
                        yield return StartCoroutine(PlayerAgentMode(speakingagent));
                    else
                    {
                        geminiAgent.GenerateNextDailogue(conversationHistory);
                        yield return new WaitUntil(() => geminiAgent.isCompleted);
                        str = geminiAgent.characterName + " : " + geminiAgent.responseMessage;
                        conversationHistory.Add(str);
                        npcllmtester.geminitDailogues.Add(str);
                        yield return StartCoroutine(WaitUntilAllAudiosArePlayed(speakingagent));
                    }
                }
                else if (speakingagent == 3) //activate claude
                {
                    if (claudeAgent.isPlayer)
                        yield return StartCoroutine(PlayerAgentMode(speakingagent));
                    else
                    {
                        claudeAgent.GenerateNextDailogue(conversationHistory);
                        yield return new WaitUntil(() => claudeAgent.isCompleted);
                        str = claudeAgent.characterName + " : " + claudeAgent.responseMessage;
                        conversationHistory.Add(str);
                        npcllmtester.claudeDailogues.Add(str);
                        yield return StartCoroutine(WaitUntilAllAudiosArePlayed(speakingagent));
                    }
                }
                else if (speakingagent == 4) //activate deepseek
                {
                    if (deepSeekAgent.isPlayer)
                        yield return StartCoroutine(PlayerAgentMode(speakingagent));
                    else
                    {
                        deepSeekAgent.GenerateNextDailogue(conversationHistory);
                        yield return new WaitUntil(() => deepSeekAgent.isCompleted);
                        str = deepSeekAgent.characterName + " : " + deepSeekAgent.responseMessage;
                        conversationHistory.Add(str);
                        npcllmtester.deepseekDailogues.Add(str);
                        yield return StartCoroutine(WaitUntilAllAudiosArePlayed(speakingagent));
                    }
                }
                else if (speakingagent == 5) //activate Mistral
                {
                    if (mistralAgent.isPlayer)
                        yield return StartCoroutine(PlayerAgentMode(speakingagent));
                    else
                    {
                        mistralAgent.GenerateNextDailogue(conversationHistory);
                        yield return new WaitUntil(() => mistralAgent.isCompleted);
                        str = mistralAgent.characterName + " : " + mistralAgent.responseMessage;
                        conversationHistory.Add(str);
                        npcllmtester.mistralDailogues.Add(str);
                        yield return StartCoroutine(WaitUntilAllAudiosArePlayed(speakingagent));
                    }
                }

                IncremeantLastSpoken(speakingagent);
            }
            currentDailogueCount++;
            if(currentDailogueCount > dailogueCount)
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
            if (chatgptAgent.isPlayer)
            {
                yield return StartCoroutine(PlayerAgentMode(agent));
            }
            else
            {
                moderatorContent = "you are a moderator named " + chatgptAgent.characterName + "  a " +
                chatgptAgent.characterDescription + " and there are other characters named , " +
                geminiAgent.characterName + " " + geminiAgent.characterDescription + ", " +
                deepSeekAgent.characterName + " " + deepSeekAgent.characterDescription + ", " +
                mistralAgent.characterName + " " + mistralAgent.characterDescription + ", " +
                claudeAgent.characterName + " " + claudeAgent.characterDescription + ", " +
                ". generate a conclusion dialogue for the conversation " +
                characterCustomization.discussionTopic_Inputfield.text + ". just like the  example\r\n\"Say Happily: Welcome guys and mom, thank you for being here.\"\r\n\"Say doubtfully: let me check the internet and make it sure\"\r\n\"Say sympathathically: so thats the end\"\r\nthe full dailogue is given above, and create a single dailogue concluding most of the valid points spoken";

                endContent += "\n" + moderatorContent;
                chatgptAgent.GenerateModeratorContent(endContent);
                yield return new WaitUntil(() => chatgptAgent.isCompleted);
                var str = chatgptAgent.characterName + " : " + chatgptAgent.responseMessage;
                conversationHistory.Add(str);
                npcllmtester.chatgptDailogues.Add(str);
                yield return StartCoroutine(WaitUntilAllAudiosArePlayed(1));
            }
            
        }
        else if (geminiAgent.isModerator)
        {
            if (geminiAgent.isPlayer)
            {
                yield return StartCoroutine(PlayerAgentMode(agent));
            }
            else
            {
                moderatorContent = "you are a moderator named " + geminiAgent.characterName + "  a " +
                geminiAgent.characterDescription + " and there are other characters named , " +
                chatgptAgent.characterName + " " + chatgptAgent.characterDescription + ", " +
                deepSeekAgent.characterName + " " + deepSeekAgent.characterDescription + ", " +
                mistralAgent.characterName + " " + mistralAgent.characterDescription + ", " +
                claudeAgent.characterName + " " + claudeAgent.characterDescription + ", " +
                ". generate a conclusion dialogue for the conversation " +
                characterCustomization.discussionTopic_Inputfield.text + ". just like the  example\r\n\"Say Happily: Welcome guys and mom, thank you for being here.\"\r\n\"Say doubtfully: let me check the internet and make it sure\"\r\n\"Say sympathathically: so thats the end\"\r\nthe full dailogue is given above, and create a single dailogue concluding most of the valid points spoken";

                endContent += "\n" + moderatorContent;
                geminiAgent.GenerateModeratorContent(endContent);
                yield return new WaitUntil(() => geminiAgent.isCompleted);
                var str = geminiAgent.characterName + " : " + geminiAgent.responseMessage;
                conversationHistory.Add(str);
                npcllmtester.geminitDailogues.Add(str);
                yield return StartCoroutine(WaitUntilAllAudiosArePlayed(2));
            }
        }
        else if (deepSeekAgent.isModerator)
        {
            if (deepSeekAgent.isPlayer)
            {
                yield return StartCoroutine(PlayerAgentMode(agent));
            }
            else
            {
                moderatorContent = "you are a moderator named " + deepSeekAgent.characterName + "  a " +
                deepSeekAgent.characterDescription + " and there are other characters named , " +
                geminiAgent.characterName + " " + geminiAgent.characterDescription + ", " +
                chatgptAgent.characterName + " " + chatgptAgent.characterDescription + ", " +
                mistralAgent.characterName + " " + mistralAgent.characterDescription + ", " +
                claudeAgent.characterName + " " + claudeAgent.characterDescription + ", " +
                ". generate a conclusion dialogue for the conversation " +
                characterCustomization.discussionTopic_Inputfield.text + ". just like the  example\r\n\"Say Happily: Welcome guys and mom, thank you for being here.\"\r\n\"Say doubtfully: let me check the internet and make it sure\"\r\n\"Say sympathathically: so thats the end\"\r\nthe full dailogue is given above, and create a single dailogue concluding most of the valid points spoken";

                endContent += "\n" + moderatorContent;
                deepSeekAgent.GenerateModeratorContent(endContent);
                yield return new WaitUntil(() => deepSeekAgent.isCompleted);
                var str = deepSeekAgent.characterName + " : " + deepSeekAgent.responseMessage;
                conversationHistory.Add(str);
                npcllmtester.deepseekDailogues.Add(str);
                yield return StartCoroutine(WaitUntilAllAudiosArePlayed(4));
            }
        }
        else if (mistralAgent.isModerator)
        {
            if (mistralAgent.isPlayer)
            {
                yield return StartCoroutine(PlayerAgentMode(agent));
            }
            else
            {
                moderatorContent = "you are a moderator named " + mistralAgent.characterName + "  a " +
                mistralAgent.characterDescription + " and there are other characters named , " +
                geminiAgent.characterName + " " + geminiAgent.characterDescription + ", " +
                deepSeekAgent.characterName + " " + deepSeekAgent.characterDescription + ", " +
                chatgptAgent.characterName + " " + chatgptAgent.characterDescription + ", " +
                claudeAgent.characterName + " " + claudeAgent.characterDescription + ", " +
                ". generate a conclusion dialogue for the conversation " +
                characterCustomization.discussionTopic_Inputfield.text + ". just like the  example\r\n\"Say Happily: Welcome guys and mom, thank you for being here.\"\r\n\"Say doubtfully: let me check the internet and make it sure\"\r\n\"Say sympathathically: so thats the end\"\r\nthe full dailogue is given above, and create a single dailogue concluding most of the valid points spoken";

                endContent += "\n" + moderatorContent;
                mistralAgent.GenerateModeratorContent(endContent);
                yield return new WaitUntil(() => mistralAgent.isCompleted);
                var str = mistralAgent.characterName + " : " + mistralAgent.responseMessage;
                conversationHistory.Add(str);
                npcllmtester.mistralDailogues.Add(str);
                yield return StartCoroutine(WaitUntilAllAudiosArePlayed(5));
            }
        }
        else if (claudeAgent.isModerator)
        {
            if (claudeAgent.isPlayer)
            {
                yield return StartCoroutine(PlayerAgentMode(agent));
            }
            else
            {
                moderatorContent = "you are a moderator named " + claudeAgent.characterName + "  a " +
                claudeAgent.characterDescription + " and there are other characters named , " +
                geminiAgent.characterName + " " + geminiAgent.characterDescription + ", " +
                deepSeekAgent.characterName + " " + deepSeekAgent.characterDescription + ", " +
                mistralAgent.characterName + " " + mistralAgent.characterDescription + ", " +
                chatgptAgent.characterName + " " + chatgptAgent.characterDescription + ", " +
                ". generate a conclusion dialogue for the conversation " +
                characterCustomization.discussionTopic_Inputfield.text + ". just like the  example\r\n\"Say Happily: Welcome guys and mom, thank you for being here.\"\r\n\"Say doubtfully: let me check the internet and make it sure\"\r\n\"Say sympathathically: so thats the end\"\r\nthe full dailogue is given above, and create a single dailogue concluding most of the valid points spoken";

                endContent += "\n" + moderatorContent;
                claudeAgent.GenerateModeratorContent(endContent);
                yield return new WaitUntil(() => claudeAgent.isCompleted);
                var str = claudeAgent.characterName + " : " + claudeAgent.responseMessage;
                conversationHistory.Add(str);
                npcllmtester.claudeDailogues.Add(str);
                yield return StartCoroutine(WaitUntilAllAudiosArePlayed(3));
            }
        }

        yield return StartCoroutine(WaitUntilAllAudiosArePlayed(0));

        subtitleText.text = string.Empty;
        DeactivateAgentComponents(0); // Stops all animations
        characterCamera.gameObject.SetActive(false); // Turns off character camera
        settingsUI.SetActive(true); // Returns to settings UI
        StartEvaluation();
    }

    #region Helper Functions

    /// <summary>
    /// Determines the next speaker based on emotional score, knowledge, and speaking capability.
    /// If multiple agents have the same highest score, triggers an interruption event.
    /// </summary>
    private int FindNextSpeaker()
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

        float score1 = (chatgptAgent.characterKnowledge + chatgptAgent.speakingCapability + chatgptAgent.emotionalScore + chatgptAgent.lastSpoken) ;
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
    /// Waits until all audio sources finish playing before proceeding to next speaker.
    /// </summary>
    private IEnumerator WaitUntilAllAudiosArePlayed(int agent)
    {
        if (chatgptAgent.audioSource.isPlaying)
            yield return new WaitUntil(() => !chatgptAgent.audioSource.isPlaying);

        if (geminiAgent.audioSource.isPlaying)
            yield return new WaitUntil(() => !geminiAgent.audioSource.isPlaying);

        if (claudeAgent.audioSource.isPlaying)
            yield return new WaitUntil(() => !claudeAgent.audioSource.isPlaying);

        if (deepSeekAgent.audioSource.isPlaying)
            yield return new WaitUntil(() => !deepSeekAgent.audioSource.isPlaying);

        if (mistralAgent.audioSource.isPlaying)
            yield return new WaitUntil(() => !mistralAgent.audioSource.isPlaying);

        CameraSettingsAgent(agent);  // Adjusts camera to focus on current agent
    }

    /// <summary>
    /// Enables/disables agent models and sets talking animations.
    /// </summary>
    private void DeactivateAgentComponents(int agent)
    {
        chatgptAgent.capsule.SetActive(agent == 1);
        geminiAgent.capsule.SetActive(agent == 2);
        claudeAgent.capsule.SetActive(agent == 3);
        deepSeekAgent.capsule.SetActive(agent == 4);
        mistralAgent.capsule.SetActive(agent == 5);

        chatgptAgent.animator.SetBool("isTalking", agent == 1);
        geminiAgent.animator.SetBool("isTalking", agent == 2);
        claudeAgent.animator.SetBool("isTalking", agent == 3);
        deepSeekAgent.animator.SetBool("isTalking", agent == 4);
        mistralAgent.animator.SetBool("isTalking", agent == 5);
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
    /// Handles player input via dictation and adds it to conversation history.
    /// </summary>
    private IEnumerator PlayerDictation(int agent)
    {
        startSpeakingButton.gameObject.SetActive(true);
        dictationManager.isStarted = true;
        dictationManager.finalstr = "";
        yield return new WaitUntil(() => !dictationManager.isStarted);
        var str = dictationManager.finalstr;
        Debug.Log("Player Dictation: " + str);
        conversationHistory.Add(str);

        if (agent == 1)
            npcllmtester.chatgptDailogues.Add(str);
        else if (agent == 2)
            npcllmtester.geminitDailogues.Add(str);
        else if (agent == 3)
            npcllmtester.claudeDailogues.Add(str);
        else if (agent == 4)
            npcllmtester.deepseekDailogues.Add(str);
        else if (agent == 5)
            npcllmtester.mistralDailogues.Add(str);
    }

    /// <summary>
    /// Sets camera for player-controlled view.
    /// </summary>
    private void CameraSettingsPlayer(int agent)
    {
        ActivateAllAgents();
        Debug.Log("Agent camera: " + agent);
        if(agent == 1)
        {
            characterCamera.transform.localScale = chatgptAgent.characterPlayerView.transform.localScale;
            characterCamera.transform.localRotation = chatgptAgent.characterPlayerView.transform.localRotation;
            characterCamera.transform.localPosition = chatgptAgent.characterPlayerView.transform.localPosition;
            targetRot = chatgptAgent.characterPlayerView.transform.localRotation;
            chatgptAgent.gameObject.SetActive(false);
        }
        else if (agent == 2)
        {
            characterCamera.transform.localScale = geminiAgent.characterPlayerView.transform.localScale;
            characterCamera.transform.localRotation = geminiAgent.characterPlayerView.transform.localRotation;
            characterCamera.transform.localPosition = geminiAgent.characterPlayerView.transform.localPosition;
            targetRot = geminiAgent.characterPlayerView.transform.localRotation;
            geminiAgent.gameObject.SetActive(false);
        }
        else if (agent == 3)
        {
            characterCamera.transform.localScale = claudeAgent.characterPlayerView.transform.localScale;
            characterCamera.transform.localRotation = claudeAgent.characterPlayerView.transform.localRotation;
            characterCamera.transform.localPosition = claudeAgent.characterPlayerView.transform.localPosition;
            targetRot = claudeAgent.characterPlayerView.transform.localRotation;
            claudeAgent.gameObject.SetActive(false);
        }
        else if(agent==4)
        {
            characterCamera.transform.localScale = deepSeekAgent.characterPlayerView.transform.localScale;
            characterCamera.transform.localRotation = deepSeekAgent.characterPlayerView.transform.localRotation;
            characterCamera.transform.localPosition = deepSeekAgent.characterPlayerView.transform.localPosition;
            targetRot = deepSeekAgent.characterPlayerView.transform.localRotation;
            deepSeekAgent.gameObject.SetActive(false);
        }
        else if(agent == 5)
        {
            characterCamera.transform.localScale = mistralAgent.characterPlayerView.transform.localScale;
            characterCamera.transform.localRotation = mistralAgent.characterPlayerView.transform.localRotation;
            characterCamera.transform.localPosition = mistralAgent.characterPlayerView.transform.localPosition;
            targetRot = mistralAgent.characterPlayerView.transform.localRotation;
            mistralAgent.gameObject.SetActive(false);
        }

        characterCamera.fieldOfView = 83.3f;
        characterCamera.gameObject.SetActive(true);
        isSpeakingStarted = true;
        subtitleText.text = "";
    }

    /// <summary>
    /// Switches to player mode: waits for all audios, sets camera, then waits for player speech.
    /// </summary>
    private IEnumerator PlayerAgentMode(int agent)
    {
        yield return StartCoroutine(WaitUntilAllAudiosArePlayed(agent));
        CameraSettingsPlayer(agent);
        yield return StartCoroutine(PlayerDictation(agent));
    }

    /// <summary>
    /// Resets and rotates camera to face the current speaking agent and plays its audio.
    /// </summary>
    private void CameraSettingsAgent(int agent)
    {
        ActivateAllAgents();
        if (!isSpeakingStarted)
        {
            characterCamera.gameObject.SetActive(true);
            isSpeakingStarted = true;
        }

        characterCamera.transform.localPosition = initialPos;
        characterCamera.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, 0.0f);
        characterCamera.transform.localScale = initialScale;
        characterCamera.fieldOfView = initialFOV;

        if (agent == 1)
        {
            targetRot = Quaternion.Euler(0.0f, chatgptAgent.characterAngle, 0.0f);
            chatgptAgent.audioSource.Play();
            subtitleText.text = chatgptAgent.characterName + ": " + chatgptAgent.responseMessage;
        }
        else if (agent == 2)
        {
            targetRot = Quaternion.Euler(0.0f, geminiAgent.characterAngle, 0.0f);
            geminiAgent.audioSource.Play();
            subtitleText.text = geminiAgent.characterName + ": " + geminiAgent.responseMessage;
        }
        else if (agent == 3)
        {
            targetRot = Quaternion.Euler(0.0f, claudeAgent.characterAngle, 0.0f);
            claudeAgent.audioSource.Play();
            subtitleText.text = claudeAgent.characterName + ": " + claudeAgent.responseMessage;
        }
        else if (agent == 4)
        {
            targetRot = Quaternion.Euler(0.0f, deepSeekAgent.characterAngle, 0.0f);
            deepSeekAgent.audioSource.Play();
            subtitleText.text = deepSeekAgent.characterName + ": " + deepSeekAgent.responseMessage;
        }
        else if (agent == 5)
        {
            targetRot = Quaternion.Euler(0.0f, mistralAgent.characterAngle, 0.0f);
            mistralAgent.audioSource.Play();
            subtitleText.text = mistralAgent.characterName + ": " + mistralAgent.responseMessage;
        }

        isAnimate = true;
        DeactivateAgentComponents(agent);
    }

    /// <summary>
    /// Activate all agents
    /// </summary>
    private void ActivateAllAgents()
    {
        chatgptAgent.gameObject.SetActive(true);
        geminiAgent.gameObject.SetActive(true);
        mistralAgent.gameObject.SetActive(true);
        claudeAgent.gameObject.SetActive(true);
        deepSeekAgent.gameObject.SetActive(true);
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
        npcllmtester.EvaluateDailogues(conversationHistory);
    }

    #endregion
}