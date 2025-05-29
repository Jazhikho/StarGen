using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class CelestialBodyData
{
    [Header("Basic Properties")]
    public string name;
    public CelestialBodyType bodyType;
    public float mass;
    public float scale; // Visual scale in Unity units
    public float radius; // Actual radius in standard units
    public Color color;
    
    [Header("Physical Properties")]
    public float temperature; // In Kelvin
    public float rotationPeriod; // In standard time units
    public float axialTilt; // In degrees
    public bool hasAtmosphere;
    public float atmosphericPressure; // In standard units
    
    [Header("Orbital Parameters")]
    public float semiMajorAxis;
    public float eccentricity;
    public float inclination;
    public float argumentOfPerifocus;
    public float longitudeOfAscendingNode;
    public float meanAnomaly;
    public float orbitalPeriod; // For reference/display
    
    [Header("Children")]
    [SerializeReference]
    public List<CelestialBodyData> satellites = new List<CelestialBodyData>();
}

public enum CelestialBodyType
{
    Barycenter,
    Star,
    Planet,
    Moon,
    Asteroid
}

[Serializable]
public class StarSystemData
{
    public string systemName;
    public float gravityConstant = 100f;
    public CelestialBodyData rootBody;
    
    [Header("Scale Settings")]
    public float distanceScale = StarSystemDataConverter.AU_TO_UNITY_DISTANCE;
    public float bodyScale = 1f;
}