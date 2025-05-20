using System;
using System.Collections.Generic;
using Libraries;

/// <summary>
/// Helper class for generating and calculating resource distributions on celestial bodies.
/// Handles natural resource types, abundances, and economic values.
/// </summary>
namespace PlanetHelpers
{
public class PlanetResourceHelper
{
	/// <summary>
	/// Generates resource distributions for a planet based on its type and geological properties.
	/// </summary>
	/// <param name="planetType">Type classification of the planet</param>
	/// <param name="mass">Planet mass in Earth masses</param>
	/// <param name="geology">Geological data including tectonic and volcanic activity</param>
	/// <returns>Dictionary mapping resource types to their abundance values (0-1)</returns>
	public Dictionary<string, float> GenerateResources(
    PlanetLibrary.PlanetType planetType,
    float mass,
    PlanetGeologyHelper.GeologyData geology)
	{
		var planetData = PlanetLibrary.PlanetData[planetType];
		var resources = new Dictionary<string, float>();
		
		// Check each possible resource type for this planet type
		foreach (PlanetLibrary.ResourceType type in Enum.GetValues(typeof(PlanetLibrary.ResourceType)))
		{
			// If the planet type can have this resource
			if (planetData.ResourceProbabilities.TryGetValue(type, out float probability))
			{
				// Use ConditionalProbability for proper randomization
				if (Roll.ConditionalProbability(probability, 0))
				{
					// Generate amount based on resource type, planet type, and geological factors
					float amount = GenerateResourceAmount(type, planetType, mass, geology);
					resources[type.ToString()] = amount;
				}
			}
		}

		return resources;
	}

	/// <summary>
	/// Calculates the overall resource value rating of a planet based on its resources.
	/// Different resources have different economic values.
	/// </summary>
	/// <param name="resources">Dictionary of resources and their abundance values</param>
	/// <returns>Overall resource value rating</returns>
	public float CalculateResourceValue(Dictionary<string, float> resources)
	{
		float totalValue = 0f;
		
		// Economic value multipliers for different resource types
		const float WATER_VALUE = 1f;           // Essential but common
		const float METALS_VALUE = 2f;          // Basic industrial materials
		const float RARE_METALS_VALUE = 5f;     // High-value industrial materials
		const float RADIOACTIVES_VALUE = 4f;    // Energy generation
		const float ORGANICS_VALUE = 3f;        // Biological materials
		const float GASES_VALUE = 1.5f;         // Atmospheric resources
		const float CRYSTALS_VALUE = 3f;        // Specialized materials
		const float EXOTIC_MATTER_VALUE = 10f;  // Extremely valuable and rare
		
		var valueMultipliers = new Dictionary<PlanetLibrary.ResourceType, float> {
			{ PlanetLibrary.ResourceType.Water, WATER_VALUE },
			{ PlanetLibrary.ResourceType.Metals, METALS_VALUE },
			{ PlanetLibrary.ResourceType.RareMetals, RARE_METALS_VALUE },
			{ PlanetLibrary.ResourceType.Radioactives, RADIOACTIVES_VALUE },
			{ PlanetLibrary.ResourceType.Organics, ORGANICS_VALUE },
			{ PlanetLibrary.ResourceType.Gases, GASES_VALUE },
			{ PlanetLibrary.ResourceType.Crystals, CRYSTALS_VALUE },
			{ PlanetLibrary.ResourceType.ExoticMatter, EXOTIC_MATTER_VALUE }
		};
		
		// Sum the weighted values of all resources
		foreach (var resource in resources)
		{
			if (Enum.TryParse<PlanetLibrary.ResourceType>(resource.Key, out PlanetLibrary.ResourceType type))
			{
				if (valueMultipliers.TryGetValue(type, out float multiplier))
				{
					totalValue += resource.Value * multiplier;
				}
			}
		}
		
		return totalValue;
	}

	/// <summary>
	/// Generates the abundance amount for a specific resource based on planetary characteristics.
	/// Different resource types are affected by different planetary properties.
	/// </summary>
	/// <param name="type">Type of resource</param>
	/// <param name="planetType">Type classification of the planet</param>
	/// <param name="mass">Planet mass in Earth masses</param>
	/// <param name="geology">Geological data including tectonic and volcanic activity</param>
	/// <returns>Resource abundance value (0-1)</returns>
	private float GenerateResourceAmount(
    PlanetLibrary.ResourceType type,
    PlanetLibrary.PlanetType planetType,
    float mass,
    PlanetGeologyHelper.GeologyData geology)
	{
		// Base amount constants
		const float BASE_AMOUNT_CENTER = 0.5f;
		const float BASE_VARIATION = 0.4f;
		
		// Start with a varied base amount
		float baseAmount = Roll.Vary(BASE_AMOUNT_CENTER, BASE_VARIATION);
		
		// Get planet data
		var planetData = PlanetLibrary.PlanetData[planetType];
		
		// Adjust based on resource type and planetary characteristics
		switch (type)
		{
			case PlanetLibrary.ResourceType.Water:
				// Water-rich planets have more water resources
				// Check the water content range
				float waterPotential = (planetData.WaterContentRange.min + planetData.WaterContentRange.max) / 2f;
				if (waterPotential > 0.5f)
					baseAmount *= 2f;
				break;
				
			case PlanetLibrary.ResourceType.Metals:
				// Larger planets have more metals due to differentiation processes
				baseAmount *= Math.Min(2f, mass * 0.5f);
				break;
				
			case PlanetLibrary.ResourceType.RareMetals:
				// Volcanic activity brings rare metals to the surface
				baseAmount *= Math.Max(0.1f, geology.VolcanicActivity * 1.5f);
				break;
				
			case PlanetLibrary.ResourceType.Radioactives:
				// Radioactives tend to be concentrated by tectonic processes
				baseAmount *= Math.Max(0.1f, geology.TectonicActivity * 1.5f);
				break;
				
			case PlanetLibrary.ResourceType.Organics:
				// Earth-like and ocean planets have more organic resources
				if (planetType == PlanetLibrary.PlanetType.Gaian ||
					planetType == PlanetLibrary.PlanetType.Oceanian ||
					planetType == PlanetLibrary.PlanetType.Tethysian)
					baseAmount *= 3f;
				break;
				
			case PlanetLibrary.ResourceType.Gases:
				// Gas giants have more gaseous resources
				if (planetType == PlanetLibrary.PlanetType.Hyperion ||
					planetType == PlanetLibrary.PlanetType.Atlantean ||
					planetType == PlanetLibrary.PlanetType.Criusian ||
					planetType == PlanetLibrary.PlanetType.Theian ||
					planetType == PlanetLibrary.PlanetType.Iapetian ||
					planetType == PlanetLibrary.PlanetType.Helian)
					baseAmount *= 5f;
				break;
				
			case PlanetLibrary.ResourceType.Crystals:
				// Volcanic activity creates crystal formations
				baseAmount *= Math.Max(0.1f, geology.VolcanicActivity * 2f);
				break;
				
			case PlanetLibrary.ResourceType.ExoticMatter:
				// Exotic matter is extremely rare
				baseAmount *= 0.1f;
				
				// Small chance of higher amounts on extreme worlds
				if (planetType == PlanetLibrary.PlanetType.Vulcanian || 
					planetType == PlanetLibrary.PlanetType.Cronusian)
				{
					if (Roll.ConditionalProbability(0.1f, 0))
						baseAmount *= 5f;
				}
				break;
		}
		
		// Add some final variation
		baseAmount = Roll.Vary(baseAmount, 0.1f);
		
		// Ensure value is within valid range
		return Math.Clamp(baseAmount, 0f, 1f);
	}
}
}
