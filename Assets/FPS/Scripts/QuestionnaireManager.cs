using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;


public class QuestionnaireManager : MonoBehaviour
{
    [System.Serializable]
    public class Question
    {
        public int id;
        public string text;
        public Slider responseField; // Or Slider, Toggle, etc.

        [Tooltip("Used only when the slider is NOT wholeNumbers. Example: 0.1 for 0..1.")]
        public float floatStep = 1f;
    }

    [Header("Questionnaire Data")]
    public Question[] questions;

    [Header("Secondary Navigation (Joystick/Keyboard)")]
    [SerializeField] private bool enableSecondaryNavigation = true;

    [Tooltip("Set the exact Up/Down navigation order for the whole questionnaire UI.\n" +
             "Include sliders, dropdowns, toggles, and your Next/Submit button(s) here.")]
    [SerializeField] private Selectable[] navigationOrder;

    [SerializeField] private float axisDeadzone = 0.5f;
    [SerializeField] private float axisRepeatDelay = 0.18f;

    [Header("Old Input Manager bindings")]
    [SerializeField] private string verticalAxis = "Vertical";     // Up/Down
    [SerializeField] private string horizontalAxis = "Horizontal"; // Left/Right
    [SerializeField] private string submitButton = "Submit";       // joystick press / enter
    [SerializeField] private string cancelButton = "Cancel";       // optional (not required)

    private int focusedIndex = 0;
    private float nextRepeatTime = 0f;


    public void FinishQuestionnaire()
    {
        Invoke("SubmitQuestionnaire", 1f);
    }

    void SubmitQuestionnaire()
    {
        foreach (var question in questions)
        {
            string response = question.responseField.value.ToString();
            DataManager.Instance.RecordQuestionnaireResponse(
                question.id, 
                question.text, 
                response
            );
        }

        Debug.Log("Questionnaire submitted!");
        DataManager.Instance.WriteQuestionnaireData();
        // Return to main menu
        StartCoroutine(DelayedLoad(2f));
    }

    private System.Collections.IEnumerator DelayedLoad(float seconds)
    {
        yield return new WaitForSeconds(seconds);
        UnityEngine.SceneManagement.SceneManager.LoadScene("MainMenu");
    }


private void Start()
    {
        if (!enableSecondaryNavigation) return;

        if (EventSystem.current == null)
            Debug.LogWarning("No EventSystem found. UI navigation requires an EventSystem in the scene.");

        FocusSelectable(0);
    }

    private void Update()
    {
        if (!enableSecondaryNavigation) return;
        if (EventSystem.current == null) return;
        if (navigationOrder == null || navigationOrder.Length == 0) return;

        HandleSecondaryNavigation();
    }

    private void HandleSecondaryNavigation()
    {
        // Button press should always work immediately
        if (Input.GetButtonDown(submitButton))
        {
            ActivateFocused();
            return;
        }

        // Optional: you could use Cancel to close dropdowns, etc. (left blank intentionally)
        // if (Input.GetButtonDown(cancelButton)) { ... }

        // Throttle axis repeat (for holding stick/keys)
        if (Time.unscaledTime < nextRepeatTime) return;

        float v = Input.GetAxisRaw(verticalAxis);
        float h = Input.GetAxisRaw(horizontalAxis);

        bool didSomething = false;

        // Up/Down: change focus
        if (v > axisDeadzone)
        {
            MoveFocus(-1);
            didSomething = true;
        }
        else if (v < -axisDeadzone)
        {
            MoveFocus(+1);
            didSomething = true;
        }

        // Left/Right: adjust current control (only if we didn't move focus this frame)
        if (!didSomething)
        {
            if (h > axisDeadzone)
            {
                NudgeFocused(+1f);
                didSomething = true;
            }
            else if (h < -axisDeadzone)
            {
                NudgeFocused(-1f);
                didSomething = true;
            }
        }

        if (didSomething)
            nextRepeatTime = Time.unscaledTime + axisRepeatDelay;
    }

    private void MoveFocus(int delta)
    {
        if (navigationOrder == null || navigationOrder.Length == 0) return;

        int next = Mathf.Clamp(focusedIndex + delta, 0, navigationOrder.Length - 1);
        FocusSelectable(next);
    }

    private void FocusSelectable(int index)
    {
        if (navigationOrder == null || navigationOrder.Length == 0) return;

        focusedIndex = Mathf.Clamp(index, 0, navigationOrder.Length - 1);

        Selectable sel = navigationOrder[focusedIndex];
        if (sel == null) return;

        // Force EventSystem "selection" highlight
        EventSystem.current.SetSelectedGameObject(sel.gameObject);

        // For good measure, also tell the Selectable it was selected
        sel.Select();
    }

    /// <summary>
    /// Left/Right behavior depending on the focused control type.
    /// - Slider: changes by slider.wholeNumbers ? 1 : Question.floatStep (matched by reference), fallback 0.01
    /// - Dropdown/TMP_Dropdown: increments/decrements option
    /// - Toggle: optional toggle on left/right (enabled here)
    /// - Button: no left/right behavior
    /// </summary>
    private void NudgeFocused(float direction)
    {
        Selectable sel = navigationOrder[focusedIndex];
        if (sel == null) return;

        // Slider
        Slider slider = sel.GetComponent<Slider>();
        if (slider != null)
        {
            AdjustSliderUsingQuestionStep(slider, direction);
            return;
        }

        // Unity Dropdown
        Dropdown dd = sel.GetComponent<Dropdown>();
        if (dd != null)
        {
            if (dd.options == null || dd.options.Count == 0) return;
            int step = (int)Mathf.Sign(direction);
            dd.value = Mathf.Clamp(dd.value + step, 0, dd.options.Count - 1);
            dd.RefreshShownValue();
            return;
        }

        // TMP_Dropdown
        TMP_Dropdown tmpDd = sel.GetComponent<TMP_Dropdown>();
        if (tmpDd != null)
        {
            if (tmpDd.options == null || tmpDd.options.Count == 0) return;
            int step = (int)Mathf.Sign(direction);
            tmpDd.value = Mathf.Clamp(tmpDd.value + step, 0, tmpDd.options.Count - 1);
            tmpDd.RefreshShownValue();
            return;
        }

        // Toggle: treat left/right as toggle
        Toggle toggle = sel.GetComponent<Toggle>();
        if (toggle != null)
        {
            toggle.isOn = !toggle.isOn;
            return;
        }
    }

    private void AdjustSliderUsingQuestionStep(Slider slider, float direction)
    {
        if (slider == null) return;

        // Find matching question to get per-slider floatStep (only needed when not wholeNumbers)
        float step = 0.01f; // fallback for non-whole sliders not in questions[]
        bool matchedQuestion = false;

        if (questions != null)
        {
            for (int i = 0; i < questions.Length; i++)
            {
                if (questions[i] != null && questions[i].responseField == slider)
                {
                    matchedQuestion = true;
                    step = Mathf.Max(0.0001f, questions[i].floatStep);
                    break;
                }
            }
        }

        if (slider.wholeNumbers)
        {
            step = 1f;
        }
        else
        {
            // If it wasn't matched to a Question, keep fallback step (0.01)
            // If it was matched, we already set step from Question.floatStep
            if (!matchedQuestion)
                step = 0.01f;
        }

        float newValue = Mathf.Clamp(slider.value + direction * step, slider.minValue, slider.maxValue);
        if (slider.wholeNumbers) newValue = Mathf.Round(newValue);
        slider.value = newValue;
    }

    /// <summary>
    /// Submit/Press behavior depending on focused control type.
    /// - Button: click
    /// - Toggle: toggle
    /// - Dropdown/TMP_Dropdown: open
    /// - Slider: nothing special (use Left/Right)
    /// </summary>
    private void ActivateFocused()
    {
        Selectable sel = navigationOrder[focusedIndex];
        if (sel == null) return;

        // Button
        Button button = sel.GetComponent<Button>();
        if (button != null)
        {
            button.onClick.Invoke();
            return;
        }

        // Toggle
        Toggle toggle = sel.GetComponent<Toggle>();
        if (toggle != null)
        {
            toggle.isOn = !toggle.isOn;
            return;
        }

        // Unity Dropdown
        Dropdown dd = sel.GetComponent<Dropdown>();
        if (dd != null)
        {
            dd.Show();
            return;
        }

        // TMP_Dropdown
        TMP_Dropdown tmpDd = sel.GetComponent<TMP_Dropdown>();
        if (tmpDd != null)
        {
            tmpDd.Show();
            return;
        }

        // Slider: no "activate" behavior needed
    }


}