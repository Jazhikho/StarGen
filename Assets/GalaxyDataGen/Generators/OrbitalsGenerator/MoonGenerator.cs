using System;
using UnityEngine;
using Libraries;

/// <summary>
/// Specialized generator for moon bodies orbiting a planet.
/// </summary>
public class MoonGenerator : CelestialBodyGenerator<Moon>
{
    StarStructure parentStar;
    
    /// <summary>
    /// Creates a new moon generator with the specified registry.
    /// </summary>
    public MoonGenerator(EntityRegistry registry = null) : base(registry)
    {
    }

    /// <summary>
    /// Generates a moon based on its physical characteristics and parent planet.
    /// </summary>
    /// <param name="mass">Moon mass in Earth masses</param>
    /// <param name="orbitalDistance">Orbital distance in planet radii</param>
    /// <param name="parentPlanet">Parent planet</param>
    /// <param name="eccentricity">Orbital eccentricity</param>
    /// <param name="parentZone">Parent's orbital zone classification</param>
    /// <returns>A fully generated moon</returns>
    public Moon GenerateMoon(
        float mass,
        float orbitalDistance,
        Planet parentPlanet,
        float eccentricity,
        ZoneType parentZone)
    {
        parentStar = registry.GetStar(parentPlanet.ParentStarID);

        // Generate basic properties common to all celestial bodies
        Moon moon = GenerateBasicProperties(mass, orbitalDistance, parentZone, eccentricity, parentStar);
        
        // Set name based on parent planet
        // moon.Name = $"{parentPlanet.Name} {(char)('a' + parentPlanet.Moons.Count)}";
        
        // Set moon-specific properties
        moon.OrbitalDistance = orbitalDistance;
        moon.ParentPlanetID = parentPlanet.ID;
        
        // Set parent system ID
        moon.ParentSystemID = parentPlanet.ParentSystemID;
        
        // Calculate orbital period
        moon.OrbitalPeriod = Utils.MoonOrbitalPeriod(
            orbitalDistance,
            parentPlanet.Mass,
            moon.Mass
        );
        
        // Calculate rotation and tidal effects
        var rotationData = CalculateRotation(
            mass, 
            moon.Radius,
            orbitalDistance, 
            parentPlanet.Mass,
            false, // not orbiting a star
            5.0f // system age
        );
        moon.RotationPeriod = rotationData.rotationPeriod;
        moon.TidallyLocked = rotationData.tidallyLocked;
        
        // Calculate axial tilt (moons often have less tilt, especially if tidally locked)
        if (!moon.TidallyLocked)
        {
            const float BASE_MOON_TILT = 7.5f;
            const float TILT_VARIATION = 0.5f;
            moon.AxialTilt = Roll.Vary(BASE_MOON_TILT, TILT_VARIATION);
            moon.AxialTilt = Mathf.Clamp(moon.AxialTilt, 0f, 15f);
        }
        else
        {
            moon.AxialTilt = 0.5f; // Even tidally locked moons have slight wobble
        }
        
        // Calculate tidal heating
        moon.TidalHeating = CalculateTidalHeating(
            moon.Mass,
            moon.Radius,
            parentPlanet.Mass,
            orbitalDistance,
            eccentricity
        );
        
        // Get parent star information for thermal calculations
        if (!string.IsNullOrEmpty(parentPlanet.ParentStarID))
        {
            // Find the parent star using registry
            StarStructure parentStar = registry.GetStar(parentPlanet.ParentStarID);
            
            if (parentStar != null)
            {
                // Calculate distance from star (approximation)
                float distanceFromStar = parentPlanet.SemiMajorAxis;
                
                // Finalize common properties using star data
                FinalizeCelestialBody(moon, parentStar.Luminosity, distanceFromStar);
                
                // Update temperature for additional moon-specific factors like tidal heating
                AdjustMoonTemperature(moon, parentPlanet);
            }
            else
            {
                Debug.LogWarning($"Failed to find parent star with ID {parentPlanet.ParentStarID} for moon of planet {parentPlanet.ID}");
                FinalizeCelestialBody(moon, 1.0f, 1.0f);
            }
        }
        else
        {
            // Fallback if no star information is available
            Debug.LogWarning($"No parent star ID found for planet {parentPlanet.ID}");
            FinalizeCelestialBody(moon, 1.0f, 1.0f);
        }
        
        return moon;
    }
    
    /// <summary>
    /// Calculates tidal heating for a moon.
    /// </summary>
    private float CalculateTidalHeating(
        float moonMass,
        float moonRadius,
        float planetMass,
        float orbitalDistance,
        float eccentricity)
    {
        // Skip the calculation if eccentricity is too low
        if (eccentricity < 0.001f)
            return 0f;
            
        // Basic tidal heating equation (simplified)
        // Based on Peale's equation
        
        // Convert to SI units
        float massInKg = moonMass * Utils.EARTH_MASS;
        float radiusInM = moonRadius * Utils.EARTH_RADIUS_TO_METERS;
        float planetMassInKg = planetMass * Utils.EARTH_MASS;
        float distanceInM = orbitalDistance * moonRadius * Utils.EARTH_RADIUS_TO_METERS;
        
        // Calculate tidal heating in W/m²
        const float LOVE_NUMBER_ROCKY = 0.3f;
        const float TIDAL_DISSIPATION_FACTOR = 100f;
        const float TIDAL_HEATING_COEFFICIENT = 21f;
        const float SURFACE_AREA_COEFFICIENT = 2f;
        
        float heating = TIDAL_HEATING_COEFFICIENT * Utils.G * planetMassInKg * planetMassInKg * 
                    radiusInM * radiusInM * radiusInM * eccentricity * eccentricity * LOVE_NUMBER_ROCKY;
        heating /= SURFACE_AREA_COEFFICIENT * TIDAL_DISSIPATION_FACTOR * massInKg * 
                Mathf.Pow(distanceInM, 6);
        
        // Convert to W/m² by dividing by surface area
        float surfaceArea = 4f * Mathf.PI * radiusInM * radiusInM;
        float heatFlux = heating / surfaceArea;
        
        return heatFlux;
    }
    
    /// <summary>
    /// Adjusts a moon's temperature based on tidal heating and planetary radiation.
    /// </summary>
    private void AdjustMoonTemperature(Moon moon, Planet parentPlanet)
    {
        // Calculate base temperature from star
        float baseTemp = moon.SurfaceTemperature;
        
        // Add planetary heating 
        float planetHeating = CalculatePlanetaryHeating(moon, parentPlanet);
        
        // Add tidal heating contribution
        float tidalTempContribution = Mathf.Sqrt(moon.TidalHeating);
        
        // Combine all heat sources
        moon.SurfaceTemperature = CombineTemperatures(
            baseTemp,
            planetHeating,
            tidalTempContribution
        );
        
        // Recalculate day/night temperatures with combined heat
        var (dayTemp, nightTemp) = GetDayNightTemperatures(moon);
        moon.DaytimeTemperature = dayTemp;
        moon.NighttimeTemperature = nightTemp;
        
        // Adjust geological activity based on tidal heating
        AdjustGeologyForTidalHeating(moon);
    }
    
    /// <summary>
    /// Calculates the temperature contribution from the parent planet to the moon.
    /// </summary>
    private float CalculatePlanetaryHeating(Moon moon, Planet parentPlanet)
    {
        // Simple approximation of planetary thermal radiation
        float planetRadius = parentPlanet.Radius * Utils.EARTH_RADIUS; // km
        float moonDistance = moon.OrbitalDistance * parentPlanet.Radius * Utils.EARTH_RADIUS; // km
        float apparentSize = Mathf.Atan2(planetRadius, moonDistance) * 2f;
        
        // Calculate incident radiation, accounting for planet albedo
        float planetEmission = Utils.STEFAN_BOLTZMANN * Mathf.Pow(parentPlanet.SurfaceTemperature, 4) * 
                            (1f - parentPlanet.Albedo);
        
        // Adjust for solid angle of planet in moon's sky
        float solidAngle = 2f * Mathf.PI * (1f - Mathf.Cos(apparentSize / 2f));
        float fractionOfSky = solidAngle / (4f * Mathf.PI);
        
        float incidentFlux = planetEmission * fractionOfSky;
        
        // Convert flux to temperature contribution
        const float PLANETARY_HEATING_FACTOR = 0.5f;
        float tempContribution = Mathf.Pow(incidentFlux / Utils.STEFAN_BOLTZMANN, 0.25f) * 
                                PLANETARY_HEATING_FACTOR;
        
        return tempContribution;
    }
    
    /// <summary>
    /// Combines multiple temperature sources using energy balance principles.
    /// </summary>
    private float CombineTemperatures(params float[] temperatures)
    {
        if (temperatures.Length == 0)
            return 0f;
            
        // Energy adds linearly, then convert back to temperature
        // Using Stefan-Boltzmann relation: E = σT⁴
        
        float totalEnergy = 0f;
        foreach (float temp in temperatures)
        {
            if (temp > 0)
                totalEnergy += Utils.STEFAN_BOLTZMANN * Mathf.Pow(temp, 4f);
        }
        
        return Mathf.Pow(totalEnergy / Utils.STEFAN_BOLTZMANN, 0.25f);
    }
    
    /// <summary>
    /// Calculates day/night temperature variations based on rotation.
    /// </summary>
    private (float dayTemp, float nightTemp) GetDayNightTemperatures(Moon moon)
    {
        float avgTemp = moon.SurfaceTemperature;
        float rotationPeriod = moon.RotationPeriod; // in Earth days
        
        float temperatureRange;
        
        if (moon.TidallyLocked)
        {
            // Tidally locked moons have extreme day/night differences
            temperatureRange = avgTemp * 0.8f;
        }
        else if (rotationPeriod < 1f)
        {
            // Fast rotators have minimal day/night differences
            temperatureRange = avgTemp * 0.1f;
        }
        else
        {
            // Moderate rotators have intermediate differences
            temperatureRange = avgTemp * Mathf.Min(0.5f, rotationPeriod / 10f);
        }
        
        // Atmosphere reduces day/night differences
        if (moon.HasAtmosphere)
        {
            float atmosphereFactor = Mathf.Min(1f, moon.AtmosphericPressure / 0.1f);
            temperatureRange *= (1f - atmosphereFactor * 0.9f);
        }
        
        float dayTemp = avgTemp + temperatureRange / 2f;
        float nightTemp = avgTemp - temperatureRange / 2f;
        
        // Ensure night temperature doesn't go below absolute zero
        nightTemp = Mathf.Max(3f, nightTemp);
        
        return (dayTemp, nightTemp);
    }
    
    /// <summary>
    /// Adjusts geological activity based on tidal heating.
    /// </summary>
    private void AdjustGeologyForTidalHeating(Moon moon)
    {
        if (moon.TidalHeating > 0)
        {
            // Use a constant from MoonLibrary or define here
            const float CRITICAL_TIDAL_FORCE = 0.5f; // W/m²
            
            float heatingFactor = moon.TidalHeating / CRITICAL_TIDAL_FORCE;
            moon.TectonicActivity = Mathf.Min(1f, moon.TectonicActivity + (heatingFactor * 0.3f));
            moon.VolcanicActivity = Mathf.Min(1f, moon.VolcanicActivity + (heatingFactor * 0.4f));
            
            // Extreme tidal heating can create geological instability
            const float EXTREME_TIDAL_FORCE = 2f; // W/m²
            if (moon.TidalHeating > EXTREME_TIDAL_FORCE)
            {
                const float GEOTHERMAL_ABUNDANCE_CENTER = 0.7f;
                float geothermalResource = Roll.Vary(GEOTHERMAL_ABUNDANCE_CENTER, 0.2f);
                moon.AddResource("Geothermal Energy", geothermalResource);
            }
        }
    }
    
    /// <summary>
    /// Override registration to specifically handle moons
    /// </summary>
    protected override void RegisterBody(Moon moon)
    {
        registry.RegisterMoon(moon);
    }
}