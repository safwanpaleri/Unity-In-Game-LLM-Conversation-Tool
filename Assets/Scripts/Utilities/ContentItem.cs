using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ContentItem : MonoBehaviour
{
    /// <summary>
    /// A ui cache placeholder script.
    /// </summary>
    [Header("Date and Time Texts")]
    public TMP_Text DateText;
    public TMP_Text TimeText;

    [Header("Metrics Score Texts")]
    public TMP_Text CoherenceScoreText;
    public TMP_Text RelevanceScoreText;
    public TMP_Text NaturalnessScoreText;
    public TMP_Text EngagementScoreText;
    public TMP_Text ContextualAccuracyScoreText;

    [Header("LLM Average Response Time Texts")]
    public TMP_Text ChatgptAvgTimeText;
    public TMP_Text GeminiAvgTimeText;
    public TMP_Text ClaudeAvgTimeText;
    public TMP_Text DeepSeekAvgTimeText;
    public TMP_Text MistralAvgTimeText;
}
