using UnityEngine;
using System.Collections.Generic;
using Libraries;

/// <summary>
/// Generator class for creating asteroid belts with realistic properties.
/// </summary>
public class AsteroidBeltGenerator
{
    /// <summary>
    /// Generates a complete asteroid belt with physical properties and notable objects.
    /// </summary>
    /// <param name="id">Unique identifier for the asteroid belt</param>
    /// <param name="innerRadius">Inner radius of the belt in AU</param>
    /// <param name="outerRadius">Outer radius of the belt in AU</param>
    /// <param name="zone">Orbital zone classification</param>
    /// <returns>A fully generated asteroid belt</returns>
    public AsteroidBelt GenerateAsteroidBelt(string id, float innerRadius, float outerRadius, ZoneType zone)
    {
        AsteroidBelt belt = new AsteroidBelt();
        belt.ID = id;
        belt.InnerRadius = innerRadius;
        belt.OuterRadius = outerRadius;
        belt.Thickness = Roll.FindRange(AsteroidLibrary.BeltThicknessRange.min, AsteroidLibrary.BeltThicknessRange.max);
        belt.AverageDensity = Roll.FindRange(AsteroidLibrary.BeltDensityRange.min, AsteroidLibrary.BeltDensityRange.max);
        belt.AverageObjectSize = Roll.FindRange(AsteroidLibrary.AverageObjectSizeRange.min, AsteroidLibrary.AverageObjectSizeRange.max);

        // Calculate volume and total mass
        float volume = AsteroidBeltCalculations.CalculateBeltVolume(innerRadius, outerRadius, belt.Thickness);
        belt.TotalMass = AsteroidBeltCalculations.CalculateBeltMass(belt.AverageDensity, volume, belt.AverageObjectSize);

        // Determine overall belt composition based on zone
        Dictionary<AsteroidLibrary.AsteroidCompositionType, float> composition = DetermineBeltComposition(zone);
        
        // Convert and add compositions to belt
        foreach (var comp in composition)
        {
            belt.AddMaterialComposition(comp.Key.ToString(), comp.Value);
        }

        // Calculate resource concentration
        Dictionary<PlanetLibrary.ResourceType, float> resources = CalculateResourceConcentration(composition);
        
        // Convert and add resources to belt
        foreach (var resource in resources)
        {
            belt.AddResourceConcentration(resource.Key.ToString(), resource.Value);
        }

        // Determine number of notable objects
        int notableObjectCount = DetermineNotableObjectCount(belt.TotalMass);
        belt.HasDwarfPlanets = notableObjectCount > 0;
        
        // Generate notable objects
        GenerateNotableObjects(belt, notableObjectCount, composition);

        // Economic and hazard assessment
        belt.EconomicValue = CalculateEconomicValue(resources, belt.TotalMass);
        belt.NavigationHazard = belt.AverageDensity > 1000f;

        return belt;
    }

    /// <summary>
    /// Determines the composition of asteroids in the belt based on their orbital zone.
    /// </summary>
    /// <param name="zone">Orbital zone classification</param>
    /// <returns>Dictionary mapping composition types to their relative abundances</returns>
    private Dictionary<AsteroidLibrary.AsteroidCompositionType, float> DetermineBeltComposition(ZoneType zone)
    {
        Dictionary<AsteroidLibrary.AsteroidCompositionType, float> composition = 
            new Dictionary<AsteroidLibrary.AsteroidCompositionType, float>();

        switch(zone)
        {
            case ZoneType.Epistellar:
            case ZoneType.Inner:
                composition.Add(AsteroidLibrary.AsteroidCompositionType.Metallic, 0.4f);  // Metal-rich inner zone
                composition.Add(AsteroidLibrary.AsteroidCompositionType.Stony, 0.3f);     // Rocky material
                composition.Add(AsteroidLibrary.AsteroidCompositionType.Basaltic, 0.2f);  // Volcanic materials
                composition.Add(AsteroidLibrary.AsteroidCompositionType.Carbonaceous, 0.1f); // Some carbon compounds
                break;
            case ZoneType.Habitable:
                composition.Add(AsteroidLibrary.AsteroidCompositionType.Carbonaceous, 0.4f); // More carbon materials
                composition.Add(AsteroidLibrary.AsteroidCompositionType.Stony, 0.3f);        // Still rocky
                composition.Add(AsteroidLibrary.AsteroidCompositionType.Metallic, 0.2f);     // Less metals
                composition.Add(AsteroidLibrary.AsteroidCompositionType.Mixed, 0.1f);        // Some mixed composition
                break;
            case ZoneType.Outer:
                composition.Add(AsteroidLibrary.AsteroidCompositionType.Carbonaceous, 0.5f); // Mostly carbon-rich
                composition.Add(AsteroidLibrary.AsteroidCompositionType.Icy, 0.3f);          // More volatiles
                composition.Add(AsteroidLibrary.AsteroidCompositionType.Mixed, 0.2f);        // Mixed materials
                break;
            case ZoneType.FarOuter:
                composition.Add(AsteroidLibrary.AsteroidCompositionType.Icy, 0.6f);         // Mostly ices
                composition.Add(AsteroidLibrary.AsteroidCompositionType.Carbonaceous, 0.3f); // Some carbon
                composition.Add(AsteroidLibrary.AsteroidCompositionType.Mixed, 0.1f);        // Some mixed
                break;
        }

        return composition;
    }

    /// <summary>
    /// Calculates resource concentrations based on the belt's material composition.
    /// </summary>
    /// <param name="composition">Dictionary of asteroid composition types and their abundances</param>
    /// <returns>Dictionary mapping resource types to their concentrations</returns>
    private Dictionary<PlanetLibrary.ResourceType, float> CalculateResourceConcentration(
        Dictionary<AsteroidLibrary.AsteroidCompositionType, float> composition)
    {
        Dictionary<PlanetLibrary.ResourceType, float> resources = 
            new Dictionary<PlanetLibrary.ResourceType, float>();
        
        // Calculate resource concentrations based on composition types
        foreach (var type in composition.Keys)
        {
            var typeData = AsteroidLibrary.AsteroidData[type];
            float typeFraction = composition[type];
            
            // Add resources from this composition type, weighted by abundance
            foreach (var resource in typeData.ResourceProbabilities.Keys)
            {
                float concentration = typeData.ResourceProbabilities[resource] * typeFraction;
                if (resources.ContainsKey(resource))
                    resources[resource] += concentration;
                else
                    resources[resource] = concentration;
            }
        }

        return resources;
    }

    /// <summary>
    /// Determines how many notable objects (larger asteroids, dwarf planets) exist in the belt.
    /// </summary>
    /// <param name="totalMass">Total mass of the belt in Earth masses</param>
    /// <returns>Number of notable objects to generate</returns>
    private int DetermineNotableObjectCount(float totalMass)
    {
        // More mass means more chance of notable objects
        float baseCount = totalMass * 5f; // 5 notable objects per Earth mass
        return (int)Mathf.Clamp(baseCount * Random.Range(0.5f, 1.5f), 0, Roll.Dice());
    }

    /// <summary>
    /// Generates notable objects within the asteroid belt, such as large asteroids or dwarf planets.
    /// </summary>
    /// <param name="belt">The asteroid belt to populate</param>
    /// <param name="count">Number of notable objects to generate</param>
    /// <param name="beltComposition">The composition of the belt</param>
    private void GenerateNotableObjects(
        AsteroidBelt belt, 
        int count, 
        Dictionary<AsteroidLibrary.AsteroidCompositionType, float> beltComposition)
    {
        for (int i = 0; i < count; i++)
        {
            var obj = new AsteroidBelt.AsteroidBeltObject();
            
            // Basic properties
            obj.ID = $"{belt.ID}-NO{i+1}";
            obj.IsObjectOfInterest = true;
            
            // Size and position
            float sizeFactor = Random.Range(1.5f, 15f); // Much larger than average
            obj.Radius = belt.AverageObjectSize * sizeFactor;
            
            // Orbital parameters (angle in radians)
            obj.OrbitalDistance = Roll.FindRange(belt.InnerRadius, belt.OuterRadius);
            obj.OrbitalAngle = Random.Range(0f, Mathf.PI * 2f); // 0 to 2π
            
            // Composition - weighted random based on belt composition
            foreach (var compType in beltComposition.Keys)
            {
                if (Roll.Dice(1, 100) <= beltComposition[compType] * 100)
                {
                    string compName = compType.ToString();
                    obj.AddComposition(compName, Roll.FindRange(0.3f, 0.9f));
                }
            }

            belt.AddNotableObject(obj);
        }

        // Sort by size, largest first - this would need to be done differently
        // Since we can't sort directly, we'll need to rebuild the list
        belt.NotableObjects.Sort((a, b) => b.Radius.CompareTo(a.Radius));
    }

    /// <summary>
    /// Calculates the economic value of the asteroid belt based on resources and mass.
    /// </summary>
    /// <param name="resources">Dictionary of resources and their concentrations</param>
    /// <param name="totalMass">Total mass in Earth masses</param>
    /// <returns>Economic value rating</returns>
    private float CalculateEconomicValue(Dictionary<PlanetLibrary.ResourceType, float> resources, float totalMass)
    {
        float value = 0f;
        
        // Resource value multipliers
        Dictionary<PlanetLibrary.ResourceType, float> valueMultipliers = 
            new Dictionary<PlanetLibrary.ResourceType, float>
        {
            { PlanetLibrary.ResourceType.Water, 1f },
            { PlanetLibrary.ResourceType.Metals, 2f },
            { PlanetLibrary.ResourceType.RareMetals, 5f },
            { PlanetLibrary.ResourceType.Radioactives, 4f },
            { PlanetLibrary.ResourceType.Organics, 1.5f },
            { PlanetLibrary.ResourceType.Gases, 0.5f },
            { PlanetLibrary.ResourceType.Crystals, 3f },
            { PlanetLibrary.ResourceType.ExoticMatter, 10f }
        };

        // Calculate weighted value of all resources
        foreach (var resource in resources.Keys)
        {
            float multiplier = valueMultipliers.ContainsKey(resource) ? valueMultipliers[resource] : 1f;
            value += resources[resource] * multiplier;
        }

        // Scale by total mass - more mass means more total resources
        value *= totalMass;

        return value;
    }
}