using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

/// <summary>
/// Manages transitions between different scenes (TitleScreen, GalaxyView, SystemView)
/// </summary>
public class SceneTransitionManager : MonoBehaviour
{
    // Scene prefabs (assign in Inspector)
    [Header("Scene Prefabs")]
    [SerializeField] private GameObject _titleScreenPrefab;
    [SerializeField] private GameObject _galaxyViewPrefab;
    [SerializeField] private GameObject _systemViewPrefab;
    
    // UI references (assign in Inspector)
    [Header("References")]
    [SerializeField] private Transform _sceneContainer;
    [SerializeField] private GameObject _menuPanel;

    private GameObject _currentScene;
    
    /// <summary>
    /// Show the title screen
    /// </summary>
    public void ShowTitleScreen()
    {
        TransitionToScene(_titleScreenPrefab);
        TitleScreen titleScreen = _currentScene.GetComponent<TitleScreen>();
        titleScreen.Initialize(FindFirstObjectByType<UIMain>());
        UpdateMenuContext();
    }
    
    /// <summary>
    /// Show the galaxy view with data
    /// </summary>
    public void ShowGalaxyView(List<StarSystem> galaxyData)
    {
        TransitionToScene(_galaxyViewPrefab);
        
        // Set galaxy data
        GalaxyView galaxyView = _currentScene.GetComponent<GalaxyView>();
        galaxyView.SetGalaxyData(galaxyData);
        
        StartCoroutine(UpdateMenuNextFrame());
    }
    
    private System.Collections.IEnumerator UpdateMenuNextFrame()
    {
        yield return null;
        UpdateMenuContext();
    }

    /// <summary>
    /// Show the system view
    /// </summary>
    public void ShowSystemView()
    {
        TransitionToScene(_systemViewPrefab);
        StartCoroutine(UpdateMenuNextFrame());
    }
    
    /// <summary>
    /// Generic method to transition to a new scene
    /// </summary>
    private void TransitionToScene(GameObject scenePrefab)
    {
        // Destroy current scene if exists
        if (_currentScene != null)
            Destroy(_currentScene);
        
        // Instantiate new scene
        _currentScene = Instantiate(scenePrefab, _sceneContainer);
    }
    
    /// <summary>
    /// Update menu context based on current scene
    /// </summary>
    private void UpdateMenuContext()
    {
        // If no current scene, nothing to update
        if (_currentScene == null) return;
        
        // Check if we have a menu panel
        if (_menuPanel == null) 
        {
            // Only log warning if we're in a scene that should have a menu
            if (_currentScene.GetComponent<GalaxyView>() != null || _currentScene.GetComponent<SystemView>() != null)
            {
                Debug.LogWarning("MenuPanel is null but current scene expects a menu");
            }
            return;
        }
        
        // Get the MenuPanel component
        MenuPanel menu = _menuPanel.GetComponent<MenuPanel>();
        if (menu == null) 
        {
            // Only log warning if we're in a scene that should have a menu
            if (_currentScene.GetComponent<GalaxyView>() != null || _currentScene.GetComponent<SystemView>() != null)
            {
                Debug.LogWarning("MenuPanel component not found on _menuPanel GameObject");
            }
            return;
        }
        
        // Update menu context based on current scene
        if (_currentScene.GetComponent<GalaxyView>() != null)
        {
            _menuPanel.SetActive(true);
            menu.SetContext(MenuPanel.MenuContext.GalaxyView);
        }
        else if (_currentScene.GetComponent<SystemView>() != null)
        {
            _menuPanel.SetActive(true);
            menu.SetContext(MenuPanel.MenuContext.SystemView);
        }
        else
        {
            // For title screen or other scenes, just deactivate the menu silently
            if (menu != null)
                menu.SetContext(MenuPanel.MenuContext.None);
            _menuPanel.SetActive(false);
        }
    }
    
    /// <summary>
    /// Get reference to current scene
    /// </summary>
    public GameObject GetCurrentScene()
    {
        return _currentScene;
    }
}