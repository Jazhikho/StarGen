using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Manages all dialog instantiation and event handling
/// </summary>
public class DialogManager : MonoBehaviour
{
    // Dialog prefabs (assign in Inspector)
    [Header("Dialog Prefabs")]
    [SerializeField] private GameObject _generationParamsDialogPrefab;
    [SerializeField] private GameObject _generationProgressWindowPrefab;
    [SerializeField] private GameObject _saveDialogPrefab;
    [SerializeField] private GameObject _loadDialogPrefab;
    
    // Dialog container (assign in Inspector)
    [Header("References")]
    [SerializeField] private Transform _dialogContainer;
    
    // Current dialog references
    private GameObject _currentDialog;
    private GameObject _progressWindow;
    
    #region Events
    public event Action<Dictionary<string, object>> GenerationParametersConfirmed;
    public event Action GenerationParametersCanceled;
    public event Action GenerationCompleted;
    public event Action GenerationCanceled;
    public event Action SaveCompleted;
    public event Action SaveCanceled;
    public event Action LoadCompleted;
    public event Action LoadCanceled;
    #endregion
    
    /// <summary>
    /// Initialize the dialog manager with required prefabs
    /// </summary>
    public void Initialize(
        GameObject generationParamsDialogPrefab,
        GameObject generationProgressWindowPrefab,
        GameObject saveDialogPrefab,
        GameObject loadDialogPrefab,
        Transform dialogContainer)
    {
        _generationParamsDialogPrefab = generationParamsDialogPrefab;
        _generationProgressWindowPrefab = generationProgressWindowPrefab;
        _saveDialogPrefab = saveDialogPrefab;
        _loadDialogPrefab = loadDialogPrefab;
        _dialogContainer = dialogContainer;
    }
    
    /// <summary>
    /// Show the generation parameters dialog
    /// </summary>
    public void ShowGenerationParametersDialog()
    {
        DestroyExistingDialogs();
        
        _currentDialog = Instantiate(_generationParamsDialogPrefab, _dialogContainer);
        GenerationParametersDialog paramsDialog = _currentDialog.GetComponent<GenerationParametersDialog>();
        paramsDialog.GenerationConfirmed += OnGenerationParametersConfirmed;
        paramsDialog.GenerationCanceled += OnGenerationParametersCanceled;
    }
    
    /// <summary>
    /// Show the generation progress dialog
    /// </summary>
    public void ShowGenerationProgressDialog(Dictionary<string, object> parameters)
    {
        DestroyExistingDialogs();
        
        _progressWindow = Instantiate(_generationProgressWindowPrefab, _dialogContainer);
        GenerationProgressDialog progressDialog = _progressWindow.GetComponent<GenerationProgressDialog>();
        progressDialog.Initialize(parameters);
        progressDialog.GenerationCompleted += OnGenerationCompleted;
        progressDialog.GenerationCanceled += OnGenerationCanceled;
    }
    
    /// <summary>
    /// Show the save dialog
    /// </summary>
    public void ShowSaveDialog()
    {
        if (!GalaxyDataStore.Instance.HasGalaxyData())
            return;
            
        GameObject saveDialog = Instantiate(_saveDialogPrefab, _dialogContainer);
        SaveDialog saveDialogComponent = saveDialog.GetComponent<SaveDialog>();
        saveDialogComponent.SaveCompleted += OnSaveCompleted;
        saveDialogComponent.SaveCanceled += OnSaveCanceled;
    }
    
    /// <summary>
    /// Show the load dialog
    /// </summary>
    public void ShowLoadDialog()
    {
        GameObject loadDialog = Instantiate(_loadDialogPrefab, _dialogContainer);
        LoadDialog loadDialogComponent = loadDialog.GetComponent<LoadDialog>();
        loadDialogComponent.LoadCompleted += OnLoadCompleted;
        loadDialogComponent.LoadCanceled += OnLoadCanceled;
    }
    
    /// <summary>
    /// Clean up progress window
    /// </summary>
    public void CleanupProgressWindow()
    {
        if (_progressWindow != null)
        {
            Destroy(_progressWindow);
            _progressWindow = null;
        }
    }
    
    /// <summary>
    /// Destroy all existing dialogs
    /// </summary>
    private void DestroyExistingDialogs()
    {
        foreach (Transform child in _dialogContainer)
        {
            if (child.GetComponent<GenerationParametersDialog>() != null ||
                child.GetComponent<GenerationProgressDialog>() != null ||
                child.GetComponent<SaveDialog>() != null ||
                child.GetComponent<LoadDialog>() != null)
            {
                Destroy(child.gameObject);
            }
        }
    }
    
    #region Event Handlers
    private void OnGenerationParametersConfirmed(Dictionary<string, object> parameters)
    {
        GenerationParametersConfirmed?.Invoke(parameters);
    }
    
    private void OnGenerationParametersCanceled()
    {
        GenerationParametersCanceled?.Invoke();
    }
    
    private void OnGenerationCompleted()
    {
        GenerationCompleted?.Invoke();
    }
    
    private void OnGenerationCanceled()
    {
        GenerationCanceled?.Invoke();
    }
    
    private void OnSaveCompleted()
    {
        SaveCompleted?.Invoke();
    }
    
    private void OnSaveCanceled()
    {
        SaveCanceled?.Invoke();
    }
    
    private void OnLoadCompleted()
    {
        LoadCompleted?.Invoke();
    }
    
    private void OnLoadCanceled()
    {
        LoadCanceled?.Invoke();
    }
    #endregion
}