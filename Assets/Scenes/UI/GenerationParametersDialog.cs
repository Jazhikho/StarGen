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

    [Header("UI References - Text Fields")]
    [SerializeField] private TMP_Text sectorXValue;
    [SerializeField] private TMP_Text sectorYValue;
    [SerializeField] private TMP_Text sectorZValue;
    [SerializeField] private TMP_Text sectorSizeValue;
    [SerializeField] private TMP_Text starDensityValue;
    [SerializeField] private TMP_Text anomalyChanceValue;

    [Header("UI References - Buttons")]
    [SerializeField] private Button cancelButton;
    [SerializeField] private Button generateButton;
    [SerializeField] private Button advancedOptionsButton;

    // Events
    public event Action GenerationCanceled;
    public event Action<Dictionary<string, object>> GenerationConfirmed;

    void Start()
    {
        InitializeControls();
        
        // Add button listeners
        cancelButton.onClick.AddListener(OnCancelButtonPressed);
        generateButton.onClick.AddListener(OnGenerateButtonPressed);
        if (advancedOptionsButton != null)
            advancedOptionsButton.onClick.AddListener(OnAdvancedOptionsPressed);

        // Add slider listeners
        sectorXSlider.onValueChanged.AddListener(value => UpdateValueText(sectorXValue, value));
        sectorYSlider.onValueChanged.AddListener(value => UpdateValueText(sectorYValue, value));
        sectorZSlider.onValueChanged.AddListener(value => UpdateValueText(sectorZValue, value));
        sectorSizeSlider.onValueChanged.AddListener(value => UpdateValueText(sectorSizeValue, value));
        starDensitySlider.onValueChanged.AddListener(value => UpdateValueText(starDensityValue, value, true));
        anomalyChanceSlider.onValueChanged.AddListener(value => UpdateValueText(anomalyChanceValue, value, true));
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

        // Update initial text values
        UpdateValueText(sectorXValue, sectorXSlider.value);
        UpdateValueText(sectorYValue, sectorYSlider.value);
        UpdateValueText(sectorZValue, sectorZSlider.value);
        UpdateValueText(sectorSizeValue, sectorSizeSlider.value);
        UpdateValueText(starDensityValue, starDensitySlider.value, true);
        UpdateValueText(anomalyChanceValue, anomalyChanceSlider.value, true);
    }

    private void UpdateValueText(TMP_Text textComponent, float value, bool isFloat = false)
    {
        if (textComponent != null)
        {
            textComponent.text = isFloat ? value.ToString("F1") : Mathf.RoundToInt(value).ToString();
        }
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