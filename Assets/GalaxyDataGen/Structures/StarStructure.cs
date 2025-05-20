using UnityEngine;
using System;
using System.Collections.Generic;
using Libraries;

/// <summary>
/// Represents a star with its physical and orbital properties.
/// </summary>
[Serializable]
public class StarStructure
{
    /// <summary>Unique identifier for the star</summary>
    public string ID;
    
    /// <summary>Mass in solar masses</summary>
    public float Mass;
    
    /// <summary>Radius in solar radii</summary>
    public float Radius;
    
    /// <summary>Luminosity in solar luminosities</summary>
    public float Luminosity;
    
    /// <summary>Surface temperature in Kelvin</summary>
    public float Temperature;
    
    /// <summary>Spectral classification (O, B, A, F, G, K, M, etc.)</summary>
    public string SpectralClass;
    
    /// <summary>Evolutionary stage (V=Main Sequence, III=Giant, etc.)</summary>
    public StellarEvolutionStage StellarEvolutionStage;
    
    /// <summary>Orbital zones calculated for this star</summary>
    public OrbitalZones Zones;
    
    /// <summary>Age in billions of years</summary>
    public float Age;
    
    /// <summary>Visual color of the star</summary>
    public Color StarColor;
    
    /// <summary>Position within the star system</summary>
    public Vector3 PositionInSystem;
    
    /// <summary>Orbital period (for binary systems) in Earth years</summary>
    public float OrbitalPeriod;
    
    /// <summary>Rotation period in Earth days</summary>
    public float RotationPeriod;
    
    /// <summary>Roche limit of star in AU</summary>
    public float RocheLimit;

    /// <summary>Inner edge of habitable zone in AU</summary>
    public float HabitableZoneInner;
    
    /// <summary>Outer edge of habitable zone in AU</summary>
    public float HabitableZoneOuter;
    
    /// <summary>Distance where volatiles condense in AU</summary>
    public float FrostLine;

    /// <summary>Max distance where planets can form in AU (Hill Sphere)</summary>
    public float HillSphere;
    
    /// <summary>Available orbits around this star</summary>
    [SerializeField]
    public List<OrbitGenerator.Orbit> Orbits = new List<OrbitGenerator.Orbit>();
    
    /// <summary>Planets orbiting this star</summary>
    [SerializeField]
    public List<Planet> Planets = new List<Planet>();
    
    /// <summary>Asteroid belts orbiting this star</summary>
    [SerializeField]
    public List<AsteroidBelt> AsteroidBelts = new List<AsteroidBelt>();
    
    /// <summary>Whether this star is a pulsar</summary>
    public bool IsPulsar;
    
    /// <summary>Whether this star has variable brightness</summary>
    public bool IsVariableStar;
    
    /// <summary>Whether this star has prominent star spots</summary>
    public bool HasStarSpots;
    
    /// <summary>Whether this star experiences significant flare activity</summary>
    public bool HasStellarFlares;
    
    /// <summary>ID of the parent star system (for later lookup)</summary>
    public string ParentSystemID;
    
    /// <summary>Constructor initializes a star with a unique ID</summary>
    public StarStructure()
    {
        ID = Guid.NewGuid().ToString();
        Orbits = new List<OrbitGenerator.Orbit>();
        Planets = new List<Planet>();
        AsteroidBelts = new List<AsteroidBelt>();
    }

    /// <summary>
    /// Calculates the habitable zone boundaries based on stellar luminosity.
    /// </summary>
    public void CalculateHabitableZone()
    {
        HabitableZoneInner = 0.95f * Mathf.Sqrt(Luminosity);
        HabitableZoneOuter = 1.37f * Mathf.Sqrt(Luminosity);
    }

    /// <summary>
    /// Calculates the frost line distance based on stellar luminosity.
    /// </summary>
    public void CalculateFrostLine()
    {
        FrostLine = 4.85f * Mathf.Sqrt(Luminosity);
    }
    
    /// <summary>
    /// Adds a planet to this star
    /// </summary>
    /// <param name="planet">The planet to add</param>
    public void AddPlanet(Planet planet)
    {
        planet.ParentStarID = this.ID;
        planet.ParentSystemID = this.ParentSystemID;
        Planets.Add(planet);
    }
    
    /// <summary>
    /// Adds an asteroid belt to this star
    /// </summary>
    /// <param name="belt">The asteroid belt to add</param>
    public void AddAsteroidBelt(AsteroidBelt belt)
    {
        belt.ParentStarID = this.ID;
        belt.ParentSystemID = this.ParentSystemID;
        AsteroidBelts.Add(belt);
    }
    
    /// <summary>
    /// Finds a planet by ID
    /// </summary>
    /// <param name="planetID">The ID of the planet to find</param>
    /// <returns>The found planet or null if not found</returns>
    public Planet FindPlanetByID(string planetID)
    {
        return Planets.Find(p => p.ID == planetID);
    }
}

/// <summary>
/// Contains the orbital zone definitions for a star or binary system.
/// </summary>
[Serializable]
public class OrbitalZones
{
    public float EpistellarInner;
    public float EpistellarOuter;
    public float InnerZoneStart;
    public float HabitableZoneInner;
    public float HabitableZoneOuter;
    public float FrostLine;
    public float SystemLimit;

    /// <summary>
    /// Determines the type of zone at a given orbital distance.
    /// </summary>
    /// <param name="distance">Distance in AU</param>
    /// <returns>The zone type at the specified distance</returns>
    public ZoneType GetZoneType(float distance)
    {
        if (distance <= EpistellarOuter) return ZoneType.Epistellar;
        if (distance <= HabitableZoneInner) return ZoneType.Inner;
        if (distance <= HabitableZoneOuter) return ZoneType.Habitable;
        if (distance <= FrostLine) return ZoneType.Outer;
        if (distance <= SystemLimit) return ZoneType.FarOuter;
        return ZoneType.Beyond;
    }
}

/// <summary>
/// Defines the different types of orbital zones in a star system.
/// </summary>
public enum ZoneType
{
    Epistellar,  // Very close to star
    Inner,       // Between epistellar and habitable
    Habitable,   // Where liquid water could exist
    Outer,       // Beyond frost line but within system
    FarOuter,    // Edge of solar system
    Beyond       // Beyond system limit
}