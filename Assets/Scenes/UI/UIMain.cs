using UnityEngine;
using TMPro;

/// <summary>
/// UIMain serves as the application's main UI controller.
/// Coordinates between different managers.
/// </summary>
public class UIMain : MonoBehaviour
{
    // Manager references (assign in Inspector)
    [Header("Managers")]
    [SerializeField] private SceneTransitionManager sceneManager;
    [SerializeField] private DialogManager dialogManager;
    [SerializeField] private GenerationFlow generationFlow;

    // UI References (assign in Inspector)
    [Header("UI References")]
    [SerializeField] private GameObject statusBar;
    [SerializeField] private GameObject menuPanel;
    [SerializeField] private TextMeshProUGUI statusBarLabel;

    void Start()
    {
        // Show title screen
        sceneManager.ShowTitleScreen();
        statusBar.SetActive(false);
        menuPanel.SetActive(false);

        dialogManager.SaveCompleted += () => statusBarLabel.text = "Galaxy saved successfully.";
        dialogManager.SaveCanceled += () => statusBarLabel.text = "Save canceled.";
        dialogManager.LoadCompleted += () => statusBarLabel.text = "Galaxy loaded successfully.";
        dialogManager.LoadCanceled += () => statusBarLabel.text = "Load canceled.";

        if (EntityRegistry.Instance == null)
        {
            GameObject entityRegistry = new GameObject("EntityRegistry");
            entityRegistry.AddComponent<EntityRegistry>();
            DontDestroyOnLoad(entityRegistry);
        }

        if (GalaxyDataStore.Instance == null)
        {
            GameObject galaxyDataStore = new GameObject("GalaxyDataStore");
            galaxyDataStore.AddComponent<GalaxyDataStore>();
            DontDestroyOnLoad(galaxyDataStore);
        }
    }

    /// <summary>
    /// Handles menu button presses
    /// </summary>
    public void OnMenuItemPressed(int id)
    {
        switch (id)
        {
            case 0: // New Galaxy
                generationFlow.StartNewGeneration();
                break;
            case 1: // Save Galaxy
                dialogManager.ShowSaveDialog();
                break;
            case 2: // Load Galaxy
                dialogManager.ShowLoadDialog();
                break;
            case 3: // Exit
                #if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
                #else
                    Application.Quit();
                #endif
                break;
        }
    }
}