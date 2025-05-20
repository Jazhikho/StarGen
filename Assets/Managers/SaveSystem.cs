using System.Collections.Generic;
using System.IO;
using UnityEngine;
using System;
using Newtonsoft.Json;

/// <summary>
/// SaveSystem handles serialization and persistence of galaxy data to disk
/// </summary>
public static class SaveSystem
{
    private static readonly string SaveDirectory = Path.Combine(Application.persistentDataPath, "Galaxies");
    
    [Serializable]
    public class GalaxySaveMetadata
    {
        public string Name;
        public string Seed;
        public DateTime CreationDate;
        public DateTime LastSaveDate;
        public int SectorCount;
        public int SystemCount;
        public Dictionary<string, object> Parameters;
    }
    
    /// <summary>
    /// Saves the current galaxy to disk
    /// </summary>
    public static void SaveGalaxy(string saveName)
    {
        var dataStore = GalaxyDataStore.Instance;
        if (dataStore == null || !dataStore.HasGalaxyData())
        {
            Debug.LogError("No galaxy data to save");
            return;
        }
        
        // Create save directory if it doesn't exist
        if (!Directory.Exists(SaveDirectory))
        {
            Directory.CreateDirectory(SaveDirectory);
        }
        
        string galaxyDirectory = Path.Combine(SaveDirectory, saveName);
        if (!Directory.Exists(galaxyDirectory))
        {
            Directory.CreateDirectory(galaxyDirectory);
        }
        
        try
        {
            // Save metadata
            var metadata = new GalaxySaveMetadata
            {
                Name = dataStore.GalaxyName,
                Seed = dataStore.GenerationSeed,
                CreationDate = dataStore.CreationDate,
                LastSaveDate = DateTime.Now,
                SectorCount = dataStore.GetAllSectors().Count,
                SystemCount = dataStore.GetAllSystems().Count,
                Parameters = dataStore.GenerationParameters
            };
            
            string metadataJson = JsonConvert.SerializeObject(metadata, Formatting.Indented);
            File.WriteAllText(Path.Combine(galaxyDirectory, "metadata.json"), metadataJson);
            
            // Save sectors
            foreach (var sector in dataStore.GetAllSectors())
            {
                string sectorJson = JsonConvert.SerializeObject(sector, Formatting.Indented, 
                    new JsonSerializerSettings { 
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                        TypeNameHandling = TypeNameHandling.Auto
                    });
                File.WriteAllText(Path.Combine(galaxyDirectory, $"sector_{sector.ID}.json"), sectorJson);
            }
            
            // Save star systems (separately to avoid circular references)
            foreach (var system in dataStore.GetAllSystems())
            {
                string systemJson = JsonConvert.SerializeObject(system, Formatting.Indented,
                    new JsonSerializerSettings {
                        ReferenceLoopHandling = ReferenceLoopHandling.Ignore,
                        TypeNameHandling = TypeNameHandling.Auto
                    });
                File.WriteAllText(Path.Combine(galaxyDirectory, $"system_{system.ID}.json"), systemJson);
            }
            
            // Update last save date
            dataStore.LastSaveDate = DateTime.Now;
            
            Debug.Log($"Galaxy saved successfully to {galaxyDirectory}");
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error saving galaxy: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Loads a galaxy from disk
    /// </summary>
    public static bool LoadGalaxy(string saveName)
    {
        string galaxyDirectory = Path.Combine(SaveDirectory, saveName);
        
        if (!Directory.Exists(galaxyDirectory))
        {
            Debug.LogError($"Save directory not found: {galaxyDirectory}");
            return false;
        }
        
        try
        {
            var dataStore = GalaxyDataStore.Instance;
            
            // Clear existing data
            dataStore.ClearGalaxyData();
            
            // Load metadata
            string metadataPath = Path.Combine(galaxyDirectory, "metadata.json");
            if (!File.Exists(metadataPath))
            {
                Debug.LogError("Metadata file not found");
                return false;
            }
            
            var metadata = JsonConvert.DeserializeObject<GalaxySaveMetadata>(
                File.ReadAllText(metadataPath));
            
            dataStore.GalaxyName = metadata.Name;
            dataStore.GenerationSeed = metadata.Seed;
            dataStore.GenerationParameters = metadata.Parameters;
            dataStore.CreationDate = metadata.CreationDate;
            dataStore.LastSaveDate = metadata.LastSaveDate;
            
            // Load sectors
            var sectorFiles = Directory.GetFiles(galaxyDirectory, "sector_*.json");
            foreach (var file in sectorFiles)
            {
                var sector = JsonConvert.DeserializeObject<Sector>(
                    File.ReadAllText(file),
                    new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
                
                dataStore.AddSector(sector);
            }
            
            // Load star systems
            var systemFiles = Directory.GetFiles(galaxyDirectory, "system_*.json");
            foreach (var file in systemFiles)
            {
                var system = JsonConvert.DeserializeObject<StarSystem>(
                    File.ReadAllText(file),
                    new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.Auto });
                
                dataStore.AddStarSystem(system);
            }
            
            Debug.Log($"Galaxy loaded successfully from {galaxyDirectory}");
            
            // Emit a signal that the galaxy was loaded
            var emitterObject = new GameObject("GalaxyLoadSignalEmitter");
            var signalEmitter = emitterObject.AddComponent<GalaxyLoadSignalEmitter>();
            signalEmitter.EmitGalaxyLoaded(saveName);
            
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error loading galaxy: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Gets a list of all saved galaxies
    /// </summary>
    public static List<SaveInfo> GetSavedGalaxies()
    {
        var savedGalaxies = new List<SaveInfo>();
        
        if (!Directory.Exists(SaveDirectory))
        {
            return savedGalaxies;
        }
        
        string[] galaxyDirectories = Directory.GetDirectories(SaveDirectory);
        
        foreach (string dir in galaxyDirectories)
        {
            string metadataPath = Path.Combine(dir, "metadata.json");
            if (File.Exists(metadataPath))
            {
                try
                {
                    var metadata = JsonConvert.DeserializeObject<GalaxySaveMetadata>(
                        File.ReadAllText(metadataPath));
                    
                    savedGalaxies.Add(new SaveInfo
                    {
                        SaveName = Path.GetFileName(dir),
                        GalaxyName = metadata.Name,
                        CreationDate = metadata.CreationDate,
                        LastSaveDate = metadata.LastSaveDate,
                        SystemCount = metadata.SystemCount
                    });
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"Error reading save metadata: {ex.Message}");
                }
            }
        }
        
        return savedGalaxies;
    }
    
    /// <summary>
    /// Deletes a saved galaxy
    /// </summary>
    public static bool DeleteSavedGalaxy(string saveName)
    {
        string galaxyDirectory = Path.Combine(SaveDirectory, saveName);
        
        if (!Directory.Exists(galaxyDirectory))
        {
            return false;
        }
        
        try
        {
            Directory.Delete(galaxyDirectory, true);
            return true;
        }
        catch (Exception ex)
        {
            Debug.LogError($"Error deleting galaxy: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Information about a saved galaxy
    /// </summary>
    public class SaveInfo
    {
        public string SaveName;
        public string GalaxyName;
        public DateTime CreationDate;
        public DateTime LastSaveDate;
        public int SystemCount;
    }
}