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
    
    void Start()
    {
        // Hook up button events
        cancelButton.onClick.AddListener(HandleCancel);
        deleteButton.onClick.AddListener(HandleDelete);
        deleteButton.interactable = false;
        
        // Populate save list
        PopulateSaveList();
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
        selectedSave = save;
        deleteButton.interactable = true;
        
        // Load the selected galaxy
        SaveSystem.LoadGalaxy(save.SaveName);
        LoadCompleted?.Invoke();
        
        // Close dialog
        Destroy(gameObject);
    }
    
    private void HandleDelete()
    {
        if (selectedSave != null)
        {
            if (SaveSystem.DeleteSavedGalaxy(selectedSave.SaveName))
            {
                // Refresh save list
                PopulateSaveList();
                selectedSave = null;
                deleteButton.interactable = false;
            }
        }
    }
    
    private void HandleCancel()
    {
        LoadCanceled?.Invoke();
        
        // Close dialog
        Destroy(gameObject);
    }
}