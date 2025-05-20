using System.Collections.Generic;
using UnityEngine;
using System;
using System.IO;
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
    
    // Save directory path
    private string SaveDirectory => Path.Combine(Application.persistentDataPath, "GalaxySaves");
    private const string SaveExtension = ".sgen";
    
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
        
        // Ensure save directory exists
        if (!Directory.Exists(SaveDirectory))
        {
            Directory.CreateDirectory(SaveDirectory);
        }
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
        if (!Directory.Exists(SaveDirectory))
        {
            Directory.CreateDirectory(SaveDirectory);
            return new List<string>();
        }
        
        string[] files = Directory.GetFiles(SaveDirectory, $"*{SaveExtension}");
        List<string> saveFiles = new List<string>();
        
        foreach (string file in files)
        {
            string fileName = Path.GetFileNameWithoutExtension(file);
            saveFiles.Add(fileName);
        }
        
        // Sort files by last write time (newest first)
        saveFiles = saveFiles.OrderBy(f => File.GetLastWriteTime(Path.Combine(SaveDirectory, f + SaveExtension))).ToList();
        
        return saveFiles;
    }
    
    /// <summary>
    /// Saves the current galaxy data to a file
    /// </summary>
    /// <param name="fileName">Name for the save file (without extension)</param>
    /// <returns>True if successful, false otherwise</returns>
    public bool SaveGalaxyData(string fileName)
    {
        if (!HasGalaxyData())
        {
            Debug.LogError("No galaxy data to save");
            return false;
        }
        
        try
        {
            string filePath = Path.Combine(SaveDirectory, fileName + SaveExtension);
            GalaxySaveData saveData = CreateSaveData();
            
            // Convert to JSON
            string jsonData = JsonUtility.ToJson(saveData, true);
            File.WriteAllText(filePath, jsonData);
            
            LastSaveDate = DateTime.Now;
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to save galaxy data: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Loads galaxy data from a file
    /// </summary>
    /// <param name="fileName">Name of the save file (without extension)</param>
    /// <returns>True if successful, false otherwise</returns>
    public bool LoadGalaxyData(string fileName)
    {
        try
        {
            string filePath = Path.Combine(SaveDirectory, fileName + SaveExtension);
            
            if (!File.Exists(filePath))
            {
                Debug.LogError($"Save file not found: {filePath}");
                return false;
            }
            
            string jsonData = File.ReadAllText(filePath);
            GalaxySaveData saveData = JsonUtility.FromJson<GalaxySaveData>(jsonData);
            
            if (saveData == null)
            {
                Debug.LogError("Failed to deserialize galaxy data");
                return false;
            }
            
            RestoreFromSaveData(saveData);
            
            // Emit event that galaxy was loaded successfully
            GalaxyLoadSignalEmitter emitter = GalaxyLoadSignalEmitter.GetInstance();
            if (emitter != null)
            {
                emitter.EmitGalaxyLoaded(fileName);
            }
            
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to load galaxy data: {ex.Message}");
            return false;
        }
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
    
    /// <summary>
    /// Deletes a save file
    /// </summary>
    /// <param name="fileName">Name of the save file (without extension)</param>
    /// <returns>True if successful, false otherwise</returns>
    public bool DeleteSaveFile(string fileName)
    {
        try
        {
            string filePath = Path.Combine(SaveDirectory, fileName + SaveExtension);

            if (!File.Exists(filePath))
            {
                Debug.LogError($"Save file not found: {filePath}");
                return false;
            }

            File.Delete(filePath);
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to delete save file: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Creates a serializable save data object from current galaxy data
    /// </summary>
    private GalaxySaveData CreateSaveData()
    {
        GalaxySaveData saveData = new GalaxySaveData
        {
            GalaxyName = this.GalaxyName,
            GenerationSeed = this.GenerationSeed,
            CreationDate = this.CreationDate.ToString("o"),
            LastSaveDate = DateTime.Now.ToString("o"),
            SectorCount = _sectors.Count,
            SystemCount = _starSystems.Count,
            // Serialize parameters as JSON string
            GenerationParametersJson = JsonUtility.ToJson(new SerializableDictionary<string, object>(GenerationParameters))
        };
        
        // Serialize sectors and systems (in a real implementation, you'd likely do this in chunks)
        saveData.SerializedSectors = new List<string>();
        foreach (var sector in _sectors)
        {
            saveData.SerializedSectors.Add(JsonUtility.ToJson(sector));
        }
        
        saveData.SerializedSystems = new List<string>();
        foreach (var system in _starSystems)
        {
            saveData.SerializedSystems.Add(JsonUtility.ToJson(system));
        }
        
        return saveData;
    }
    
    /// <summary>
    /// Restores galaxy data from loaded save data
    /// </summary>
    private void RestoreFromSaveData(GalaxySaveData saveData)
    {
        ClearGalaxyData();
        
        this.GalaxyName = saveData.GalaxyName;
        this.GenerationSeed = saveData.GenerationSeed;
        this.CreationDate = DateTime.Parse(saveData.CreationDate);
        this.LastSaveDate = DateTime.Parse(saveData.LastSaveDate);
        
        // Deserialize parameters
        SerializableDictionary<string, object> paramDict = JsonUtility.FromJson<SerializableDictionary<string, object>>(saveData.GenerationParametersJson);
        this.GenerationParameters = paramDict.ToDictionary();
        
        // Deserialize sectors
        foreach (string sectorJson in saveData.SerializedSectors)
        {
            Sector sector = JsonUtility.FromJson<Sector>(sectorJson);
            _sectors.Add(sector);
        }
        
        // Deserialize systems
        foreach (string systemJson in saveData.SerializedSystems)
        {
            StarSystem system = JsonUtility.FromJson<StarSystem>(systemJson);
            _starSystems.Add(system);
        }
        
        // Rebuild any runtime data structures
        foreach (var sector in _sectors)
        {
            sector.RebuildDistanceMap();
        }
    }
}

/// <summary>
/// Serializable container for galaxy save data
/// </summary>
[Serializable]
public class GalaxySaveData
{
    public string GalaxyName;
    public string GenerationSeed;
    public string CreationDate;
    public string LastSaveDate;
    public int SectorCount;
    public int SystemCount;
    public string GenerationParametersJson;
    public List<string> SerializedSectors;
    public List<string> SerializedSystems;
}

/// <summary>
/// Helper class for serializing dictionaries
/// </summary>
[Serializable]
public class SerializableDictionary<TKey, TValue>
{
    [Serializable]
    public class KeyValuePair
    {
        public string Key;
        public string Value;
    }
    
    public List<KeyValuePair> Entries = new List<KeyValuePair>();
    
    public SerializableDictionary() { }
    
    public SerializableDictionary(Dictionary<TKey, TValue> dictionary)
    {
        foreach (var kvp in dictionary)
        {
            Entries.Add(new KeyValuePair
            {
                Key = kvp.Key.ToString(),
                Value = JsonUtility.ToJson(kvp.Value)
            });
        }
    }
    
    public Dictionary<TKey, TValue> ToDictionary()
    {
        Dictionary<TKey, TValue> result = new Dictionary<TKey, TValue>();
        
        foreach (var entry in Entries)
        {
            TKey key = (TKey)Convert.ChangeType(entry.Key, typeof(TKey));
            TValue value = JsonUtility.FromJson<TValue>(entry.Value);
            result[key] = value;
        }
        
        return result;
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