using UnityEngine;
using Starlight;
using System.Collections.Generic;

public class StarSelectionHandler : MonoBehaviour
{
    [Header("References")]
    public GalaxyView galaxyView;
    public SystemInfoPanel systemInfoPanel;
    public StarClickDetector starClickDetector;
    
    [Header("Camera")]
    public GalaxyCameraController cameraController;
    
    [Header("Double Click Settings")]
    public float doubleClickTimeThreshold = 0.5f;
    
    private StarSystem _selectedSystem;
    private float _lastSystemClickTime;
    private bool _isFocusedOnSystem = false;
    
    void Start()
    {
        if (starClickDetector != null)
        {
            starClickDetector.OnStarClicked += OnStarClicked;
            starClickDetector.OnStarDoubleClicked += OnStarDoubleClicked;
        }
        
        // Try to get camera controller if not assigned
        if (cameraController == null && galaxyView != null && galaxyView.mainCamera != null)
        {
            cameraController = galaxyView.mainCamera.GetComponent<GalaxyCameraController>();
        }
    }
    
    void OnDestroy()
    {
        if (starClickDetector != null)
        {
            starClickDetector.OnStarClicked -= OnStarClicked;
            starClickDetector.OnStarDoubleClicked -= OnStarDoubleClicked;
        }
    }
    
    private void OnStarClicked(Star star, Vector3 screenPos)
    {
        StarSystem selectedSystem = FindSystemForStar(star);
        
        if (selectedSystem != null && systemInfoPanel != null)
        {
            _selectedSystem = selectedSystem;
            systemInfoPanel.ShowSystemInfo(selectedSystem);
        }
    }
    
    private void OnStarDoubleClicked(Star star, Vector3 screenPos)
    {
        StarSystem selectedSystem = FindSystemForStar(star);
        
        if (selectedSystem != null)
        {
            bool isDoubleClickOnSameSystem = (_selectedSystem == selectedSystem);
            
            // Update selected system and focus camera
            _selectedSystem = selectedSystem;
            
            if (cameraController != null)
            {
                // Focus camera on the system
                Vector3 systemPosition = selectedSystem.Position * galaxyView.positionScale;
                cameraController.LookAtPoint(systemPosition);
                
                // If already focused on this system and this is a second double-click within threshold,
                // transition to system view
                if (_isFocusedOnSystem && isDoubleClickOnSameSystem && 
                    (Time.time - _lastSystemClickTime < doubleClickTimeThreshold))
                {
                    TransitionToSystemView(selectedSystem);
                }
                else
                {
                    // First focus - just point camera at system
                    _isFocusedOnSystem = true;
                    Debug.Log($"Camera focused on system: {selectedSystem.ID}");
                }
            }
            else
            {
                // If no camera controller but already focused on system, transition anyway
                if (_isFocusedOnSystem && isDoubleClickOnSameSystem && 
                    (Time.time - _lastSystemClickTime < doubleClickTimeThreshold))
                {
                    TransitionToSystemView(selectedSystem);
                }
                else
                {
                    _isFocusedOnSystem = true;
                    Debug.Log($"Selected system: {selectedSystem.ID} (no camera controller available)");
                }
            }
            
            _lastSystemClickTime = Time.time;
        }
    }
    
    private void TransitionToSystemView(StarSystem system)
    {
        Debug.Log($"Transitioning to system view for: {system.ID}");
        
        // Find the SceneTransitionManager
        SceneTransitionManager sceneManager = FindFirstObjectByType<SceneTransitionManager>();
        if (sceneManager != null)
        {
            sceneManager.ShowSystemView(system);
        }
        else
        {
            Debug.LogError("Cannot transition to SystemView: SceneTransitionManager not found");
        }
    }
    
    private StarSystem FindSystemForStar(Star star)
    {
        if (galaxyView != null)
        {
            return galaxyView.FindSystemForStar(star);
        }
        
        return null;
    }
}