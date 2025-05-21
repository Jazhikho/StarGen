using UnityEngine;
using System.Text;

namespace Starlight
{
    [RequireComponent(typeof(StarManager))]
    public class StarClickDetector : MonoBehaviour
    {
        [Header("Click Settings")]
        public Camera clickCamera;
        public float clickRadius = 0.01f; // As fraction of screen height
        
        [Header("Debug")]
        public bool debugClicks = true;
        
        // Events
        public System.Action<Star, Vector3> OnStarClicked;
        public System.Action<Star, Vector3> OnStarDoubleClicked;
        
        private StarManager _starManager;
        private float _lastClickTime;
        private Star _lastClickedStar;
        private const float DOUBLE_CLICK_TIME = 0.3f; // seconds
        
        private void Awake()
        {
            _starManager = GetComponent<StarManager>();
            
            if (clickCamera == null)
                clickCamera = Camera.main;
        }
        
        private void Update()
        {
            if (Input.GetMouseButtonDown(0))
            {
                CheckStarClick(Input.mousePosition);
            }
        }
        
        private void CheckStarClick(Vector3 screenPos)
        {
            if (clickCamera == null || _starManager == null || _starManager.StarCount == 0)
                return;
                
            float closestDistance = float.MaxValue;
            Star closestStar = null;
            Vector3 closestStarScreenPos = Vector3.zero;
            float clickableRadius = Screen.height * clickRadius;
            
            // Performance optimization: only check stars within frustum
            Plane[] frustumPlanes = GeometryUtility.CalculateFrustumPlanes(clickCamera);
            
            // Find the closest star to the mouse cursor
            foreach (var star in _starManager.Stars)
            {
                // Early frustum culling - skip stars outside camera view
                if (!GeometryUtility.TestPlanesAABB(frustumPlanes, new Bounds(star.Position, Vector3.one * 0.1f)))
                    continue;
                
                // Convert star position to screen space
                Vector3 starScreenPos = clickCamera.WorldToScreenPoint(star.Position);
                
                // Skip stars behind the camera
                if (starScreenPos.z < 0)
                    continue;
                    
                // Quick distance check before pixel calculation
                float screenDistSqr = (new Vector2(screenPos.x, screenPos.y) - new Vector2(starScreenPos.x, starScreenPos.y)).sqrMagnitude;
                if (screenDistSqr > clickableRadius * clickableRadius)
                    continue;
                
                // More precise distance calculation only for nearby stars
                float distInPixels = Mathf.Sqrt(screenDistSqr);
                
                // Check if within clickable radius and closer than previous candidates
                if (distInPixels <= clickableRadius && starScreenPos.z < closestDistance)
                {
                    closestDistance = starScreenPos.z;
                    closestStar = star;
                    closestStarScreenPos = starScreenPos;
                }
            }
            
            if (closestStar != null)
            {
                if (debugClicks)
                {
                    // Calculate color from temperature for debugging
                    Color starColor = StarManager.BlackbodyToRGB(closestStar.Temperature);
                    
                    // Find the system this star belongs to
                    StarSystem system = FindSystemForStar(closestStar);
                    if (system != null)
                    {
                        StringBuilder systemInfo = new StringBuilder();
                        systemInfo.AppendLine($"Clicked system: {system.ID}");
                        systemInfo.AppendLine($"System Position: {system.Position}");
                        systemInfo.AppendLine($"Population: {system.SystemPopulation}");
                        systemInfo.AppendLine($"Total Stars: {system.Stars.Count}");
                        
                        // List all stars in the system
                        systemInfo.AppendLine("\n--- Stars ---");
                        foreach (var sysstar in system.Stars)
                        {
                            systemInfo.AppendLine($"Star ID: {sysstar.ID}");
                            systemInfo.AppendLine($"  Spectral Class: {sysstar.SpectralClass}");
                            systemInfo.AppendLine($"  Mass: {sysstar.Mass:F2} M☉");
                            systemInfo.AppendLine($"  Luminosity: {sysstar.Luminosity:F2} L☉");
                            systemInfo.AppendLine($"  Temperature: {sysstar.Temperature:F0} K");
                            systemInfo.AppendLine($"  Radius: {sysstar.Radius:F2} R☉");
                            
                            // List planets
                            if (sysstar.Planets.Count > 0)
                            {
                                systemInfo.AppendLine($"  --- Planets ({sysstar.Planets.Count}) ---");
                                foreach (var planet in sysstar.Planets)
                                {
                                    systemInfo.AppendLine($"    {planet.ID}: {planet.PlanetType}");
                                    systemInfo.AppendLine($"      Mass: {planet.Mass:F2} M⊕, Radius: {planet.Radius:F2} R⊕");
                                    systemInfo.AppendLine($"      Semi-Major Axis: {planet.SemiMajorAxis:F2} AU");
                                    
                                    // List moons if present
                                    if (planet.Moons != null && planet.Moons.Count > 0)
                                    {
                                        systemInfo.AppendLine($"      --- Moons ({planet.Moons.Count}) ---");
                                        foreach (var moon in planet.Moons)
                                        {
                                            systemInfo.AppendLine($"        {moon.ID}: {moon.PlanetType}");
                                        }
                                    }
                                }
                            }
                            
                            // List asteroid belts
                            if (sysstar.AsteroidBelts.Count > 0)
                            {
                                systemInfo.AppendLine($"  --- Asteroid Belts ({sysstar.AsteroidBelts.Count}) ---");
                                foreach (var belt in sysstar.AsteroidBelts)
                                {
                                    systemInfo.AppendLine($"    {belt.ID}: Inner: {belt.InnerRadius:F2} AU, Outer: {belt.OuterRadius:F2} AU");
                                }
                            }
                        }
                        
                        // Print binary pairs if present
                        if (system.BinaryPairs != null && system.BinaryPairs.Count > 0)
                        {
                            systemInfo.AppendLine("\n--- Binary Pairs ---");
                            foreach (var pair in system.BinaryPairs)
                            {
                                systemInfo.AppendLine($"Binary Pair ID: {pair.ID}");
                                systemInfo.AppendLine($"  Primary: {pair.PrimaryID}");
                                systemInfo.AppendLine($"  Secondary: {pair.SecondaryID}");
                                systemInfo.AppendLine($"  Separation: {pair.SeparationDistance:F2} AU");
                                
                                // Add circumbinary planets if any
                                if (pair.CircumbinaryPlanets != null && pair.CircumbinaryPlanets.Count > 0)
                                {
                                    systemInfo.AppendLine($"  --- Circumbinary Planets ({pair.CircumbinaryPlanets.Count}) ---");
                                    foreach (var planet in pair.CircumbinaryPlanets)
                                    {
                                        systemInfo.AppendLine($"    {planet.ID}: {planet.PlanetType}");
                                        systemInfo.AppendLine($"      Mass: {planet.Mass:F2} M⊕, Radius: {planet.Radius:F2} R⊕");
                                        systemInfo.AppendLine($"      Semi-Major Axis: {planet.SemiMajorAxis:F2} AU");
                                    }
                                }
                            }
                        }
                        
                        Debug.Log(systemInfo.ToString());
                    }
                    else
                    {
                        // Fall back to basic star info if no system is found
                        Debug.Log($"Clicked star at {closestStar.Position}\n" +
                                  $"Temperature: {closestStar.Temperature}K\n" + 
                                  $"Luminosity: {closestStar.Luminosity} L☉\n" +
                                  $"Color: RGB({starColor.r:F2}, {starColor.g:F2}, {starColor.b:F2})\n" +
                                  $"Distance from camera: {Vector3.Distance(clickCamera.transform.position, closestStar.Position):F2} units");
                    }
                }
                
                // Check for double click
                bool isDoubleClick = (Time.time - _lastClickTime < DOUBLE_CLICK_TIME) && 
                                     (_lastClickedStar == closestStar);
                
                if (isDoubleClick)
                {
                    OnStarDoubleClicked?.Invoke(closestStar, closestStarScreenPos);
                    if (debugClicks)
                        Debug.Log("Double-click detected!");
                }
                else
                {
                    OnStarClicked?.Invoke(closestStar, closestStarScreenPos);
                }
                
                _lastClickTime = Time.time;
                _lastClickedStar = closestStar;
            }
        }
        
        private StarSystem FindSystemForStar(Star star)
        {
            // Try to find GalaxyView component
            GalaxyView galaxyView = FindFirstObjectByType<GalaxyView>();
            if (galaxyView != null)
            {
                return galaxyView.FindSystemForStar(star);
            }
            
            // If no GalaxyView, try to find StarSelectionHandler which might have access to systems
            StarSelectionHandler selectionHandler = FindFirstObjectByType<StarSelectionHandler>();
            if (selectionHandler != null && selectionHandler.galaxyView != null)
            {
                return selectionHandler.galaxyView.FindSystemForStar(star);
            }
            
            return null;
        }
    }
}