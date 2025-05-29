using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

/// <summary>
/// GalaxyDataStore maintains the galaxy data in memory during runtime
/// and provides access methods for all galaxy components.
/// </summary>
public class GalaxyDataStore : MonoBehaviour
{
    // Singleton instance
    public static GalaxyDataStore Instance { get; private set; }
    
    // Core data collections
    private List<Sector> _sectors = new List<Sector>();
    private List<StarSystem> _starSystems = new List<StarSystem>();
    
    // Generation metadata
    public string GalaxyName { get; set; }
    public string GenerationSeed { get; set; }
    public Dictionary<string, object> GenerationParameters { get; set; } = new Dictionary<string, object>();
    public DateTime CreationDate { get; set; }
    public DateTime LastSaveDate { get; set; }
    
    private void Awake()
    {
        // Implement singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
    
    /// <summary>
    /// Checks if galaxy data exists in memory
    /// </summary>
    public bool HasGalaxyData()
    {
        return _sectors.Count > 0 || _starSystems.Count > 0;
    }
    
    /// <summary>
    /// Adds a sector to the galaxy
    /// </summary>
    public void AddSector(Sector sector)
    {
        _sectors.Add(sector);
    }
    
    /// <summary>
    /// Adds a star system to the galaxy
    /// </summary>
    public void AddStarSystem(StarSystem system)
    {
        _starSystems.Add(system);
    }
    
    /// <summary>
    /// Gets all sectors in the galaxy
    /// </summary>
    public List<Sector> GetAllSectors()
    {
        return _sectors;
    }
    
    /// <summary>
    /// Gets all star systems in the galaxy
    /// </summary>
    public List<StarSystem> GetAllSystems()
    {
        return _starSystems;
    }
    
    /// <summary>
    /// Finds a sector by ID
    /// </summary>
    public Sector FindSectorByID(string id)
    {
        return _sectors.Find(s => s.ID == id);
    }
    
    /// <summary>
    /// Finds a star system by ID
    /// </summary>
    public StarSystem FindSystemByID(string id)
    {
        return _starSystems.Find(s => s.ID == id);
    }
    
    /// <summary>
    /// Clears all galaxy data from memory
    /// </summary>
    public void ClearGalaxyData()
    {
        _sectors.Clear();
        _starSystems.Clear();
        GalaxyName = string.Empty;
        GenerationSeed = string.Empty;
        GenerationParameters.Clear();
    }
    
    /// <summary>
    /// Sets up a new galaxy with the specified parameters
    /// </summary>
    public void InitializeNewGalaxy(string name, string seed, Dictionary<string, object> parameters)
    {
        GalaxyName = name;
        GenerationSeed = seed;
        GenerationParameters = parameters;
        CreationDate = DateTime.Now;
    }

    /// <summary>
    /// Gets a list of all available save files (without extension)
    /// </summary>
    public List<string> GetSaveFiles()
    {
        // Delegate to SaveSystem to get all saved galaxies
        var savedGalaxies = SaveSystem.GetSavedGalaxies();
        return savedGalaxies.Select(info => info.SaveName).ToList();
    }
    
    /// <summary>
    /// Stores the generated galaxy data
    /// </summary>
    public void StoreGalaxyData(List<StarSystem> systems, GalaxyGenerationMetadata metadata)
    {
        // Clear existing data
        ClearGalaxyData();
        
        // Add all systems
        _starSystems = new List<StarSystem>(systems);
        
        if (metadata != null)
        {
            // Store metadata information
            this.GenerationParameters = metadata.GenerationParameters ?? new Dictionary<string, object>();
            this.CreationDate = DateTime.Now;
            
            // Store additional metadata in GenerationParameters
            if (metadata.StarCount > 0)
                this.GenerationParameters["star_count"] = metadata.StarCount;
            if (metadata.SystemCount > 0)
                this.GenerationParameters["system_count"] = metadata.SystemCount;
            if (metadata.PlanetCount > 0)
                this.GenerationParameters["planet_count"] = metadata.PlanetCount;
        }
    }
}

/// <summary>
/// Metadata about the galaxy generation
/// </summary>
[Serializable]
public class GalaxyGenerationMetadata
{
    public string GenerationTime;
    public int StarCount;
    public int SystemCount;
    public int PlanetCount;
    public Dictionary<string, object> GenerationParameters;
}