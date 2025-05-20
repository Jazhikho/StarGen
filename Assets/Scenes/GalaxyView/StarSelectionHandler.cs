using UnityEngine;
using Starlight;
using System.Collections.Generic;

public class StarSelectionHandler : MonoBehaviour
{
    [Header("References")]
    public GalaxyView galaxyView;
    public SystemInfoPanel systemInfoPanel;
    public StarClickDetector starClickDetector;
    
    void Start()
    {
        if (starClickDetector != null)
        {
            starClickDetector.OnStarClicked += OnStarClicked;
            starClickDetector.OnStarDoubleClicked += OnStarDoubleClicked;
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
            systemInfoPanel.ShowSystemInfo(selectedSystem);
        }
    }
    
    private void OnStarDoubleClicked(Star star, Vector3 screenPos)
    {
        StarSystem selectedSystem = FindSystemForStar(star);
        
        if (selectedSystem != null)
        {
            // TODO: Implement system view transition
            Debug.Log($"Double-clicked system: {selectedSystem.ID}");
        }
    }
    
    private StarSystem FindSystemForStar(Star star)
    {
        GalaxyDataStore dataStore = GalaxyDataStore.Instance;
        if (dataStore == null || !dataStore.HasGalaxyData())
            return null;
            
        List<StarSystem> systems = dataStore.GetAllSystems();
        
        float closestDistSq = float.MaxValue;
        StarSystem closestSystem = null;
        
        foreach (var system in systems)
        {
            Vector3 scaledPos = system.Position * galaxyView.positionScale;
            float distSq = (scaledPos - star.Position).sqrMagnitude;
            
            if (distSq < closestDistSq)
            {
                closestDistSq = distSq;
                closestSystem = system;
            }
        }
        
        // Distance threshold
        float threshold = 1.0f;
        
        if (closestDistSq <= threshold * threshold)
        {
            return closestSystem;
        }
        
        return null;
    }
}