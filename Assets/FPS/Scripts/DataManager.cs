using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }

    [Header("Session Info")]
    public string sceneName;
    public string participantID;
    public SelectionMethod selectionMethod;

    public float deadzoneMax;
    public float maxLeanValue;

    private DateTime trialStartTime;
    private bool trialActive = false;

    private List<TrialRecord> trialBuffer = new List<TrialRecord>();
    private List<QuestionnaireData> questionnaireBuffer = new List<QuestionnaireData>();
    private string csvFilePath;
    private string questionnaireFilePath;

    [Serializable]
    private class TrialRecord
    {
        public string sceneName;
        public string participantID;
        public SelectionMethod selectionMethod;
        public DateTime date;
        public DateTime startTime;
        public DateTime finishTime;
        public float trialDurationSeconds;
    }

    [System.Serializable]
    private class QuestionnaireData
    {
        public string participantID = "P1";
        public SelectionMethod technique;
        public int questionID;
        public string question;
        public string response;
        public DateTime timestamp;
    }

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void InitializeSession()
    {
        deadzoneMax = SelectionConfig.Instance.deadzoneValue;
        maxLeanValue = SelectionConfig.Instance.maxLeanDistance;
        
        participantID = SelectionConfig.Instance.participantID;
        selectionMethod = SelectionConfig.Instance.selectionMethod;

        trialBuffer.Clear();
        questionnaireBuffer.Clear();

        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string directory = GetDataDirectory();

        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        csvFilePath = Path.Combine(
            directory,
            $"P{participantID}_{selectionMethod}_{timestamp}.csv"
        );
        questionnaireFilePath = Path.Combine(directory,
            $"P{participantID}_{selectionMethod}_{timestamp}_questionnaire.csv");

        Debug.Log("Session initialized");
    }

    // Call when a trial starts
    public void StartTrial()
    {
        sceneName = SceneManager.GetActiveScene().name;
        trialStartTime = DateTime.Now;
        trialActive = true;
    }

    // Call when a trial finishes
    public void FinishTrial()
    {
        if (!trialActive) return;

        DateTime finishTime = DateTime.Now;
        float durationSeconds = (float)(finishTime - trialStartTime).TotalSeconds;

        trialBuffer.Add(new TrialRecord
        {
            sceneName = sceneName,
            participantID = participantID,
            selectionMethod = selectionMethod,
            date = trialStartTime.Date,
            startTime = trialStartTime,
            finishTime = finishTime,
            trialDurationSeconds = durationSeconds
        });

        trialActive = false;
        WriteCSV();
    }

    void OnApplicationQuit()
    {
        WriteCSV();
        WriteQuestionnaireData();
    }

    public void RecordQuestionnaireResponse(int questionID, string question, string response)
    {
        QuestionnaireData qData = new QuestionnaireData
        {
            participantID = this.participantID,
            technique = this.selectionMethod,
            questionID = questionID,
            question = question,
            response = response,
            timestamp = DateTime.Now
        };

        questionnaireBuffer.Add(qData);

        Debug.Log($"DataManager: Questionnaire response buffered - Q{questionID} (Total buffered: {questionnaireBuffer.Count})");
    }

    private void WriteCSV()
    {
        if (trialBuffer.Count == 0) return;

        StringBuilder csv = new StringBuilder();
        csv.AppendLine("ParticipantID,SelectionMethod,Date,Hour,StartTime,FinishTime,TrialDurationSeconds");

        foreach (var trial in trialBuffer)
        {
            csv.AppendLine(
                $"{trial.sceneName}," +
                $"{trial.participantID}," +
                $"{trial.selectionMethod}," +
                $"{trial.date:yyyy-MM-dd}," +
                $"{trial.startTime:HH:mm:ss}," +
                $"{trial.startTime:HH:mm:ss.fff}," +
                $"{trial.finishTime:HH:mm:ss.fff}" +
                $"{trial.trialDurationSeconds:F3}"
            );
        }

        File.WriteAllText(csvFilePath, csv.ToString());
        Debug.Log($"CSV written to {csvFilePath}");
    }

    public void WriteQuestionnaireData()
    {
        if (questionnaireBuffer.Count == 0)
        {
            Debug.LogWarning("DataManager: No questionnaire data to write");
            return;
        }

        StringBuilder csv = new StringBuilder();

        // Header
        csv.AppendLine("Timestamp,ParticipantID,Technique,QuestionID,Question,Response");

        // Data rows
        foreach (var q in questionnaireBuffer)
        {
            csv.Append($"{q.timestamp:yyyy-MM-dd HH:mm:ss},");
            csv.Append($"{q.participantID},");
            csv.Append($"{q.technique},");
            csv.Append($"{q.questionID},");
            csv.Append($"\"{q.question}\","); // Quotes for safety
            csv.Append($"\"{q.response}\",");

            csv.AppendLine();
        }

        File.WriteAllText(questionnaireFilePath, csv.ToString());

        Debug.Log($"<color=green>DataManager: Questionnaire data written to file ({questionnaireBuffer.Count} responses)</color>");
        Debug.Log($"File path: {questionnaireFilePath}");
    }

    private string GetDataDirectory()
    {
#if UNITY_EDITOR
        return Path.Combine(Application.dataPath, "EditorData");
#else
        return Application.persistentDataPath;
#endif
    }
}
