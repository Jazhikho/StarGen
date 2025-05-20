using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Creates a distant starfield background for the galaxy view
/// </summary>
public class BackgroundStarfield : MonoBehaviour
{
    [SerializeField] private int _starCount = 3000;
    [SerializeField] private float _sphereRadius = 500f;
    [SerializeField] private float _minStarSize = 2f;
    [SerializeField] private float _maxStarSize = 5f;
    
    public int StarCount { get => _starCount; set => _starCount = value; }
    public float SphereRadius { get => _sphereRadius; set => _sphereRadius = value; }
    public float MinStarSize { get => _minStarSize; set => _minStarSize = value; }
    public float MaxStarSize { get => _maxStarSize; set => _maxStarSize = value; }
    
    private ParticleSystem _particleSystem;
    
    void Start()
    {
        CreateStarfield();
    }
    
    private void CreateStarfield()
    {
        _particleSystem = gameObject.AddComponent<ParticleSystem>();
        
        var main = _particleSystem.main;
        main.maxParticles = _starCount;
        main.startLifetime = Mathf.Infinity;
        main.startSize3D = true;
        main.startSpeed = 0;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.playOnAwake = true;
        main.loop = false;
        
        var emission = _particleSystem.emission;
        emission.enabled = false;
        
        var renderer = _particleSystem.GetComponent<ParticleSystemRenderer>();
        renderer.material = new Material(Shader.Find("Particles/Standard Unlit"));
        renderer.renderMode = ParticleSystemRenderMode.Billboard;
        
        GenerateStars();
    }
    
    private void GenerateStars()
    {
        ParticleSystem.Particle[] particles = new ParticleSystem.Particle[_starCount];
        
        for (int i = 0; i < _starCount; i++)
        {
            // Random point on unit sphere
            Vector3 position = Random.onUnitSphere * _sphereRadius;
            
            // Random size
            float size = Random.Range(_minStarSize, _maxStarSize);
            
            // Random color variation
            float hue = Random.Range(0.5f, 0.7f);  // Mostly white to light blue
            float sat = Random.Range(0.0f, 0.3f);  // Low saturation
            float val = Random.Range(0.7f, 1.0f);  // High value/brightness
            Color color = Color.HSVToRGB(hue, sat, val);
            
            particles[i].position = position;
            particles[i].startSize = size * 0.01f;  // Scale down for Unity's particle system
            particles[i].startColor = color;
            particles[i].remainingLifetime = Mathf.Infinity;
            particles[i].velocity = Vector3.zero;
        }
        
        _particleSystem.SetParticles(particles, _starCount);
    }
}