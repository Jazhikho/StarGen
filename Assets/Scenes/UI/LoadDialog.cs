using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;

public class LoadDialog : MonoBehaviour
{
    [SerializeField] private Transform saveListContent;
    [SerializeField] private GameObject saveEntryPrefab;
    [SerializeField] private Button cancelButton;
    [SerializeField] private Button deleteButton;
    
    // Events
    public event Action LoadCompleted;
    public event Action LoadCanceled;
    
    private SaveSystem.SaveInfo selectedSave;
    
    private void Awake()
    {
        // Hook up button events
        cancelButton.onClick.AddListener(HandleCancel);
        deleteButton.onClick.AddListener(HandleDelete);
    }
    
    /// <summary>
    /// Reset the dialog to its initial state
    /// </summary>
    public void ResetDialog()
    {
        try
        {
            // Reset selection state
            selectedSave = null;
            deleteButton.interactable = false;
            
            // Populate save list
            PopulateSaveList();
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"LoadDialog: Exception in ResetDialog: {ex.Message}");
        }
    }
    
    private void PopulateSaveList()
    {
        // Clear existing entries
        foreach (Transform child in saveListContent)
        {
            Destroy(child.gameObject);
        }
        
        // Get saved galaxies
        List<SaveSystem.SaveInfo> savedGalaxies = SaveSystem.GetSavedGalaxies();
        
        if (savedGalaxies.Count == 0)
        {
            // Create "No saves" entry
            GameObject entryObject = Instantiate(saveEntryPrefab, saveListContent);
            TextMeshProUGUI entryText = entryObject.GetComponentInChildren<TextMeshProUGUI>();
            entryText.text = "No saved galaxies found";
            entryObject.GetComponent<Button>().interactable = false;
            return;
        }
        
        // Create entry for each saved galaxy
        foreach (var save in savedGalaxies)
        {
            GameObject entryObject = Instantiate(saveEntryPrefab, saveListContent);
            
            // Set text
            TextMeshProUGUI entryText = entryObject.GetComponentInChildren<TextMeshProUGUI>();
            entryText.text = $"{save.GalaxyName}\n" +
                            $"Created: {save.CreationDate:g}\n" +
                            $"Last Saved: {save.LastSaveDate:g}\n" +
                            $"Systems: {save.SystemCount}";
            
            // Set button action
            Button entryButton = entryObject.GetComponent<Button>();
            entryButton.onClick.AddListener(() => SelectSave(save));
        }
    }
    
    private void SelectSave(SaveSystem.SaveInfo save)
    {
        try
        {
            selectedSave = save;
            deleteButton.interactable = true;
            
            // Check if GalaxyDataStore exists
            if (GalaxyDataStore.Instance == null)
            {
                Debug.LogError("LoadDialog: GalaxyDataStore.Instance is null");
                return;
            }
            
            // Load the selected galaxy
            bool success = SaveSystem.LoadGalaxy(save.SaveName, GalaxyDataStore.Instance);
            
            if (success)
            {
                // Emit event that galaxy was loaded successfully
                GalaxyLoadSignalEmitter emitter = GalaxyLoadSignalEmitter.GetInstance();
                if (emitter != null)
                {
                    emitter.EmitGalaxyLoaded(save.SaveName);
                }
                
                LoadCompleted?.Invoke();
                // Dialog will be hidden by the DialogManager
            }
            else
            {
                Debug.LogError($"Failed to load galaxy: {save.SaveName}");
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"LoadDialog: Exception in SelectSave: {ex.Message}\n{ex.StackTrace}");
        }
    }
    
    private void HandleDelete()
    {
        try
        {
            if (selectedSave != null)
            {
                if (SaveSystem.DeleteSave(selectedSave.SaveName))
                {
                    // Refresh save list
                    PopulateSaveList();
                    selectedSave = null;
                    deleteButton.interactable = false;
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"LoadDialog: Exception in HandleDelete: {ex.Message}");
        }
    }
    
    private void HandleCancel()
    {
        LoadCanceled?.Invoke();
        // Dialog will be hidden by the DialogManager
    }
}