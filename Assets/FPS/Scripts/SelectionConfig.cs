using Meta.XR.ImmersiveDebugger.UserInterface.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;


public class SelectionConfig : MonoBehaviour
{
    public static SelectionConfig Instance { get; private set; }

    public SelectionMethod selectionMethod = SelectionMethod.Dwell; // default

    public TMP_Dropdown participantIDDropdown = null;

    public string experimentSceneName = "MainScene"; // set this to your scene name
    public string trainSceneName = "SetupScene";

    public string participantID = "P1";

    public float deadzoneValue;
    public float maxLeanDistance;

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

    public void ChooseDwell() => SetAndStart(SelectionMethod.Dwell);
    public void ChooseSpeech() => SetAndStart(SelectionMethod.Speech);
    public void ChooseFaceGesture() => SetAndStart(SelectionMethod.FaceGesture);
    public void ChooseJoystick() => SetAndStart(SelectionMethod.Joystick);

    public void SetParticipantID(string id)
    {
        participantID = id;
        DataManager.Instance.participantID = id;
    }
    public void SetParticipantInfo()
    {
        participantID = participantIDDropdown.options[participantIDDropdown.value].text;
        DataManager.Instance.participantID = participantID;

    }

    void SetAndStart(SelectionMethod method)
    {

        selectionMethod = method;
        DataManager.Instance.InitializeSession();

    }

    public void StartScene()
    {
        SceneManager.LoadScene(experimentSceneName);
    }

    public void StartTrain()
    {
        SceneManager.LoadScene(trainSceneName);
    }

    public void SetLeanSettings()
    {

    }
}
