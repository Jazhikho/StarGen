using System.Collections.Generic;
using UnityEngine;
using SimpleKeplerOrbits;

namespace SimpleKeplerOrbits.SystemBuilder
{
    public class StarSystemManager : MonoBehaviour
    {
        [Header("Prefab Settings")]
        [SerializeField] private GameObject spherePrefab;
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
                    bodyObject = new GameObject(bodyData.name);
                    break;
                    
                case CelestialBodyType.Star:
                case CelestialBodyType.Planet:
                case CelestialBodyType.Moon:
                case CelestialBodyType.Asteroid:
                    bodyObject = CreatePhysicalBody(bodyData);
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
        
        private GameObject CreatePhysicalBody(CelestialBodyData bodyData)
        {
            GameObject body = spherePrefab != null ? 
                Instantiate(spherePrefab) : 
                GameObject.CreatePrimitive(PrimitiveType.Sphere);
                
            body.name = bodyData.name;
            body.transform.localScale = Vector3.one * bodyData.scale;
            
            // Set material and color
            Renderer renderer = body.GetComponent<Renderer>();
            if (renderer != null)
            {
                Material mat = GetMaterialForType(bodyData.bodyType);
                if (mat != null)
                {
                    renderer.material = mat;
                    renderer.material.color = bodyData.color;
                }
            }
            
            // Add label if enabled
            if (showLabels)
            {
                AddLabel(body, bodyData.name);
            }
            
            return body;
        }
        
        private void SetupOrbit(GameObject body, CelestialBodyData bodyData, GameObject attractor, float gConst)
        {
            KeplerOrbitMover orbit = body.AddComponent<KeplerOrbitMover>();
            
            // Get attractor mass from parent body data
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
            label.transform.localPosition = Vector3.up * 2;
            
            TextMesh textMesh = label.AddComponent<TextMesh>();
            textMesh.text = text;
            textMesh.fontSize = 20;
            textMesh.alignment = TextAlignment.Center;
            textMesh.anchor = TextAnchor.MiddleCenter;
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
}