using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Generates sectors of space containing star systems.
/// </summary>
public class SectorGenerator
{
    // No need for a separate random instance - we'll use Roll instead
    
    // Configuration parameters with defaults
    public int SectorSizeX { get; set; } = 5;
    public int SectorSizeY { get; set; } = 5;
    public int SectorSizeZ { get; set; } = 5;
    public int ParsecsPerSector { get; set; } = 10;
    public float StarDensity { get; set; } = 0.12f;
    public float AnomalyChance { get; set; } = 0.001f;
    public int RandomSeed { get; set; } = 0;
    public int GalaxyType { get; set; } = 0; // 0=uniform, 1=spiral, 2=elliptical, 3=irregular
    public int SpectralDistribution { get; set; } = 1; // 0=hot, 1=realistic, 2=cool
    
    private const float LightYearsPerParsec = 3.26f;
    
    private Vector3Int _galacticCenterSector;
    
    // Define coordinate offsets for adjusting starting sector positions relative to galactic center
    private const int EarthOffsetX = 8015; // Distance in parsecs
    private const int EarthOffsetY = 25;   // Small galactic y offset
    private const int EarthOffsetZ = -5;   // Small galactic z offset
    
    private SystemGenerator _systemGenerator = new SystemGenerator();

    /// <summary>
    /// Initializes the sector generator with the specified parameters.
    /// </summary>
    public void Initialize(Dictionary<string, object> parameters)
    {
        // Set parameters from the dictionary
        if (parameters.ContainsKey("sector_size_x"))
            SectorSizeX = Convert.ToInt32(parameters["sector_size_x"]);

        if (parameters.ContainsKey("sector_size_y"))
            SectorSizeY = Convert.ToInt32(parameters["sector_size_y"]);

        if (parameters.ContainsKey("sector_size_z"))
            SectorSizeZ = Convert.ToInt32(parameters["sector_size_z"]);

        if (parameters.ContainsKey("parsecs_per_sector"))
            ParsecsPerSector = Convert.ToInt32(parameters["parsecs_per_sector"]);

        if (parameters.ContainsKey("star_density"))
            StarDensity = Convert.ToSingle(parameters["star_density"]);

        if (parameters.ContainsKey("anomaly_chance"))
            AnomalyChance = Convert.ToSingle(parameters["anomaly_chance"]);

        if (parameters.ContainsKey("random_seed"))
            RandomSeed = Convert.ToInt32(parameters["random_seed"]);

        if (parameters.ContainsKey("galaxy_type"))
            GalaxyType = Convert.ToInt32(parameters["galaxy_type"]);

        if (parameters.ContainsKey("spectral_distribution"))
            SpectralDistribution = Convert.ToInt32(parameters["spectral_distribution"]);

        // Define the galactic center in sector coordinates
        _galacticCenterSector = new Vector3Int(0, 0, 0);

        // Initialize random number generator with seed
        if (RandomSeed == 0)
        {
            // Use system time as seed if no seed specified
            Roll.SetSeed(DateTime.Now.Millisecond);
        }
        else
        {
            Roll.SetSeed(RandomSeed);
        }
        
        _systemGenerator = new SystemGenerator(EntityRegistry.Instance);
    }

    /// <summary>
    /// Generates a sector at the specified coordinates.
    /// </summary>
    /// <param name="sectorCoords">The coordinates of the sector in the galactic grid</param>
    /// <returns>A populated sector with randomized star systems</returns>
    public Sector GenerateSector(Vector3Int sectorCoords)
    {
        // Create a new sector with the specified coordinates
        var sector = ScriptableObject.CreateInstance<Sector>();
        sector.ID = Guid.NewGuid().ToString();
        sector.SectorX = sectorCoords.x;
        sector.SectorY = sectorCoords.y;
        sector.SectorZ = sectorCoords.z;
        
        // Calculate vector from galactic center to this sector
        Vector3Int relativeCoords = new Vector3Int(
            sectorCoords.x + EarthOffsetX,
            sectorCoords.y + EarthOffsetY, 
            sectorCoords.z - EarthOffsetZ
        );
        
        // Calculate the position of this sector's center in parsecs from galactic center
        Vector3 sectorCenterParsecs = new Vector3(
            relativeCoords.x * ParsecsPerSector + (ParsecsPerSector / 2.0f),
            relativeCoords.y * ParsecsPerSector + (ParsecsPerSector / 2.0f),
            relativeCoords.z * ParsecsPerSector + (ParsecsPerSector / 2.0f)
        );
        
        // Calculate spherical coordinates
        // Distance from galactic core (in parsecs)
        float distanceFromCore = sectorCenterParsecs.magnitude;
        
        // Calculate azimuth angle (theta) - angle on galactic plane from Earth's meridian
        float theta = Mathf.Atan2(sectorCenterParsecs.y, sectorCenterParsecs.x) * (180f / Mathf.PI);
        if (theta < 0) theta += 360f;
        
        // Calculate elevation angle (phi) - angle from galactic plane
        float horizontalDistance = Mathf.Sqrt(
            sectorCenterParsecs.x * sectorCenterParsecs.x + 
            sectorCenterParsecs.y * sectorCenterParsecs.y
        );
        float phi = Mathf.Atan2(sectorCenterParsecs.z, horizontalDistance) * (180f / Mathf.PI);
        
        // Format the sector ID using these coordinates
        // Galactic sector identified by azimuth, elevation, and distance from core
        sector.ID = $"SEC_{theta:F1}/{phi:F1}/{distanceFromCore:F0}";
        
        // Generate the parsec grid with the configured size
        sector.GenerateParsecsGrid(ParsecsPerSector);

        // Loop through all parsecs in the sector
        foreach (var coord in sector.GenerateGrid(ParsecsPerSector))
        {
            // Apply galaxy structure modifications to star density based on galaxy type
            float modifiedDensity = GalaxyStructureHelper.ApplyGalaxyStructure(
                relativeCoords, // Use relative coordinates for density calculation
                coord, 
                StarDensity,
                GalaxyType,
                SectorSizeX,
                SectorSizeY,
                SectorSizeZ,
                ParsecsPerSector);
            
            if (Roll.FindRange(0f, 1f) < modifiedDensity)
            {
                // Calculate absolute position in light years
                Vector3 position = new Vector3(
                    (sectorCoords.x * ParsecsPerSector + coord.x) * LightYearsPerParsec,
                    (sectorCoords.y * ParsecsPerSector + coord.y) * LightYearsPerParsec,
                    (sectorCoords.z * ParsecsPerSector + coord.z) * LightYearsPerParsec
                );

                // Add random offset (up to 0.5 ly in any direction)
                Vector3 offset = new Vector3(
                    Roll.FindRange(-0.5f, 0.5f) * LightYearsPerParsec,
                    Roll.FindRange(-0.5f, 0.5f) * LightYearsPerParsec,
                    Roll.FindRange(-0.5f, 0.5f) * LightYearsPerParsec
                );

                // Create and initialize the new star system
                var newSystem = new StarSystem
                {
                    ID = $"{sector.ID}-{coord.x}{coord.y}{coord.z}",
                    Position = position + offset,
                    ParentSectorID = sector.ID  // Added missing property
                };

                // Add the system to the sector
                sector.Systems.Add(newSystem);  // Assuming Systems is a List<StarSystem> in Sector
                
                // Generate the system's stars, planets, etc.
                _systemGenerator.GenerateSystem(newSystem);
            }
            else if (Roll.FindRange(0f, 1f) < AnomalyChance)
            {
                // Generate an anomaly (to be implemented)
            }
        }
        
        // Calculate distances between all systems
        sector.CalculateDistanceMap();
        return sector;
    }
}