using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Starlight;

public class GalaxyView : MonoBehaviour
{
    [Header("References")]
    public Camera mainCamera;
    public StarManager starManager;
    public StarClickDetector starClickDetector;
    public Transform starContainer;
    
    [Header("UI Elements")]
    public GameObject systemInfoPanel;
    
    [Header("Star Rendering")]
    public float positionScale = 50.0f;
    public int maxVisibleStars = 50000;

    // Private fields
    private List<StarSystem> _galaxyData;
    private bool _isCreatingVisuals = false;
    private GalaxyCameraController _cameraController;
    private SystemInfoPanel _systemInfoController;
    private StarSystem _lastDoubleClickedSystem;
    private float _lastDoubleClickTime;
    private const float _doubleClickTransitionThreshold = 0.5f;
    
    void Awake()
    {
        // Initialize camera controller if needed
        if (mainCamera != null && mainCamera.GetComponent<GalaxyCameraController>() == null)
        {
            _cameraController = mainCamera.gameObject.AddComponent<GalaxyCameraController>();
        }
        else if (mainCamera != null)
        {
            _cameraController = mainCamera.GetComponent<GalaxyCameraController>();
        }
        
        // Initialize star container if needed
        if (starContainer == null)
        {
            starContainer = new GameObject("StarContainer").transform;
            starContainer.SetParent(transform);
        }
        
        // Initialize star manager if needed
        if (starManager == null)
        {
            GameObject starRendererObj = new GameObject("StarRenderer");
            starRendererObj.transform.SetParent(transform);
            starManager = starRendererObj.AddComponent<StarManager>();
        }
        
        // Initialize star click detector
        if (starClickDetector == null && starManager != null)
        {
            starClickDetector = starManager.gameObject.AddComponent<StarClickDetector>();
            starClickDetector.clickCamera = mainCamera;
        }
        
        // Check if there's a separate StarSelectionHandler component
        StarSelectionHandler selectionHandler = GetComponent<StarSelectionHandler>();
        if (selectionHandler != null)
        {
            // If we have a dedicated handler, initialize it with our references
            selectionHandler.galaxyView = this;
            selectionHandler.systemInfoPanel = _systemInfoController;
            selectionHandler.starClickDetector = starClickDetector;
        }
        
        // Initialize system info controller
        if (systemInfoPanel != null)
        {
            _systemInfoController = systemInfoPanel.GetComponent<SystemInfoPanel>();
            if (_systemInfoController == null)
            {
                _systemInfoController = systemInfoPanel.AddComponent<SystemInfoPanel>();
            }
        }
    }
    
    void Start()
    {
        // Connect star click events
        if (starClickDetector != null)
        {
            starClickDetector.OnStarClicked += OnStarClicked;
            starClickDetector.OnStarDoubleClicked += OnStarDoubleClicked;
        }
        
        // Check if we already have data to display
        if (_galaxyData != null && _galaxyData.Count > 0)
        {
            CreateVisualRepresentation();
        }
    }
    
    private IEnumerator LoadGalaxyDataDeferred(GalaxyDataStore dataStore)
    {
        _galaxyData = dataStore.GetAllSystems();
        
        // Wait for a frame to prevent UI freeze
        yield return null;
        
        // Create the visual representation
        CreateVisualRepresentation();
        
        // Wait another frame for visual representation to complete
        yield return null;
        
        // Ensure camera is properly positioned after everything is set up
        if (_cameraController != null)
        {
            _cameraController.ResetView();
        }
    }
    
    public void SetGalaxyData(List<StarSystem> data)
    {
        if (data == null || data.Count == 0)
        {
            Debug.LogError("Attempted to set null or empty galaxy data");
            _galaxyData = null; // Clear any existing data
            return;
        }

        try
        {
            _galaxyData = data;
            Debug.Log($"Galaxy data set successfully with {data.Count} star systems");
            
            // If we're already active, create the visual representation right away
            if (gameObject.activeInHierarchy)
            {
                CreateVisualRepresentation();
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to set galaxy data: {ex.Message}");
            _galaxyData = null;
        }
    }
    
    private void CreateVisualRepresentation()
    {
        if (_isCreatingVisuals || _galaxyData == null)
            return;
            
        try
        {
            _isCreatingVisuals = true;
            Debug.Log("Creating galaxy visual representation...");
            
            // Create starfield first
            CreateStarfield();
            
            // Create background stars
            CreateBackgroundStars();
            
            // Calculate galaxy bounds for camera setup after starfield is created
            CalculateGalaxyBounds();
            
            // Force camera reset to ensure proper positioning
            if (_cameraController != null)
            {
                _cameraController.ResetView();
            }
            
            Debug.Log("Galaxy visual representation created successfully");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Failed to create galaxy visual representation: {ex.Message}\n{ex.StackTrace}");
        }
        finally
        {
            _isCreatingVisuals = false;
        }
    }
    
    private void CalculateGalaxyBounds()
    {
        if (_galaxyData == null || _galaxyData.Count == 0)
            return;
            
        Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        
        foreach (var system in _galaxyData)
        {
            Vector3 position = system.Position * positionScale;
            
            min.x = Mathf.Min(min.x, position.x);
            min.y = Mathf.Min(min.y, position.y);
            min.z = Mathf.Min(min.z, position.z);
            
            max.x = Mathf.Max(max.x, position.x);
            max.y = Mathf.Max(max.y, position.y);
            max.z = Mathf.Max(max.z, position.z);
        }
        
        Vector3 center = (min + max) / 2;
        Vector3 size = max - min;
        
        // Update camera controller with galaxy bounds
        if (_cameraController != null)
        {
            _cameraController.SetGalaxyBounds(center, size);
        }
        else if (mainCamera != null)
        {
            // Position camera to view entire galaxy
            float maxDimension = Mathf.Max(size.x, size.y, size.z);
            float distance = maxDimension * 1.5f;
            
            // Position camera at the center of the galaxy
            mainCamera.transform.position = center + new Vector3(0, distance * 0.3f, distance);
            mainCamera.transform.LookAt(center);
        }
    }
    
    private Vector3 CalculateGalaxyCenter()
    {
        if (_galaxyData == null || _galaxyData.Count == 0)
            return Vector3.zero;
            
        Vector3 sum = Vector3.zero;
        foreach (var system in _galaxyData)
        {
            sum += system.Position;
        }
        return sum / _galaxyData.Count;
    }
    
    private void CreateStarfield()
    {
        if (_galaxyData == null || starManager == null)
            return;
        
        // Limit the number of stars to avoid performance issues
        int starCount = Mathf.Min(_galaxyData.Count, maxVisibleStars);
        
        // Create a list to store the stars
        List<Star> stars = new List<Star>(starCount);
        
        // Calculate galaxy center for proper 3D distribution
        Vector3 galaxyCenter = CalculateGalaxyCenter();
        
        for (int i = 0; i < starCount; i++)
        {
            StarSystem system = _galaxyData[i];
            
            // Get the primary star from the system
            StarStructure primaryStar = system.PrimaryStar;
            
            if (primaryStar != null)
            {
                // Create a proper 3D distribution by adding Z variance
                Vector3 position = system.Position * positionScale;
                
                // Add vertical spread to prevent "wall" effect
                if (Mathf.Abs(position.z) < 1f) // If Z is nearly flat
                {
                    float distanceFromCenter = Vector3.Distance(new Vector3(position.x, position.y, 0), new Vector3(galaxyCenter.x, galaxyCenter.y, 0));
                    float zVariance = Mathf.Clamp(distanceFromCenter * 0.1f, 10f, 500f); // Scale Z based on distance from center
                    position.z += UnityEngine.Random.Range(-zVariance, zVariance);
                }
                
                // Convert to Starlight Star format
                Star star = new Star(
                    position,
                    primaryStar.Luminosity,
                    primaryStar.Temperature
                );
                
                stars.Add(star);
            }
        }
        
        // Set stars in the Starlight StarManager
        starManager.SetStarList(stars);
        
    }
    
    private void CreateBackgroundStars()
    {
        // Check if we already have a background starfield or if BackgroundStarfield component exists
        if (transform.Find("BackgroundStars") != null || GetComponent<BackgroundStarfield>() != null)
            return;
            
        // Use the dedicated BackgroundStarfield component instead of creating another StarManager
        BackgroundStarfield backgroundStarfield = gameObject.AddComponent<BackgroundStarfield>();
        
        // Configure background starfield properties
        backgroundStarfield.StarCount = 2000;
        backgroundStarfield.SphereRadius = 5000f;
        backgroundStarfield.MinStarSize = 1f;
        backgroundStarfield.MaxStarSize = 3f;
    }
    
    private void OnStarClicked(Star star, Vector3 screenPos)
    {
        // Delegate to the StarSelectionHandler if it exists, otherwise handle locally
        StarSelectionHandler selectionHandler = GetComponent<StarSelectionHandler>();
        if (selectionHandler != null)
        {
            // Let the dedicated handler deal with it
            return;
        }
        
        // Fallback to local handling
        StarSystem selectedSystem = FindSystemForStar(star);
        if (selectedSystem != null && _systemInfoController != null)
        {
            _systemInfoController.ShowSystemInfo(selectedSystem);
        }
    }
    
    private void OnStarDoubleClicked(Star star, Vector3 screenPos)
    {
        // Delegate to the StarSelectionHandler if it exists, otherwise handle locally
        StarSelectionHandler selectionHandler = GetComponent<StarSelectionHandler>();
        if (selectionHandler != null)
        {
            // Let the dedicated handler deal with it
            return;
        }
        
        // Fallback to local handling
        StarSystem selectedSystem = FindSystemForStar(star);
        if (selectedSystem != null && _cameraController != null)
        {
            // Focus camera on the selected system
            _cameraController.LookAtPoint(selectedSystem.Position * positionScale);
            Debug.Log($"Camera focused on system: {selectedSystem.ID}");
            
            // Store selection for potential transition on second double-click
            if (_lastDoubleClickedSystem == selectedSystem && 
                Time.time - _lastDoubleClickTime < _doubleClickTransitionThreshold)
            {
                // This is a second double-click on the same system, transition to system view
                TransitionToSystemView(selectedSystem);
            }
            else
            {
                // First double-click, store the system
                _lastDoubleClickedSystem = selectedSystem;
                _lastDoubleClickTime = Time.time;
            }
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
    
    public StarSystem FindSystemForStar(Star star)
    {
        if (_galaxyData == null || _galaxyData.Count == 0)
            return null;
            
        // Find the system closest to the star's position
        float closestDistSq = float.MaxValue;
        StarSystem closestSystem = null;
        
        foreach (var system in _galaxyData)
        {
            // Compare scaled position
            Vector3 scaledPos = system.Position * positionScale;
            float distSq = (scaledPos - star.Position).sqrMagnitude;
            
            if (distSq < closestDistSq)
            {
                closestDistSq = distSq;
                closestSystem = system;
            }
        }
        
        // Use a distance threshold to ensure we found the right system
        float threshold = 1.0f; // Increase threshold to match StarSelectionHandler
        
        if (closestDistSq <= threshold * threshold)
        {
            return closestSystem;
        }
        
        return null;
    }
    
    // Public property to expose galaxy data to other components
    public List<StarSystem> GalaxyData => _galaxyData;
    
    private void OnDestroy()
    {
        // Disconnect events
        if (starClickDetector != null)
        {
            starClickDetector.OnStarClicked -= OnStarClicked;
            starClickDetector.OnStarDoubleClicked -= OnStarDoubleClicked;
        }
    }
}