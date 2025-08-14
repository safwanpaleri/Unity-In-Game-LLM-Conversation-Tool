using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using static UnityEngine.GraphicsBuffer;

public class CharacterCustomizationManager : MonoBehaviour
{
    [Header("NPCAgentScripts")]
    [SerializeField] private NPCAgentScript[] NPCAgentScripts;
    private int currentIndex = 0;

    [Header("UI Cache")]
    [SerializeField] private TMP_InputField characterName_Text;
    [SerializeField] private TMP_InputField characterDescription_Text;
    [SerializeField] public TMP_InputField discussionTopic_Inputfield;

    [Header("Cache")]
    [SerializeField] private Camera characterCamera;
    private bool isAnimate = false;
    private Quaternion targetRot;
    
    // Update is called once per frame
    void Update()
    {
        //Smoothly rotate camera to currently selected NPC Agent
        if(isAnimate)
        {
            if (Quaternion.Angle(characterCamera.transform.rotation, targetRot) < 1.0f)
            {
                characterCamera.transform.rotation = targetRot;
                isAnimate = false;
            }
            characterCamera.gameObject.transform.rotation = Quaternion.Slerp(characterCamera.gameObject.transform.rotation, targetRot, 5.0f * Time.deltaTime);
        }
    }

    private void OnEnable()
    {
        LoadData();
        characterCamera.gameObject.SetActive(true);
    }

    public void onNextButtonPressed()
    {
        SaveData();

        if (currentIndex < NPCAgentScripts.Length-1)
            currentIndex++;
        else
            currentIndex = 0;

        LoadData();
        
    }

    public void OnPreviousButtonPressed()
    {
        SaveData();

        if (currentIndex < 1)
            currentIndex = NPCAgentScripts.Length - 1;
        else
            currentIndex--;

        LoadData();
    }

    /// <summary>
    /// Saves the data changed into the npc agent script.
    /// </summary>
    private void SaveData()
    {
        NPCAgentScripts[currentIndex].characterName = characterName_Text.text;
        NPCAgentScripts[currentIndex].characterDescription = characterDescription_Text.text;
    }

    /// <summary>
    /// Loads the data of NPC agent in the currentIndex from the list.
    /// Also rotates camera to face currently selected NPC Agent.
    /// </summary>
    private void LoadData()
    {
        characterName_Text.text = NPCAgentScripts[currentIndex].characterName;
        characterDescription_Text.text = NPCAgentScripts[currentIndex].characterDescription;
        targetRot = Quaternion.Euler(0.0f, NPCAgentScripts[currentIndex].characterAngle, 0.0f);
        isAnimate = true;
    }
}