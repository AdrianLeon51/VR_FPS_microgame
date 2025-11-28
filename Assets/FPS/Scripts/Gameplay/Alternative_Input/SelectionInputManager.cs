using UnityEngine;

public enum SelectionMethod
{
    Dwell,
    Speech,
    FaceGesture
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
        // Read the selection method from the persistent config if available
        //if (SelectionConfig.Instance != null)
        //{
        //    selectionMethod = SelectionConfig.Instance.selectionMethod;
        //}
    }

    void Start()
    {
        // Subscribe to selection events
        dwellSelection.OnDwellSelected += HandleSelectEvent;
        speechSelection.OnSpeechSelected += HandleSelectEvent;
        gestureSelection.OnGestureSelected += HandleSelectEvent;

        eyeGaze.OnDwellComplete += HandleCenterSelectEvent;
        

        EnableOnly(selectionMethod);
        
    }

    public void EnableOnly(SelectionMethod method)
    {
        dwellSelection.enabled = (method == SelectionMethod.Dwell);
        speechSelection.enabled = (method == SelectionMethod.Speech);
        gestureSelection.enabled = (method == SelectionMethod.FaceGesture);
    }

    private void HandleCenterSelectEvent(GameObject target)
    {
        if (target == eyeGaze.CurrentGazeTarget && target.name == "CenterDot")
        {
            OnObjectSelected.Invoke(target);
        }
    }

    private void HandleSelectEvent(GameObject target)
    {
        // Selection only valid if user is still looking at this object
        if (target == eyeGaze.CurrentGazeTarget && target.name != "CenterDot")
            OnObjectSelected?.Invoke(target);
    }
}
