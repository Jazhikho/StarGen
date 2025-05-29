using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class SaveDialog : MonoBehaviour
{
    [SerializeField] private TMP_InputField saveNameInput;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button cancelButton;
    
    // Events
    public event Action SaveCompleted;
    public event Action SaveCanceled;

    private void Awake()
    {
        // Hook up button events
        saveButton.onClick.AddListener(HandleSave);
        cancelButton.onClick.AddListener(HandleCancel);
    }
    
    /// <summary>
    /// Reset the dialog to its initial state
    /// </summary>
    public void ResetDialog()
    {
        try
        {
            // Set default save name
            string defaultName = "Galaxy_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
            
            if (GalaxyDataStore.Instance != null && !string.IsNullOrEmpty(GalaxyDataStore.Instance.GalaxyName))
            {
                defaultName = $"{GalaxyDataStore.Instance.GalaxyName}_{DateTime.Now:yyyyMMdd_HHmmss}";
            }
            
            saveNameInput.text = defaultName;
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"SaveDialog: Exception in ResetDialog: {ex.Message}");
            
            // Use a fallback name in case of error
            saveNameInput.text = "Galaxy_" + DateTime.Now.ToString("yyyyMMdd_HHmmss");
        }
    }
    
    private void HandleSave()
    {
        try
        {
            string saveName = saveNameInput.text.Trim();
            
            if (string.IsNullOrEmpty(saveName))
            {
                Debug.LogError("Save name cannot be empty");
                return;
            }
            
            // Check if GalaxyDataStore exists
            if (GalaxyDataStore.Instance == null)
            {
                Debug.LogError("SaveDialog: GalaxyDataStore.Instance is null");
                return;
            }
            
            bool success = SaveSystem.SaveGalaxy(saveName, GalaxyDataStore.Instance);
            
            if (success)
            {
                SaveCompleted?.Invoke();
                // Dialog will be hidden by the DialogManager
            }
            else
            {
                Debug.LogError($"Failed to save galaxy: {saveName}");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"SaveDialog: Exception in HandleSave: {ex.Message}\n{ex.StackTrace}");
        }
    }
    
    private void HandleCancel()
    {
        SaveCanceled?.Invoke();
        // Dialog will be hidden by the DialogManager
    }
}