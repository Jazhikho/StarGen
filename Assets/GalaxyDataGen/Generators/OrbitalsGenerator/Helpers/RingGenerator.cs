using System;
using System.Collections.Generic;
using System.Linq;
using Libraries;

public class RingGenerator
{
    /// <summary>
    /// Generates a complete ring system for a planet if appropriate.
    /// </summary>
    /// <param name="planet">The planet to generate rings for</param>
    public void GenerateRings(Planet planet)
    {
        var planetTypeEnum = (PlanetLibrary.PlanetType)Enum.Parse(typeof(PlanetLibrary.PlanetType), planet.PlanetType);
        var ringData = RingLibrary.RingData.ContainsKey(planetTypeEnum) ? 
            RingLibrary.RingData[planetTypeEnum] : null;

        // Check if planet should have rings based on ring data probability
        float ringChance = ringData?.ComplexityProbability ?? RingLibrary.RingConstants.BASE_RING_CHANCE;
        bool shouldHaveRings = Roll.ConditionalProbability(ringChance, 0);

        // Update planet's HasRings property
        planet.HasRings = shouldHaveRings;

        if (!shouldHaveRings)
        {
            planet.Rings.HasRings = false;
            return;
        }

        // Create and populate ring system
        var ringSystem = planet.Rings; // Use the existing Rings property
        ringSystem.HasRings = true;
        ringSystem.Complexity = DetermineComplexity(ringData);
        ringSystem.TotalMass = CalculateRingMass(planet.Mass, ringData);
        ringSystem.Gaps.Clear(); // Clear existing gaps

        // Set radial extent based on Roche limit
        CalculateRingExtent(ringSystem, planet.Radius);
        
        // Generate gaps if complex
        if (ringSystem.Complexity == Planet.RingSystem.RingComplexity.Complex)
        {
            GenerateGaps(ringSystem);
        }
        
        // Set composition using AddComposition method
        Dictionary<string, float> composition = GenerateComposition(ringData);
        foreach (var material in composition)
        {
            ringSystem.AddComposition(material.Key, material.Value);
        }
        
        // Set opacity
        ringSystem.Opacity = CalculateOpacity(ringSystem, ringData);
    }
    
    /// <summary>
    /// Determines the complexity of the ring system based on planet type data.
    /// </summary>
    /// <param name="ringData">Ring type data for the planet type, or null</param>
    /// <returns>Complexity level of the ring system</returns>
    private Planet.RingSystem.RingComplexity DetermineComplexity(RingLibrary.RingTypeData ringData)
    {
        if (ringData == null) return Planet.RingSystem.RingComplexity.Simple;
        
        // Define probabilities as an array of options with weights
        var ringComplexities = new[] {
            Planet.RingSystem.RingComplexity.Simple,
            Planet.RingSystem.RingComplexity.Moderate,
            Planet.RingSystem.RingComplexity.Complex
        };
        
        var weights = new float[] { 0.4f, 0.4f, 0.2f }; // 40%, 40%, 20%
        
        return Roll.Choice(ringComplexities, weights);
    }


    /// <summary>
    /// Calculates the total mass of the ring system relative to the planet's mass.
    /// </summary>
    /// <param name="planetMass">Mass of the planet in Earth masses</param>
    /// <param name="ringData">Ring type data for the planet type, or null</param>
    /// <returns>Mass of the ring system in Earth masses</returns>
    private float CalculateRingMass(float planetMass, RingLibrary.RingTypeData ringData)
    {
        // Use default mass ratios if no specific data available
        if (ringData == null)
        {
            const float DEFAULT_MASS_RATIO_MIN = 0.000001f;
            const float DEFAULT_MASS_RATIO_MAX = 0.00001f;
            
            float midPoint = (DEFAULT_MASS_RATIO_MIN + DEFAULT_MASS_RATIO_MAX) / 2f;
            return planetMass * Roll.Vary(midPoint, 0.9f); // High variation due to wide range
        }
        
        // For known data, use the average of min/max with appropriate variation
        float averageMassRatio = (ringData.MassRatio.min + ringData.MassRatio.max) / 2f;
        float variation = (ringData.MassRatio.max - ringData.MassRatio.min) / averageMassRatio;
        
        return planetMass * Roll.Vary(averageMassRatio, variation / 2f);
    }

    /// <summary>
    /// Calculates the inner and outer radii of the ring system based on the Roche limit.
    /// </summary>
    /// <param name="ringSystem">Ring system to calculate extent for</param>
    /// <param name="planetRadius">Radius of the planet in Earth radii</param>
    private void CalculateRingExtent(Planet.RingSystem ringSystem, float planetRadius)
    {
        // Calculate Roche limit - minimum stable orbital distance for ring particles
        float rocheLimit = RingLibrary.RingConstants.ROCHE_MULTIPLIER * planetRadius;
        ringSystem.InnerRadius = rocheLimit;
        
        // Define multiplier ranges for each complexity level
        const float SIMPLE_MIN = 1.2f;
        const float SIMPLE_MAX = 1.5f;
        const float MODERATE_MIN = 1.5f;
        const float MODERATE_MAX = 2.0f;
        const float COMPLEX_MIN = 2.0f;
        const float COMPLEX_MAX = 3.0f;
        
        // Outer radius depends on ring complexity
        float multiplier;
        switch (ringSystem.Complexity)
        {
            case Planet.RingSystem.RingComplexity.Simple:
                multiplier = Roll.Vary((SIMPLE_MIN + SIMPLE_MAX) / 2f, 0.15f);
                multiplier = Math.Clamp(multiplier, SIMPLE_MIN, SIMPLE_MAX);
                break;
            case Planet.RingSystem.RingComplexity.Moderate:
                multiplier = Roll.Vary((MODERATE_MIN + MODERATE_MAX) / 2f, 0.15f);
                multiplier = Math.Clamp(multiplier, MODERATE_MIN, MODERATE_MAX);
                break;
            case Planet.RingSystem.RingComplexity.Complex:
                multiplier = Roll.Vary((COMPLEX_MIN + COMPLEX_MAX) / 2f, 0.15f);
                multiplier = Math.Clamp(multiplier, COMPLEX_MIN, COMPLEX_MAX);
                break;
            default:
                multiplier = SIMPLE_MAX;
                break;
        }
        
        ringSystem.OuterRadius = rocheLimit * multiplier;
    }

    /// <summary>
    /// Generates gaps in the ring system for complex ring structures.
    /// </summary>
    /// <param name="ringSystem">Ring system to generate gaps for</param>
    private void GenerateGaps(Planet.RingSystem ringSystem)
    {
        int minGaps = RingLibrary.RingConstants.MIN_GAPS;
        int maxGaps = RingLibrary.RingConstants.MAX_GAPS;
        int numberOfGaps = minGaps + (int)Roll.Vary((maxGaps - minGaps) / 2f, 0.5f);
        numberOfGaps = Math.Clamp(numberOfGaps, minGaps, maxGaps);
        
        float ringWidth = ringSystem.OuterRadius - ringSystem.InnerRadius;
        float minGapWidth = RingLibrary.RingConstants.MIN_GAP_WIDTH;
        float maxGapWidth = RingLibrary.RingConstants.MAX_GAP_WIDTH;
        
        // Generate each gap
        for (int i = 0; i < numberOfGaps; i++)
        {
            // Random position within the ring system (avoid very inner and outer edges)
            float gapStart = ringSystem.InnerRadius + (ringWidth * Roll.FindRange(0.1f, 0.9f));
            
            // Gap width varies but scales with the ring width
            float gapWidthFactor = Roll.Vary((minGapWidth + maxGapWidth) / 2f, 0.3f);
            gapWidthFactor = Math.Clamp(gapWidthFactor, minGapWidth, maxGapWidth);
            float gapWidth = ringWidth * gapWidthFactor;
            
            ringSystem.Gaps.Add(new Planet.RingSystem.RingGap
            {
                InnerRadius = gapStart,
                OuterRadius = gapStart + gapWidth,
                Name = $"Gap {i + 1}"
            });
        }

        // Sort gaps by radius for easier processing
        ringSystem.Gaps.Sort((a, b) => a.InnerRadius.CompareTo(b.InnerRadius));
    }

    /// <summary>
    /// Generates the material composition of the ring system.
    /// </summary>
    /// <param name="ringData">Ring type data for the planet type, or null</param>
    /// <returns>Dictionary of materials and their relative abundances</returns>
    private Dictionary<string, float> GenerateComposition(RingLibrary.RingTypeData ringData)
    {
        var composition = new Dictionary<string, float>();
        
        if (ringData == null)
        {
            // Default composition if no specific data
            composition["Silicates"] = Roll.Vary(0.5f, 0.2f); // 50% ± 20%
            composition["Water Ice"] = Roll.Vary(0.3f, 0.2f); // 30% ± 20%
            composition["Metals"] = Roll.Vary(0.15f, 0.2f);   // 15% ± 20%
            
            // Normalize to ensure percentages sum to 1.0
            float total = composition.Values.Sum();
            foreach (var material in composition.Keys.ToList())
            {
                composition[material] /= total;
            }
            
            return composition;
        }
        
        // Generate composition based on planet type's ring data
        float totalComposition = 0f;
        foreach (var material in ringData.CompositionRanges)
        {
            float average = (material.Value.min + material.Value.max) / 2f;
            float range = (material.Value.max - material.Value.min) / 2f;
            composition[material.Key] = Roll.Vary(average, range / average);
            totalComposition += composition[material.Key];
        }
        
        // Normalize to ensure percentages sum to 1.0
        if (totalComposition > 0)
        {
            foreach (var material in composition.Keys.ToList())
            {
                composition[material] /= totalComposition;
            }
        }
        
        return composition;
    }

    /// <summary>
    /// Calculates the opacity of the ring system.
    /// </summary>
    /// <param name="ringSystem">The ring system</param>
    /// <param name="ringData">Ring type data for the planet type, or null</param>
    /// <returns>Opacity value between 0 and 1</returns>
    private float CalculateOpacity(Planet.RingSystem ringSystem, RingLibrary.RingTypeData ringData)
    {
        if (ringData == null)
        {
            return Roll.FindRange(0.2f, 0.6f);
        }
        
        return Roll.FindRange(ringData.OpacityRange.min, ringData.OpacityRange.max);
    }
}