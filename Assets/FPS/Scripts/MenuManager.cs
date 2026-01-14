using TMPro;
using UnityEngine;

public class MenuController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Dropdown participantIDDropdown;

    // -------- Participant --------

    public void ApplyParticipantID()
    {
        if (participantIDDropdown == null)
        {
            Debug.LogError("Participant dropdown not assigned.");
            return;
        }

        string id = participantIDDropdown.options[participantIDDropdown.value].text;
        SelectionConfig.Instance.SetParticipantID(id);
    }

    // -------- Selection buttons --------

    public void ChooseDwell() => SelectionConfig.Instance.ChooseDwell();
    public void ChooseSpeech() => SelectionConfig.Instance.ChooseSpeech();
    public void ChooseFaceGesture() => SelectionConfig.Instance.ChooseFaceGesture();
    public void ChooseJoystick() => SelectionConfig.Instance.ChooseJoystick();

    // -------- Scene buttons --------

    public void StartExperiment()
    {
        ApplyParticipantID();
        SelectionConfig.Instance.StartScene();
    }

    public void StartTraining()
    {
        ApplyParticipantID();
        SelectionConfig.Instance.StartTrain();
    }
}
