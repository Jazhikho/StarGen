using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class MenuPanel : MonoBehaviour
{
    [Header("Main Menu Buttons")]
    [SerializeField] private Button newButton;
    [SerializeField] private Button saveButton;
    [SerializeField] private Button loadButton;
    [SerializeField] private Button viewButton;
    [SerializeField] private Button exitButton;

    [Header("Tools Dropdown")]
    [SerializeField] private TMP_Dropdown toolsDropdown;

    private UIMain _uiMain;

    public enum MenuContext
    {
        None,
        GalaxyView,
        SystemView
    }

    public enum ToolType
    {
        // Galaxy View Tools
        AddSystem,
        DeleteSystem,
        JumpRoutes,
        
        // System View Tools
        EditSystem,
        EditPlanets,
        EditStations,
        
        // Shared Tools
        Measure,
        Information
    }

    private MenuContext _currentContext = MenuContext.None;
    private Dictionary<ToolType, string> _toolLabels;
    private List<ToolType> _currentTools;

    void Start()
    {
        // Initialize tool labels
        _toolLabels = new Dictionary<ToolType, string>
        {
            { ToolType.AddSystem, "Add System" },
            { ToolType.DeleteSystem, "Delete System" },
            { ToolType.JumpRoutes, "Jump Routes" },
            { ToolType.EditSystem, "Edit System" },
            { ToolType.EditPlanets, "Edit Planets" },
            { ToolType.EditStations, "Edit Stations" },
            { ToolType.Measure, "Measure Distance" },
            { ToolType.Information, "Information" }
        };

        // Find UI main reference
        _uiMain = GetComponentInParent<UIMain>();

        if (_uiMain == null)
        {
            Debug.LogError("MenuPanel: Could not find UIMain parent");
            return;
        }

        // Connect main menu button events
        newButton.onClick.AddListener(() => _uiMain.OnMenuItemPressed(0));
        saveButton.onClick.AddListener(() => _uiMain.OnMenuItemPressed(1));
        loadButton.onClick.AddListener(() => _uiMain.OnMenuItemPressed(2));
        exitButton.onClick.AddListener(() => _uiMain.OnMenuItemPressed(3));
        viewButton.onClick.AddListener(() => _uiMain.OnMenuItemPressed(4));

        // Connect dropdown event
        toolsDropdown.onValueChanged.AddListener(OnToolSelected);

        // Set initial visibility
        this.gameObject.SetActive(false);
    }

    public void SetContext(MenuContext context)
    {
        _currentContext = context;
        UpdateToolsDropdown();
    }

    private void UpdateToolsDropdown()
    {
        // Check for null references
        if (toolsDropdown == null)
        {
            Debug.LogWarning("MenuPanel: toolsDropdown is null");
            return;
        }
        
        // Clear existing options
        toolsDropdown.ClearOptions();
        
        // Create new options based on context
        _currentTools = new List<ToolType>();
        List<string> optionLabels = new List<string>();

        // Add default option
        optionLabels.Add("Tools");

        switch (_currentContext)
        {
            case MenuContext.None:
                toolsDropdown.gameObject.SetActive(false);
                return;

            case MenuContext.GalaxyView:
                toolsDropdown.gameObject.SetActive(true);
                _currentTools.Add(ToolType.AddSystem);
                _currentTools.Add(ToolType.DeleteSystem);
                _currentTools.Add(ToolType.JumpRoutes);
                _currentTools.Add(ToolType.Measure);
                _currentTools.Add(ToolType.Information);
                break;

            case MenuContext.SystemView:
                toolsDropdown.gameObject.SetActive(true);
                _currentTools.Add(ToolType.EditSystem);
                _currentTools.Add(ToolType.EditPlanets);
                _currentTools.Add(ToolType.EditStations);
                _currentTools.Add(ToolType.Measure);
                _currentTools.Add(ToolType.Information);
                break;
        }

        // Add tool labels to dropdown
        foreach (var tool in _currentTools)
        {
            if (_toolLabels.ContainsKey(tool))
            {
                optionLabels.Add(_toolLabels[tool]);
            }
            else
            {
                Debug.LogWarning($"MenuPanel: Missing tool label for {tool}");
                optionLabels.Add(tool.ToString());
            }
        }

        toolsDropdown.AddOptions(optionLabels);
        toolsDropdown.value = 0; // Reset to "Tools"
    }

    private void OnToolSelected(int index)
    {
        if (index == 0) // "Tools" option
        {
            return;
        }

        // Adjust index to account for the default option
        int toolIndex = index - 1;
        
        if (toolIndex >= 0 && toolIndex < _currentTools.Count)
        {
            ToolType selectedTool = _currentTools[toolIndex];
            HandleToolSelection(selectedTool);
            
            // Reset dropdown to default option after selection
            toolsDropdown.value = 0;
        }
    }

    private void HandleToolSelection(ToolType tool)
    {
        switch (tool)
        {
            case ToolType.AddSystem:
                Debug.Log("Add System tool selected - functionality not implemented");
                break;
            case ToolType.DeleteSystem:
                Debug.Log("Delete System tool selected - functionality not implemented");
                break;
            case ToolType.JumpRoutes:
                Debug.Log("Jump Routes tool selected - functionality not implemented");
                break;
            case ToolType.EditSystem:
                Debug.Log("Edit System tool selected - functionality not implemented");
                break;
            case ToolType.EditPlanets:
                Debug.Log("Edit Planets tool selected - functionality not implemented");
                break;
            case ToolType.EditStations:
                Debug.Log("Edit Stations tool selected - functionality not implemented");
                break;
            case ToolType.Measure:
                Debug.Log("Measure tool selected - functionality not implemented");
                break;
            case ToolType.Information:
                Debug.Log("Information tool selected - functionality not implemented");
                break;
        }
    }

    // View option handlers
    private void OnShowAnomaliesChanged(bool value)
    {
        Debug.Log($"Show Anomalies: {value} - functionality not implemented");
    }

    private void OnHighlightPopulatedChanged(bool value)
    {
        Debug.Log($"Highlight Populated: {value} - functionality not implemented");
    }

    private void OnViewOptionsButtonPressed()
    {
        Debug.Log("View Options button pressed - functionality not implemented");
    }
}