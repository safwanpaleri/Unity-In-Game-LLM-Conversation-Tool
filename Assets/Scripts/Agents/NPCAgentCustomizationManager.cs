using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NPCAgentCustomizationManager : MonoBehaviour
{
    [Header("NPCAgentScripts")]
    [SerializeField] private ChatGPTAgent chatgptAgent;
    [SerializeField] private GeminiAgent geminiAgent;
    [SerializeField] private ClaudeAgent claudeAgent;
    [SerializeField] private DeepSeekAgent deepSeekAgent;
    [SerializeField] private MistralAgent mistralAgent;

    private int currentIndex = 0;

    [Header("UI Cache")]
    [SerializeField] private TMP_InputField characterName_Text;
    [SerializeField] private TMP_InputField characterDescription_Text;
    [SerializeField] public TMP_InputField discussionTopic_Inputfield;
    [SerializeField] public Slider characterKnowledge_Slider;
    [SerializeField] public Slider characterSpeakingCapabilities_Slider;
    [SerializeField] public TMP_Dropdown moderatorDropdown;
    [SerializeField] public TMP_Dropdown playerDropdown;

    [Header("Cache")]
    [SerializeField] private Camera characterCamera;
    private bool isAnimate = false;
    private Quaternion targetRot;

    // Update is called once per frame
    void Update()
    {
        // Smoothly rotate the camera towards the target rotation if animation is active
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
    /// public function to change/view agent settings
    /// </summary>
    public void onCharacterSettingsPressed()
    {
        ChangeUIItems();
    }

    /// <summary>
    /// Iterating through each agent settins
    /// </summary>
    public void onNextButtonPressed()
    {
        SaveUIItems();

        if (currentIndex < 4)
            currentIndex++;
        else
            currentIndex = 0;

        ChangeUIItems();
    }

    /// <summary>
    /// Iterating through each agent settins
    /// </summary>
    public void OnPreviousButtonPressed()
    {
        SaveUIItems();

        if (currentIndex < 1)
            currentIndex = 4;
        else
            currentIndex--;

        ChangeUIItems();
    }

    /// <summary>
    /// Updates UI fields based on the currently selected agent.
    /// </summary>
    public void ChangeUIItems()
    {
        if (currentIndex == 0)
        {
            characterName_Text.text = chatgptAgent.characterName;
            characterDescription_Text.text = chatgptAgent.characterDescription;
            characterKnowledge_Slider.value = chatgptAgent.characterKnowledge * 10;
            characterSpeakingCapabilities_Slider.value = chatgptAgent.speakingCapability * 10;
            characterCamera.gameObject.SetActive(true);
            targetRot = Quaternion.Euler(0.0f, chatgptAgent.characterAngle, 0.0f);
            isAnimate = true;
        }
        else if (currentIndex == 1)
        {
            characterName_Text.text = geminiAgent.characterName;
            characterDescription_Text.text = geminiAgent.characterDescription;
            characterKnowledge_Slider.value = geminiAgent.characterKnowledge * 10;
            characterSpeakingCapabilities_Slider.value = geminiAgent.speakingCapability * 10;
            characterCamera.gameObject.SetActive(true);
            targetRot = Quaternion.Euler(0.0f, geminiAgent.characterAngle, 0.0f);
            isAnimate = true;
        }
        else if (currentIndex == 2)
        {
            characterName_Text.text = claudeAgent.characterName;
            characterDescription_Text.text = claudeAgent.characterDescription;
            characterKnowledge_Slider.value = claudeAgent.characterKnowledge * 10f;
            characterSpeakingCapabilities_Slider.value = claudeAgent.speakingCapability * 10f;
            characterCamera.gameObject.SetActive(true);
            targetRot = Quaternion.Euler(0.0f, claudeAgent.characterAngle, 0.0f);
            isAnimate = true;
        }
        else if (currentIndex == 3)
        {
            characterName_Text.text = deepSeekAgent.characterName;
            characterDescription_Text.text = deepSeekAgent.characterDescription;
            characterKnowledge_Slider.value = deepSeekAgent.characterKnowledge * 10f;
            characterSpeakingCapabilities_Slider.value = deepSeekAgent.speakingCapability * 10;
            characterCamera.gameObject.SetActive(true);
            targetRot = Quaternion.Euler(0.0f, deepSeekAgent.characterAngle, 0.0f);
            isAnimate = true;
        }
        else if (currentIndex == 4)
        {
            characterName_Text.text = mistralAgent.characterName;
            characterDescription_Text.text = mistralAgent.characterDescription;
            characterKnowledge_Slider.value = mistralAgent.characterKnowledge * 10;
            characterSpeakingCapabilities_Slider.value = mistralAgent.speakingCapability * 10;
            characterCamera.gameObject.SetActive(true);
            targetRot = Quaternion.Euler(0.0f, mistralAgent.characterAngle, 0.0f);
            isAnimate = true;
        }
    }

    /// <summary>
    /// Saves current UI input values to the selected agent.
    /// </summary>
    public void SaveUIItems()
    {
        if(currentIndex == 0)
        {
            chatgptAgent.characterName = characterName_Text.text;
            chatgptAgent.characterDescription = characterDescription_Text.text;
            chatgptAgent.characterKnowledge = characterKnowledge_Slider.value / 10.0f;
            chatgptAgent.speakingCapability = characterSpeakingCapabilities_Slider.value / 10.0f;
        }
        else if (currentIndex == 1)
        {
            geminiAgent.characterName = characterName_Text.text;
            geminiAgent.characterDescription = characterDescription_Text.text;
            geminiAgent.characterKnowledge = characterKnowledge_Slider.value / 10.0f;
            geminiAgent.speakingCapability = characterSpeakingCapabilities_Slider.value / 10.0f;
        }
        else if (currentIndex == 2)
        {
            claudeAgent.characterName = characterName_Text.text;
            claudeAgent.characterDescription = characterDescription_Text.text;
            claudeAgent.characterKnowledge = characterKnowledge_Slider.value / 10.0f;
            claudeAgent.speakingCapability = characterSpeakingCapabilities_Slider.value / 10.0f;
        }
        else if (currentIndex == 3)
        {
            deepSeekAgent.characterName = characterName_Text.text;
            deepSeekAgent.characterDescription= characterDescription_Text.text;
            deepSeekAgent.characterKnowledge = characterKnowledge_Slider.value / 10.0f;
            deepSeekAgent.speakingCapability = characterSpeakingCapabilities_Slider.value / 10.0f;
        }
        else if ( currentIndex == 4)
        {
            mistralAgent.characterName = characterName_Text.text;
            mistralAgent.characterDescription = characterDescription_Text.text;
            mistralAgent.characterKnowledge = characterKnowledge_Slider.value / 10.0f;
            mistralAgent.speakingCapability = characterSpeakingCapabilities_Slider.value / 10.0f;
        }
    }

    /// <summary>
    /// Changes which agent is the moderator based on dropdown selection.
    /// Ensures only one moderator is active at a time.
    /// </summary>
    public void ChangedModerator()
    {
        if(moderatorDropdown.value == 0)
        {
            chatgptAgent.isModerator = true;
            geminiAgent.isModerator = false;
            deepSeekAgent.isModerator = false;
            mistralAgent.isModerator = false;
            claudeAgent.isModerator = false;
        }
        else if (moderatorDropdown.value == 1)
        {
            chatgptAgent.isModerator = false;
            geminiAgent.isModerator = true;
            deepSeekAgent.isModerator = false;
            mistralAgent.isModerator = false;
            claudeAgent.isModerator = false;
        }
        else if (moderatorDropdown.value == 2)
        {
            chatgptAgent.isModerator = false;
            geminiAgent.isModerator = false;
            deepSeekAgent.isModerator = true;
            mistralAgent.isModerator = false;
            claudeAgent.isModerator = false;
        }
        else if (moderatorDropdown.value == 3)
        {
            chatgptAgent.isModerator = false;
            geminiAgent.isModerator = false;
            deepSeekAgent.isModerator = false;
            mistralAgent.isModerator = true;
            claudeAgent.isModerator = false;
        }
        else if (moderatorDropdown.value == 4)
        {
            chatgptAgent.isModerator = false;
            geminiAgent.isModerator = false;
            deepSeekAgent.isModerator = false;
            mistralAgent.isModerator = false;
            claudeAgent.isModerator = true;
        }
    }

    /// <summary>
    /// Changes which agent is the player based on dropdown selection.
    /// Ensures only one player is active at a time (or none if index 0).
    /// </summary>
    public void ChangedPlayer()
    {
        if(playerDropdown.value == 0)
        {
            chatgptAgent.isPlayer = false;
            geminiAgent.isPlayer = false;
            deepSeekAgent.isPlayer = false;
            mistralAgent.isPlayer = false;
            claudeAgent.isPlayer = false;
        }
        else if (playerDropdown.value == 1)
        {
            chatgptAgent.isPlayer = true;
            geminiAgent.isPlayer = false;
            deepSeekAgent.isPlayer = false;
            mistralAgent.isPlayer = false;
            claudeAgent.isPlayer = false;
        }
        else if (playerDropdown.value == 2)
        {
            chatgptAgent.isPlayer = false;
            geminiAgent.isPlayer = true;
            deepSeekAgent.isPlayer = false;
            mistralAgent.isPlayer = false;
            claudeAgent.isPlayer = false;
        }
        else if (playerDropdown.value == 3)
        {
            chatgptAgent.isPlayer = false;
            geminiAgent.isPlayer = false;
            deepSeekAgent.isPlayer = true;
            mistralAgent.isPlayer = false;
            claudeAgent.isPlayer = false;
        }
        else if (playerDropdown.value == 4)
        {
            chatgptAgent.isPlayer = false;
            geminiAgent.isPlayer = false;
            deepSeekAgent.isPlayer = false;
            mistralAgent.isPlayer = true;
            claudeAgent.isPlayer = false;
        }
        else if (playerDropdown.value == 5)
        {
            chatgptAgent.isPlayer = false;
            geminiAgent.isPlayer = false;
            deepSeekAgent.isPlayer = false;
            mistralAgent.isPlayer = false;
            claudeAgent.isPlayer = true;
        }
    }
}
