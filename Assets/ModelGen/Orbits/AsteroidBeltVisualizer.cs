using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Handles the visualization of asteroid belts using particle systems or instanced rendering
/// </summary>
[RequireComponent(typeof(ParticleSystem))]
public class AsteroidBeltVisualizer : MonoBehaviour
{
    [Header("Asteroid Belt Properties")]
    [SerializeField] private float innerRadius = 10f;
    [SerializeField] private float outerRadius = 15f;
    [SerializeField] private float thickness = 1f;
    [SerializeField] private float density = 100f; // Particles per unit area
    [SerializeField] private float minAsteroidSize = 0.1f;
    [SerializeField] private float maxAsteroidSize = 0.5f;
    [SerializeField] private Color baseColor = new Color(0.6f, 0.6f, 0.6f);
    [SerializeField] private float colorVariation = 0.2f;
    
    [Header("Notable Objects")]
    [SerializeField] private GameObject notableObjectPrefab;
    [SerializeField] private List<Transform> notableObjects = new List<Transform>();
    
    [Header("Rendering Settings")]
    [SerializeField] private Material asteroidMaterial;
    [SerializeField] private Mesh asteroidMesh;
    [SerializeField] private bool useParticleSystem = true;
    [SerializeField] private bool useMeshInstancing = false;
    [SerializeField] private int maxParticles = 10000;
    [SerializeField] private int maxMeshInstances = 1000;
    
    private new ParticleSystem particleSystem;
    private ParticleSystemRenderer particleRenderer;
    private List<Matrix4x4> meshTransforms = new List<Matrix4x4>();
    private List<Vector4> meshColors = new List<Vector4>();
    private MaterialPropertyBlock propertyBlock;
    
    private void Awake()
    {
        particleSystem = GetComponent<ParticleSystem>();
        particleRenderer = GetComponent<ParticleSystemRenderer>();
        propertyBlock = new MaterialPropertyBlock();
    }
    
    /// <summary>
    /// Initialize the asteroid belt with data from the generator
    /// </summary>
    /// <param name="beltData">The asteroid belt data from the generator</param>
    public void Initialize(AsteroidBelt beltData)
    {
        if (beltData == null)
        {
            Debug.LogError("AsteroidBeltVisualizer: No belt data provided");
            return;
        }
        
        // Set properties from data
        innerRadius = beltData.InnerRadius * StarSystemDataConverter.AU_TO_UNITY_DISTANCE;
        outerRadius = beltData.OuterRadius * StarSystemDataConverter.AU_TO_UNITY_DISTANCE;
        thickness = beltData.Thickness * StarSystemDataConverter.AU_TO_UNITY_DISTANCE;
        
        // Scale density by actual volume to avoid too many particles
        float totalVolume = CalculateBeltVolume();
        float particlesPerUnitVolume = beltData.AverageDensity * 0.01f;
        int targetParticleCount = Mathf.Min(maxParticles, Mathf.FloorToInt(totalVolume * particlesPerUnitVolume));
        
        // Get color from belt composition if available
        if (beltData.GetMaterialComposition().Count > 0)
        {
            baseColor = GetColorFromComposition(beltData.GetMaterialComposition());
        }
        
        // Clear any existing notable objects
        foreach (var obj in notableObjects)
        {
            if (obj != null) Destroy(obj.gameObject);
        }
        notableObjects.Clear();
        
        // Create notable objects
        if (beltData.NotableObjects != null && beltData.NotableObjects.Count > 0)
        {
            CreateNotableObjects(beltData.NotableObjects);
        }
        
        // Setup visualization based on selected method
        if (useParticleSystem)
        {
            SetupParticleSystem(targetParticleCount);
        }
        else if (useMeshInstancing)
        {
            SetupMeshInstancing(Mathf.Min(targetParticleCount, maxMeshInstances));
        }
    }
    
    /// <summary>
    /// Initialize the asteroid belt with manually provided parameters
    /// </summary>
    public void Initialize(float innerRadius, float outerRadius, float thickness, float density, Color color)
    {
        this.innerRadius = innerRadius;
        this.outerRadius = outerRadius;
        this.thickness = thickness;
        this.density = density;
        this.baseColor = color;
        
        float totalVolume = CalculateBeltVolume();
        int targetParticleCount = Mathf.Min(maxParticles, Mathf.FloorToInt(totalVolume * density));
        
        if (useParticleSystem)
        {
            SetupParticleSystem(targetParticleCount);
        }
        else if (useMeshInstancing)
        {
            SetupMeshInstancing(Mathf.Min(targetParticleCount, maxMeshInstances));
        }
    }
    
    /// <summary>
    /// Set up the particle system to represent the asteroid belt
    /// </summary>
    private void SetupParticleSystem(int particleCount)
    {
        if (particleSystem == null)
        {
            Debug.LogError("AsteroidBeltVisualizer: ParticleSystem component missing");
            return;
        }
        
        // Clear existing particles
        particleSystem.Clear();
        
        // Configure particle system
        var main = particleSystem.main;
        main.maxParticles = particleCount;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.playOnAwake = true;
        main.loop = false;
        main.duration = 1f;
        main.startLifetime = Mathf.Infinity;
        main.startSpeed = 0f;
        main.startSize = new ParticleSystem.MinMaxCurve(minAsteroidSize, maxAsteroidSize);
        
        // Set particle color
        var startColor = main.startColor;
        startColor.mode = ParticleSystemGradientMode.RandomColor;
        Gradient colorGradient = new Gradient();
        
        GradientColorKey[] colorKeys = new GradientColorKey[2];
        colorKeys[0].color = new Color(
            Mathf.Clamp01(baseColor.r - colorVariation), 
            Mathf.Clamp01(baseColor.g - colorVariation), 
            Mathf.Clamp01(baseColor.b - colorVariation)
        );
        colorKeys[0].time = 0f;
        colorKeys[1].color = new Color(
            Mathf.Clamp01(baseColor.r + colorVariation), 
            Mathf.Clamp01(baseColor.g + colorVariation), 
            Mathf.Clamp01(baseColor.b + colorVariation)
        );
        colorKeys[1].time = 1f;
        
        GradientAlphaKey[] alphaKeys = new GradientAlphaKey[1];
        alphaKeys[0].alpha = 1.0f;
        alphaKeys[0].time = 0.0f;
        
        colorGradient.SetKeys(colorKeys, alphaKeys);
        startColor.gradient = colorGradient;
        main.startColor = startColor;
        
        // Disable emission
        var emission = particleSystem.emission;
        emission.enabled = false;
        
        // Set up shape module for a ring
        var shape = particleSystem.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Donut;
        shape.radius = (innerRadius + outerRadius) * 0.5f;
        shape.radiusThickness = (outerRadius - innerRadius) / (innerRadius + outerRadius);
        shape.arc = 360f;
        shape.arcMode = ParticleSystemShapeMultiModeValue.Random;
        shape.donutRadius = 0f;
        shape.position = new Vector3(0, 0, 0);
        shape.rotation = new Vector3(0, 0, 0);
        shape.scale = new Vector3(1, thickness / (innerRadius + outerRadius), 1);
        
        // Set renderer settings
        if (particleRenderer != null && asteroidMaterial != null)
        {
            particleRenderer.renderMode = ParticleSystemRenderMode.Billboard;
            particleRenderer.material = asteroidMaterial;
            
            if (asteroidMesh != null)
            {
                particleRenderer.renderMode = ParticleSystemRenderMode.Mesh;
                particleRenderer.mesh = asteroidMesh;
            }
        }
        
        // Emit particles
        particleSystem.Emit(particleCount);
    }
    
    /// <summary>
    /// Set up mesh instancing to represent the asteroid belt
    /// </summary>
    private void SetupMeshInstancing(int instanceCount)
    {
        if (asteroidMesh == null || asteroidMaterial == null)
        {
            Debug.LogError("AsteroidBeltVisualizer: Mesh or material missing for instancing");
            return;
        }
        
        // Clear existing instances
        meshTransforms.Clear();
        meshColors.Clear();
        
        // Generate random positions and orientations
        for (int i = 0; i < instanceCount; i++)
        {
            // Random position within the belt
            Vector3 position = GetRandomPositionInBelt();
            
            // Random rotation
            Quaternion rotation = Random.rotation;
            
            // Random size
            float size = Random.Range(minAsteroidSize, maxAsteroidSize);
            Vector3 scale = Vector3.one * size;
            
            // Add to transforms list
            Matrix4x4 matrix = Matrix4x4.TRS(position, rotation, scale);
            meshTransforms.Add(matrix);
            
            // Random color variation
            Color color = new Color(
                Mathf.Clamp01(baseColor.r + Random.Range(-colorVariation, colorVariation)),
                Mathf.Clamp01(baseColor.g + Random.Range(-colorVariation, colorVariation)),
                Mathf.Clamp01(baseColor.b + Random.Range(-colorVariation, colorVariation)),
                1.0f
            );
            meshColors.Add(color);
        }
    }
    
    /// <summary>
    /// Gets a random position within the asteroid belt volume
    /// </summary>
    private Vector3 GetRandomPositionInBelt()
    {
        // Get random angle in radians
        float angle = Random.Range(0f, Mathf.PI * 2f);
        
        // Get random radius between inner and outer
        float radius = Mathf.Sqrt(Random.Range(innerRadius * innerRadius, outerRadius * outerRadius));
        
        // Get random height within thickness
        float height = Random.Range(-thickness / 2f, thickness / 2f);
        
        // Calculate position
        float x = radius * Mathf.Cos(angle);
        float z = radius * Mathf.Sin(angle);
        
        return new Vector3(x, height, z);
    }
    
    /// <summary>
    /// Calculates the volume of the asteroid belt in cubic units
    /// </summary>
    private float CalculateBeltVolume()
    {
        return Mathf.PI * (outerRadius * outerRadius - innerRadius * innerRadius) * thickness;
    }
    
    /// <summary>
    /// Determines a representative color based on material composition
    /// </summary>
    private Color GetColorFromComposition(Dictionary<string, float> composition)
    {
        Color result = baseColor;
        
        if (composition.ContainsKey("Metallic") && composition["Metallic"] > 0.3f)
        {
            result = new Color(0.7f, 0.7f, 0.7f); // Silver-gray
        }
        else if (composition.ContainsKey("Stony") && composition["Stony"] > 0.3f)
        {
            result = new Color(0.6f, 0.5f, 0.4f); // Brown-gray
        }
        else if (composition.ContainsKey("Carbonaceous") && composition["Carbonaceous"] > 0.3f)
        {
            result = new Color(0.3f, 0.3f, 0.3f); // Dark gray
        }
        else if (composition.ContainsKey("Icy") && composition["Icy"] > 0.3f)
        {
            result = new Color(0.8f, 0.9f, 1.0f); // Icy blue-white
        }
        
        return result;
    }
    
    /// <summary>
    /// Creates GameObjects for notable objects in the asteroid belt
    /// </summary>
    private void CreateNotableObjects(List<AsteroidBelt.AsteroidBeltObject> objects)
    {
        if (notableObjectPrefab == null) return;
        
        foreach (var obj in objects)
        {
            // Calculate position in Unity space
            float radius = obj.OrbitalDistance * StarSystemDataConverter.AU_TO_UNITY_DISTANCE;
            float angle = obj.OrbitalAngle;
            
            float x = radius * Mathf.Cos(angle);
            float z = radius * Mathf.Sin(angle);
            
            // Create the object
            GameObject notableObj = Instantiate(notableObjectPrefab, transform);
            notableObj.name = obj.ID;
            notableObj.transform.position = new Vector3(x, 0, z);
            
            // Scale based on radius
            float scale = obj.Radius * 0.001f; // Convert km to Unity units
            notableObj.transform.localScale = Vector3.one * Mathf.Max(0.2f, scale);
            
            // Add it to our list
            notableObjects.Add(notableObj.transform);
        }
    }
    
    private void Update()
    {
        // If using mesh instancing, render the asteroids
        if (useMeshInstancing && !useParticleSystem && asteroidMesh != null && asteroidMaterial != null)
        {
            // Maximum of 1023 instances per batch
            const int batchSize = 1023;
            
            for (int i = 0; i < meshTransforms.Count; i += batchSize)
            {
                int count = Mathf.Min(batchSize, meshTransforms.Count - i);
                Matrix4x4[] batch = new Matrix4x4[count];
                
                // Copy the relevant slice of transforms
                for (int j = 0; j < count; j++)
                {
                    batch[j] = meshTransforms[i + j];
                }
                
                // Draw the batch
                Graphics.DrawMeshInstanced(asteroidMesh, 0, asteroidMaterial, batch, count);
            }
        }
    }
    
    /// <summary>
    /// Gizmo drawing for visualization in the editor
    /// </summary>
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        
        // Draw inner ring
        DrawCircle(Vector3.zero, innerRadius, 32);
        
        // Draw outer ring
        DrawCircle(Vector3.zero, outerRadius, 32);
        
        // Draw notable objects
        Gizmos.color = Color.red;
        foreach (var obj in notableObjects)
        {
            if (obj != null)
            {
                Gizmos.DrawWireSphere(obj.position, 0.5f);
            }
        }
    }
    
    private void DrawCircle(Vector3 center, float radius, int segments)
    {
        float angle = 0;
        float angleStep = 2 * Mathf.PI / segments;
        
        Vector3 previousPoint = center + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
        
        for (int i = 0; i < segments + 1; i++)
        {
            angle += angleStep;
            Vector3 nextPoint = center + new Vector3(Mathf.Cos(angle) * radius, 0, Mathf.Sin(angle) * radius);
            Gizmos.DrawLine(previousPoint, nextPoint);
            previousPoint = nextPoint;
        }
    }
} 