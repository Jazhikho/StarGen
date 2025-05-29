using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// Converts between actual star system data and visualization data structures
/// </summary>
public class StarSystemDataConverter
{
    // Scale constants - adjust these to balance visibility vs realism
    public const float AU_TO_UNITY_DISTANCE = 10f;  // 1 AU = 10 Unity units
    public const float EARTH_RADIUS_TO_UNITY = 0.05f; // Earth radius = 0.05 Unity units
    public const float SOLAR_RADIUS_TO_UNITY = 1f;   // Sun radius = 1 Unity units
    
    /// <summary>
    /// Converts a StarSystem to StarSystemData for visualization
    /// </summary>
    public static StarSystemData ConvertToVisualizationData(StarSystem starSystem)
    {
        if (starSystem == null) return null;
        
        var systemData = new StarSystemData
        {
            systemName = $"System_{starSystem.ID}",
            gravityConstant = 100f // Adjusted for Unity scale
        };
        
        // Handle the root structure - could be a single star, binary, or more complex
        if (starSystem.Stars.Count == 1 && starSystem.BinaryPairs.Count == 0)
        {
            // Simple single star system
            systemData.rootBody = ConvertStar(starSystem.Stars[0], null);
        }
        else if (starSystem.BinaryPairs.Count > 0)
        {
            // Binary system - create barycenter as root
            var rootPair = starSystem.GetRootBinaryPair();
            if (rootPair != null)
            {
                systemData.rootBody = ConvertBinaryPair(rootPair, starSystem);
            }
            else
            {
                // Fallback - treat first star as root
                systemData.rootBody = ConvertStar(starSystem.Stars[0], null);
            }
        }
        else
        {
            // Multiple stars without binary pairs - shouldn't happen, but handle gracefully
            systemData.rootBody = ConvertStar(starSystem.Stars[0], null);
        }
        
        return systemData;
    }
    
    /// <summary>
    /// Converts a binary pair to a barycenter celestial body
    /// </summary>
    private static CelestialBodyData ConvertBinaryPair(StarSystem.BinaryPair binaryPair, StarSystem system)
    {
        // Get stars from their IDs
        StarStructure primaryStar = null;
        StarStructure secondaryStar = null;
        
        if (binaryPair.PrimaryType == "Star")
            primaryStar = system.FindStarByID(binaryPair.PrimaryID);
            
        if (binaryPair.SecondaryType == "Star")
            secondaryStar = system.FindStarByID(binaryPair.SecondaryID);
        
        // Calculate total mass
        float totalMass = 0f;
        if (primaryStar != null) totalMass += primaryStar.Mass;
        if (secondaryStar != null) totalMass += secondaryStar.Mass;
        
        var barycentrData = new CelestialBodyData
        {
            name = binaryPair.ID,
            bodyType = CelestialBodyType.Barycenter,
            mass = totalMass * 332946f, // Total mass in Earth masses
            scale = 0.1f // Small visual representation
        };
        
        // Add stars to the barycenter
        if (primaryStar != null)
        {
            var primaryData = ConvertStar(primaryStar, binaryPair);
            // Primary star orbit
            SetBinaryOrbitParameters(primaryData, binaryPair.PrimaryOrbitRadius, binaryPair.OrbitalPeriod, true);
            barycentrData.satellites.Add(primaryData);
        }
        
        if (secondaryStar != null)
        {
            var secondaryData = ConvertStar(secondaryStar, binaryPair);
            // Secondary star orbit
            SetBinaryOrbitParameters(secondaryData, binaryPair.SecondaryOrbitRadius, binaryPair.OrbitalPeriod, false);
            barycentrData.satellites.Add(secondaryData);
        }
        
        // Add circumbinary planets
        foreach (var planet in binaryPair.CircumbinaryPlanets)
        {
            var planetData = ConvertPlanet(planet, binaryPair.CircumbinaryOrbits);
            barycentrData.satellites.Add(planetData);
        }
        
        // Add circumbinary asteroid belts
        foreach (var belt in binaryPair.CircumbinaryAsteroidBelts)
        {
            var beltData = ConvertAsteroidBelt(belt);
            barycentrData.satellites.Add(beltData);
        }
        
        return barycentrData;
    }
    
    /// <summary>
    /// Sets orbital parameters for binary star components
    /// </summary>
    private static void SetBinaryOrbitParameters(CelestialBodyData bodyData, float orbitRadius, float period, bool isPrimary)
    {
        bodyData.semiMajorAxis = orbitRadius * AU_TO_UNITY_DISTANCE;
        bodyData.eccentricity = 0.05f; // Low eccentricity for binary stars
        bodyData.inclination = Random.Range(-5f, 5f); // Small random inclination
        bodyData.argumentOfPerifocus = isPrimary ? 0f : 180f; // Opposite sides
        bodyData.longitudeOfAscendingNode = 0f;
        bodyData.meanAnomaly = isPrimary ? 0f : 180f; // Start on opposite sides
        
        // Convert period from years to our time scale (simplified)
        bodyData.orbitalPeriod = period * 365.25f; // Convert to days for now
    }
    
    /// <summary>
    /// Converts a StarStructure to CelestialBodyData
    /// </summary>
    private static CelestialBodyData ConvertStar(StarStructure star, StarSystem.BinaryPair parentBinary)
    {
        var starData = new CelestialBodyData
        {
            name = star.ID,
            bodyType = CelestialBodyType.Star,
            mass = star.Mass * 332946f, // Convert solar masses to Earth masses
            scale = star.Radius * SOLAR_RADIUS_TO_UNITY,
            radius = star.Radius,
            color = star.StarColor,
            temperature = star.Temperature
        };
        
        // Add planets as satellites
        foreach (var planet in star.Planets)
        {
            var planetData = ConvertPlanet(planet, star.Orbits);
            starData.satellites.Add(planetData);
        }
        
        // Add asteroid belts as satellites
        foreach (var belt in star.AsteroidBelts)
        {
            var beltData = ConvertAsteroidBelt(belt);
            starData.satellites.Add(beltData);
        }
        
        return starData;
    }
    
    /// <summary>
    /// Converts a Planet to CelestialBodyData
    /// </summary>
    private static CelestialBodyData ConvertPlanet(Planet planet, List<OrbitGenerator.Orbit> availableOrbits)
    {
        var planetData = new CelestialBodyData
        {
            name = planet.ID,
            bodyType = CelestialBodyType.Planet,
            mass = planet.Mass,
            scale = planet.Radius * EARTH_RADIUS_TO_UNITY,
            radius = planet.Radius,
            color = GetPlanetColor(planet),
            temperature = planet.SurfaceTemperature,
            rotationPeriod = planet.RotationPeriod,
            axialTilt = planet.AxialTilt,
            hasAtmosphere = planet.HasAtmosphere,
            atmosphericPressure = planet.AtmosphericPressure
        };
        
        // Find matching orbit data
        var orbit = FindOrbitForPlanet(planet, availableOrbits);
        if (orbit != null)
        {
            SetOrbitParameters(planetData, orbit);
        }
        else
        {
            // Fallback orbital parameters
            planetData.semiMajorAxis = planet.SemiMajorAxis * AU_TO_UNITY_DISTANCE;
            planetData.eccentricity = planet.Eccentricity;
            planetData.orbitalPeriod = planet.OrbitalPeriod * 365.25f;
        }
        
        // Add moons as satellites
        foreach (var moon in planet.Moons)
        {
            var moonData = ConvertMoon(moon);
            planetData.satellites.Add(moonData);
        }
        
        return planetData;
    }
    
    /// <summary>
    /// Converts a Moon to CelestialBodyData
    /// </summary>
    private static CelestialBodyData ConvertMoon(Moon moon)
    {
        var moonData = new CelestialBodyData
        {
            name = moon.ID,
            bodyType = CelestialBodyType.Moon,
            mass = moon.Mass,
            scale = moon.Radius * EARTH_RADIUS_TO_UNITY,
            radius = moon.Radius,
            color = GetMoonColor(moon),
            temperature = moon.SurfaceTemperature,
            rotationPeriod = moon.RotationPeriod,
            axialTilt = moon.AxialTilt,
            hasAtmosphere = moon.HasAtmosphere
        };
        
        // Convert moon orbital parameters (relative to planet)
        moonData.semiMajorAxis = moon.OrbitalDistance * EARTH_RADIUS_TO_UNITY;
        moonData.eccentricity = moon.Eccentricity;
        moonData.orbitalPeriod = moon.OrbitalPeriod;
        moonData.inclination = moon.Inclination;
        moonData.meanAnomaly = Random.Range(0f, 360f);
        
        return moonData;
    }
    
    /// <summary>
    /// Finds the orbit data that matches a planet's semi-major axis
    /// </summary>
    private static OrbitGenerator.Orbit FindOrbitForPlanet(Planet planet, List<OrbitGenerator.Orbit> orbits)
    {
        return orbits?.FirstOrDefault(o => Mathf.Approximately(o.Distance, planet.SemiMajorAxis));
    }
    
    /// <summary>
    /// Sets orbital parameters from OrbitGenerator.Orbit data
    /// </summary>
    private static void SetOrbitParameters(CelestialBodyData bodyData, OrbitGenerator.Orbit orbit)
    {
        // Clamp orbital distance to prevent Unity issues
        bodyData.semiMajorAxis = orbit.Distance * AU_TO_UNITY_DISTANCE;
        bodyData.eccentricity = Mathf.Clamp01(orbit.Eccentricity); // Ensure valid eccentricity
        bodyData.inclination = Mathf.Clamp(orbit.Inclination, -180f, 180f);
        bodyData.argumentOfPerifocus = Random.Range(0f, 360f);
        bodyData.longitudeOfAscendingNode = Random.Range(0f, 360f);
        bodyData.meanAnomaly = Random.Range(0f, 360f);
        bodyData.orbitalPeriod = CalculateOrbitalPeriod(orbit.Distance, bodyData.mass);
        
        // Validate all values are finite
        if (!IsValidFloat(bodyData.semiMajorAxis) || !IsValidFloat(bodyData.eccentricity))
        {
            Debug.LogWarning($"Invalid orbital parameters for {bodyData.name}, using defaults");
        }
    }

    private static bool IsValidFloat(float value)
    {
        return !float.IsNaN(value) && !float.IsInfinity(value) && value > -1e6f && value < 1e6f;
    }
    
    /// <summary>
    /// Calculates orbital period using Kepler's 3rd law (simplified)
    /// </summary>
    private static float CalculateOrbitalPeriod(float semiMajorAxisAU, float bodyMass)
    {
        // Simplified - assumes solar mass attractor
        return Mathf.Sqrt(Mathf.Pow(semiMajorAxisAU, 3)) * 365.25f; // In Earth days
    }
    
    /// <summary>
    /// Determines planet color based on properties
    /// </summary>
    private static Color GetPlanetColor(Planet planet)
    {
        // Simple color assignment based on temperature and type
        if (planet.SurfaceTemperature > 2000) return new Color(1f, 0.3f, 0.1f); // Hot/lava
        if (planet.SurfaceTemperature > 373) return new Color(0.8f, 0.4f, 0.2f);  // Venus-like
        if (planet.WaterCoverage > 0.3f) return new Color(0.2f, 0.5f, 1f);       // Earth-like
        if (planet.HasAtmosphere) return new Color(0.8f, 0.6f, 0.4f);            // Atmospheric
        return new Color(0.5f, 0.5f, 0.5f); // Rocky/default
    }
    
    /// <summary>
    /// Determines moon color based on properties
    /// </summary>
    private static Color GetMoonColor(Moon moon)
    {
        if (moon.HasAtmosphere) return new Color(0.9f, 0.7f, 0.5f);
        if (moon.SurfaceTemperature < 200) return new Color(0.8f, 0.9f, 1f); // Ice
        return new Color(0.6f, 0.6f, 0.6f); // Rocky
    }

    /// <summary>
    /// Converts an AsteroidBelt to CelestialBodyData
    /// </summary>
    private static CelestialBodyData ConvertAsteroidBelt(AsteroidBelt belt)
    {
        // Calculate a midpoint for the belt's distance
        float middleRadius = (belt.InnerRadius + belt.OuterRadius) * 0.5f;
        float width = belt.OuterRadius - belt.InnerRadius;
        
        var beltData = new CelestialBodyData
        {
            name = belt.ID,
            bodyType = CelestialBodyType.Asteroid,
            mass = belt.TotalMass,
            scale = width * 0.5f, // Use half the width as scale
            semiMajorAxis = middleRadius * AU_TO_UNITY_DISTANCE,
            radius = width,
            color = GetAsteroidBeltColor(belt)
        };
        
        // Set orbital parameters - typically circular
        beltData.eccentricity = 0.01f; // Very low eccentricity for belts
        beltData.inclination = UnityEngine.Random.Range(0f, 5f); // Small random inclination
        beltData.orbitalPeriod = CalculateOrbitalPeriod(middleRadius, beltData.mass);
        
        return beltData;
    }

    /// <summary>
    /// Determines asteroid belt color based on composition
    /// </summary>
    private static Color GetAsteroidBeltColor(AsteroidBelt belt)
    {
        // Get composition dictionary
        var composition = belt.GetMaterialComposition();
        
        // Base color for asteroid belts
        Color baseColor = new Color(0.6f, 0.6f, 0.6f);
        
        if (composition.Count > 0)
        {
            // Find dominant composition type
            string dominantType = "";
            float highestPercentage = 0f;
            
            foreach (var kvp in composition)
            {
                if (kvp.Value > highestPercentage)
                {
                    highestPercentage = kvp.Value;
                    dominantType = kvp.Key;
                }
            }
            
            // Set color based on dominant composition
            switch (dominantType)
            {
                case "Metallic":
                    return new Color(0.7f, 0.7f, 0.75f); // Silver-gray
                case "Stony":
                    return new Color(0.65f, 0.6f, 0.55f); // Brown-gray
                case "Carbonaceous":
                    return new Color(0.3f, 0.3f, 0.35f); // Dark gray
                case "Icy":
                    return new Color(0.8f, 0.9f, 1.0f); // Icy blue-white
                case "Mixed":
                    return new Color(0.6f, 0.55f, 0.5f); // Mixed color
                case "Basaltic":
                    return new Color(0.4f, 0.4f, 0.45f); // Dark gray with blue tint
            }
        }
        
        return baseColor; // Default color
    }
}