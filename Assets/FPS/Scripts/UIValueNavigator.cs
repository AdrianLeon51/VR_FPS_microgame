using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIValueNavigator : MonoBehaviour
{
    [Header("Optional References")]
    public Slider slider;
    public TMP_Dropdown dropdown;
    public TMP_Text targetText;

    [Header("Slider Settings")]
    public float sliderStep = 1f;

    [Header("Text Settings")]
    [TextArea]
    public string[] textOptions;
    public int currentTextIndex = 0;

    public void NextValue()
    {
        if (slider != null)
        {
            slider.value = Mathf.Min(slider.value + sliderStep, slider.maxValue);
        }

        if (dropdown != null && dropdown.options.Count > 0)
        {
            if (dropdown.value < dropdown.options.Count - 1)
            {
                dropdown.value++;
                dropdown.RefreshShownValue();
                dropdown.onValueChanged.Invoke(dropdown.value);
            }
        }

        if (targetText != null && textOptions != null && textOptions.Length > 0)
        {
            if (currentTextIndex < textOptions.Length - 1)
            {
                currentTextIndex++;
                targetText.text = textOptions[currentTextIndex];
            }
        }
    }

    public void PreviousValue()
    {
        if (slider != null)
        {
            slider.value = Mathf.Max(slider.value - sliderStep, slider.minValue);
        }

        if (dropdown != null && dropdown.options.Count > 0)
        {
            if (dropdown.value > 0)
            {
                dropdown.value--;
                dropdown.RefreshShownValue();
                dropdown.onValueChanged.Invoke(dropdown.value);
            }
        }

        if (targetText != null && textOptions != null && textOptions.Length > 0)
        {
            if (currentTextIndex > 0)
            {
                currentTextIndex--;
                targetText.text = textOptions[currentTextIndex];
            }
        }
    }

    private void Start()
    {
        if (targetText != null && textOptions != null && textOptions.Length > 0)
        {
            currentTextIndex = Mathf.Clamp(currentTextIndex, 0, textOptions.Length - 1);
            targetText.text = textOptions[currentTextIndex];
        }
    }
}