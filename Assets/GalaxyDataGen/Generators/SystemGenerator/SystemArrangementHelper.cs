using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using Libraries;

/// <summary>
/// Provides methods for organizing stars into hierarchical binary systems using ID-based references.
/// </summary>
public static class SystemArrangementHelper
{
    /// <summary>
    /// Organizes a list of stars into a hierarchical binary structure using ID-based references.
    /// </summary>
    /// <param name="stars">List of stars to organize</param>
    /// <param name="system">The star system to add binary pairs to</param>
    /// <returns>ID of the root binary pair</returns>
    public static string OrganizeStarHierarchy(List<StarStructure> stars, StarSystem system)
    {
        if (stars.Count == 0) return null;
        
        // Single star systems don't need a binary pair
        if (stars.Count == 1) return null;
        
        // Multiple stars need binary organization
        stars = stars.OrderByDescending(s => s.Mass).ToList();
        return BuildIDHierarchy(new List<StarStructure>(stars), system);
    }

    /// <summary>
    /// Recursively builds a binary hierarchy from a list of stars using ID-based references.
    /// </summary>
    private static string BuildIDHierarchy(List<StarStructure> stars, StarSystem system)
    {
        if (stars.Count == 0) return null;
        
        stars = stars.OrderByDescending(s => s.Mass).ToList();
        
        if (stars.Count == 1)
        {
            // Single star case - no binary pair needed
            return null;
        }

        var pair = new StarSystem.BinaryPair();
        system.AddBinaryPair(pair);
        
        if (stars.Count == 2)
        {
            // Simple binary case
            pair.SetPrimary(stars[0].ID, "Star");
            pair.SetSecondary(stars[1].ID, "Star");
            pair.SeparationDistance = CalculateSeparation(stars[0], stars[1]);
            pair.OrbitalPeriod = CalculateOrbitalPeriod(stars[0].Mass, stars[1].Mass, pair.SeparationDistance);
            
            // Calculate orbit radii based on mass ratio and separation
            float totalMass = stars[0].Mass + stars[1].Mass;
            float primaryRatio = stars[0].Mass / totalMass;
            
            pair.PrimaryOrbitRadius = pair.SeparationDistance * (1.0f - primaryRatio);
            pair.SecondaryOrbitRadius = pair.SeparationDistance * primaryRatio;
        }
        else
        {
            // Multi-star case - recursive organization
            var primaryStar = stars[0];  // Most massive star
            var remainingStars = new List<StarStructure>(stars.Skip(1));

            string secondaryGroupID = BuildIDHierarchy(remainingStars, system);
            
            // Set primary to the most massive star
            pair.SetPrimary(primaryStar.ID, "Star");
            
            // The secondary is either a star ID or binary pair ID
            var secondaryType = system.FindBinaryPairByID(secondaryGroupID) != null ? "BinaryPair" : "Star";
            pair.SetSecondary(secondaryGroupID, secondaryType);
            
            // Calculate separation based on the components
            object primary = primaryStar;
            object secondary = secondaryType == "Star" 
                ? system.FindStarByID(secondaryGroupID) 
                : (object)system.FindBinaryPairByID(secondaryGroupID);
            
            pair.SeparationDistance = CalculateSeparation(primary, secondary, system);
            
            // Calculate orbital period
            float primaryMass = GetTotalMass(primary, system);
            float secondaryMass = GetTotalMass(secondary, system);
            pair.OrbitalPeriod = CalculateOrbitalPeriod(primaryMass, secondaryMass, pair.SeparationDistance);
            
            // Calculate orbit radii
            float totalMass = primaryMass + secondaryMass;
            float primaryRatio = primaryMass / totalMass;
            
            pair.PrimaryOrbitRadius = pair.SeparationDistance * (1.0f - primaryRatio);
            pair.SecondaryOrbitRadius = pair.SeparationDistance * primaryRatio;
        }

        return pair.ID;
    }

    /// <summary>
    /// Calculates a realistic separation between binary components.
    /// </summary>
    private static float CalculateSeparation(object primary, object secondary, StarSystem system = null)
    {
        float primaryMass = GetTotalMass(primary, system);
        float secondaryMass = GetTotalMass(secondary, system);
        float totalMass = primaryMass + secondaryMass;
        
        // Mass ratio affects the likely separation
        float massRatio = (primaryMass > 0 || secondaryMass > 0) 
            ? Mathf.Min(primaryMass, secondaryMass) / Mathf.Max(primaryMass, secondaryMass)
            : 1f;
        
        float separationType = Roll.Distribution();
        float minSeparation, maxSeparation;
        
        // More massive systems tend to have wider separations
        float massFactor = Mathf.Pow(totalMass, 1f/3f); // Cube root of total mass
        
        if (separationType < 2000) // 20% chance for close binaries
        {
            minSeparation = Mathf.Max(0.1f, GetMinimumSeparation(primary, secondary, system));
            maxSeparation = 1f * massFactor;
        }
        else if (separationType < 6000) // 40% chance for medium separation
        {
            minSeparation = 1f * massFactor;
            maxSeparation = 50f * massFactor;
        }
        else if (separationType < 9000) // 30% chance for wide
        {
            minSeparation = 50f * massFactor;
            maxSeparation = 1000f * massFactor;
        }
        else // 10% chance for very wide
        {
            minSeparation = 1000f * massFactor;
            maxSeparation = 20000f * massFactor;
        }

        // Adjust separation range based on mass ratio
        if (massRatio < 0.2f)
        {
            minSeparation *= 2f;
            maxSeparation *= 2f;
        }
        
        return Roll.FindRange(minSeparation, maxSeparation);
    }

    /// <summary>
    /// Overload for direct star-to-star calculation
    /// </summary>
    private static float CalculateSeparation(StarStructure primary, StarStructure secondary)
    {
        return CalculateSeparation(primary, secondary, null);
    }

    /// <summary>
    /// Calculates the minimum safe separation based on stellar radii.
    /// </summary>
    private static float GetMinimumSeparation(object primary, object secondary, StarSystem system)
    {
        float primaryRadius = GetTotalRadius(primary, system);
        float secondaryRadius = GetTotalRadius(secondary, system);
        
        // Minimum separation is approximately 2.5 times the sum of the radii
        return (primaryRadius + secondaryRadius) * 2.5f / Utils.AU_TO_SOLAR_RADIUS;
    }

    /// <summary>
    /// Gets the total radius of a component, accounting for nested binary pairs.
    /// </summary>
    private static float GetTotalRadius(object component, StarSystem system)
    {
        return GetComponentProperty(component, system, 
            star => star.Radius,
            (primary, secondary) => Mathf.Max(primary, secondary));
    }

    /// <summary>
    /// Gets the total mass of a component or ID.
    /// </summary>
    private static float GetTotalMass(object component, StarSystem system)
    {
        return GetComponentProperty(component, system, 
            star => star.Mass,
            (primary, secondary) => primary + secondary);
    }
    
    /// <summary>
    /// Generic helper to get and combine properties from binary hierarchy components
    /// </summary>
    /// <param name="component">The component (star, binary pair, or ID)</param>
    /// <param name="system">The containing star system</param>
    /// <param name="getStarProperty">Function to get property from a star</param>
    /// <param name="combineValues">Function to combine two property values (sum, max, etc)</param>
    /// <returns>The calculated property value</returns>
    private static float GetComponentProperty(
        object component, 
        StarSystem system,
        Func<StarStructure, float> getStarProperty,
        Func<float, float, float> combineValues)
    {
        switch (component)
        {
            case StarStructure RootStar:
                return getStarProperty(RootStar);
                
            case StarSystem.BinaryPair pair:
                float primaryValue = 0f, secondaryValue = 0f;
                
                // Get primary property
                if (pair.PrimaryType == "Star")
                {
                    var primaryStar = system?.FindStarByID(pair.PrimaryID);
                    if (primaryStar != null) primaryValue = getStarProperty(primaryStar);
                }
                else if (pair.PrimaryType == "BinaryPair")
                {
                    var nestedPair = system?.FindBinaryPairByID(pair.PrimaryID);
                    if (nestedPair != null) primaryValue = GetComponentProperty(nestedPair, system, getStarProperty, combineValues);
                }
                
                // Get secondary property
                if (pair.SecondaryType == "Star")
                {
                    var secondaryStar = system?.FindStarByID(pair.SecondaryID);
                    if (secondaryStar != null) secondaryValue = getStarProperty(secondaryStar);
                }
                else if (pair.SecondaryType == "BinaryPair")
                {
                    var nestedPair = system?.FindBinaryPairByID(pair.SecondaryID);
                    if (nestedPair != null) secondaryValue = GetComponentProperty(nestedPair, system, getStarProperty, combineValues);
                }
                
                // Combine the values using the provided function
                return combineValues(primaryValue, secondaryValue);
                
            case string id when system != null:
                // Try to find component by ID
                var star = system.FindStarByID(id);
                if (star != null) return getStarProperty(star);
                
                var binPair = system.FindBinaryPairByID(id);
                if (binPair != null) return GetComponentProperty(binPair, system, getStarProperty, combineValues);
                
                return 0f;
                
            default:
                return 0f;
        }
    }

    /// <summary>
    /// Calculates orbital period using Kepler's Third Law.
    /// </summary>
    private static float CalculateOrbitalPeriod(float primaryMass, float secondaryMass, float separation)
    {
        float totalMass = primaryMass + secondaryMass;
        // Kepler's Third Law: P^2 = a^3/(M1 + M2)
        float orbitalPeriod = Mathf.Sqrt(Mathf.Pow(separation, 3) / totalMass);
        return orbitalPeriod;
    }
}