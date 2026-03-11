using System;
using UnityEngine;
using UnityEngine.SceneManagement;

[Serializable]
public struct HumanJoystickRuntimeSettings
{
    public float MaxTranslationSpeed;
    public float Deadzone;
    public float BodyOffsetForMaxSpeed;
    public float TransferSensitivity;
    public float TransferFactor;
}

public class HumanJoystickRuntimeSettingsManager : MonoBehaviour
{
    private const string TestSetupSceneName = "testSetupScene";
    private static bool s_BootstrapRegistered;

    public static HumanJoystickRuntimeSettingsManager Instance { get; private set; }

    [Header("Fallback Defaults")]
    [SerializeField] private HumanJoystickRuntimeSettings m_FallbackDefaults = new HumanJoystickRuntimeSettings
    {
        MaxTranslationSpeed = 6f,
        Deadzone = 0.01f,
        BodyOffsetForMaxSpeed = 0.3f,
        TransferSensitivity = 1.5f,
        TransferFactor = 1f
    };

    public HumanJoystickRuntimeSettings Defaults => m_Defaults;
    public HumanJoystickRuntimeSettings Current => m_Current;

    private HumanJoystickRuntimeSettings m_Defaults;
    private HumanJoystickRuntimeSettings m_Current;
    private bool m_RuntimeInitialized;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
    private static void RegisterBootstrap()
    {
        if (s_BootstrapRegistered)
            return;

        s_BootstrapRegistered = true;
        SceneManager.sceneLoaded += BootstrapOnSceneLoaded;
    }

    private static void BootstrapOnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name != TestSetupSceneName)
            return;

        EnsureManager(scene);
    }

    private static void EnsureManager(Scene loadedScene)
    {
        if (Instance != null)
        {
            Instance.AttachSettingsMenuUiIfPresent(loadedScene);
            return;
        }

        var managerObject = new GameObject(nameof(HumanJoystickRuntimeSettingsManager));
        var manager = managerObject.AddComponent<HumanJoystickRuntimeSettingsManager>();
        manager.InitializeRuntime(loadedScene);
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
    }

    private void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void InitializeRuntime(Scene initialScene)
    {
        if (m_RuntimeInitialized)
            return;

        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        m_RuntimeInitialized = true;

        if (!TryCaptureDefaultsFromScene(initialScene, out m_Defaults))
        {
            m_Defaults = Clamp(m_FallbackDefaults);
        }
        else
        {
            m_Defaults = Clamp(m_Defaults);
        }

        m_Current = m_Defaults;

        SceneManager.sceneLoaded += OnSceneLoaded;
        ApplySettingsToAllLoadedScenes();
        AttachSettingsMenuUiIfPresent(initialScene);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        ApplySettingsToScene(scene);
        AttachSettingsMenuUiIfPresent(scene);
    }

    public void ApplyFromUI(HumanJoystickRuntimeSettings newSettings)
    {
        m_Current = Clamp(newSettings);
        ApplySettingsToAllLoadedScenes();
    }

    public void ResetToDefaults()
    {
        m_Current = m_Defaults;
        ApplySettingsToAllLoadedScenes();
    }

    private void ApplySettingsToAllLoadedScenes()
    {
        for (int i = 0; i < SceneManager.sceneCount; i++)
        {
            var scene = SceneManager.GetSceneAt(i);
            if (scene.isLoaded)
                ApplySettingsToScene(scene);
        }
    }

    private void ApplySettingsToScene(Scene scene)
    {
        ForEachHumanJoystick(scene, ApplySettingsToJoystick);
    }

    private void ApplySettingsToJoystick(HumanJoystickTranslation joystick)
    {
        joystick._maxTranslationSpeed = m_Current.MaxTranslationSpeed;
        joystick._deadzone = m_Current.Deadzone;
        joystick._bodyOffsetForMaxSpeed = m_Current.BodyOffsetForMaxSpeed;
        joystick._transferSensitivity = m_Current.TransferSensitivity;
        joystick._transferFactor = m_Current.TransferFactor;
    }

    private bool TryCaptureDefaultsFromScene(Scene scene, out HumanJoystickRuntimeSettings defaults)
    {
        defaults = m_FallbackDefaults;
        HumanJoystickTranslation firstJoystick = null;

        ForEachHumanJoystick(scene, joystick =>
        {
            if (firstJoystick == null)
                firstJoystick = joystick;
        });

        if (firstJoystick == null)
            return false;

        defaults = new HumanJoystickRuntimeSettings
        {
            MaxTranslationSpeed = firstJoystick._maxTranslationSpeed,
            Deadzone = firstJoystick._deadzone,
            BodyOffsetForMaxSpeed = firstJoystick._bodyOffsetForMaxSpeed,
            TransferSensitivity = firstJoystick._transferSensitivity,
            TransferFactor = firstJoystick._transferFactor
        };

        return true;
    }

    private static void ForEachHumanJoystick(Scene scene, Action<HumanJoystickTranslation> action)
    {
        if (!scene.isLoaded)
            return;

        var roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            var joysticks = roots[i].GetComponentsInChildren<HumanJoystickTranslation>(true);
            for (int j = 0; j < joysticks.Length; j++)
            {
                action(joysticks[j]);
            }
        }
    }

    private void AttachSettingsMenuUiIfPresent(Scene scene)
    {
        if (scene.name != TestSetupSceneName || !scene.isLoaded)
            return;

        var menu = FindGameObjectByName(scene, "SettingsMenu");
        if (menu == null)
        {
            Debug.LogWarning("HumanJoystickRuntimeSettingsManager: SettingsMenu not found in testSetupScene.");
            return;
        }

        if (menu.GetComponent<TestSetupSettingsMenuUI>() == null)
        {
            menu.AddComponent<TestSetupSettingsMenuUI>();
        }
    }

    private static GameObject FindGameObjectByName(Scene scene, string name)
    {
        var roots = scene.GetRootGameObjects();
        for (int i = 0; i < roots.Length; i++)
        {
            var transforms = roots[i].GetComponentsInChildren<Transform>(true);
            for (int j = 0; j < transforms.Length; j++)
            {
                if (transforms[j].name == name)
                    return transforms[j].gameObject;
            }
        }

        return null;
    }

    private static HumanJoystickRuntimeSettings Clamp(HumanJoystickRuntimeSettings value)
    {
        value.MaxTranslationSpeed = Mathf.Clamp(value.MaxTranslationSpeed, 0.1f, 20f);
        value.Deadzone = Mathf.Clamp(value.Deadzone, 0f, 0.9f);
        value.BodyOffsetForMaxSpeed = Mathf.Clamp(value.BodyOffsetForMaxSpeed, 0.05f, 2f);
        value.TransferSensitivity = Mathf.Clamp(value.TransferSensitivity, 0f, 5f);
        value.TransferFactor = Mathf.Clamp(value.TransferFactor, 0f, 10f);
        return value;
    }
}
