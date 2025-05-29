using System.Collections.Generic;
using UnityEngine;
using SimpleKeplerOrbits;

public class StarSystemManager : MonoBehaviour
{
    [Header("Prefab Settings")]
    [SerializeField] private PlanetModel planetModelPrefab;
    [SerializeField] private StarModel starModelPrefab;
    [SerializeField] private GameObject asteroidBeltPrefab;
    [SerializeField] private GameObject notableAsteroidPrefab;
    
    [Header("Materials")]
    [SerializeField] private Material starMaterial;
    [SerializeField] private Material planetMaterial;
    [SerializeField] private Material moonMaterial;
    [SerializeField] private Material asteroidMaterial;
    
    [Header("Visual Settings")]
    [SerializeField] private bool showOrbits = true;
    [SerializeField] private bool showLabels = true;
    [SerializeField] private float orbitLineWidth = 0.1f;
    
    [Header("System Data")]
    [SerializeField] private StarSystemData currentSystemData;
    
    private Dictionary<string, GameObject> celestialBodies = new Dictionary<string, GameObject>();
    private GameObject systemRoot;
    
    /// <summary>
    /// Generates a system from raw StarSystem data
    /// </summary>
    public void GenerateSystemFromData(StarSystem starSystemData)
    {
        if (starSystemData == null)
        {
            Debug.LogWarning("No star system data provided!");
            return;
        }
        
        // Convert to visualization format
        var visualData = StarSystemDataConverter.ConvertToVisualizationData(starSystemData);
        GenerateSystem(visualData);
    }
    
    /// <summary>
    /// Generates a system from StarSystemData
    /// </summary>
    public void GenerateSystem(StarSystemData systemData)
    {
        ClearCurrentSystem();
        currentSystemData = systemData;
        
        if (systemData == null || systemData.rootBody == null)
        {
            Debug.LogWarning("No system data provided!");
            return;
        }
        
        // Create system root
        systemRoot = new GameObject(systemData.systemName);
        systemRoot.transform.parent = transform;
        
        // Build the system recursively
        CreateCelestialBody(systemData.rootBody, null, systemRoot.transform, systemData.gravityConstant);
    }
    
    private GameObject CreateCelestialBody(CelestialBodyData bodyData, GameObject attractor, 
        Transform parent, float gConst)
    {
        GameObject bodyObject = null;
        
        // Create the appropriate game object based on type
        switch (bodyData.bodyType)
        {
            case CelestialBodyType.Barycenter:
                bodyObject = CreateBarycenter(bodyData);
                break;
                
            case CelestialBodyType.Star:
                bodyObject = CreateStar(bodyData);
                break;
                
            case CelestialBodyType.Planet:
                bodyObject = CreatePlanet(bodyData);
                break;
                
            case CelestialBodyType.Moon:
                bodyObject = CreateMoon(bodyData);
                break;
                
            case CelestialBodyType.Asteroid:
                bodyObject = CreateAsteroidBelt(bodyData);
                break;
        }
        
        if (bodyObject == null) return null;
        
        bodyObject.transform.parent = parent;
        celestialBodies[bodyData.name] = bodyObject;
        
        // Set up orbit if this is not the root
        if (attractor != null)
        {
            SetupOrbit(bodyObject, bodyData, attractor, gConst);
        }
        
        // Create satellites
        foreach (var satelliteData in bodyData.satellites)
        {
            CreateCelestialBody(satelliteData, bodyObject, bodyObject.transform, gConst);
        }
        
        return bodyObject;
    }
    
    private GameObject CreateBarycenter(CelestialBodyData bodyData)
    {
        GameObject barycenter = new GameObject(bodyData.name);
        
        // Add a very small visual indicator
        GameObject indicator = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        indicator.transform.parent = barycenter.transform;
        indicator.transform.localScale = Vector3.one * 0.01f;
        
        var renderer = indicator.GetComponent<Renderer>();
        if (renderer != null)
        {
            renderer.material.color = Color.gray;
        }
        
        if (showLabels)
        {
            AddLabel(barycenter, bodyData.name);
        }
        
        return barycenter;
    }
    
    private GameObject CreateStar(CelestialBodyData bodyData)
    {
        GameObject star = Instantiate(starModelPrefab.gameObject);
        var starModel = star.GetComponent<StarModel>();
        
        if (starModel != null)
        {
            // Configure star settings based on data
            ConfigureStarModel(starModel, bodyData);
            
            // Generate the star after configuration
            try
            {
                starModel.GenerateStar();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error generating star {bodyData.name}: {e.Message}\n{e.StackTrace}");
            }
        }
        else
        {
            Debug.LogError($"StarModel component missing on instantiated star prefab for {bodyData.name}");
        }
        
        star.name = bodyData.name;
        
        SetupStarLight(star, bodyData);
        
        if (showLabels)
        {
            AddLabel(star, bodyData.name);
        }
        
        return star;
    }
    
    private GameObject CreatePlanet(CelestialBodyData bodyData)
    {
        GameObject planet = Instantiate(planetModelPrefab.gameObject);
        var planetModel = planet.GetComponent<PlanetModel>();
        
        if (planetModel != null)
        {
            // Configure planet settings based on data
            ConfigurePlanetModel(planetModel, bodyData);
            
            // Generate the planet after configuration
            try
            {
                planetModel.GeneratePlanet();
            }
            catch (System.Exception e)
            {
                Debug.LogError($"Error generating planet {bodyData.name}: {e.Message}\n{e.StackTrace}");
            }
        }
        else
        {
            Debug.LogError($"PlanetModel component missing on instantiated planet prefab for {bodyData.name}");
        }
        
        planet.name = bodyData.name;
        
        if (showLabels)
        {
            AddLabel(planet, bodyData.name);
        }
        
        return planet;
    }
    
    private GameObject CreateMoon(CelestialBodyData bodyData)
    {
        // For now, moons use simple spheres - could extend to use planet models later
        return CreatePlanet(bodyData);
    }
    
    private GameObject CreateAsteroidBelt(CelestialBodyData bodyData)
    {
        GameObject beltObject = null;
        
        if (asteroidBeltPrefab != null)
        {
            beltObject = Instantiate(asteroidBeltPrefab);
        }
        else
        {
            // Create a default object with particle system if prefab is missing
            beltObject = new GameObject(bodyData.name);
            beltObject.AddComponent<ParticleSystem>();
        }
        
        beltObject.name = bodyData.name;
        
        // Add AsteroidBeltVisualizer component if not already present
        var visualizer = beltObject.GetComponent<AsteroidBeltVisualizer>();
        if (visualizer == null)
        {
            visualizer = beltObject.AddComponent<AsteroidBeltVisualizer>();
        }
        
        // Configure the visualizer
        if (visualizer != null)
        {
            // Set material and mesh if needed
            if (asteroidMaterial != null)
            {
                var renderer = beltObject.GetComponent<ParticleSystemRenderer>();
                if (renderer != null)
                {
                    renderer.material = asteroidMaterial;
                }
            }
            
            // Pass the notable object prefab if available
            if (notableAsteroidPrefab != null)
            {
                var prop = visualizer.GetType().GetField("notableObjectPrefab", 
                    System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Public);
                if (prop != null)
                {
                    prop.SetValue(visualizer, notableAsteroidPrefab);
                }
            }
            
            // Initialize with parameters from bodyData
            visualizer.Initialize(
                bodyData.semiMajorAxis - bodyData.scale, // Inner radius
                bodyData.semiMajorAxis + bodyData.scale, // Outer radius
                bodyData.scale * 0.2f,                   // Thickness
                100f,                                    // Density
                bodyData.color                           // Color
            );
        }
        
        if (showLabels)
        {
            AddLabel(beltObject, bodyData.name);
        }
        
        return beltObject;
    }
    
    private void ConfigureStarModel(StarModel starModel, CelestialBodyData bodyData)
    {
        if (starModel.starSettings == null)
        {
            // Create default star settings if none exist
            starModel.starSettings = ScriptableObject.CreateInstance<StarSettings>();
            starModel.starSettings.starMaterial = starMaterial; // Use the manager's star material
        }

        // Configure the settings
        starModel.starSettings.starRadius = bodyData.radius;
        starModel.starSettings.starColor = bodyData.color;
        starModel.starSettings.lightIntensity = CalculateLightIntensity(bodyData.temperature);
        starModel.starSettings.lightRange = bodyData.radius * 50f;
        starModel.starSettings.emissionStrength = 1f;
        
        // Initialize mesh components before generating
        starModel.InitializeMeshComponents();
    }
    
    private void ConfigurePlanetModel(PlanetModel planetModel, CelestialBodyData bodyData)
    {
        if (planetModel.shapeSettings == null)
        {
            // Create default shape settings if none exist
            planetModel.shapeSettings = ScriptableObject.CreateInstance<ShapeSettings>();
            planetModel.shapeSettings.planetRadius = bodyData.radius;
        }

        if (planetModel.colorSettings == null)
        {
            // Create default color settings if none exist
            planetModel.colorSettings = ScriptableObject.CreateInstance<ColorSettings>();
            planetModel.colorSettings.planetMaterial = planetMaterial; // Use the manager's planet material
        }

        // Configure the settings
        planetModel.shapeSettings.planetRadius = bodyData.radius;
        
        // Set some default color values
        planetModel.colorSettings.biomeColorSettings.biomes = new ColorSettings.BiomeColorSettings.Biome[]
        {
            new ColorSettings.BiomeColorSettings.Biome
            {
                gradient = new Gradient(),
                tint = bodyData.color,
                startHeight = 0,
                tintPercent = 0.5f
            }
        };

        // Initialize mesh components before generating
        planetModel.InitializeMeshComponents();
    }
    
    private void SetupStarLight(GameObject star, CelestialBodyData bodyData)
    {
        Light starLight = star.GetComponent<Light>();
        if (starLight == null)
        {
            starLight = star.AddComponent<Light>();
        }
        
        starLight.type = LightType.Point;
        starLight.color = bodyData.color;
        starLight.intensity = CalculateLightIntensity(bodyData.temperature);
        starLight.range = bodyData.scale * 50f; // Scale with star size
    }
    
    private float CalculateLightIntensity(float temperature)
    {
        return Mathf.Clamp(temperature / 5778f, 0.1f, 3f); // Solar temperature as baseline
    }
        
    private void SetupOrbit(GameObject body, CelestialBodyData bodyData, GameObject attractor, float gConst)
    {
        KeplerOrbitMover orbit = body.AddComponent<KeplerOrbitMover>();
        
        // Get attractor mass
        float attractorMass = GetAttractorMass(attractor);
        
        orbit.AttractorSettings.AttractorObject = attractor.transform;
        orbit.AttractorSettings.AttractorMass = attractorMass;
        orbit.AttractorSettings.GravityConstant = gConst;
        
        // Create orbit from orbital elements
        orbit.OrbitData = new KeplerOrbitData(
            eccentricity: bodyData.eccentricity,
            semiMajorAxis: bodyData.semiMajorAxis,
            meanAnomalyDeg: bodyData.meanAnomaly,
            inclinationDeg: bodyData.inclination,
            argOfPerifocusDeg: bodyData.argumentOfPerifocus,
            ascendingNodeDeg: bodyData.longitudeOfAscendingNode,
            attractorMass: attractorMass,
            gConst: gConst
        );
        
        orbit.ForceUpdateViewFromInternalState();
        
        // Add orbit visualization
        if (showOrbits)
        {
            var lineDisplay = body.AddComponent<KeplerOrbitLineDisplay>();
            lineDisplay.OrbitPointsCount = 64;
            
            // Optional: Add LineRenderer for runtime visualization
            var lineRenderer = body.AddComponent<LineRenderer>();
            lineRenderer.startWidth = orbitLineWidth;
            lineRenderer.endWidth = orbitLineWidth;
            lineDisplay.LineRendererReference = lineRenderer;
        }
    }
    
    private float GetAttractorMass(GameObject attractor)
    {
        // Try to find the mass from our data
        foreach (var kvp in celestialBodies)
        {
            if (kvp.Value == attractor)
            {
                var bodyData = FindBodyDataByName(currentSystemData.rootBody, kvp.Key);
                if (bodyData != null) return bodyData.mass;
            }
        }
        return 1000f; // Default mass
    }
    
    private CelestialBodyData FindBodyDataByName(CelestialBodyData root, string name)
    {
        if (root.name == name) return root;
        
        foreach (var satellite in root.satellites)
        {
            var found = FindBodyDataByName(satellite, name);
            if (found != null) return found;
        }
        
        return null;
    }
    
    private Material GetMaterialForType(CelestialBodyType type)
    {
        switch (type)
        {
            case CelestialBodyType.Star: return starMaterial;
            case CelestialBodyType.Planet: return planetMaterial;
            case CelestialBodyType.Moon: return moonMaterial;
            case CelestialBodyType.Asteroid: return asteroidMaterial;
            default: return null;
        }
    }
    
    private void AddLabel(GameObject body, string text)
    {
        GameObject label = new GameObject("Label");
        label.transform.parent = body.transform;
        label.transform.localPosition = Vector3.up * (body.transform.localScale.x + 0.5f);
        
        TextMesh textMesh = label.AddComponent<TextMesh>();
        textMesh.text = text;
        textMesh.fontSize = 20;
        textMesh.alignment = TextAlignment.Center;
        textMesh.anchor = TextAnchor.MiddleCenter;
        
        // Make label always face camera (billboard effect)
        label.AddComponent<Billboard>();
    }
    
    public void ClearCurrentSystem()
    {
        if (systemRoot != null)
        {
            DestroyImmediate(systemRoot);
        }
        celestialBodies.Clear();
    }
    
    public GameObject GetCelestialBody(string name)
    {
        return celestialBodies.ContainsKey(name) ? celestialBodies[name] : null;
    }
}