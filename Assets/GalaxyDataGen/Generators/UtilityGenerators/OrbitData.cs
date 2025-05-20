using System;
using System.Collections.Generic;
using UnityEngine;
using Libraries;

public static class OrbitData
{
    /// <summary>
    /// Determines if a planet can retain moons based on its mass and the stellar mass
    /// </summary>
    public static bool DetermineHasMoons(float planetMass, float stellarMass)
    {
        // Larger planets are more likely to have moons
        // Scale chances based on planet mass
        float baseChance = 0f;

        if (planetMass >= 100f) baseChance = 0.95f;      // Gas giant range
        else if (planetMass >= 10f) baseChance = 0.8f;   // Ice giant/sub-gas giant range
        else if (planetMass >= 2f) baseChance = 0.6f;    // Super-Earth/Sub-Neptune range
        else if (planetMass >= 0.5f) baseChance = 0.4f;  // Earth-like range
        else if (planetMass >= 0.1f) baseChance = 0.2f;  // Small rocky planet range
        else baseChance = 0.05f;                         // Dwarf planet range

        // Adjust based on stellar mass (higher mass stars might disrupt moon formation)
        baseChance *= Mathf.Max(0.5f, 2f - (stellarMass * 0.5f));

        return Roll.FindRange(0f, 1f) <= baseChance;
    }

    /// <summary>
    /// Generates orbital eccentricity for planets or asteroid belts
    /// </summary>
    public static float GenerateEccentricity(bool isAsteroidBelt)
    {
        if (isAsteroidBelt)
        {
            // Asteroid belts tend to have higher eccentricities
            return Roll.FindRange(0.1f, 0.3f);
        }
        
        // Most planets have low eccentricity with occasional high ones
        if (Roll.FindRange(0f, 1f) <= 0.1f) // 10% chance of high eccentricity
        {
            return Roll.FindRange(0.1f, 0.5f);
        }
        
        return Roll.FindRange(0.01f, 0.1f);
    }

    /// <summary>
    /// Generates orbital inclination for planets
    /// </summary>
    public static float GenerateInclination()
    {
        // Most planets stay within a few degrees
        // But allow for occasional high inclinations
        if (Roll.FindRange(0f, 1f) <= 0.05f) // 5% chance of high inclination
        {
            return Roll.FindRange(10f, 30f);
        }
        
        return Roll.FindRange(0f, 10f);
    }

    /// <summary>
    /// Calculates the approximate equilibrium temperature at a given distance from a star
    /// </summary>
    /// <param name="stellarLuminosity">Star's luminosity in solar units</param>
    /// <param name="orbitalDistance">Distance from star in AU</param>
    /// <param name="albedo">Planet's albedo (reflectivity), default 0.3</param>
    /// <returns>Equilibrium temperature in Kelvin</returns>
    public static float CalculateEquilibriumTemperature(float stellarLuminosity, float orbitalDistance, float albedo = 0.3f)
    {
        return Astro.BlackbodyTemperature(stellarLuminosity, albedo, orbitalDistance);
    }

    /// <summary>
    /// Estimates planet density based on mass and orbital zone
    /// </summary>
    public static float EstimatePlanetDensity(float mass, ZoneType zone)
    {
        // Base density on mass and zone
        float baseDensity;
        
        if (mass >= 10f) {
            // Gas/Ice giants - very low density
            baseDensity = 0.15f + mass * 0.005f;
        } else if (mass >= 2f) {
            // Super-Earths - variable density
            baseDensity = 0.6f + mass * 0.1f;
        } else {
            // Terrestrial planets - higher density
            baseDensity = 0.8f + mass * 0.2f;
        }
        
        // Modify based on zone
        switch (zone)
        {
            case ZoneType.Epistellar:
                baseDensity *= 0.95f; // Slight reduction due to heat expansion
                break;
            case ZoneType.FarOuter:
                baseDensity *= 1.1f; // Slight increase for ice-rich bodies
                break;
        }
        
        return Roll.Vary(baseDensity, 0.15f); // Add some variation
    }

    /// <summary>
    /// Estimates water content based on zone and temperature
    /// </summary>
    public static float EstimateWaterContent(ZoneType zone, float temperature)
    {
        float baseWaterContent = 0f;
        
        switch (zone)
        {
            case ZoneType.Epistellar:
                baseWaterContent = 0.00f; // Arid
                break;
                
            case ZoneType.Inner:
                if (temperature > 600f) 
                    baseWaterContent = 0.05f; // Very dry
                else 
                    baseWaterContent = Roll.FindRange(0.1f, 0.4f); // somewhat dry
                break;
                
            case ZoneType.Habitable:
                baseWaterContent = Roll.FindRange(0.0f, 1.0f); // Variable water content
                break;
                
            case ZoneType.Outer:
                baseWaterContent = Roll.FindRange(0.5f, 0.9f); // Ice-rich
                break;
                
            case ZoneType.FarOuter:
                baseWaterContent = Roll.FindRange(0.7f, 0.95f); // Very ice-rich
                break;
        }
        
        return baseWaterContent;
    }

    /// <summary>
    /// Determines planet type based on physical characteristics
    /// </summary>
    public static string DeterminePlanetType(
        float mass, 
        float density, 
        ZoneType zone, 
        float stellarLuminosity,
        float orbitalDistance,
        float waterContent = -1f) // -1 means auto-calculate
    {
        // Calculate temperature if not provided
        float temperature = CalculateEquilibriumTemperature(stellarLuminosity, orbitalDistance);
        
        // Estimate water content if not provided
        if (waterContent < 0) 
        {
            waterContent = EstimateWaterContent(zone, temperature);
        }
        
        // First handle special cases
        if (mass < 0.01f) return "Asterian"; // Asteroid-sized
        if (mass < 0.1f) return "Metian";    // Mercury-like or smaller
        
        bool insideFrostLine = zone == ZoneType.Epistellar || zone == ZoneType.Inner || zone == ZoneType.Habitable;
        bool isHot = temperature > 700f;
        bool isExtreme = temperature > 1200f || (zone == ZoneType.Epistellar && mass > 0.3f);
        
        // Extreme volcanic activity for Earth-sized or smaller
        if (isExtreme && mass < 2f) return "Vulcanian";
        
        // Stripped cores - extremely dense for their mass
        if (isHot && mass > 5f && density > 0.8f * mass) return "Cronusian";
        
        // Carbon planets - special composition type
        if (density > 1.1f && zone != ZoneType.FarOuter && mass < 10f) return "Lelantian";
        
        // Now classify by mass and frost line position
        if (mass >= 50f) {
            if (zone == ZoneType.Epistellar || zone == ZoneType.Inner) return "Hyperion"; // Hot gas giant
            return "Atlantean"; // Cold gas giant
        }
        else if (mass >= 30f) {
            if (insideFrostLine) return "Hyperion";      // Hot gas giant
            return "Atlantean";                         // Cold gas giant
        }
        else if (mass >= 20f) {
            if (insideFrostLine) return "Hyperion";      // Hot gas giant
            if (density < 0.2f) return "Helian";        // Helium-rich
            return "Iapetian";                          // Ice giant
        }
        else if (mass >= 10f) {
            if (insideFrostLine) return "Criusian";      // Hot mini-Neptune
            return "Iapetian";                          // Ice giant
        }
        else if (mass >= 2f) {
            if (insideFrostLine) {
                if (waterContent > 0.9f) return "Oceanian"; // Ocean super-Earth
                return "Rhean";                             // Super-Earth
            } else {
                if (temperature > 200f) return "Theian";    // Cold mini-Neptune
                return "Dionean";                           // Icy Super-Earth
            }
        }
        else {
            // Earth-sized and smaller planets
            if (!insideFrostLine) return "Phoeboan";        // Ice dwarf
            
            // Inside frost line - classify by water content
            if (waterContent > 0.9f) return "Oceanian";     // Ocean world
            if (waterContent > 0.5f) return "Gaian";        // Earth-like
            if (waterContent > 0.25f) return "Tethysian";   // Mesic
            if (waterContent > 0.01f) return "Promethean";  // Arid
            return "Menoetian";                             // Barren
        }
    }
}