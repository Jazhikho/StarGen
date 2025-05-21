using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;

public class GenerationParametersDialog : MonoBehaviour
{
    [Header("UI References - Sliders")]
    [SerializeField] private Slider sectorXSlider;
    [SerializeField] private Slider sectorYSlider;
    [SerializeField] private Slider sectorZSlider;
    [SerializeField] private Slider sectorSizeSlider;
    [SerializeField] private Slider starDensitySlider;
    [SerializeField] private Slider anomalyChanceSlider;

    [Header("UI References - Text Display")]
    [SerializeField] private TMP_Text sectorXValue;
    [SerializeField] private TMP_Text sectorYValue;
    [SerializeField] private TMP_Text sectorZValue;
    [SerializeField] private TMP_Text sectorSizeValue;
    [SerializeField] private TMP_Text starDensityValue;
    [SerializeField] private TMP_Text anomalyChanceValue;

    [Header("UI References - Input Fields")]
    [SerializeField] private TMP_InputField sectorXInput;
    [SerializeField] private TMP_InputField sectorYInput;
    [SerializeField] private TMP_InputField sectorZInput;
    [SerializeField] private TMP_InputField sectorSizeInput;
    [SerializeField] private TMP_InputField starDensityInput;
    [SerializeField] private TMP_InputField anomalyChanceInput;

    [Header("UI References - Buttons")]
    [SerializeField] private Button cancelButton;
    [SerializeField] private Button generateButton;
    [SerializeField] private Button advancedOptionsButton;

    // Events
    public event Action GenerationCanceled;
    public event Action<Dictionary<string, object>> GenerationConfirmed;

    // Flag to prevent circular updates
    private bool _isUpdating = false;

    void Start()
    {
        InitializeControls();
        
        // Add button listeners
        cancelButton.onClick.AddListener(OnCancelButtonPressed);
        generateButton.onClick.AddListener(OnGenerateButtonPressed);
        if (advancedOptionsButton != null)
            advancedOptionsButton.onClick.AddListener(OnAdvancedOptionsPressed);

        // Add slider listeners
        sectorXSlider.onValueChanged.AddListener(value => UpdateFromSlider(sectorXValue, sectorXInput, value));
        sectorYSlider.onValueChanged.AddListener(value => UpdateFromSlider(sectorYValue, sectorYInput, value));
        sectorZSlider.onValueChanged.AddListener(value => UpdateFromSlider(sectorZValue, sectorZInput, value));
        sectorSizeSlider.onValueChanged.AddListener(value => UpdateFromSlider(sectorSizeValue, sectorSizeInput, value));
        starDensitySlider.onValueChanged.AddListener(value => UpdateFromSlider(starDensityValue, starDensityInput, value, true));
        anomalyChanceSlider.onValueChanged.AddListener(value => UpdateFromSlider(anomalyChanceValue, anomalyChanceInput, value, true));

        // Add input field listeners
        if (sectorXInput != null) sectorXInput.onEndEdit.AddListener(value => UpdateFromInput(sectorXSlider, sectorXValue, value));
        if (sectorYInput != null) sectorYInput.onEndEdit.AddListener(value => UpdateFromInput(sectorYSlider, sectorYValue, value));
        if (sectorZInput != null) sectorZInput.onEndEdit.AddListener(value => UpdateFromInput(sectorZSlider, sectorZValue, value));
        if (sectorSizeInput != null) sectorSizeInput.onEndEdit.AddListener(value => UpdateFromInput(sectorSizeSlider, sectorSizeValue, value));
        if (starDensityInput != null) starDensityInput.onEndEdit.AddListener(value => UpdateFromInput(starDensitySlider, starDensityValue, value, true));
        if (anomalyChanceInput != null) anomalyChanceInput.onEndEdit.AddListener(value => UpdateFromInput(anomalyChanceSlider, anomalyChanceValue, value, true));
    }

    private void InitializeControls()
    {
        // Set slider ranges
        sectorXSlider.minValue = 1; sectorXSlider.maxValue = 7; sectorXSlider.wholeNumbers = true; sectorXSlider.value = 5;
        sectorYSlider.minValue = 1; sectorYSlider.maxValue = 7; sectorYSlider.wholeNumbers = true; sectorYSlider.value = 5;
        sectorZSlider.minValue = 1; sectorZSlider.maxValue = 7; sectorZSlider.wholeNumbers = true; sectorZSlider.value = 5;
        sectorSizeSlider.minValue = 5; sectorSizeSlider.maxValue = 10; sectorSizeSlider.wholeNumbers = true; sectorSizeSlider.value = 10;
        starDensitySlider.minValue = 1; starDensitySlider.maxValue = 20; starDensitySlider.wholeNumbers = false; starDensitySlider.value = 12;
        anomalyChanceSlider.minValue = 0; anomalyChanceSlider.maxValue = 5; anomalyChanceSlider.wholeNumbers = false; anomalyChanceSlider.value = 0.1f;

        // Initialize input fields if they exist
        SetupInputField(sectorXInput);
        SetupInputField(sectorYInput);
        SetupInputField(sectorZInput);
        SetupInputField(sectorSizeInput);
        SetupInputField(starDensityInput);
        SetupInputField(anomalyChanceInput);

        // Update initial text values and inputs
        UpdateFromSlider(sectorXValue, sectorXInput, sectorXSlider.value);
        UpdateFromSlider(sectorYValue, sectorYInput, sectorYSlider.value);
        UpdateFromSlider(sectorZValue, sectorZInput, sectorZSlider.value);
        UpdateFromSlider(sectorSizeValue, sectorSizeInput, sectorSizeSlider.value);
        UpdateFromSlider(starDensityValue, starDensityInput, starDensitySlider.value, true);
        UpdateFromSlider(anomalyChanceValue, anomalyChanceInput, anomalyChanceSlider.value, true);
    }

    private void SetupInputField(TMP_InputField inputField)
    {
        if (inputField == null) return;
        
        // Set input field properties for better number input handling
        inputField.contentType = TMP_InputField.ContentType.DecimalNumber;
        inputField.interactable = true;
    }

    private void UpdateFromSlider(TMP_Text textComponent, TMP_InputField inputField, float value, bool isFloat = false)
    {
        if (_isUpdating) return;
        _isUpdating = true;

        string formattedValue = isFloat ? value.ToString("F1") : Mathf.RoundToInt(value).ToString();
        
        // Update text label if it exists
        if (textComponent != null)
        {
            textComponent.text = formattedValue;
        }
        
        // Update input field if it exists
        if (inputField != null)
        {
            inputField.text = formattedValue;
        }
        
        _isUpdating = false;
    }

    private void UpdateFromInput(Slider slider, TMP_Text textComponent, string valueText, bool isFloat = false)
    {
        if (_isUpdating || slider == null) return;
        if (!float.TryParse(valueText, out float value)) return;
        
        _isUpdating = true;
        value = Mathf.Clamp(value, slider.minValue, slider.maxValue);
        slider.value = isFloat ? value : Mathf.RoundToInt(value);
        
        // Update the text component directly as well
        if (textComponent != null)
        {
            textComponent.text = isFloat ? value.ToString("F1") : Mathf.RoundToInt(value).ToString();
        }
        
        _isUpdating = false;
    }

    private void OnCancelButtonPressed()
    {
        GenerationCanceled?.Invoke();
        Destroy(gameObject);
    }

    private void OnGenerateButtonPressed()
    {
        var parameters = new Dictionary<string, object>
        {
            ["sector_size_x"] = (int)sectorXSlider.value,
            ["sector_size_y"] = (int)sectorYSlider.value,
            ["sector_size_z"] = (int)sectorZSlider.value,
            ["parsecs_per_sector"] = (int)sectorSizeSlider.value,
            ["star_density"] = starDensitySlider.value / 100.0f,
            ["anomaly_chance"] = anomalyChanceSlider.value / 100.0f,
            ["random_seed"] = 0,
            ["galaxy_type"] = 0,
            ["spectral_distribution"] = 1
        };

        GenerationConfirmed?.Invoke(parameters);
        Destroy(gameObject);
    }

    private void OnAdvancedOptionsPressed()
    {
        Debug.Log("Advanced options would be shown here");
    }
}