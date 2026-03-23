using UnityEngine;

public struct HumanJoystickSettingsValues
{
    public float MaxTranslationSpeed;
    public float Deadzone;
    public float BodyOffsetForMaxSpeed;
    public float TransferSensitivity;
    public float TransferFactor;
}

public static class HumanJoystickSettingsPrefs
{
    public const string KeyMaxSpeed = "hj.max_translation_speed";
    public const string KeyDeadzone = "hj.deadzone";
    public const string KeyBodyOffsetForMaxSpeed = "hj.body_offset_for_max_speed";
    public const string KeyTransferSensitivity = "hj.transfer_sensitivity";
    public const string KeyTransferFactor = "hj.transfer_factor";
    public const string KeyHasSaved = "hj.has_saved";

    public static HumanJoystickSettingsValues Clamp(HumanJoystickSettingsValues value)
    {
        value.MaxTranslationSpeed = Mathf.Clamp(value.MaxTranslationSpeed, 0.1f, 20f);
        value.Deadzone = Mathf.Clamp(value.Deadzone, 0f, 0.9f);
        value.BodyOffsetForMaxSpeed = Mathf.Clamp(value.BodyOffsetForMaxSpeed, 0.05f, 2f);
        value.TransferSensitivity = Mathf.Clamp(value.TransferSensitivity, 0f, 5f);
        value.TransferFactor = Mathf.Clamp(value.TransferFactor, 0f, 10f);
        return value;
    }

    public static bool HasSavedValues()
    {
        return PlayerPrefs.GetInt(KeyHasSaved, 0) == 1;
    }

    public static HumanJoystickSettingsValues LoadOrDefaults(HumanJoystickSettingsValues defaults)
    {
        if (!HasSavedValues())
            return Clamp(defaults);

        var loaded = new HumanJoystickSettingsValues
        {
            MaxTranslationSpeed = PlayerPrefs.GetFloat(KeyMaxSpeed, defaults.MaxTranslationSpeed),
            Deadzone = PlayerPrefs.GetFloat(KeyDeadzone, defaults.Deadzone),
            BodyOffsetForMaxSpeed = PlayerPrefs.GetFloat(KeyBodyOffsetForMaxSpeed, defaults.BodyOffsetForMaxSpeed),
            TransferSensitivity = PlayerPrefs.GetFloat(KeyTransferSensitivity, defaults.TransferSensitivity),
            TransferFactor = PlayerPrefs.GetFloat(KeyTransferFactor, defaults.TransferFactor)
        };

        return Clamp(loaded);
    }

    public static void Save(HumanJoystickSettingsValues value)
    {
        value = Clamp(value);
        PlayerPrefs.SetFloat(KeyMaxSpeed, value.MaxTranslationSpeed);
        PlayerPrefs.SetFloat(KeyDeadzone, value.Deadzone);
        PlayerPrefs.SetFloat(KeyBodyOffsetForMaxSpeed, value.BodyOffsetForMaxSpeed);
        PlayerPrefs.SetFloat(KeyTransferSensitivity, value.TransferSensitivity);
        PlayerPrefs.SetFloat(KeyTransferFactor, value.TransferFactor);
        PlayerPrefs.SetInt(KeyHasSaved, 1);
        PlayerPrefs.Save();
    }

    public static HumanJoystickSettingsValues ResetToDefaults(HumanJoystickSettingsValues defaults)
    {
        var clampedDefaults = Clamp(defaults);
        Save(clampedDefaults);
        return clampedDefaults;
    }
}
