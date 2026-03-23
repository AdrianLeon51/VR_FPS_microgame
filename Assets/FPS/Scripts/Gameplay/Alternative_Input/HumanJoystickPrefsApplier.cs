using UnityEngine;

[DisallowMultipleComponent]
[RequireComponent(typeof(HumanJoystickTranslation))]
public class HumanJoystickPrefsApplier : MonoBehaviour
{
    [Header("Fallback Defaults (used when no PlayerPrefs saved yet)")]
    [SerializeField] private float m_DefaultMaxTranslationSpeed = 6f;
    [SerializeField] private float m_DefaultDeadzone = 0.01f;
    [SerializeField] private float m_DefaultBodyOffsetForMaxSpeed = 0.3f;
    [SerializeField] private float m_DefaultTransferSensitivity = 1.5f;
    [SerializeField] private float m_DefaultTransferFactor = 1f;

    private HumanJoystickTranslation m_Joystick;

    private void Awake()
    {
        m_Joystick = GetComponent<HumanJoystickTranslation>();
    }

    private void OnEnable()
    {
        ApplyFromPrefs();
    }

    public void ApplyFromPrefs()
    {
        if (m_Joystick == null)
            m_Joystick = GetComponent<HumanJoystickTranslation>();

        if (m_Joystick == null)
            return;

        var defaults = new HumanJoystickSettingsValues
        {
            MaxTranslationSpeed = m_DefaultMaxTranslationSpeed,
            Deadzone = m_DefaultDeadzone,
            BodyOffsetForMaxSpeed = m_DefaultBodyOffsetForMaxSpeed,
            TransferSensitivity = m_DefaultTransferSensitivity,
            TransferFactor = m_DefaultTransferFactor
        };

        var settings = HumanJoystickSettingsPrefs.LoadOrDefaults(defaults);
        ApplyToJoystick(m_Joystick, settings);

        if (m_Joystick.isActiveAndEnabled)
            m_Joystick.CalibrateLeaningKS();
    }

    private static void ApplyToJoystick(HumanJoystickTranslation joystick, HumanJoystickSettingsValues settings)
    {
        joystick._maxTranslationSpeed = settings.MaxTranslationSpeed;
        joystick._deadzone = settings.Deadzone;
        joystick._bodyOffsetForMaxSpeed = settings.BodyOffsetForMaxSpeed;
        joystick._transferSensitivity = settings.TransferSensitivity;
        joystick._transferFactor = settings.TransferFactor;
    }
}
