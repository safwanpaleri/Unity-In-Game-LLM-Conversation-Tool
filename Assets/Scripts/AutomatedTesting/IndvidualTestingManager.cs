using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using XCharts.Runtime;

public class IndvidualTestingManager : MonoBehaviour
{
    public enum LLM
    {
        Chatgpt = 0,
        Gemini = 1,
        Claude = 2,
        DeepSeek = 3,
        Mistral = 4
    }

    [Header("CurrentLLMToTest")]
    public LLM currentTestingLLM;

    [Header("Buttons")]
    [SerializeField] Button StartButton;
    [SerializeField] Button StopButton;
    [SerializeField] Button ViewGraphButton;
    [SerializeField] Button ViewPreviousScoreButton;
    [SerializeField] Button CloseTableButton;
    [SerializeField] Button DeleteTableButton;
    [SerializeField] Dropdown llmDropdown;

    [Header("Automated LLM Scripts")]
    [SerializeField] ChatGptAutomatedTesting chatGptAutomated;
    [SerializeField] GeminiAutomatedTesting geminiAutomated;
    [SerializeField] ClaudeAutomatedTesting claudeAutomated;
    [SerializeField] DeepSeekAutomatedTesting deepSeekAutomated;
    [SerializeField] MistralAutomatedTesting mistralAutomated;

    [Header("Table Heading")]
    [SerializeField] TMP_Text Heading1;
    [SerializeField] TMP_Text Heading2;
    [SerializeField] TMP_Text Heading3;
    [SerializeField] TMP_Text Heading4;
    [SerializeField] TMP_Text Heading5;

    [Header("Charts")]
    [SerializeField] BarChart barChart;
    [SerializeField] CandlestickChart candlestickChart;
    private void Start()
    {
        SetLLMForTesting();

    }

    public void onDropdownValueChanged()
    {
        if (llmDropdown.value == 0)
            currentTestingLLM = LLM.Chatgpt;
        else if (llmDropdown.value == 1)
            currentTestingLLM = LLM.Gemini;
        else if (llmDropdown.value == 2)
            currentTestingLLM = LLM.Claude;
        else if (llmDropdown.value == 3)
            currentTestingLLM = LLM.DeepSeek;
        else if (llmDropdown.value == 4)
            currentTestingLLM = LLM.Mistral;

        SetLLMForTesting();
    }

    private void SetLLMForTesting()
    {
        StartButton.onClick.RemoveAllListeners();
        StopButton.onClick.RemoveAllListeners();
        ViewGraphButton.onClick.RemoveAllListeners();
        ViewPreviousScoreButton.onClick.RemoveAllListeners();
        CloseTableButton.onClick.RemoveAllListeners();
        DeleteTableButton.onClick.RemoveAllListeners();

        if (currentTestingLLM == LLM.Chatgpt)
        {
            StartButton.onClick.AddListener(() => chatGptAutomated.StartTest());
            StopButton.onClick.AddListener(() => chatGptAutomated.StopTest());
            ViewGraphButton.onClick.AddListener(() => chatGptAutomated.FetchData());
            ViewPreviousScoreButton.onClick.AddListener(() => chatGptAutomated.ViewTableData());
            CloseTableButton.onClick.AddListener(() => chatGptAutomated.ClearData());
            DeleteTableButton.onClick.AddListener(() => chatGptAutomated.DeleteData());
            Heading1.text = Heading2.text = Heading3.text = Heading4.text = Heading5.text = "ChatGpt";
            barChart.GetChartComponent<Title>().text = "Average Response Time - Chatgpt";
            candlestickChart.GetChartComponent<Title>().text = "Metric Score - Chatgpt";
        }
        if (currentTestingLLM == LLM.Gemini)
        {
            StartButton.onClick.AddListener(() => geminiAutomated.StartTest());
            StopButton.onClick.AddListener(() => geminiAutomated.StopTest());
            ViewGraphButton.onClick.AddListener(() => geminiAutomated.FetchData());
            ViewPreviousScoreButton.onClick.AddListener(() => geminiAutomated.ViewTableData());
            CloseTableButton.onClick.AddListener(() => geminiAutomated.ClearData());
            DeleteTableButton.onClick.AddListener(() => geminiAutomated.DeleteData());
            Heading1.text = Heading2.text = Heading3.text = Heading4.text = Heading5.text = "Gemini";
            barChart.GetChartComponent<Title>().text = "Average Response Time - Gemini";
            candlestickChart.GetChartComponent<Title>().text = "Metric Score - Gemini";
        }
        if (currentTestingLLM == LLM.Claude)
        {
            StartButton.onClick.AddListener(() => claudeAutomated.StartTest());
            StopButton.onClick.AddListener(() => claudeAutomated.StopTest());
            ViewGraphButton.onClick.AddListener(() => claudeAutomated.FetchData());
            ViewPreviousScoreButton.onClick.AddListener(() => claudeAutomated.ViewTableData());
            CloseTableButton.onClick.AddListener(() => claudeAutomated.ClearData());
            DeleteTableButton.onClick.AddListener(() => claudeAutomated.DeleteData());
            Heading1.text = Heading2.text = Heading3.text = Heading4.text = Heading5.text = "Claude";
            barChart.GetChartComponent<Title>().text = "Average Response Time - Claude";
            candlestickChart.GetChartComponent<Title>().text = "Metric Score - Claude";
        }
        if (currentTestingLLM == LLM.DeepSeek)
        {
            StartButton.onClick.AddListener(() => deepSeekAutomated.StartTest());
            StopButton.onClick.AddListener(() => deepSeekAutomated.StopTest());
            ViewGraphButton.onClick.AddListener(() => deepSeekAutomated.FetchData());
            ViewPreviousScoreButton.onClick.AddListener(() => deepSeekAutomated.ViewTableData());
            CloseTableButton.onClick.AddListener(() => deepSeekAutomated.ClearData());
            DeleteTableButton.onClick.AddListener(() => deepSeekAutomated.DeleteData());
            Heading1.text = Heading2.text = Heading3.text = Heading4.text = Heading5.text = "Deepseek";
            barChart.GetChartComponent<Title>().text = "Average Response Time - Deepseek";
            candlestickChart.GetChartComponent<Title>().text = "Metric Score - Deepseek";
        }
        if (currentTestingLLM == LLM.Mistral)
        {
            StartButton.onClick.AddListener(() => mistralAutomated.StartTest());
            StopButton.onClick.AddListener(() => mistralAutomated.StopTest());
            ViewGraphButton.onClick.AddListener(() => mistralAutomated.FetchData());
            ViewPreviousScoreButton.onClick.AddListener(() => mistralAutomated.ViewTableData());
            CloseTableButton.onClick.AddListener(() => mistralAutomated.ClearData());
            DeleteTableButton.onClick.AddListener(() => mistralAutomated.DeleteData());
            Heading1.text = Heading2.text = Heading3.text = Heading4.text = Heading5.text = "Mistral";
            barChart.GetChartComponent<Title>().text = "Average Response Time - Mistral";
            candlestickChart.GetChartComponent<Title>().text = "Metric Score - Mistral";
        }
    }
}
