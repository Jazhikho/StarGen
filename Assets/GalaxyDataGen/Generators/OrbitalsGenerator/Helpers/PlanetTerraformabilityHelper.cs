using UnityEngine;
using System;
using System.Collections.Generic;
using Libraries;

public class PlanetTerraformabilityHelper
{
	// Minimum requirements for terraforming
	private const float MIN_MASS = 0.1f;        // Minimum mass (in Earth masses) to hold atmosphere
	private const float MAX_MASS = 10.0f;       // Maximum mass for manageable gravity
	private const float MIN_GRAVITY = 0.4f;     // Minimum gravity (in g) for atmosphere retention
	private const float MAX_GRAVITY = 1.5f;     // Maximum gravity for human habitability
	private const float MAX_TEMPERATURE = 400f; // Maximum starting temperature (K) for terraforming
	private const float MIN_TEMPERATURE = 150f; // Minimum starting temperature (K) for terraforming

	/// <summary>
	/// Evaluates whether a celestial body can be terraformed and what challenges would be involved.
	/// </summary>
	/// <param name="planet">The orbital body to evaluate</param>
	/// <returns>A tuple containing: terraformable status, terraforming difficulty index (0-1), and list of challenges</returns>
	public (bool terraformable, float terraformingIndex, string[] challenges) EvaluateTerraformability(OrbitalBody planet)
	{
		List<string> challenges = new List<string>();
		float baseScore = 0f;

		// Check absolute disqualifiers
		if (!IsPlanetTypeTerraformable(planet.PlanetType))
		{
			return (false, 0f, new[] { "Planet type not suitable for terraforming" });
		}

		// Check mass and gravity constraints
		if (planet.Mass < MIN_MASS || planet.Mass > MAX_MASS)
		{
			challenges.Add("Mass outside acceptable range");
		}
		else
		{
			baseScore += CalculateMassScore(planet.Mass);
		}

		if (planet.SurfaceGravity < MIN_GRAVITY || planet.SurfaceGravity > MAX_GRAVITY)
		{
			challenges.Add("Surface gravity outside habitable range");
		}
		else
		{
			baseScore += CalculateGravityScore(planet.SurfaceGravity);
		}

		// Temperature evaluation
		if (planet.SurfaceTemperature > MAX_TEMPERATURE || planet.SurfaceTemperature < MIN_TEMPERATURE)
		{
			challenges.Add("Temperature extreme");
		}
		else
		{
			baseScore += CalculateTemperatureScore(planet.SurfaceTemperature);
		}

		// Magnetic field evaluation
		if (planet.MagneticFieldStrength < 0.2f)
		{
			challenges.Add("Weak magnetic field");
		}
		baseScore += planet.MagneticFieldStrength * 0.2f;

		// Rotation period evaluation
		if (planet.TidallyLocked)
		{
			challenges.Add("Tidally locked");
			baseScore *= 0.5f;
		}
		else if (planet.RotationPeriod > 100f) // Very slow rotation
		{
			challenges.Add("Very slow rotation");
			baseScore *= 0.8f;
		}

		// Resource availability for terraforming
		float resourceScore = EvaluateResources(planet.GetResources());
		baseScore += resourceScore;

		// Normalize the final score to 0-1 range
		float finalScore = Mathf.Clamp(baseScore / 5f, 0f, 1f);

		// Determine if terraformable
		bool isTerraformable = finalScore > 0.3f && challenges.Count < 3;

		return (isTerraformable, finalScore, challenges.ToArray());
	}

	/// <summary>
	/// Determines if a planet type is fundamentally suitable for terraforming.
	/// Only certain planet types have the basic composition needed.
	/// </summary>
	/// <param name="planetType">The planet type string identifier</param>
	/// <returns>True if the planet type can potentially be terraformed</returns>
	private bool IsPlanetTypeTerraformable(string planetType)
	{
		// Check terraformable planet types
		switch (planetType)
		{
			case "Gaian":      // Earth-like already
			case "Oceanian":   // Water world
			case "Tethysian":  // Mesic terrestrial
			case "Promethean": // Arid terrestrial
			case "Rhean":      // Super-Earth inside frost line
			case "Dionean":    // Icy Super-Earth
			case "Menoetian":  // Barren terrestrial
			case "Phoeboan":   // Ice dwarf
				return true;
			default:
				return false;
		}
	}

	/// <summary>
	/// Calculates a score based on the planet's mass suitability for terraforming.
	/// Planets too small can't hold atmospheres; planets too large have crushing gravity.
	/// </summary>
	/// <param name="mass">Planet mass in Earth masses</param>
	/// <returns>Mass suitability score (0-1)</returns>
	private float CalculateMassScore(float mass)
	{
		// Optimal mass range is 0.5 to 2.0 Earth masses
		if (mass >= 0.5f && mass <= 2.0f)
			return 1.0f;
		else if (mass < 0.5f)
			return mass / 0.5f;
		else
			return 2.0f / mass;
	}

	/// <summary>
	/// Calculates a score based on the planet's surface gravity suitability for human habitation.
	/// </summary>
	/// <param name="gravity">Surface gravity in Earth g's</param>
	/// <returns>Gravity suitability score (0-1)</returns>
	private float CalculateGravityScore(float gravity)
	{
		// Optimal gravity range is 0.8 to 1.2 g
		if (gravity >= 0.8f && gravity <= 1.2f)
			return 1.0f;
		else if (gravity < 0.8f)
			return gravity / 0.8f;
		else
			return 1.2f / gravity;
	}

	/// <summary>
	/// Calculates a score based on the planet's temperature suitability for terraforming.
	/// Extreme temperatures require more energy to mitigate.
	/// </summary>
	/// <param name="temperature">Surface temperature in Kelvin</param>
	/// <returns>Temperature suitability score (0-1)</returns>
	private float CalculateTemperatureScore(float temperature)
	{
		// Optimal temperature range is 250K to 350K
		const float optimalMin = 250f;
		const float optimalMax = 350f;

		if (temperature >= optimalMin && temperature <= optimalMax)
			return 1.0f;
		else if (temperature < optimalMin)
			return temperature / optimalMin;
		else
			return optimalMax / temperature;
	}

	/// <summary>
	/// Evaluates the availability of resources necessary for terraforming.
	/// Water, metals, and gases are particularly important.
	/// </summary>
	/// <param name="resources">Dictionary of resources and their abundance</param>
	/// <returns>Resource availability score (0-1)</returns>
	private float EvaluateResources(Dictionary<string, float> resources)
	{
		const float WATER_WEIGHT = 0.3f;
		const float METALS_WEIGHT = 0.2f;
		const float GASES_WEIGHT = 0.2f;
		
		float score = 0f;

		// Check for important terraforming resources
		if (resources.TryGetValue("Water", out float water))
			score += water * WATER_WEIGHT;  // Water is critical for many terraforming processes
		
		if (resources.TryGetValue("Metals", out float metals))
			score += metals * METALS_WEIGHT;  // Metals needed for infrastructure
		
		if (resources.TryGetValue("Gases", out float gases))
			score += gases * GASES_WEIGHT;   // Gases needed for atmospheric composition

		return score;
	}
}