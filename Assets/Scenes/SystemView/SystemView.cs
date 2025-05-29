using UnityEngine;

public class SystemView : MonoBehaviour
{
    [Header("References")]
    public Camera mainCamera;
    public Transform systemContainer;
    
    [Header("System Components")]
    public StarSystemManager systemManager;
    
    [Header("Camera Control")]
    public float initialCameraDistance = 100f;
    public Vector3 initialCameraOffset = new Vector3(0, 20f, -50f);
    
    // System data
    private StarSystem _currentSystem;
    private SystemCameraController _cameraController;
    private StarSystemData _currentSystemData;
    
    private void Awake()
    {
        // Initialize components if they don't exist
        if (mainCamera == null)
            mainCamera = Camera.main;
        
        // Get or add camera controller
        if (mainCamera != null)
        {
            _cameraController = mainCamera.GetComponent<SystemCameraController>();
            if (_cameraController == null)
            {
                _cameraController = mainCamera.gameObject.AddComponent<SystemCameraController>();
            }
        }
            
        if (systemContainer == null)
        {
            systemContainer = new GameObject("SystemContainer").transform;
            systemContainer.SetParent(transform);
        }
        
        if (systemManager == null)
        {
            GameObject systemManagerObj = new GameObject("StarSystemManager");
            systemManagerObj.transform.SetParent(systemContainer);
            systemManager = systemManagerObj.AddComponent<StarSystemManager>();
        }
    }
    
    void Start()
    {
        Debug.Log("SystemView started");
        
        // Position camera for system viewing (fallback position)
        if (mainCamera != null)
        {
            mainCamera.transform.position = initialCameraOffset;
            mainCamera.transform.LookAt(Vector3.zero);
        }
    }
    
    public void SetSystemData(StarSystem system)
    {
        _currentSystem = system;
        Debug.Log($"SystemView: Loading system {system?.ID ?? "null"}");
        
        if (system != null)
        {
            // Convert to visualization data
            _currentSystemData = StarSystemDataConverter.ConvertToVisualizationData(system);
            
            if (systemManager != null && _currentSystemData != null)
            {
                systemManager.GenerateSystemFromData(system);
                
                // Set up camera based on system type
                SetupCameraForSystem();
            }
            else
            {
                Debug.LogError("SystemManager or converted SystemData is null");
            }
        }
    }
    
    private void SetupCameraForSystem()
    {
        if (_currentSystemData == null || _cameraController == null || _currentSystemData.rootBody == null)
        {
            Debug.LogWarning("Cannot setup camera - missing required data");
            return;
        }
        
        var rootBody = _currentSystemData.rootBody;
        
        switch (rootBody.bodyType)
        {
            case CelestialBodyType.Star:
                SetupCameraForSingleStar(rootBody);
                break;
                
            case CelestialBodyType.Barycenter:
                SetupCameraForBarycenter(rootBody);
                break;
                
            default:
                SetupCameraWithSystemBounds();
                break;
        }
    }
    
    private void SetupCameraForSingleStar(CelestialBodyData starData)
    {
        // For a single star system, focus on the star but back off to show planets
        float maxPlanetDistance = GetMaxSatelliteDistance(starData);
        
        if (maxPlanetDistance > 0)
        {
            // Back off to show the full planetary system
            float viewDistance = maxPlanetDistance * 1.5f;
            Vector3 size = Vector3.one * (viewDistance * 2f);
            _cameraController.SetSystemBounds(Vector3.zero, size);
        }
        else
        {
            // No planets, just focus on the star
            float starRadius = starData.scale;
            float viewDistance = starRadius * 10f; // Good viewing distance for a lone star
            Vector3 size = Vector3.one * (viewDistance * 2f);
            _cameraController.SetSystemBounds(Vector3.zero, size);
        }
        
        Debug.Log($"Camera setup for single star system, max planet distance: {maxPlanetDistance}");
    }
    
    private void SetupCameraForBarycenter(CelestialBodyData barycenterData)
    {
        // For barycenter, we need to see both orbiting objects
        float maxOrbitRadius = 0f;
        float maxSystemRadius = 0f;
        
        // Find the maximum orbit radius of direct satellites (the binary components)
        foreach (var satellite in barycenterData.satellites)
        {
            maxOrbitRadius = Mathf.Max(maxOrbitRadius, satellite.semiMajorAxis);
            
            // Also check for any planets around each component
            float componentSystemRadius = GetMaxSatelliteDistance(satellite);
            maxSystemRadius = Mathf.Max(maxSystemRadius, satellite.semiMajorAxis + componentSystemRadius);
        }
        
        // Use the larger of the two to determine camera distance
        float systemRadius = Mathf.Max(maxOrbitRadius * 2f, maxSystemRadius);
        
        // Back off enough to see the full binary system (1 AU = 10 Unity units)
        float viewDistance = systemRadius * 2f;
        Vector3 size = Vector3.one * (viewDistance * 2f);
        
        _cameraController.SetSystemBounds(Vector3.zero, size);
        
        Debug.Log($"Camera setup for barycenter system, orbit radius: {maxOrbitRadius}, system radius: {systemRadius}");
    }
    
    private void SetupCameraWithSystemBounds()
    {
        // Fallback method using original calculation
        float maxDistance = CalculateMaxSystemDistance();
        
        if (maxDistance > 0)
        {
            Vector3 center = Vector3.zero;
            Vector3 size = Vector3.one * (maxDistance * 2.5f);
            _cameraController.SetSystemBounds(center, size);
        }
        else
        {
            // Ultimate fallback
            _cameraController.SetSystemBounds(Vector3.zero, Vector3.one * 50f);
        }
        
        Debug.Log($"Camera setup with system bounds, max distance: {maxDistance}");
    }
    
    private float GetMaxSatelliteDistance(CelestialBodyData body)
    {
        float maxDistance = 0f;
        
        foreach (var satellite in body.satellites)
        {
            maxDistance = Mathf.Max(maxDistance, satellite.semiMajorAxis);
            
            // Recursively check satellites of satellites
            float subDistance = GetMaxSatelliteDistance(satellite);
            maxDistance = Mathf.Max(maxDistance, satellite.semiMajorAxis + subDistance);
        }
        
        return maxDistance;
    }
    
    private float CalculateMaxSystemDistance()
    {
        if (_currentSystem == null) return 0f;
        
        float maxDistance = 0f;
        
        // Check star planets
        foreach (var star in _currentSystem.Stars)
        {
            foreach (var planet in star.Planets)
            {
                float scaledDistance = planet.SemiMajorAxis * StarSystemDataConverter.AU_TO_UNITY_DISTANCE;
                maxDistance = Mathf.Max(maxDistance, scaledDistance);
            }
        }
        
        // Check binary pairs
        foreach (var pair in _currentSystem.BinaryPairs)
        {
            float scaledSeparation = pair.SeparationDistance * StarSystemDataConverter.AU_TO_UNITY_DISTANCE;
            maxDistance = Mathf.Max(maxDistance, scaledSeparation);
            
            foreach (var planet in pair.CircumbinaryPlanets)
            {
                float scaledDistance = planet.SemiMajorAxis * StarSystemDataConverter.AU_TO_UNITY_DISTANCE;
                maxDistance = Mathf.Max(maxDistance, scaledDistance);
            }
        }
        
        return maxDistance;
    }
    
    public void ShowSystemDetails()
    {
        if (_currentSystem != null)
        {
            Debug.Log($"System: {_currentSystem.ID}");
            Debug.Log($"Stars: {_currentSystem.Stars.Count}");
            Debug.Log($"Binary Pairs: {_currentSystem.BinaryPairs.Count}");
            Debug.Log($"Total Planets: {GetTotalPlanetCount()}");
            Debug.Log($"Root Body Type: {_currentSystemData?.rootBody?.bodyType}");
        }
    }
    
    private int GetTotalPlanetCount()
    {
        int count = 0;
        foreach (var star in _currentSystem.Stars)
        {
            count += star.Planets.Count;
        }
        foreach (var pair in _currentSystem.BinaryPairs)
        {
            count += pair.CircumbinaryPlanets.Count;
        }
        return count;
    }
    
    public void OnEditModeEnabled()
    {
        Debug.Log("SystemView: Edit mode enabled");
        // TODO: Implement edit mode functionality
    }
    
    public void OnEditModeDisabled()
    {
        Debug.Log("SystemView: Edit mode disabled");
        // TODO: Implement edit mode functionality
    }
    
    // Method to manually recalculate camera bounds if needed
    public void RecalculateCameraBounds()
    {
        SetupCameraForSystem();
    }
    
    // Public property to expose current system data
    public StarSystem CurrentSystem => _currentSystem;
}