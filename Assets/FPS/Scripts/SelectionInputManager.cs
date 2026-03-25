using UnityEngine;
using Unity.FPS.Gameplay;
using Oculus.Interaction;


public enum SelectionMethod
{
    Dwell,
    Speech,
    FaceGesture,
    Joystick
}

public class SelectionInputManager : MonoBehaviour
{
    public SelectionMethod selectionMethod;

    public EyeGaze eyeGaze;
    public DwellSelection dwellSelection;
    public VoiceManager speechSelection;
    public FaceGestureSelection gestureSelection;

    public event System.Action<GameObject> OnObjectSelected;

    void Awake()
    {
        selectionMethod = SelectionConfig.Instance.selectionMethod;
        SetMovement();
    }

    private void SetMovement()
    {
        if (selectionMethod != SelectionMethod.Joystick)
        {
            VRPlayerInputHandler.movementMode = VRPlayerInputHandler.MovementMode.HumanJoystick;
        }
        else 
        {
            VRPlayerInputHandler.movementMode = VRPlayerInputHandler.MovementMode.Thumbstick;
        }
    }

    void Start()
    {
        DataManager.Instance.StartTrial();

        Debug.Log("Selection method: " +  selectionMethod);
        // Subscribe to selection events
        dwellSelection.OnDwellSelected += HandleSelectEvent;
        speechSelection.OnSpeechSelected += HandleSelectEvent;
        gestureSelection.OnGestureSelected += HandleSelectEvent;

        //eyeGaze.OnDwellComplete += HandleCenterSelectEvent;


        EnableOnly(selectionMethod);

    }

    public void EnableOnly(SelectionMethod method)
    {
        dwellSelection.enabled = (method == SelectionMethod.Dwell);
        speechSelection.enabled = (method == SelectionMethod.Speech);
        gestureSelection.enabled = (method == SelectionMethod.FaceGesture);
    }


    private void HandleSelectEvent(GameObject target)
    {
        // Check if target is null first
        if (target == null)
        {
            Debug.LogWarning("SelectionInputManager: HandleSelectEvent called with null target");
            VRPlayerInputHandler.fireSucceed = false;
            return;
        }
        Debug.Log("Selec: " + eyeGaze.CurrentGazeTarget);
        // Selection only valid if user is still looking at this object
        if (target == eyeGaze.CurrentGazeTarget)
        {
            Debug.Log("Selection Event performed " + target.name);
            OnObjectSelected?.Invoke(target);
            VRPlayerInputHandler.fireSucceed = true;
        }
        else
        {
            //Selection performed but gaze has moved away - RECORD DATA
            Debug.Log("SelectionInputManager: Selection event ignored, gaze moved away from target.");
            VRPlayerInputHandler.fireSucceed = false;
        }

    }

    private void OnDestroy()
    {
        DataManager.Instance.FinishTrial();
    }
}

