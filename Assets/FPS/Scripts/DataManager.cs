using UnityEngine;
using System;
using System.IO;
using System.Text;
using System.Collections.Generic;

public class DataManager : MonoBehaviour
{
    public static DataManager Instance { get; private set; }

    [Header("Session Info")]
    public string participantID;
    public SelectionMethod selectionMethod;

    private DateTime trialStartTime;
    private bool trialActive = false;

    private List<TrialRecord> trialBuffer = new List<TrialRecord>();
    private string csvFilePath;

    [Serializable]
    private class TrialRecord
    {
        public string participantID;
        public SelectionMethod selectionMethod;
        public DateTime date;
        public DateTime startTime;
        public DateTime finishTime;
        public float trialDurationSeconds;
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
        participantID = SelectionConfig.Instance.participantID;
        selectionMethod = SelectionConfig.Instance.selectionMethod;

        string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        string directory = GetDataDirectory();

        if (!Directory.Exists(directory))
            Directory.CreateDirectory(directory);

        csvFilePath = Path.Combine(
            directory,
            $"P{participantID}_{selectionMethod}_{timestamp}.csv"
        );

        Debug.Log("Session initialized");
    }

    // Call when a trial starts
    public void StartTrial()
    {
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
    }

    private void WriteCSV()
    {
        if (trialBuffer.Count == 0) return;

        StringBuilder csv = new StringBuilder();
        csv.AppendLine("ParticipantID,SelectionMethod,Date,Hour,StartTime,FinishTime,TrialDurationSeconds");

        foreach (var trial in trialBuffer)
        {
            csv.AppendLine(
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

    private string GetDataDirectory()
    {
#if UNITY_EDITOR
        return Path.Combine(Application.dataPath, "EditorData");
#else
        return Application.persistentDataPath;
#endif
    }
}
