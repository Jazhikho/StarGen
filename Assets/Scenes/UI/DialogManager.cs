using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Manages all dialog visibility and event handling
/// </summary>
public class DialogManager : MonoBehaviour
{
    // Dialog GameObject references (assign in Inspector)
    [Header("Dialog Objects")]
    [SerializeField] private GameObject _generationParamsDialog;
    [SerializeField] private GameObject _generationProgressWindow;
    [SerializeField] private GameObject _saveDialog;
    [SerializeField] private GameObject _loadDialog;
    
    // Current active dialog references
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

    private void Start()
    {
        // Make sure all dialogs are hidden initially
        if (_generationParamsDialog != null) _generationParamsDialog.SetActive(false);
        if (_generationProgressWindow != null) _generationProgressWindow.SetActive(false);
        if (_saveDialog != null) _saveDialog.SetActive(false);
        if (_loadDialog != null) _loadDialog.SetActive(false);
    }
    
    /// <summary>
    /// Legacy initialization for backward compatibility
    /// </summary>
    public void Initialize(
        GameObject generationParamsDialogPrefab,
        GameObject generationProgressWindowPrefab,
        GameObject saveDialogPrefab,
        GameObject loadDialogPrefab,
        Transform dialogContainer)
    {
        Debug.LogWarning("DialogManager: Using legacy Initialize method, child GameObjects should be configured instead");
    }
    
    /// <summary>
    /// Show the generation parameters dialog
    /// </summary>
    public void ShowGenerationParametersDialog()
    {
        HideAllDialogs();
        
        if (_generationParamsDialog == null)
        {
            Debug.LogError("DialogManager: _generationParamsDialog is null");
            return;
        }
        
        _generationParamsDialog.SetActive(true);
        _currentDialog = _generationParamsDialog;
        
        GenerationParametersDialog paramsDialog = _currentDialog.GetComponent<GenerationParametersDialog>();
        if (paramsDialog != null)
        {
            paramsDialog.GenerationConfirmed += OnGenerationParametersConfirmed;
            paramsDialog.GenerationCanceled += OnGenerationParametersCanceled;
        }
        else
        {
            Debug.LogError("DialogManager: GenerationParametersDialog component not found on dialog");
        }
    }
    
    /// <summary>
    /// Show the generation progress dialog
    /// </summary>
    public void ShowGenerationProgressDialog(Dictionary<string, object> parameters)
    {
        HideAllDialogs();
        
        if (_generationProgressWindow == null)
        {
            Debug.LogError("DialogManager: _generationProgressWindow is null");
            return;
        }
        
        _generationProgressWindow.SetActive(true);
        _progressWindow = _generationProgressWindow;
        
        GenerationProgressDialog progressDialog = _progressWindow.GetComponent<GenerationProgressDialog>();
        if (progressDialog != null)
        {
            progressDialog.Initialize(parameters);
            progressDialog.GenerationCompleted += OnGenerationCompleted;
            progressDialog.GenerationCanceled += OnGenerationCanceled;
        }
        else
        {
            Debug.LogError("DialogManager: GenerationProgressDialog component not found on dialog");
        }
    }
    
    /// <summary>
    /// Show the save dialog
    /// </summary>
    public void ShowSaveDialog()
    {
        try
        {
            // Ensure GalaxyDataStore exists
            if (GalaxyDataStore.Instance == null)
            {
                Debug.LogWarning("DialogManager: GalaxyDataStore.Instance is null, creating instance");
                GameObject galaxyDataStore = new GameObject("GalaxyDataStore");
                var dataStoreComponent = galaxyDataStore.AddComponent<GalaxyDataStore>();
                DontDestroyOnLoad(galaxyDataStore);
                
                // Verify creation was successful
                if (GalaxyDataStore.Instance == null)
                {
                    Debug.LogError("DialogManager: Failed to create GalaxyDataStore instance");
                    return;
                }
            }
            
            // Cache the instance to avoid race conditions
            var dataStore = GalaxyDataStore.Instance;
            if (dataStore == null)
            {
                Debug.LogError("DialogManager: GalaxyDataStore.Instance became null unexpectedly");
                return;
            }
            
            // Check if there's data to save
            if (!dataStore.HasGalaxyData())
            {
                Debug.LogWarning("No galaxy data to save");
                return;
            }
            
            HideAllDialogs();
            
            if (_saveDialog == null)
            {
                Debug.LogError("DialogManager: _saveDialog is null");
                return;
            }
            
            _saveDialog.SetActive(true);
            SaveDialog saveDialogComponent = _saveDialog.GetComponent<SaveDialog>();
            
            if (saveDialogComponent == null)
            {
                Debug.LogError("DialogManager: SaveDialog component not found on dialog");
                _saveDialog.SetActive(false);
                return;
            }
            
            // Reset the dialog and connect events
            saveDialogComponent.ResetDialog();
            saveDialogComponent.SaveCompleted += OnSaveCompleted;
            saveDialogComponent.SaveCanceled += OnSaveCanceled;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"DialogManager: Exception in ShowSaveDialog: {ex.Message}\n{ex.StackTrace}");
        }
    }
    
    /// <summary>
    /// Show the load dialog
    /// </summary>
    public void ShowLoadDialog()
    {
        try
        {
            // Ensure GalaxyDataStore exists
            if (GalaxyDataStore.Instance == null)
            {
                Debug.LogWarning("DialogManager: GalaxyDataStore.Instance is null, creating instance");
                GameObject galaxyDataStore = new GameObject("GalaxyDataStore");
                var dataStoreComponent = galaxyDataStore.AddComponent<GalaxyDataStore>();
                DontDestroyOnLoad(galaxyDataStore);
                
                // Verify creation was successful
                if (GalaxyDataStore.Instance == null)
                {
                    Debug.LogError("DialogManager: Failed to create GalaxyDataStore instance");
                    return;
                }
            }
            
            HideAllDialogs();
            
            if (_loadDialog == null)
            {
                Debug.LogError("DialogManager: _loadDialog is null");
                return;
            }
            
            _loadDialog.SetActive(true);
            LoadDialog loadDialogComponent = _loadDialog.GetComponent<LoadDialog>();
            
            if (loadDialogComponent == null)
            {
                Debug.LogError("DialogManager: LoadDialog component not found on dialog");
                _loadDialog.SetActive(false);
                return;
            }
            
            // Reset the dialog and connect events
            loadDialogComponent.ResetDialog();
            loadDialogComponent.LoadCompleted += OnLoadCompleted;
            loadDialogComponent.LoadCanceled += OnLoadCanceled;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"DialogManager: Exception in ShowLoadDialog: {ex.Message}\n{ex.StackTrace}");
        }
    }
    
    /// <summary>
    /// Clean up progress window
    /// </summary>
    public void CleanupProgressWindow()
    {
        if (_progressWindow != null)
        {
            _progressWindow.SetActive(false);
            _progressWindow = null;
        }
    }
    
    /// <summary>
    /// Hide all dialog windows
    /// </summary>
    private void HideAllDialogs()
    {
        if (_generationParamsDialog != null) _generationParamsDialog.SetActive(false);
        if (_generationProgressWindow != null) _generationProgressWindow.SetActive(false);
        if (_saveDialog != null) _saveDialog.SetActive(false);
        if (_loadDialog != null) _loadDialog.SetActive(false);
        
        // Unset current references
        _currentDialog = null;
        _progressWindow = null;
    }
    
    /// <summary>
    /// Legacy method for backwards compatibility
    /// </summary>
    private void DestroyExistingDialogs()
    {
        HideAllDialogs();
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
        if (_saveDialog != null) _saveDialog.SetActive(false);
    }
    
    private void OnSaveCanceled()
    {
        SaveCanceled?.Invoke();
        if (_saveDialog != null) _saveDialog.SetActive(false);
    }
    
    private void OnLoadCompleted()
    {
        LoadCompleted?.Invoke();
        if (_loadDialog != null) _loadDialog.SetActive(false);
    }
    
    private void OnLoadCanceled()
    {
        LoadCanceled?.Invoke();
        if (_loadDialog != null) _loadDialog.SetActive(false);
    }
    #endregion
}