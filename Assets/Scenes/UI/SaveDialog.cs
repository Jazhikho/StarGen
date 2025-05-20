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
    
    void Start()
    {
        // Set default save name
        string defaultName = $"{GalaxyDataStore.Instance.GalaxyName}_{DateTime.Now:yyyyMMdd_HHmmss}";
        saveNameInput.text = defaultName;
        
        // Hook up button events
        saveButton.onClick.AddListener(HandleSave);
        cancelButton.onClick.AddListener(HandleCancel);
    }
    
    private void HandleSave()
    {
        string saveName = saveNameInput.text.Trim();
        
        if (string.IsNullOrEmpty(saveName))
        {
            Debug.LogError("Save name cannot be empty");
            return;
        }
        
        SaveSystem.SaveGalaxy(saveName);
        SaveCompleted?.Invoke();
        
        // Close dialog
        Destroy(gameObject);
    }
    
    private void HandleCancel()
    {
        SaveCanceled?.Invoke();
        
        // Close dialog
        Destroy(gameObject);
    }
}