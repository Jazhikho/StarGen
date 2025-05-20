using UnityEngine;
using System;
using System.Collections.Generic;
using TMPro;

/// <summary>
/// Manages the galaxy generation workflow
/// </summary>
public class GenerationFlow : MonoBehaviour
{
    // Dependencies (assign in Inspector)
    [Header("Manager References")]
    [SerializeField] private DialogManager _dialogManager;
    [SerializeField] private SceneTransitionManager _sceneManager;
    
    [Header("UI References")]
    [SerializeField] private GameObject _statusBar;
    [SerializeField] private GameObject _menuPanel;
    [SerializeField] private TextMeshProUGUI _statusBarLabel;

    void Start()
    {
        // Subscribe to dialog events
        _dialogManager.GenerationParametersConfirmed += OnGenerationParametersConfirmed;
        _dialogManager.GenerationParametersCanceled += OnGenerationParametersCanceled;
        _dialogManager.GenerationCompleted += OnGenerationCompleted;
        _dialogManager.GenerationCanceled += OnGenerationCanceled;
    }

    /// <summary>
    /// Start new galaxy generation
    /// </summary>
    public void StartNewGeneration()
    {
        _dialogManager.ShowGenerationParametersDialog();
    }

    /// <summary>
    /// Handle generation parameters confirmation
    /// </summary>
    private void OnGenerationParametersConfirmed(Dictionary<string, object> parameters)
    {
        _dialogManager.ShowGenerationProgressDialog(parameters);
    }

    /// <summary>
    /// Handle generation parameters cancellation
    /// </summary>
    private void OnGenerationParametersCanceled()
    {
        // Do nothing, keep showing current scene
    }

    /// <summary>
    /// Handle generation completion
    /// </summary>
    private void OnGenerationCompleted()
    {
        _dialogManager.CleanupProgressWindow();
        
        List<StarSystem> galaxyData = GalaxyDataStore.Instance.GetAllSystems();
        _sceneManager.ShowGalaxyView(galaxyData);
        
        _statusBar.SetActive(true);
        _menuPanel.SetActive(true);
        _statusBarLabel.text = "Generation complete. Showing galaxy view.";
    }

    /// <summary>
    /// Handle generation cancellation
    /// </summary>
    private void OnGenerationCanceled()
    {
        _sceneManager.ShowTitleScreen();
        _statusBar.SetActive(false);
        _menuPanel.SetActive(false);
    }

    /// <summary>
    /// Handle galaxy being loaded
    /// </summary>
    public void OnGalaxyLoaded(string fileName)
    {
        List<StarSystem> galaxyData = GalaxyDataStore.Instance.GetAllSystems();
        _sceneManager.ShowGalaxyView(galaxyData);
    }

    private void OnEnable()
    {
        GalaxyLoadSignalEmitter.GalaxyLoadedEvent += OnGalaxyLoaded;
    }

    private void OnDisable()
    {
        GalaxyLoadSignalEmitter.GalaxyLoadedEvent -= OnGalaxyLoaded;
        
        // Unsubscribe from dialog events
        if (_dialogManager != null)
        {
            _dialogManager.GenerationParametersConfirmed -= OnGenerationParametersConfirmed;
            _dialogManager.GenerationParametersCanceled -= OnGenerationParametersCanceled;
            _dialogManager.GenerationCompleted -= OnGenerationCompleted;
            _dialogManager.GenerationCanceled -= OnGenerationCanceled;
        }
    }
}