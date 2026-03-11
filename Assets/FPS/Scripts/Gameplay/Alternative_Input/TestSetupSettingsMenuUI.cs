using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class TestSetupSettingsMenuUI : MonoBehaviour
{
    private const string RuntimePanelName = "RuntimeSettingsPanel";

    private Slider m_MaxSpeedSlider;
    private Slider m_DeadzoneSlider;
    private Slider m_MaxLeanSlider;
    private Slider m_TransferSensitivitySlider;
    private Slider m_TransferFactorSlider;

    private TMP_Text m_MaxSpeedValueText;
    private TMP_Text m_DeadzoneValueText;
    private TMP_Text m_MaxLeanValueText;
    private TMP_Text m_TransferSensitivityValueText;
    private TMP_Text m_TransferFactorValueText;

    private const int MaxSpeedDecimals = 2;
    private const int DeadzoneDecimals = 3;
    private const int MaxLeanDecimals = 3;
    private const int TransferSensitivityDecimals = 2;
    private const int TransferFactorDecimals = 2;

    private bool m_Built;
    private bool m_Refreshing;

    private void Start()
    {
        if (!EnsureUiBuilt())
            return;

        RefreshFromManager();
    }

    private void OnEnable()
    {
        if (!m_Built)
            return;

        RefreshFromManager();
    }

    private bool EnsureUiBuilt()
    {
        if (m_Built)
            return true;

        if (HumanJoystickRuntimeSettingsManager.Instance == null)
            return false;

        var rootRect = GetComponent<RectTransform>();
        if (rootRect == null)
            return false;

        // The scene canvas is very small by default, so we expand it for usable controls.
        rootRect.sizeDelta = new Vector2(700f, 900f);

        RectTransform panelRect = FindOrCreatePanel(rootRect);
        BuildControls(panelRect);
        m_Built = true;
        return true;
    }

    private RectTransform FindOrCreatePanel(RectTransform parent)
    {
        Transform existing = parent.Find(RuntimePanelName);
        if (existing != null)
            return existing as RectTransform;

        var panel = new GameObject(RuntimePanelName, typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup));
        panel.transform.SetParent(parent, false);

        var panelRect = panel.GetComponent<RectTransform>();
        panelRect.anchorMin = new Vector2(0.1f, 0.1f);
        panelRect.anchorMax = new Vector2(0.9f, 0.9f);
        panelRect.offsetMin = Vector2.zero;
        panelRect.offsetMax = Vector2.zero;

        var bg = panel.GetComponent<Image>();
        bg.color = new Color(0.08f, 0.08f, 0.08f, 0.92f);

        var layout = panel.GetComponent<VerticalLayoutGroup>();
        layout.padding = new RectOffset(24, 24, 24, 24);
        layout.spacing = 14f;
        layout.childControlWidth = true;
        layout.childControlHeight = false;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        return panelRect;
    }

    private void BuildControls(RectTransform panel)
    {
        CreateTitle(panel, "Human Joystick Settings");

        CreateSliderRow(panel, "Max Translation Speed", 0.1f, 12f, out m_MaxSpeedSlider, out m_MaxSpeedValueText);
        CreateSliderRow(panel, "Deadzone", 0f, 0.9f, out m_DeadzoneSlider, out m_DeadzoneValueText);
        CreateSliderRow(panel, "Max Lean Distance", 0.05f, 1.5f, out m_MaxLeanSlider, out m_MaxLeanValueText);
        CreateSliderRow(panel, "Transfer Sensitivity", 0f, 5f, out m_TransferSensitivitySlider, out m_TransferSensitivityValueText);
        CreateSliderRow(panel, "Transfer Factor", 0f, 10f, out m_TransferFactorSlider, out m_TransferFactorValueText);

        m_MaxSpeedSlider.onValueChanged.AddListener(_ => OnSliderChanged());
        m_DeadzoneSlider.onValueChanged.AddListener(_ => OnSliderChanged());
        m_MaxLeanSlider.onValueChanged.AddListener(_ => OnSliderChanged());
        m_TransferSensitivitySlider.onValueChanged.AddListener(_ => OnSliderChanged());
        m_TransferFactorSlider.onValueChanged.AddListener(_ => OnSliderChanged());

        CreateButtonRow(panel);
    }

    private void CreateTitle(Transform parent, string text)
    {
        var titleGo = new GameObject("RuntimeSettingsTitle", typeof(RectTransform), typeof(LayoutElement));
        titleGo.transform.SetParent(parent, false);

        var layout = titleGo.GetComponent<LayoutElement>();
        layout.preferredHeight = 56f;

        var title = CreateText(titleGo.transform, "Title", text, 34, TextAlignmentOptions.Center);
        title.color = Color.white;

        var titleRect = title.rectTransform;
        titleRect.anchorMin = Vector2.zero;
        titleRect.anchorMax = Vector2.one;
        titleRect.offsetMin = Vector2.zero;
        titleRect.offsetMax = Vector2.zero;
    }

    private void CreateSliderRow(
        Transform parent,
        string label,
        float min,
        float max,
        out Slider slider,
        out TMP_Text valueText)
    {
        var row = new GameObject(label + " Row", typeof(RectTransform), typeof(LayoutElement), typeof(HorizontalLayoutGroup));
        row.transform.SetParent(parent, false);

        var layoutElement = row.GetComponent<LayoutElement>();
        layoutElement.preferredHeight = 64f;

        var hLayout = row.GetComponent<HorizontalLayoutGroup>();
        hLayout.spacing = 12f;
        hLayout.padding = new RectOffset(8, 8, 4, 4);
        hLayout.childAlignment = TextAnchor.MiddleLeft;
        hLayout.childControlWidth = false;
        hLayout.childForceExpandWidth = false;

        var labelText = CreateText(row.transform, "Label", label, 24, TextAlignmentOptions.Left);
        var labelLayout = labelText.gameObject.AddComponent<LayoutElement>();
        labelLayout.preferredWidth = 280f;
        labelLayout.minWidth = 280f;

        slider = CreateSlider(row.transform);
        slider.minValue = min;
        slider.maxValue = max;
        slider.wholeNumbers = false;
        slider.value = min;

        var sliderLayout = slider.gameObject.AddComponent<LayoutElement>();
        sliderLayout.preferredWidth = 270f;
        sliderLayout.minWidth = 270f;
        sliderLayout.flexibleWidth = 1f;

        valueText = CreateText(row.transform, "Value", "0", 24, TextAlignmentOptions.Right);
        var valueLayout = valueText.gameObject.AddComponent<LayoutElement>();
        valueLayout.preferredWidth = 90f;
        valueLayout.minWidth = 90f;
    }

    private Slider CreateSlider(Transform parent)
    {
        var sliderGo = new GameObject("Slider", typeof(RectTransform), typeof(Slider));
        sliderGo.transform.SetParent(parent, false);

        var sliderRect = sliderGo.GetComponent<RectTransform>();
        sliderRect.sizeDelta = new Vector2(260f, 24f);

        var slider = sliderGo.GetComponent<Slider>();
        slider.direction = Slider.Direction.LeftToRight;

        var background = CreateImage(sliderGo.transform, "Background", new Color(0.22f, 0.22f, 0.22f, 1f));
        Stretch(background.rectTransform);

        var fillArea = new GameObject("Fill Area", typeof(RectTransform));
        fillArea.transform.SetParent(sliderGo.transform, false);
        var fillAreaRect = fillArea.GetComponent<RectTransform>();
        fillAreaRect.anchorMin = new Vector2(0f, 0.25f);
        fillAreaRect.anchorMax = new Vector2(1f, 0.75f);
        fillAreaRect.offsetMin = new Vector2(10f, 0f);
        fillAreaRect.offsetMax = new Vector2(-10f, 0f);

        var fill = CreateImage(fillArea.transform, "Fill", new Color(0.20f, 0.70f, 0.35f, 1f));
        Stretch(fill.rectTransform);

        var handleArea = new GameObject("Handle Slide Area", typeof(RectTransform));
        handleArea.transform.SetParent(sliderGo.transform, false);
        var handleAreaRect = handleArea.GetComponent<RectTransform>();
        handleAreaRect.anchorMin = Vector2.zero;
        handleAreaRect.anchorMax = Vector2.one;
        handleAreaRect.offsetMin = new Vector2(10f, 0f);
        handleAreaRect.offsetMax = new Vector2(-10f, 0f);

        var handle = CreateImage(handleArea.transform, "Handle", new Color(0.92f, 0.92f, 0.92f, 1f));
        handle.rectTransform.sizeDelta = new Vector2(22f, 30f);

        slider.fillRect = fill.rectTransform;
        slider.handleRect = handle.rectTransform;
        slider.targetGraphic = handle;

        return slider;
    }

    private void CreateButtonRow(Transform parent)
    {
        var row = new GameObject("Buttons Row", typeof(RectTransform), typeof(LayoutElement), typeof(HorizontalLayoutGroup));
        row.transform.SetParent(parent, false);

        var rowLayoutElement = row.GetComponent<LayoutElement>();
        rowLayoutElement.preferredHeight = 64f;

        var rowLayout = row.GetComponent<HorizontalLayoutGroup>();
        rowLayout.spacing = 16f;
        rowLayout.childAlignment = TextAnchor.MiddleCenter;
        rowLayout.childControlWidth = false;
        rowLayout.childForceExpandWidth = false;

        var applyButton = CreateButton(row.transform, "Apply", new Color(0.16f, 0.56f, 0.30f, 1f));
        applyButton.onClick.AddListener(ApplySettings);

        var resetButton = CreateButton(row.transform, "Reset", new Color(0.48f, 0.17f, 0.17f, 1f));
        resetButton.onClick.AddListener(ResetSettings);
    }

    private Button CreateButton(Transform parent, string label, Color color)
    {
        var buttonGo = new GameObject(label + " Button", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
        buttonGo.transform.SetParent(parent, false);

        var layout = buttonGo.GetComponent<LayoutElement>();
        layout.preferredWidth = 150f;
        layout.preferredHeight = 52f;

        var image = buttonGo.GetComponent<Image>();
        image.color = color;

        var button = buttonGo.GetComponent<Button>();
        button.targetGraphic = image;

        var text = CreateText(buttonGo.transform, "Label", label, 24, TextAlignmentOptions.Center);
        text.color = Color.white;
        Stretch(text.rectTransform);

        return button;
    }

    private void OnSliderChanged()
    {
        if (m_Refreshing)
            return;

        UpdateValueText(m_MaxSpeedValueText, m_MaxSpeedSlider.value, MaxSpeedDecimals);
        UpdateValueText(m_DeadzoneValueText, m_DeadzoneSlider.value, DeadzoneDecimals);
        UpdateValueText(m_MaxLeanValueText, m_MaxLeanSlider.value, MaxLeanDecimals);
        UpdateValueText(m_TransferSensitivityValueText, m_TransferSensitivitySlider.value, TransferSensitivityDecimals);
        UpdateValueText(m_TransferFactorValueText, m_TransferFactorSlider.value, TransferFactorDecimals);
    }

    private void ApplySettings()
    {
        var manager = HumanJoystickRuntimeSettingsManager.Instance;
        if (manager == null)
            return;

        var newSettings = manager.Current;
        newSettings.MaxTranslationSpeed = m_MaxSpeedSlider.value;
        newSettings.Deadzone = m_DeadzoneSlider.value;
        newSettings.BodyOffsetForMaxSpeed = m_MaxLeanSlider.value;
        newSettings.TransferSensitivity = m_TransferSensitivitySlider.value;
        newSettings.TransferFactor = m_TransferFactorSlider.value;

        manager.ApplyFromUI(newSettings);
        RefreshFromManager();
    }

    private void ResetSettings()
    {
        var manager = HumanJoystickRuntimeSettingsManager.Instance;
        if (manager == null)
            return;

        manager.ResetToDefaults();
        RefreshFromManager();
    }

    private void RefreshFromManager()
    {
        var manager = HumanJoystickRuntimeSettingsManager.Instance;
        if (manager == null)
            return;

        m_Refreshing = true;

        var settings = manager.Current;
        m_MaxSpeedSlider.value = settings.MaxTranslationSpeed;
        m_DeadzoneSlider.value = settings.Deadzone;
        m_MaxLeanSlider.value = settings.BodyOffsetForMaxSpeed;
        m_TransferSensitivitySlider.value = settings.TransferSensitivity;
        m_TransferFactorSlider.value = settings.TransferFactor;

        UpdateValueText(m_MaxSpeedValueText, settings.MaxTranslationSpeed, MaxSpeedDecimals);
        UpdateValueText(m_DeadzoneValueText, settings.Deadzone, DeadzoneDecimals);
        UpdateValueText(m_MaxLeanValueText, settings.BodyOffsetForMaxSpeed, MaxLeanDecimals);
        UpdateValueText(m_TransferSensitivityValueText, settings.TransferSensitivity, TransferSensitivityDecimals);
        UpdateValueText(m_TransferFactorValueText, settings.TransferFactor, TransferFactorDecimals);

        m_Refreshing = false;
    }

    private static void UpdateValueText(TMP_Text text, float value, int decimals)
    {
        if (text == null)
            return;

        text.text = value.ToString("F" + decimals);
    }

    private static Image CreateImage(Transform parent, string name, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var image = go.GetComponent<Image>();
        image.color = color;
        return image;
    }

    private static TextMeshProUGUI CreateText(
        Transform parent,
        string name,
        string value,
        float size,
        TextAlignmentOptions alignment)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        go.transform.SetParent(parent, false);

        var text = go.GetComponent<TextMeshProUGUI>();
        text.text = value;
        text.fontSize = size;
        text.alignment = alignment;
        text.color = Color.white;
        text.enableAutoSizing = false;

        var defaultFont = TMP_Settings.defaultFontAsset;
        if (defaultFont != null)
            text.font = defaultFont;

        return text;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
