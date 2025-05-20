using UnityEngine;
using System;
using Libraries;

/// <summary>
/// Provides methods for calculating orbital zones around stars and binary systems.
/// </summary>
public static class ZoneHelper
{
    /// <summary>
    /// Calculates the orbital zones for a single star.
    /// </summary>
    /// <param name="star">The star to calculate zones for</param>
    /// <returns>Orbital zones object with boundaries in AU</returns>
    public static OrbitalZones CalculateOrbitalZones(StarStructure star)
    {
        var zones = new OrbitalZones();

        // Epistellar zone (very close to the star)
        zones.EpistellarInner = CalculateEpistellarInnerLimit(star);
        zones.EpistellarOuter = star.Radius * Utils.SOLAR_RADIUS_TO_AU;  // Fixed conversion

        // Inner system zone starts where epistellar ends
        zones.InnerZoneStart = zones.EpistellarOuter;

        // Calculate habitable zone based on stellar luminosity
        zones.HabitableZoneInner = 0.95f * Mathf.Sqrt(star.Luminosity);
        zones.HabitableZoneOuter = 1.37f * Mathf.Sqrt(star.Luminosity);

        // Frost Line (where volatiles condense)
        zones.FrostLine = 4.85f * Mathf.Sqrt(star.Luminosity);

        // System Limit (outer boundary of star's influence)
        zones.SystemLimit = CalculateSystemLimit(star);

        return zones;
    }

    /// <summary>
    /// Calculates the orbital zones for a binary pair.
    /// </summary>
    /// <param name="binaryPair">The binary pair to calculate circumbinary zones for</param>
    /// <param name="parentSystem">The parent system containing the binary pair</param>
    /// <returns>Orbital zones object with boundaries in AU</returns>
    public static OrbitalZones CalculateCircumbinaryZones(StarSystem.BinaryPair binaryPair, StarSystem parentSystem)
    {
        var zones = new OrbitalZones();
        float totalLuminosity = GetTotalLuminosity(binaryPair, parentSystem);
        float separation = binaryPair.SeparationDistance;

        // Circumbinary planets must be beyond critical stability radius
        zones.EpistellarInner = separation * 3f;
        zones.EpistellarOuter = separation * 4f;
        zones.InnerZoneStart = zones.EpistellarOuter;

        // Habitable zone based on combined luminosity
        zones.HabitableZoneInner = Math.Max(0.95f * Mathf.Sqrt(totalLuminosity), zones.InnerZoneStart);
        zones.HabitableZoneOuter = 1.37f * Mathf.Sqrt(totalLuminosity);

        // Frost line
        zones.FrostLine = 4.85f * Mathf.Sqrt(totalLuminosity);

        // System limit
        zones.SystemLimit = CalculateCircumbinarySystemLimit(binaryPair, parentSystem);

        return zones;
    }

    /// <summary>
    /// Adjusts orbital zones to account for interference from a binary companion.
    /// </summary>
    /// <param name="zones">The zones to adjust</param>
    /// <param name="companionDistance">Distance to the companion star in AU</param>
    public static void AdjustZonesForBinaryInterference(OrbitalZones zones, float companionDistance)
    {
        // Truncate zones at about 1/3 of companion distance
        float maxLimit = companionDistance * 0.3f;
        zones.SystemLimit = Math.Min(zones.SystemLimit, maxLimit);

        // If companion is very close, it might truncate other zones too
        if (maxLimit < zones.FrostLine)
            zones.FrostLine = maxLimit;

        if (maxLimit < zones.HabitableZoneOuter)
            zones.HabitableZoneOuter = maxLimit;

        if (maxLimit < zones.HabitableZoneInner)
            zones.HabitableZoneInner = -1.0f; // Indicate zone is unavailable
            zones.HabitableZoneOuter = -1.0f; // Indicate zone is unavailable
    }

    /// <summary>
    /// Calculates the minimum distance from a star where planets can form.
    /// </summary>
    private static float CalculateEpistellarInnerLimit(StarStructure star)
    {
        // Calculate Roche limit
        float rocheLimit = Utils.StellarRocheLimit(star.Mass, 0, star.Radius);
        
        // Consider stellar wind and radiation pressure
        float radiationLimit = 0.01f * Mathf.Sqrt(star.Luminosity);
        
        return Math.Max(rocheLimit, radiationLimit);
    }

    /// <summary>
    /// Calculates the outer limit of a star's planetary system.
    /// </summary>
    private static float CalculateSystemLimit(StarStructure star)
    {
        // Base limit scales with stellar mass
        float baseLimit = 50f * Mathf.Pow(star.Mass, 0.333f);
        
        // Adjust for stellar temperature (hotter stars push limit outward)
        float tempFactor = star.Temperature / 5778f; // Relative to Sun's temperature
        
        return baseLimit * Mathf.Sqrt(tempFactor);
    }

    /// <summary>
    /// Calculates the outer limit of a binary system.
    /// </summary>
    private static float CalculateCircumbinarySystemLimit(StarSystem.BinaryPair binaryPair, StarSystem parentSystem)
    {
        float totalMass = GetTotalMass(binaryPair, parentSystem);
        float separation = binaryPair.SeparationDistance;
        
        // System limit is typically about 1/5 of the binary's Hill sphere
        return separation * 0.2f * Math.Min(100f, Mathf.Pow(totalMass, 0.333f)); //clamped to prevent unrealistic values for massive stars
    }

    /// <summary>
    /// Generic method to calculate total property (mass or luminosity) for a binary component
    /// </summary>
    private static float GetTotalProperty(StarSystem.BinaryPair pair, StarSystem system, 
        Func<StarStructure, float> propertyExtractor)
    {
        float total = 0f;
        
        // Handle primary component
        if (pair.PrimaryType == "Star")
        {
            var primaryStar = system.FindStarByID(pair.PrimaryID);
            if (primaryStar != null)
                total += propertyExtractor(primaryStar);
        }
        else if (pair.PrimaryType == "BinaryPair")
        {
            var primaryPair = system.FindBinaryPairByID(pair.PrimaryID);
            if (primaryPair != null)
                total += GetTotalProperty(primaryPair, system, propertyExtractor);
        }
            
        // Handle secondary component
        if (pair.SecondaryType == "Star")
        {
            var secondaryStar = system.FindStarByID(pair.SecondaryID);
            if (secondaryStar != null)
                total += propertyExtractor(secondaryStar);
        }
        else if (pair.SecondaryType == "BinaryPair")
        {
            var secondaryPair = system.FindBinaryPairByID(pair.SecondaryID);
            if (secondaryPair != null)
                total += GetTotalProperty(secondaryPair, system, propertyExtractor);
        }
            
        return total;
    }

    /// <summary>
    /// Calculates the total mass of a binary component.
    /// </summary>
    private static float GetTotalMass(StarSystem.BinaryPair pair, StarSystem system)
    {
        return GetTotalProperty(pair, system, star => star.Mass);
    }

    /// <summary>
    /// Calculates the total luminosity of a binary component.
    /// </summary>
    private static float GetTotalLuminosity(StarSystem.BinaryPair pair, StarSystem system)
    {
        return GetTotalProperty(pair, system, star => star.Luminosity);
    }
}