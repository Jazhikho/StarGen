using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Registry service for celestial entities that enables easy lookup without direct references.
/// </summary>
public class EntityRegistry : MonoBehaviour
{
    // Singleton instance for easy access
    public static EntityRegistry Instance { get; private set; }
    
    // Collections for different entity types
    private Dictionary<string, StarStructure> stars = new Dictionary<string, StarStructure>();
    private Dictionary<string, StarSystem> systems = new Dictionary<string, StarSystem>();
    private Dictionary<string, Planet> planets = new Dictionary<string, Planet>();
    private Dictionary<string, Moon> moons = new Dictionary<string, Moon>();
    private Dictionary<string, AsteroidBelt> asteroidBelts = new Dictionary<string, AsteroidBelt>();
    
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    #region Registration Methods
    public void RegisterStar(StarStructure star) => stars[star.ID] = star;
    public void RegisterSystem(StarSystem system) => systems[system.ID] = system;
    public void RegisterPlanet(Planet planet) => planets[planet.ID] = planet;
    public void RegisterMoon(Moon moon) => moons[moon.ID] = moon;
    public void RegisterAsteroidBelt(AsteroidBelt belt) => asteroidBelts[belt.ID] = belt;
    #endregion
    
    #region Lookup Methods
    public StarStructure GetStar(string id) => stars.TryGetValue(id, out var star) ? star : null;
    public StarSystem GetSystem(string id) => systems.TryGetValue(id, out var system) ? system : null;
    public Planet GetPlanet(string id) => planets.TryGetValue(id, out var planet) ? planet : null;
    public Moon GetMoon(string id) => moons.TryGetValue(id, out var moon) ? moon : null;
    public AsteroidBelt GetAsteroidBelt(string id) => asteroidBelts.TryGetValue(id, out var belt) ? belt : null;
    #endregion
    
    #region Collection Access
    public IReadOnlyCollection<StarStructure> GetAllStars() => stars.Values;
    public IReadOnlyCollection<StarSystem> GetAllSystems() => systems.Values;
    public IReadOnlyCollection<Planet> GetAllPlanets() => planets.Values;
    public IReadOnlyCollection<Moon> GetAllMoons() => moons.Values;
    public IReadOnlyCollection<AsteroidBelt> GetAllAsteroidBelts() => asteroidBelts.Values;
    #endregion
    
    #region Saving/Loading Support
    /// <summary>
    /// Get serializable collections for saving
    /// </summary>
    public (List<StarStructure> stars, List<StarSystem> systems, List<Planet> planets, List<Moon> moons, List<AsteroidBelt> belts) GetSerializableData()
    {
        return (
            new List<StarStructure>(stars.Values),
            new List<StarSystem>(systems.Values),
            new List<Planet>(planets.Values),
            new List<Moon>(moons.Values),
            new List<AsteroidBelt>(asteroidBelts.Values)
        );
    }
    
    /// <summary>
    /// Load data from serialized collections
    /// </summary>
    public void LoadSerializableData(List<StarStructure> starList, List<StarSystem> systemList, List<Planet> planetList, List<Moon> moonList, List<AsteroidBelt> beltList)
    {
        Clear();
        
        foreach (var star in starList) RegisterStar(star);
        foreach (var system in systemList) RegisterSystem(system);
        foreach (var planet in planetList) RegisterPlanet(planet);
        foreach (var moon in moonList) RegisterMoon(moon);
        foreach (var belt in beltList) RegisterAsteroidBelt(belt);
    }
    #endregion
    
    /// <summary>
    /// Clear all stored data
    /// </summary>
    public void Clear()
    {
        stars.Clear();
        systems.Clear();
        planets.Clear();
        moons.Clear();
        asteroidBelts.Clear();
    }
}