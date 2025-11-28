using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Oculus.Voice;

public class VoiceManager : MonoBehaviour
{
    public AppVoiceExperience appVoiceExperience;
    public EyeGaze eyeGaze;

    public event System.Action<GameObject> OnSpeechSelected;

    private GameObject target;


    // Update is called once per frame
    void Update()
    {
        target = eyeGaze.CurrentGazeTarget;

        if (target == null)
        {
            appVoiceExperience.Deactivate();
            return;
        }

        if (!appVoiceExperience.Active)
        {
            Debug.Log("Voice activated");
            appVoiceExperience.Activate();
        }
        else
        {
            
            return;
        }
    }

    public void VoiceObjectSelected()
    {
        Debug.Log("Voice Selection Triggered");
        OnSpeechSelected?.Invoke(target);
    }

}
