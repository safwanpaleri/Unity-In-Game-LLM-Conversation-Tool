using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class NPCManager : MonoBehaviour
{
    [System.Serializable]
    public class Dailogue
    {
        public string emotion;
        public string dailogue;
    }

    [SerializeField] private GeminiTestManager geminiAPI;

    private string result = string.Empty;

    [Header("Dailogues list")]
    [SerializeField] private List<Dailogue> Remi_dailogues = new List<Dailogue>();
    [SerializeField] private List<Dailogue> Roth_dailogues = new List<Dailogue>();
    [SerializeField] private List<Dailogue> Louise_dailogues = new List<Dailogue>();
    [SerializeField] private List<Dailogue> Josh_dailogues = new List<Dailogue>();

    [Header("NPC Agents")]
    [SerializeField] private NPCAgentScript Remi_NPCAgent;
    [SerializeField] private NPCAgentScript Roth_NPCAgent;
    [SerializeField] private NPCAgentScript Louise_NPCAgent;
    [SerializeField] private NPCAgentScript Josh_NPCAgent;

    [Header("Order")]
    [SerializeField] private List<string> Order = new List<string>();

    [Header("Cache")]
    [SerializeField] private CharacterCustomizationManager characterCustomization;
    [SerializeField] private Camera characterCamera;

    [Header("UI Cache")]
    [SerializeField] private TMP_Text subtitleText;
    [SerializeField] private GameObject settingsUI;
    private string prompt = string.Empty;
    private bool isAnimate = false;
    private Quaternion targetRot;
    private bool isSpeakingStarted = false;


    private void Update()
    {
        // Smoothly rotate the camera toward the target rotation
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
    /// Called to start generating and playing the NPC conversation.
    /// </summary>
    public void StartConversation()
   {
        StartCoroutine(GenerateConversation_Coroutine());
   }

    /// <summary>
    /// Generates conversation using the Gemini API and then processes it.
    /// </summary>
    private IEnumerator GenerateConversation_Coroutine()
    {
        // Build prompt based on agent name and descriptions
        prompt = "Create a complete conversation between 4 persons about " + characterCustomization.discussionTopic_Inputfield.text +
            ", 1st person is " + Remi_NPCAgent.characterName + " " + Remi_NPCAgent.characterDescription +
            ", 2nd person is " + Roth_NPCAgent.characterName + " " + Roth_NPCAgent.characterDescription +
            ", 3rd person is " + Louise_NPCAgent.characterName + " " + Louise_NPCAgent.characterDescription +
            ", 4th person is " + Josh_NPCAgent.characterName + " " + Josh_NPCAgent.characterDescription +
            ". after every dailogue add a ';'. and don't need setting. the coversation should be like the example below\r\n" +
            "\"Remi: Say happily: i like how the weather looks like\"\r\n" +
            "\"Roth: say confused: whats great about it?\"\r\n" +
            "\"Louise: Say happily: yes he is right\"";

        geminiAPI.prompt = prompt;
        geminiAPI.SendMessageToGemini();

        yield return new WaitUntil(() => geminiAPI.isCompleted);

        result = geminiAPI.result;
        Process_Conversation();
    }

    /// <summary>
    /// Processes the generated conversation and organizes dialogues per NPC.
    /// </summary>
    public void Process_Conversation()
    {
        // Assign dialogue to the corresponding NPC list
        var split = result.Split(";");
        foreach (var item in split)
        {
            var split2 = item.Split(':');

            foreach (var item2 in split2)
                Debug.LogWarning(item2);
            Debug.LogWarning(Louise_NPCAgent.characterName);
            if (split2.Length > 2)
            {
                Dailogue dailogue = new Dailogue();
                dailogue.emotion = split2[1];
                dailogue.dailogue = split2[2];
                if (split2[0].Contains(Remi_NPCAgent.characterName))
                    Remi_dailogues.Add(dailogue);

                if (split2[0].Contains(Roth_NPCAgent.characterName))
                    Roth_dailogues.Add(dailogue);

                if (split2[0].Contains(Louise_NPCAgent.characterName))
                    Louise_dailogues.Add(dailogue);
                
                if (split2[0].Contains(Josh_NPCAgent.characterName))
                    Josh_dailogues.Add(dailogue);

                Order.Add(split2[0]);
            }
        }
        Debug.Log("Processing Done");
        StartCoroutine(NPCConversation());
    }

    /// <summary>
    /// Plays the NPC conversation in sequence based on the order.
    /// </summary>
    private IEnumerator NPCConversation()
    {
        // Handle each NPC's dialogue based on the conversation order
        foreach (var item in Order)
        {
            if (item.Contains(Remi_NPCAgent.characterName))
                yield return HandleAgentDialogue(Remi_NPCAgent, Remi_dailogues);

            else if (item.Contains(Roth_NPCAgent.characterName))
                yield return HandleAgentDialogue(Roth_NPCAgent, Roth_dailogues);

            else if (item.Contains(Louise_NPCAgent.characterName))
                yield return HandleAgentDialogue(Louise_NPCAgent, Louise_dailogues);

            else if (item.Contains(Josh_NPCAgent.characterName))
                yield return HandleAgentDialogue(Josh_NPCAgent, Josh_dailogues);
        }

        // Wait for any final audio to finish
        if (Remi_NPCAgent.audioSource != null && Remi_NPCAgent.audioSource.isPlaying)
            yield return new WaitUntil(() => !Remi_NPCAgent.audioSource.isPlaying);

        if (Roth_NPCAgent.audioSource != null && Roth_NPCAgent.audioSource.isPlaying)
            yield return new WaitUntil(() => !Roth_NPCAgent.audioSource.isPlaying);

        if (Louise_NPCAgent.audioSource != null && Louise_NPCAgent.audioSource.isPlaying)
            yield return new WaitUntil(() => !Louise_NPCAgent.audioSource.isPlaying);

        if (Josh_NPCAgent.audioSource != null && Josh_NPCAgent.audioSource.isPlaying)
            yield return new WaitUntil(() => !Josh_NPCAgent.audioSource.isPlaying);


        //After going through all the dailogues
        // Reset subtitles and deactivate all NPC indicators
        subtitleText.text = string.Empty;

        Remi_NPCAgent.capsule.SetActive(false);
        Roth_NPCAgent.capsule.SetActive(false);
        Louise_NPCAgent.capsule.SetActive(false);
        Josh_NPCAgent.capsule.SetActive(false);

        Remi_NPCAgent.animator.SetBool("isTalking", false);
        Roth_NPCAgent.animator.SetBool("isTalking", false);
        Louise_NPCAgent.animator.SetBool("isTalking", false);
        Josh_NPCAgent.animator.SetBool("isTalking", false);

        Remi_dailogues.Clear();
        Josh_dailogues.Clear();
        Louise_dailogues.Clear();
        Roth_dailogues.Clear();
        Order.Clear();

        characterCamera.gameObject.SetActive(false);
        settingsUI.SetActive(true);
    }

    /// <summary>
    /// Handles playing one agent's dialogue, camera rotation, and UI updates.
    /// </summary>
    private IEnumerator HandleAgentDialogue(NPCAgentScript agent, List<Dailogue> agentDialogueList)
    {
        if(agentDialogueList.Count == 0)
            yield break;

        var dailogue = agentDialogueList[0];
        agent.textToSpeak = dailogue.emotion + " : " + dailogue.dailogue;
        agent.TextToSpeech();

        yield return new WaitUntil(() => agent.isCompleted);

        // Ensuring no other audio is playing before continuing
        if (Remi_NPCAgent.audioSource?.isPlaying ?? false)
            yield return new WaitUntil(() => !Remi_NPCAgent.audioSource.isPlaying);

        if (Roth_NPCAgent.audioSource?.isPlaying ?? false)
            yield return new WaitUntil(() => !Roth_NPCAgent.audioSource.isPlaying);

        if (Louise_NPCAgent.audioSource?.isPlaying ?? false)
            yield return new WaitUntil(() => !Louise_NPCAgent.audioSource.isPlaying);

        if (Josh_NPCAgent.audioSource?.isPlaying ?? false)
            yield return new WaitUntil(() => !Josh_NPCAgent.audioSource.isPlaying);

        // Activate camera on first dailogue
        if (!isSpeakingStarted)
        {
            characterCamera.gameObject.SetActive(true);
            isSpeakingStarted = true;
        }

        // Update camera position based on agent.
        targetRot = Quaternion.Euler(0.0f, agent.characterAngle, 0.0f);
        isAnimate = true;

        // Play agent audio and update subtitles
        agent.audioSource.Play();
        subtitleText.text = agent.characterName + ": " + dailogue.dailogue;
        agentDialogueList.RemoveAt(0);

        //stop talking animation of previous speaker
        //and activate talking animation of current speaker
        Remi_NPCAgent.capsule.SetActive(agent == Remi_NPCAgent);
        Roth_NPCAgent.capsule.SetActive(agent == Roth_NPCAgent);
        Louise_NPCAgent.capsule.SetActive(agent == Louise_NPCAgent);
        Josh_NPCAgent.capsule.SetActive(agent == Josh_NPCAgent);

        Remi_NPCAgent.animator.SetBool("isTalking", agent == Remi_NPCAgent);
        Roth_NPCAgent.animator.SetBool("isTalking", agent == Roth_NPCAgent);
        Louise_NPCAgent.animator.SetBool("isTalking", agent == Louise_NPCAgent);
        Josh_NPCAgent.animator.SetBool("isTalking", agent == Josh_NPCAgent);
    }
}
