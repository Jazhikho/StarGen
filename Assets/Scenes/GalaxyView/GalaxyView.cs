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
    public float positionScale = 5.0f;
    public int maxVisibleStars = 50000;
    public Material starMaterial; // Optional if not referenced in StarManager
    
    // Private fields
    private List<StarSystem> _galaxyData;
    private bool _isCreatingVisuals = false;
    private GalaxyCameraController _cameraController;
    private SystemInfoPanel _systemInfoController;
    
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
            
            // Initialize with default mesh and material if available
            starManager.starMesh = Resources.GetBuiltinResource<Mesh>("Quad.fbx");
            if (starMaterial != null)
                starManager.starMaterial = starMaterial;
        }
        
        // Initialize star click detector
        if (starClickDetector == null && starManager != null)
        {
            starClickDetector = starManager.gameObject.AddComponent<StarClickDetector>();
            starClickDetector.clickCamera = mainCamera;
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
    }
    
    public void SetGalaxyData(List<StarSystem> data)
    {
        if (data == null || data.Count == 0)
        {
            Debug.LogError("Attempted to set null or empty galaxy data");
            return;
        }

        _galaxyData = data;
        
        // If we're already active, create the visual representation right away
        if (gameObject.activeInHierarchy)
        {
            CreateVisualRepresentation();
        }
    }
    
    private void CreateVisualRepresentation()
    {
        if (_isCreatingVisuals || _galaxyData == null)
            return;
            
        _isCreatingVisuals = true;
        
        // Calculate galaxy bounds for camera setup
        CalculateGalaxyBounds();
        
        // Create starfield
        CreateStarfield();
        
        // Create background stars
        CreateBackgroundStars();
        
        _isCreatingVisuals = false;
    }
    
    private void CalculateGalaxyBounds()
    {
        if (_galaxyData == null || _galaxyData.Count == 0)
            return;
            
        Vector3 min = new Vector3(float.MaxValue, float.MaxValue, float.MaxValue);
        Vector3 max = new Vector3(float.MinValue, float.MinValue, float.MinValue);
        
        foreach (var system in _galaxyData)
        {
            Vector3 position = system.Position;
            
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
            
            // Position camera
            mainCamera.transform.position = center + new Vector3(0, distance * 0.3f, distance);
            mainCamera.transform.LookAt(center);
        }
    }
    
    private void CreateStarfield()
    {
        if (_galaxyData == null || starManager == null)
            return;
        
        // Limit the number of stars to avoid performance issues
        int starCount = Mathf.Min(_galaxyData.Count, maxVisibleStars);
        
        // Create a list to store the stars
        List<Star> stars = new List<Star>(starCount);
        
        for (int i = 0; i < starCount; i++)
        {
            StarSystem system = _galaxyData[i];
            
            // Get the primary star from the system
            StarStructure primaryStar = system.PrimaryStar;
            
            if (primaryStar != null)
            {
                // Convert to Starlight Star format
                Star star = new Star(
                    system.Position * positionScale, // Scale position for better visibility
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
        // Check if we already have a background starfield
        if (transform.Find("BackgroundStars") != null)
            return;
            
        // Create a new GameObject for background stars
        GameObject backgroundObj = new GameObject("BackgroundStars");
        backgroundObj.transform.SetParent(transform);
        
        // Add a StarManager component
        StarManager backgroundStarManager = backgroundObj.AddComponent<StarManager>();
        
        // Copy settings from main star manager
        if (starManager != null)
        {
            backgroundStarManager.starMaterial = starManager.starMaterial;
            backgroundStarManager.starMesh = starManager.starMesh;
            backgroundStarManager.emissionTint = starManager.emissionTint;
            backgroundStarManager.blurAmount = starManager.blurAmount;
            backgroundStarManager.emissionEnergy = starManager.emissionEnergy * 0.1f; // Dimmer
            backgroundStarManager.billboardSizeDeg = starManager.billboardSizeDeg * 0.5f; // Smaller
        }
        
        // Create random background stars
        List<Star> backgroundStars = new List<Star>(1000);
        float radius = 2000f;
        
        for (int i = 0; i < 1000; i++)
        {
            Vector3 randomDir = Random.onUnitSphere;
            Vector3 position = randomDir * radius;
            
            float luminosity = Random.Range(0.05f, 0.5f);
            float temperature = Random.Range(3000f, 12000f);
            
            Star star = new Star(position, luminosity, temperature);
            backgroundStars.Add(star);
        }
        
        backgroundStarManager.SetStarList(backgroundStars);
    }
    
    private void OnStarClicked(Star star, Vector3 screenPos)
    {
        StarSystem selectedSystem = FindSystemForStar(star);
        
        if (selectedSystem != null && _systemInfoController != null)
        {
            _systemInfoController.ShowSystemInfo(selectedSystem);
        }
    }
    
    private void OnStarDoubleClicked(Star star, Vector3 screenPos)
    {
        StarSystem selectedSystem = FindSystemForStar(star);
        
        if (selectedSystem != null && _cameraController != null)
        {
            // Focus camera on the selected system
            _cameraController.LookAtPoint(selectedSystem.Position * positionScale);
            
            // Here you could also implement a zoom-in effect
            Debug.Log($"Double-clicked on system: {selectedSystem.ID}");
        }
    }
    
    private StarSystem FindSystemForStar(Star star)
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
        float threshold = 0.1f * 0.1f; // Square the threshold
        
        if (closestDistSq <= threshold)
        {
            return closestSystem;
        }
        
        return null;
    }
    
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