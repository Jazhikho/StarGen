using System;
using System.Collections.Generic;
using System.Linq;
using Libraries;

/// <summary>
/// Helper class for generating and calculating atmospheric properties of planets.
/// Handles atmospheric composition, pressure, and retention based on planetary characteristics.
/// </summary>
namespace PlanetHelpers
{
public class PlanetAtmosphereHelper
{
	/// <summary>
	/// Generates atmospheric properties for a planet based on its physical characteristics.
	/// </summary>
	/// <param name="planetType">Type classification of the planet</param>
	/// <param name="mass">Planet mass in Earth masses</param>
	/// <param name="surfaceGravity">Surface gravity in Earth g's</param>
	/// <param name="temperature">Surface temperature in Kelvin</param>
	/// <returns>Tuple containing atmosphere presence, pressure, and composition</returns>
	public (bool hasAtmosphere, float pressure, Dictionary<string, float> composition) GenerateAtmosphere(
		PlanetLibrary.PlanetType planetType,
		float mass,
		float surfaceGravity,
		float temperature)
	{
		// Determine if planet has substantial atmosphere
		bool hasAtmosphere = DetermineHasAtmosphere(planetType, mass, surfaceGravity, temperature);
		
		if (!hasAtmosphere)
		{
			return (false, 0f, new Dictionary<string, float>());
		}

		// Calculate atmospheric pressure (in Earth atmospheres)
		float pressure = CalculateAtmosphericPressure(planetType, mass, surfaceGravity, temperature);
		
		// Generate composition
		var composition = GenerateComposition(surfaceGravity, planetType, temperature);

		return (true, pressure, composition);
	}

	/// <summary>
	/// Determines whether a planet can maintain a substantial atmosphere based on its properties.
	/// </summary>
	/// <param name="planetType">Type classification of the planet</param>
	/// <param name="mass">Planet mass in Earth masses</param>
	/// <param name="surfaceGravity">Surface gravity in Earth g's</param>
	/// <param name="temperature">Surface temperature in Kelvin</param>
	/// <returns>True if the planet can maintain an atmosphere</returns>
	private bool DetermineHasAtmosphere(PlanetLibrary.PlanetType planetType, float mass, float surfaceGravity, float temperature)
	{
		float baseChance = PlanetLibrary.PlanetData[planetType].AtmosphereProbability;

		// Adjust for mass and gravity (heavier planets retain atmosphere better)
		baseChance *= Math.Min(1.0f, mass * 0.5f);
		baseChance *= Math.Min(1.0f, surfaceGravity * 0.5f);

		// Very hot temperatures make it harder to retain atmosphere
		if (temperature > 500)
		{
			baseChance *= 0.5f;
		}

		// Check for basic atmospheric retention of common gases
		if (mass > 0.1f && surfaceGravity > 0.2f)
		{
			bool canRetainNitrogen = Astro.CanRetainGas(surfaceGravity, temperature, 28f); // Nitrogen
			bool canRetainOxygen = Astro.CanRetainGas(surfaceGravity, temperature, 32f);   // Oxygen
			
			if (!canRetainNitrogen && !canRetainOxygen)
			{
				baseChance *= 0.1f; // Significant penalty if can't retain common gases
			}
		}

		return Roll.FindRange(0f, 1f) <= baseChance;
	}

	/// <summary>
	/// Calculates the atmospheric pressure based on planetary characteristics.
	/// </summary>
	/// <param name="planetType">Type classification of the planet</param>
	/// <param name="mass">Planet mass in Earth masses</param>
	/// <param name="surfaceGravity">Surface gravity in Earth g's</param>
	/// <param name="temperature">Surface temperature in Kelvin</param>
	/// <returns>Atmospheric pressure in Earth atmospheres</returns>
	private float CalculateAtmosphericPressure(PlanetLibrary.PlanetType planetType, float mass, float surfaceGravity, float temperature)
	{
		// Use Astro calculations for base pressure calculation
		float volatileContent = PlanetLibrary.PlanetData[planetType].AtmospherePressureMod;
		return Astro.AtmosphericPressure(mass, surfaceGravity, temperature, volatileContent);
	}

	/// <summary>
	/// Generates the atmospheric composition based on planet type and temperature.
	/// </summary>
	/// <param name="planetType">Type classification of the planet</param>
	/// <param name="temperature">Surface temperature in Kelvin</param>
	/// <returns>Dictionary of atmospheric components and their percentages</returns>
	private Dictionary<string, float> GenerateComposition(float gravity, PlanetLibrary.PlanetType planetType, float temperature)
	{
		var planetData = PlanetLibrary.PlanetData[planetType];
		
		// Choose one of the possible chemistry types for this planet
		var chemType = planetData.ChemistryTypes[Roll.Dice(1, planetData.ChemistryTypes.Length) - 1];
		
		// Get template composition
		var template = PlanetLibrary.AtmosphereTemplates[chemType];
		
		// Create a new composition with variation
		var composition = new Dictionary<string, float>();
		float totalPercentage = 0f;
		
		// First pass: generate varied percentages
		foreach (var gas in template.Composition)
		{
			// Check if this gas can be retained at the current temperature
			float molecularWeight = GetMolecularWeight(gas.Key);
			if (Astro.CanRetainGas(gravity, temperature, molecularWeight))
			{
				float value = gas.Value * Roll.FindRange(0.8f, 1.2f);
				composition[gas.Key] = value;
				totalPercentage += value;
			}
		}

		// Second pass: normalize to ensure total is 100%
		if (totalPercentage > 0)
		{
			foreach (var gas in composition.Keys.ToList())
			{
				composition[gas] = composition[gas] / totalPercentage * 100f;
			}
		}

		return composition;
	}

	/// <summary>
	/// Returns the molecular weight of common atmospheric gases.
	/// </summary>
	/// <param name="gasName">Name of the gas</param>
	/// <returns>Molecular weight in atomic mass units</returns>
	private float GetMolecularWeight(string gasName)
	{
		return gasName switch
		{
			"Hydrogen" => 2f,
			"Helium" => 4f,
			"Methane" => 16f,
			"Ammonia" => 17f,
			"Water Vapor" => 18f,
			"Nitrogen" => 28f,
			"Carbon Dioxide" => 44f,
			"Oxygen" => 32f,
			"Argon" => 40f,
			"Sulfur Dioxide" => 64f,
			_ => 28f // Default to nitrogen weight if unknown
		};
	}
}
}
