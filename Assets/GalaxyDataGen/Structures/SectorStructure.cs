using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Represents a sector of space containing star systems.
/// Each sector is a cube of parsecs containing a collection of star systems.
/// </summary>
[CreateAssetMenu(fileName = "NewSector", menuName = "Galaxy/Sector")]
[Serializable]
public class Sector : ScriptableObject
{
    /// <summary>Unique identifier for the sector</summary>
    public string ID;
    
    /// <summary>Relative X coordinate in the galactic grid</summary>
    public int SectorX;
    
    /// <summary>Relative Y coordinate in the galactic grid</summary>
    public int SectorY;
    
    /// <summary>Relative Z coordinate in the galactic grid</summary>
    public int SectorZ;
    
    /// <summary>Collection of star systems in this sector</summary>
    [SerializeField]
    public List<StarSystem> Systems = new List<StarSystem>();
    
    /// <summary>Grid of parsec coordinates within the sector</summary>
    [SerializeField]
    public List<Vector3> ParsecGrid = new List<Vector3>();
    
    // Making the DistanceMap a serializable class since Unity's JsonUtility
    // doesn't handle nested dictionaries directly
    [Serializable]
    public class SerializableDistanceMap
    {
        [Serializable]
        public class SystemDistancePair
        {
            public string SystemID;
            public float Distance;
            
            public SystemDistancePair(string systemId, float distance)
            {
                SystemID = systemId;
                Distance = distance;
            }
        }
        
        [Serializable]
        public class SystemDistances
        {
            public string SourceSystemID;
            public List<SystemDistancePair> Distances = new List<SystemDistancePair>();
            
            public SystemDistances(string sourceSystemId)
            {
                SourceSystemID = sourceSystemId;
            }
        }
        
        public List<SystemDistances> DistanceData = new List<SystemDistances>();
    }
    
    [SerializeField]
    private SerializableDistanceMap _serializableDistanceMap = new SerializableDistanceMap();
    
    // Runtime dictionary for efficient lookups
    [NonSerialized]
    private Dictionary<string, Dictionary<string, float>> _distanceMap = new Dictionary<string, Dictionary<string, float>>();

    /// <summary>
    /// OnEnable is called when the object becomes enabled and active
    /// </summary>
    private void OnEnable()
    {
        // Initialize ID if needed
        if (string.IsNullOrEmpty(ID))
        {
            ID = Guid.NewGuid().ToString();
        }
        
        // Rebuild the runtime distance map from serialized data
        RebuildDistanceMap();
    }
    
    /// <summary>
    /// OnValidate is called when the script is loaded or a value is changed in the Inspector
    /// </summary>
    private void OnValidate()
    {
        // Ensure all systems have the correct parent sector ID
        foreach (var system in Systems)
        {
            system.ParentSectorID = this.ID;
        }
    }
    
    /// <summary>
    /// Adds a star system to this sector
    /// </summary>
    /// <param name="system">The system to add</param>
    public void AddSystem(StarSystem system)
    {
        system.ParentSectorID = this.ID;
        Systems.Add(system);
    }

    /// <summary>
    /// Generates the grid of parsec coordinates for this sector.
    /// </summary>
    /// <param name="size">The number of parsecs per dimension</param>
    public void GenerateParsecsGrid(int size = 10)
    {
        ParsecGrid.Clear();
        ParsecGrid.AddRange(GenerateGrid(size));
    }

    /// <summary>
    /// Calculates distances between all systems in the sector.
    /// </summary>
    public void CalculateDistanceMap()
    {
        _distanceMap.Clear();
        _serializableDistanceMap.DistanceData.Clear();
        
        foreach (var system1 in Systems)
        {
            if (!_distanceMap.ContainsKey(system1.ID))
            {
                _distanceMap[system1.ID] = new Dictionary<string, float>();
            }
            
            var systemDistances = new SerializableDistanceMap.SystemDistances(system1.ID);
            
            foreach (var system2 in Systems)
            {
                if (system1 == system2) continue;
                float distance = Vector3.Distance(system1.Position, system2.Position);
                _distanceMap[system1.ID][system2.ID] = distance;
                systemDistances.Distances.Add(new SerializableDistanceMap.SystemDistancePair(system2.ID, distance));
            }
            
            _serializableDistanceMap.DistanceData.Add(systemDistances);
        }
    }
    
    /// <summary>
    /// Rebuilds the runtime distance map from serialized data
    /// </summary>
    public void RebuildDistanceMap()
    {
        _distanceMap.Clear();
        
        foreach (var systemDistances in _serializableDistanceMap.DistanceData)
        {
            if (!_distanceMap.ContainsKey(systemDistances.SourceSystemID))
            {
                _distanceMap[systemDistances.SourceSystemID] = new Dictionary<string, float>();
            }
            
            foreach (var distancePair in systemDistances.Distances)
            {
                _distanceMap[systemDistances.SourceSystemID][distancePair.SystemID] = distancePair.Distance;
            }
        }
    }
    
    /// <summary>
    /// Gets the distance between two systems.
    /// </summary>
    /// <param name="system1ID">The ID of the first system</param>
    /// <param name="system2ID">The ID of the second system</param>
    /// <returns>The distance between the systems, or -1 if not found</returns>
    public float GetSystemDistance(string system1ID, string system2ID)
    {
        if (_distanceMap.ContainsKey(system1ID) && _distanceMap[system1ID].ContainsKey(system2ID))
        {
            return _distanceMap[system1ID][system2ID];
        }
        
        return -1f;
    }

    /// <summary>
    /// Generates a 3D grid of coordinates.
    /// </summary>
    /// <param name="size">Size of the grid in each dimension</param>
    /// <returns>Enumerable of Vector3 grid positions</returns>
    public IEnumerable<Vector3> GenerateGrid(int size)
    {
        for (int x = 0; x < size; x++)
            for (int y = 0; y < size; y++)
                for (int z = 0; z < size; z++)
                    yield return new Vector3(x, y, z);
    }
    
    /// <summary>
    /// Find a system by ID
    /// </summary>
    /// <param name="systemID">The ID of the system to find</param>
    /// <returns>The found system or null if not found</returns>
    public StarSystem FindSystemByID(string systemID)
    {
        return Systems.Find(s => s.ID == systemID);
    }
}