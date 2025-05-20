using System;
using System.Linq;
using System.Collections.Generic;
using Libraries;

/// <summary>
/// Helper class for calculating habitability ratings of celestial bodies.
/// Evaluates multiple environmental factors to determine overall habitability for Earth-like life.
/// </summary>
namespace PlanetHelpers
{
public class PlanetHabitabilityHelper
{
	/// <summary>
	/// Calculates the habitability index of a planet based on its physical and environmental characteristics.
	/// Higher values indicate better suitability for Earth-like life.
	/// </summary>
	/// <param name="planetType">Type classification of the planet</param>
	/// <param name="temperature">Surface temperature in Kelvin</param>
	/// <param name="atmosphericPressure">Atmospheric pressure in Earth atmospheres</param>
	/// <param name="atmosphericComposition">Dictionary of atmospheric gases and their percentages</param>
	/// <param name="waterCoverage">Fraction of surface covered by liquid water (0-1)</param>
	/// <param name="magneticFieldStrength">Strength of planetary magnetic field (0-1)</param>
	/// <returns>Habitability index (0-1) where 1 is most habitable</returns>
	public float CalculateHabitabilityIndex(
		PlanetLibrary.PlanetType planetType,
		float temperature,
		float atmosphericPressure,
		Dictionary<string, float> atmosphericComposition,
		float waterCoverage,
		float magneticFieldStrength)
	{
		// Uninhabitable planet types - immediately return zero
		if (new[] {
			PlanetLibrary.PlanetType.Cronusian,   // Stripped gas giant core
			PlanetLibrary.PlanetType.Criusian,    // Hot mini-Neptune
			PlanetLibrary.PlanetType.Theian,      // Cold mini-Neptune
			PlanetLibrary.PlanetType.Iapetian,    // Ice giant
			PlanetLibrary.PlanetType.Helian,      // Helium planet
			PlanetLibrary.PlanetType.Hyperion,    // Hot gas giant
			PlanetLibrary.PlanetType.Atlantean,   // Cold gas giant
			PlanetLibrary.PlanetType.Asterian,    // Asteroid belt
			PlanetLibrary.PlanetType.Vulcanian    // Extreme volcanic world
		}.Contains(planetType))
		{
			return 0f;
		}

		// Calculate individual habitability factors
		float tempScore = CalculateTemperatureScore(temperature);
		float pressureScore = CalculatePressureScore(atmosphericPressure);
		float compositionScore = CalculateAtmosphereScore(atmosphericComposition);
		float waterScore = CalculateWaterScore(waterCoverage);
		float shieldingScore = CalculateShieldingScore(magneticFieldStrength, atmosphericPressure);

		// Define weights as constants
		const float TEMP_WEIGHT = 0.3f;
		const float PRESSURE_WEIGHT = 0.2f;
		const float COMPOSITION_WEIGHT = 0.2f;
		const float WATER_WEIGHT = 0.2f;
		const float SHIELDING_WEIGHT = 0.1f;

		// Calculate weighted average - temperature and water are most critical for Earth-like life
		float habitabilityIndex = (
			tempScore * TEMP_WEIGHT +
			pressureScore * PRESSURE_WEIGHT +
			compositionScore * COMPOSITION_WEIGHT +
			waterScore * WATER_WEIGHT +
			shieldingScore * SHIELDING_WEIGHT
		);

		return Math.Clamp(habitabilityIndex, 0f, 1f);
	}

	/// <summary>
	/// Calculates a score based on temperature suitability for Earth-like life.
	/// Ideal range is 273K to 313K (0°C to 40°C).
	/// </summary>
	/// <param name="temperature">Surface temperature in Kelvin</param>
	/// <returns>Temperature score (0-1)</returns>
	private float CalculateTemperatureScore(float temperature)
	{
		const float MIN_IDEAL_TEMP = 273f;  // 0°C
		const float MAX_IDEAL_TEMP = 313f;  // 40°C
		const float TEMP_FALLOFF_RATE = 50f;
		
		if (temperature < MIN_IDEAL_TEMP)
			return Math.Max(0f, 1f - (MIN_IDEAL_TEMP - temperature) / TEMP_FALLOFF_RATE);
		else if (temperature > MAX_IDEAL_TEMP)
			return Math.Max(0f, 1f - (temperature - MAX_IDEAL_TEMP) / TEMP_FALLOFF_RATE);
		else
			return 1f;
	}

	/// <summary>
	/// Calculates a score based on atmospheric pressure suitability for Earth-like life.
	/// Ideal range is 0.5 to 2.0 Earth atmospheres.
	/// </summary>
	/// <param name="pressure">Atmospheric pressure in Earth atmospheres</param>
	/// <returns>Pressure score (0-1)</returns>
	private float CalculatePressureScore(float pressure)
	{
		const float MIN_IDEAL_PRESSURE = 0.5f;
		const float MAX_IDEAL_PRESSURE = 2.0f;
		const float PRESSURE_FALLOFF_RATE = 3.0f;
		
		if (pressure < MIN_IDEAL_PRESSURE)
			return Math.Max(0f, pressure / MIN_IDEAL_PRESSURE);
		else if (pressure > MAX_IDEAL_PRESSURE)
			return Math.Max(0f, 1f - (pressure - MAX_IDEAL_PRESSURE) / PRESSURE_FALLOFF_RATE);
		else
			return 1f;
	}

	/// <summary>
	/// Calculates a score based on atmospheric composition suitability for Earth-like life.
	/// Evaluates presence of oxygen, carbon dioxide, and nitrogen.
	/// </summary>
	/// <param name="composition">Dictionary of atmospheric components and their percentages</param>
	/// <returns>Atmosphere composition score (0-1)</returns>
	private float CalculateAtmosphereScore(Dictionary<string, float> composition)
	{
		const float MIN_IDEAL_OXYGEN = 15f;
		const float MAX_IDEAL_OXYGEN = 30f;
		const float MAX_TOLERABLE_CO2 = 1f;
		const float MIN_NITROGEN = 70f;
		
		const float OXYGEN_IDEAL_SCORE = 0.6f;
		const float OXYGEN_PRESENT_SCORE = 0.3f;
		const float LOW_CO2_SCORE = 0.2f;
		const float NITROGEN_SCORE = 0.2f;
		
		float score = 0f;
		
		// Check for oxygen - critical for Earth-like life
		if (composition.TryGetValue("Oxygen", out float oxygen))
		{
			if (oxygen >= MIN_IDEAL_OXYGEN && oxygen <= MAX_IDEAL_OXYGEN)
				score += OXYGEN_IDEAL_SCORE;
			else if (oxygen > 0f)
				score += OXYGEN_PRESENT_SCORE;
		}
		
		// Check for carbon dioxide - toxic in high concentrations
		if (composition.TryGetValue("Carbon Dioxide", out float co2))
		{
			if (co2 < MAX_TOLERABLE_CO2)
				score += LOW_CO2_SCORE;
		}
		
		// Check for nitrogen - important buffer gas
		if (composition.TryGetValue("Nitrogen", out float nitrogen))
		{
			if (nitrogen >= MIN_NITROGEN)
				score += NITROGEN_SCORE;
		}
		
		return Math.Clamp(score, 0f, 1f);
	}

	/// <summary>
	/// Calculates a score based on water coverage suitability for Earth-like life.
	/// Ideal range is 0.3 to 0.8 (30% to 80% coverage).
	/// </summary>
	/// <param name="waterCoverage">Fraction of surface covered by liquid water (0-1)</param>
	/// <returns>Water score (0-1)</returns>
	private float CalculateWaterScore(float waterCoverage)
	{
		const float MIN_IDEAL_WATER = 0.3f;
		const float MAX_IDEAL_WATER = 0.8f;
		
		if (waterCoverage < MIN_IDEAL_WATER)
			return waterCoverage / MIN_IDEAL_WATER;
		else if (waterCoverage > MAX_IDEAL_WATER)
			return 1f - ((waterCoverage - MAX_IDEAL_WATER) / (1f - MAX_IDEAL_WATER));
		else
			return 1f;
	}

	/// <summary>
	/// Calculates a score based on radiation shielding from magnetic field and atmosphere.
	/// Either a strong magnetic field or thick atmosphere can provide adequate shielding.
	/// </summary>
	/// <param name="magneticFieldStrength">Strength of planetary magnetic field (0-1)</param>
	/// <param name="atmosphericPressure">Atmospheric pressure in Earth atmospheres</param>
	/// <returns>Radiation shielding score (0-1)</returns>
	private float CalculateShieldingScore(float magneticFieldStrength, float atmosphericPressure)
	{
		const float MIN_SHIELDING_ATMOSPHERE = 0.5f;
		
		// Both magnetic field and atmosphere provide shielding
		float magneticScore = magneticFieldStrength;
		float atmosphereScore = Math.Min(1f, atmosphericPressure / MIN_SHIELDING_ATMOSPHERE);
		
		// Take the better of the two scores - either protection source is sufficient
		return Math.Max(magneticScore, atmosphereScore);
	}
}
}
