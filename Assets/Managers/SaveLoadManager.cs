using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Manages saving and loading of galaxy data
/// </summary>
public class SaveLoadManager : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Dropdown saveFilesDropdown;
    public Button loadButton;
    public Button saveButton;
    public Button deleteButton;
    public TMP_InputField newSaveNameInput;
    public Button createNewSaveButton;
    
    [Header("Configuration")]
    [SerializeField] private bool autoLoadLatestOnStart = false;
    
    // Singleton instance
    private static SaveLoadManager _instance;
    
    /// <summary>
    /// Gets the singleton instance
    /// </summary>
    public static SaveLoadManager Instance
    {
        get
        {
            if (_instance == null)
            {
                var go = new GameObject("SaveLoadManager");
                _instance = go.AddComponent<SaveLoadManager>();
                DontDestroyOnLoad(go);
            }
            return _instance;
        }
    }
    
    /// <summary>
    /// Event triggered when a save operation is completed
    /// </summary>
    public delegate void SaveCompletedEventHandler(bool success, string fileName);
    public event SaveCompletedEventHandler OnSaveCompleted;
    
    /// <summary>
    /// Event triggered when a load operation is completed
    /// </summary>
    public delegate void LoadCompletedEventHandler(bool success, string fileName);
    public event LoadCompletedEventHandler OnLoadCompleted;
    
    /// <summary>
    /// Called when the component is first initialized
    /// </summary>
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    /// <summary>
    /// Called at runtime start
    /// </summary>
    private void Start()
    {
        InitializeUI();
        
        if (autoLoadLatestOnStart)
        {
            LoadLatestSave();
        }
    }
    
    /// <summary>
    /// Initialize UI components if they exist
    /// </summary>
    private void InitializeUI()
    {
        if (saveFilesDropdown != null)
        {
            saveFilesDropdown.onValueChanged.AddListener(OnSaveFileSelected);
            RefreshSaveFilesList();
        }
        
        if (loadButton != null)
        {
            loadButton.onClick.AddListener(LoadSelectedSave);
            loadButton.interactable = false;
        }
        
        if (saveButton != null)
        {
            saveButton.onClick.AddListener(SaveToSelectedFile);
            saveButton.interactable = false;
        }
        
        if (deleteButton != null)
        {
            deleteButton.onClick.AddListener(DeleteSelectedSave);
            deleteButton.interactable = false;
        }
        
        if (createNewSaveButton != null)
        {
            createNewSaveButton.onClick.AddListener(CreateNewSave);
        }
    }
    
    /// <summary>
    /// Refreshes the save files dropdown list
    /// </summary>
    public void RefreshSaveFilesList()
    {
        if (saveFilesDropdown == null) return;
        
        List<string> saveFiles = GalaxyDataStore.Instance.GetSaveFiles();
        
        saveFilesDropdown.ClearOptions();
        
        List<TMP_Dropdown.OptionData> options = new List<TMP_Dropdown.OptionData>();
        options.Add(new TMP_Dropdown.OptionData("Select a save file..."));
        
        foreach (var saveFile in saveFiles)
        {
            options.Add(new TMP_Dropdown.OptionData(saveFile));
        }
        
        saveFilesDropdown.AddOptions(options);
        saveFilesDropdown.value = 0;
        
        // Disable buttons until a save file is selected
        if (loadButton != null) loadButton.interactable = false;
        if (saveButton != null) saveButton.interactable = false;
        if (deleteButton != null) deleteButton.interactable = false;
    }
    
    /// <summary>
    /// Handles save file selection from dropdown
    /// </summary>
    private void OnSaveFileSelected(int index)
    {
        bool fileSelected = index > 0;
        
        if (loadButton != null) loadButton.interactable = fileSelected;
        if (saveButton != null) saveButton.interactable = fileSelected && GalaxyDataStore.Instance.HasGalaxyData();
        if (deleteButton != null) deleteButton.interactable = fileSelected;
    }
    
    /// <summary>
    /// Loads the selected save file
    /// </summary>
    public void LoadSelectedSave()
    {
        if (saveFilesDropdown == null || saveFilesDropdown.value <= 0) return;
        
        string selectedSave = saveFilesDropdown.options[saveFilesDropdown.value].text;
        LoadSaveFile(selectedSave);
    }
    
    /// <summary>
    /// Saves to the selected file
    /// </summary>
    public void SaveToSelectedFile()
    {
        if (saveFilesDropdown == null || saveFilesDropdown.value <= 0) return;
        
        string selectedSave = saveFilesDropdown.options[saveFilesDropdown.value].text;
        SaveGalaxyData(selectedSave);
    }
    
    /// <summary>
    /// Deletes the selected save file
    /// </summary>
    public void DeleteSelectedSave()
    {
        if (saveFilesDropdown == null || saveFilesDropdown.value <= 0) return;
        
        string selectedSave = saveFilesDropdown.options[saveFilesDropdown.value].text;
        DeleteSaveFile(selectedSave);
    }
    
    /// <summary>
    /// Creates a new save file with the name from the input field
    /// </summary>
    public void CreateNewSave()
    {
        if (newSaveNameInput == null || string.IsNullOrWhiteSpace(newSaveNameInput.text)) return;
        
        string newSaveName = newSaveNameInput.text.Trim();
        SaveGalaxyData(newSaveName);
        
        // Clear the input field
        newSaveNameInput.text = "";
    }
    
    /// <summary>
    /// Saves the current galaxy data to a file
    /// </summary>
    /// <param name="fileName">Name for the save file (without extension)</param>
    /// <returns>True if successful, false otherwise</returns>
    public bool SaveGalaxyData(string fileName)
    {
        bool success = GalaxyDataStore.Instance.SaveGalaxyData(fileName);
        
        if (success)
        {
            Debug.Log($"Galaxy data saved to {fileName}");
            RefreshSaveFilesList();
            
            // Select the saved file in the dropdown
            if (saveFilesDropdown != null)
            {
                for (int i = 0; i < saveFilesDropdown.options.Count; i++)
                {
                    if (saveFilesDropdown.options[i].text == fileName)
                    {
                        saveFilesDropdown.value = i;
                        break;
                    }
                }
            }
        }
        else
        {
            Debug.LogError($"Failed to save galaxy data to {fileName}");
        }
        
        OnSaveCompleted?.Invoke(success, fileName);
        return success;
    }
    
    /// <summary>
    /// Loads galaxy data from a file
    /// </summary>
    /// <param name="fileName">Name of the save file (without extension)</param>
    /// <returns>True if successful, false otherwise</returns>
    public bool LoadSaveFile(string fileName)
    {
        bool success = GalaxyDataStore.Instance.LoadGalaxyData(fileName);
        
        if (success)
        {
            Debug.Log($"Galaxy data loaded from {fileName}");
            
            // Update save button interactability
            if (saveButton != null)
            {
                saveButton.interactable = saveFilesDropdown.value > 0 && GalaxyDataStore.Instance.HasGalaxyData();
            }
        }
        else
        {
            Debug.LogError($"Failed to load galaxy data from {fileName}");
        }
        
        OnLoadCompleted?.Invoke(success, fileName);
        return success;
    }
    
    /// <summary>
    /// Deletes a save file
    /// </summary>
    /// <param name="fileName">Name of the save file (without extension)</param>
    /// <returns>True if successful, false otherwise</returns>
    public bool DeleteSaveFile(string fileName)
    {
        bool success = GalaxyDataStore.Instance.DeleteSaveFile(fileName);
        
        if (success)
        {
            Debug.Log($"Save file deleted: {fileName}");
            RefreshSaveFilesList();
        }
        else
        {
            Debug.LogError($"Failed to delete save file: {fileName}");
        }
        
        return success;
    }
    
    /// <summary>
    /// Loads the most recent save file
    /// </summary>
    /// <returns>True if successful, false otherwise</returns>
    public bool LoadLatestSave()
    {
        List<string> saveFiles = GalaxyDataStore.Instance.GetSaveFiles();
        
        if (saveFiles.Count == 0)
        {
            Debug.Log("No save files found to load.");
            return false;
        }
        
        string latestSave = saveFiles[saveFiles.Count - 1]; // Assuming the last file is the most recent
        return LoadSaveFile(latestSave);
    }
} 