using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class settingUI : MonoBehaviour
{
    public Slider deadzoneSlider;
    public float headDeadzoneValue = 0.01f;
    public float bodyDeadzoneValue = 0.1f;
    public Slider maxLeanSlider;
    public float headMaxLeanValue = 0.08f;
    public float bodyMaxLeanValue = 0.3f;
    public void SetHeadLean()
    {
        deadzoneSlider.value = headDeadzoneValue;
        maxLeanSlider.value = headMaxLeanValue;
    }

    public void SetBodyLean()
    {
        deadzoneSlider.value = bodyDeadzoneValue;
        maxLeanSlider.value = bodyMaxLeanValue;
    }

    public void SaveSettings()
    {
        SelectionConfig.Instance.deadzoneValue = deadzoneSlider.value;
        SelectionConfig.Instance.maxLeanDistance = maxLeanSlider.value;
    }
}
