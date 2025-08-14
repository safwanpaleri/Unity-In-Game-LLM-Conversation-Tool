using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Windows.Speech;

public class DictationManager : MonoBehaviour
{

    private DictationRecognizer m_DictationRecognizer;

    public bool isStarted = false;
    public string tempstr = "";
    public string finalstr = "";

    [SerializeField] private string m_Hypotheses;
    [SerializeField] private string m_Recognitions;
    void Start()
    {
        // Initialize DictationRecognizer
        m_DictationRecognizer = new DictationRecognizer();

        // When recognized text is finalized, append it to m_Recognitions
        m_DictationRecognizer.DictationResult += (text, confidence) =>
        {
            m_Recognitions += text + "\n";
        };

        // When recognized text is finalized, append it to m_Recognitions
        m_DictationRecognizer.DictationHypothesis += (text) =>
        {
            if (text.Contains(m_Hypotheses))
                m_Hypotheses = text;
            else
            {
                finalstr += m_Hypotheses + ".";
                m_Hypotheses = text;
            }
        };

        // When dictation completes, you could add handling here if needed
        m_DictationRecognizer.DictationComplete += (completionCause) =>
        {
            
        };

        // On error, log and restart dictation to maintain continuous listening
        m_DictationRecognizer.DictationError += (error, hresult) =>
        {
            m_DictationRecognizer.Start();
        };

        // On error, log and restart dictation to maintain continuous listening
        m_DictationRecognizer.AutoSilenceTimeoutSeconds = 30.0f;
    }

    /// <summary>
    /// Starts the dictation session and resets final string.
    /// </summary>
    public void StartDictation()
    {
        finalstr = "";
        m_DictationRecognizer.Start();
        isStarted = true;
    }

    /// <summary>
    /// Stops the dictation session and appends any remaining hypothesis to final string.
    /// </summary>
    public void StopDictation()
    {
        m_DictationRecognizer.Stop();
        finalstr += m_Hypotheses;
        isStarted = false;
    }

    /// <summary>
    /// Currently deactivated. (Activate if dictation not working properly to debug).
    /// Coroutine to log the status of the DictationRecognizer every second while running.
    /// </summary>
    private IEnumerator DictationUpdate()
    {
        while (isStarted)
        {
            Debug.LogWarning(m_DictationRecognizer.Status);
            yield return new WaitForSeconds(1f);
        }
    }
}