using TMPro;
using Unity.FPS.Gameplay;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class TestSetupSettingsMenuUI : MonoBehaviour
{
    [Header("Sliders")]
    [SerializeField] private Slider m_MaxSpeedSlider;
    [SerializeField] private Slider m_DeadzoneSlider;
    [SerializeField] private Slider m_MaxLeanSlider;
    [SerializeField] private Slider m_TransferSensitivitySlider;
    [SerializeField] private Slider m_TransferFactorSlider;

    [Header("Value Labels")]
    [SerializeField] private TMP_Text m_MaxSpeedValueText;
    [SerializeField] private TMP_Text m_DeadzoneValueText;
    [SerializeField] private TMP_Text m_MaxLeanValueText;
    [SerializeField] private TMP_Text m_TransferSensitivityValueText;
    [SerializeField] private TMP_Text m_TransferFactorValueText;

    [Header("Buttons")]
    [SerializeField] private Button m_ApplyButton;
    [SerializeField] private Button m_ResetButton;

    [Header("Authoritative Defaults")]
    [SerializeField] private float m_DefaultMaxTranslationSpeed = 6f;
    [SerializeField] private float m_DefaultDeadzone = 0.01f;
    [SerializeField] private float m_DefaultBodyOffsetForMaxSpeed = 0.3f;
    [SerializeField] private float m_DefaultTransferSensitivity = 1.5f;
    [SerializeField] private float m_DefaultTransferFactor = 1f;

    private const int MaxSpeedDecimals = 2;
    private const int DeadzoneDecimals = 3;
    private const int MaxLeanDecimals = 3;
    private const int TransferSensitivityDecimals = 2;
    private const int TransferFactorDecimals = 2;

    private bool m_Refreshing;
    private bool m_ListenersBound;

    private void OnEnable()
    {
        BindListeners();
        RefreshFromManager();
    }

    private void OnDisable()
    {
        UnbindListeners();
    }

    public void OnSliderChanged()
    {
        if (m_Refreshing)
            return;

        UpdateAllValueTexts();
    }

    public void ApplySettings()
    {
        
        if (!HasValidReferences())
            return;
        Debug.Log("TestSetupSettingsMenuUI ApplySettings called.", this);
        var newSettings = new HumanJoystickSettingsValues
        {
            //MaxTranslationSpeed = m_MaxSpeedSlider.value,
            //Deadzone = m_DeadzoneSlider.value,
            BodyOffsetForMaxSpeed = m_MaxLeanSlider.value,
            //TransferSensitivity = m_TransferSensitivitySlider.value,
            //TransferFactor = m_TransferFactorSlider.value
            
        };
        Debug.Log("TestSetupSettingsMenuUI after newSettings",this);
        var appliedSettings = HumanJoystickSettingsPrefs.Clamp(newSettings);
        Debug.Log(
            //$"TestSetupSettingsMenuUI ApplySettings -> MaxTranslationSpeed={appliedSettings.MaxTranslationSpeed:F3}, " +
            //$"Deadzone={appliedSettings.Deadzone:F3}, " +
            $"BodyOffsetForMaxSpeed={appliedSettings.BodyOffsetForMaxSpeed:F3}", 
            //$"TransferSensitivity={appliedSettings.TransferSensitivity:F3}, " +
            //$"TransferFactor={appliedSettings.TransferFactor:F3}",
            this
        );
        Debug.Log("TestSetupSettingsMenuUI after appliedSettings",this);

        HumanJoystickSettingsPrefs.Save(appliedSettings);
        ApplySettingsToActiveJoystick(appliedSettings);
        RefreshFromManager();
        Debug.Log("TestSetupSettingsMenuUI after refreshFromManager",this);
    }

    public void ResetSettings()
    {
        var resetSettings = HumanJoystickSettingsPrefs.ResetToDefaults(GetDefaults());
        ApplySettingsToActiveJoystick(resetSettings);
        RefreshFromManager();
    }

    private void RefreshFromManager()
    {
        if (!HasValidReferences())
            return;

        m_Refreshing = true;

        var settings = HumanJoystickSettingsPrefs.LoadOrDefaults(GetDefaults());
        //m_MaxSpeedSlider.value = settings.MaxTranslationSpeed;
        //m_DeadzoneSlider.value = settings.Deadzone;
        m_MaxLeanSlider.value = settings.BodyOffsetForMaxSpeed;
        //m_TransferSensitivitySlider.value = settings.TransferSensitivity;
        //m_TransferFactorSlider.value = settings.TransferFactor;

        UpdateAllValueTexts();
        m_Refreshing = false;
    }

    private void UpdateAllValueTexts()
    {
        //UpdateValueText(m_MaxSpeedValueText, m_MaxSpeedSlider != null ? m_MaxSpeedSlider.value : 0f, MaxSpeedDecimals);
        //UpdateValueText(m_DeadzoneValueText, m_DeadzoneSlider != null ? m_DeadzoneSlider.value : 0f, DeadzoneDecimals);
        UpdateValueText(m_MaxLeanValueText, m_MaxLeanSlider != null ? m_MaxLeanSlider.value : 0f, MaxLeanDecimals);
        //UpdateValueText(m_TransferSensitivityValueText, m_TransferSensitivitySlider != null ? m_TransferSensitivitySlider.value : 0f, TransferSensitivityDecimals);
        //UpdateValueText(m_TransferFactorValueText, m_TransferFactorSlider != null ? m_TransferFactorSlider.value : 0f, TransferFactorDecimals);
    }

    private void BindListeners()
    {
        if (m_ListenersBound)
            return;

        if (m_MaxSpeedSlider != null) m_MaxSpeedSlider.onValueChanged.AddListener(OnAnySliderChanged);
        if (m_DeadzoneSlider != null) m_DeadzoneSlider.onValueChanged.AddListener(OnAnySliderChanged);
        if (m_MaxLeanSlider != null) m_MaxLeanSlider.onValueChanged.AddListener(OnAnySliderChanged);
        if (m_TransferSensitivitySlider != null) m_TransferSensitivitySlider.onValueChanged.AddListener(OnAnySliderChanged);
        if (m_TransferFactorSlider != null) m_TransferFactorSlider.onValueChanged.AddListener(OnAnySliderChanged);

        if (m_ApplyButton != null) m_ApplyButton.onClick.AddListener(ApplySettings);
        if (m_ResetButton != null) m_ResetButton.onClick.AddListener(ResetSettings);

        m_ListenersBound = true;
    }

    private void UnbindListeners()
    {
        if (!m_ListenersBound)
            return;

        if (m_MaxSpeedSlider != null) m_MaxSpeedSlider.onValueChanged.RemoveListener(OnAnySliderChanged);
        if (m_DeadzoneSlider != null) m_DeadzoneSlider.onValueChanged.RemoveListener(OnAnySliderChanged);
        if (m_MaxLeanSlider != null) m_MaxLeanSlider.onValueChanged.RemoveListener(OnAnySliderChanged);
        if (m_TransferSensitivitySlider != null) m_TransferSensitivitySlider.onValueChanged.RemoveListener(OnAnySliderChanged);
        if (m_TransferFactorSlider != null) m_TransferFactorSlider.onValueChanged.RemoveListener(OnAnySliderChanged);

        if (m_ApplyButton != null) m_ApplyButton.onClick.RemoveListener(ApplySettings);
        if (m_ResetButton != null) m_ResetButton.onClick.RemoveListener(ResetSettings);

        m_ListenersBound = false;
    }

    private void OnAnySliderChanged(float _)
    {
        OnSliderChanged();
    }

    private HumanJoystickSettingsValues GetDefaults()
    {
        return HumanJoystickSettingsPrefs.Clamp(new HumanJoystickSettingsValues
        {
            MaxTranslationSpeed = m_DefaultMaxTranslationSpeed,
            Deadzone = m_DefaultDeadzone,
            BodyOffsetForMaxSpeed = m_DefaultBodyOffsetForMaxSpeed,
            TransferSensitivity = m_DefaultTransferSensitivity,
            TransferFactor = m_DefaultTransferFactor
        });
    }

    private void ApplySettingsToActiveJoystick(HumanJoystickSettingsValues settings)
    {
        if (!TryResolveActiveJoystick(out var joystick))
        {
            Debug.LogWarning("TestSetupSettingsMenuUI: No active joystick found to apply settings immediately.");
            return;
        }

        //joystick._maxTranslationSpeed = settings.MaxTranslationSpeed;
        //joystick._deadzone = settings.Deadzone;
        joystick._bodyOffsetForMaxSpeed = settings.BodyOffsetForMaxSpeed;
        //joystick._transferSensitivity = settings.TransferSensitivity;
        //joystick._transferFactor = settings.TransferFactor;
        joystick.CalibrateLeaningKS();

        Debug.Log("at joystick: " +
            //$"TestSetupSettingsMenuUI ApplySettingsToActiveJoystick -> MaxTranslationSpeed={settings.MaxTranslationSpeed:F3}, " +
            //$"Deadzone={settings.Deadzone:F3}, " +
            $"BodyOffsetForMaxSpeed={settings.BodyOffsetForMaxSpeed:F3}" ,
            //$"TransferSensitivity={settings.TransferSensitivity:F3}, " +
            //$"TransferFactor={settings.TransferFactor:F3}",
            joystick
        );
    }

    private bool TryResolveActiveJoystick(out HumanJoystickTranslation joystick)
    {
        joystick = null;

        // Preferred source: active VRPlayerInputHandler.humanJoystick reference.
        var handlers = FindObjectsOfType<VRPlayerInputHandler>(true);
        for (int i = 0; i < handlers.Length; i++)
        {
            var handler = handlers[i];
            if (!handler.isActiveAndEnabled)
                continue;

            if (handler.humanJoystick != null && handler.humanJoystick.isActiveAndEnabled)
            {
                joystick = handler.humanJoystick;
                return true;
            }
        }

        // Fallback: first active and enabled joystick in loaded scene objects.
        var joysticks = FindObjectsOfType<HumanJoystickTranslation>(true);
        for (int i = 0; i < joysticks.Length; i++)
        {
            if (joysticks[i].isActiveAndEnabled)
            {
                joystick = joysticks[i];
                return true;
            }
        }

        return false;
    }

    private bool HasValidReferences()
    {
        bool ok = true;
        //ok &= Require(m_MaxSpeedSlider, nameof(m_MaxSpeedSlider));
        //ok &= Require(m_DeadzoneSlider, nameof(m_DeadzoneSlider));
        ok &= Require(m_MaxLeanSlider, nameof(m_MaxLeanSlider));
        //ok &= Require(m_TransferSensitivitySlider, nameof(m_TransferSensitivitySlider));
        //ok &= Require(m_TransferFactorSlider, nameof(m_TransferFactorSlider));
        //ok &= Require(m_MaxSpeedValueText, nameof(m_MaxSpeedValueText));
        //ok &= Require(m_DeadzoneValueText, nameof(m_DeadzoneValueText));
        ok &= Require(m_MaxLeanValueText, nameof(m_MaxLeanValueText));
        //ok &= Require(m_TransferSensitivityValueText, nameof(m_TransferSensitivityValueText));
        //ok &= Require(m_TransferFactorValueText, nameof(m_TransferFactorValueText));
        return ok;
    }

    private bool Require(Object reference, string fieldName)
    {
        if (reference != null)
            return true;

        Debug.LogError($"TestSetupSettingsMenuUI: Missing reference '{fieldName}' on {name}.", this);
        return false;
    }

    private static void UpdateValueText(TMP_Text text, float value, int decimals)
    {
        if (text == null)
            return;

        text.text = value.ToString("F" + decimals);
    }
}
