using System;
using Libraries;

/// <summary>
/// Helper class for calculating water-related properties of celestial bodies.
/// Handles water coverage, phase states, and hydrological characteristics.
/// </summary>
namespace PlanetHelpers
{
public class PlanetHydrologyHelper
{
	/// <summary>
	/// Calculates the fraction of a planet's surface covered by liquid water.
	/// Takes into account temperature, pressure, and planet type.
	/// </summary>
	/// <param name="planetType">Type classification of the planet</param>
	/// <param name="surfaceTemperature">Surface temperature in Kelvin</param>
	/// <param name="atmosphericPressure">Atmospheric pressure in Earth atmospheres</param>
	/// <returns>Fraction of surface covered by liquid water (0-1)</returns>
	public float CalculateWaterCoverage(
    PlanetLibrary.PlanetType planetType, 
    float surfaceTemperature, 
    float atmosphericPressure)
	{
		// Check if liquid water can physically exist under these conditions
		if (!CanWaterExist(surfaceTemperature, atmosphericPressure))
		{
			return 0f;
		}

		// Get data from planet type
		var planetData = PlanetLibrary.PlanetData[planetType];
		var waterRange = planetData.WaterContentRange;
		
		// If planet type can't have water, return zero
		if (waterRange.max <= 0.001f)
		{
			return 0f;
		}

		// Calculate initial coverage based on planet type's water content
		// Pick a value within the range, weighted toward the higher end
		float baseWaterContent = waterRange.min + (waterRange.max - waterRange.min) * 
								Roll.FindRange(0.3f, 1.0f);
		
		// Calculate coverage from water content
		float coverage = CalculateCoverage(baseWaterContent, surfaceTemperature);
		
		// Modify based on temperature ranges
		coverage = ModifyForTemperature(coverage, surfaceTemperature);
		
		// Modify based on atmospheric pressure
		coverage = ModifyForPressure(coverage, atmosphericPressure);
		
		return Math.Clamp(coverage, 0f, 1f);
	}

	/// <summary>
	/// Determines if liquid water can exist under the given temperature and pressure conditions.
	/// Based on the phase diagram of water.
	/// </summary>
	/// <param name="temperature">Surface temperature in Kelvin</param>
	/// <param name="pressure">Atmospheric pressure in Earth atmospheres</param>
	/// <returns>True if water can exist in liquid form</returns>
	private bool CanWaterExist(float temperature, float pressure)
	{
		const float FREEZING_POINT = 273f;     // 0°C
		const float BOILING_POINT = 373f;      // 100°C
		const float CRITICAL_POINT_TEMP = 647f; // 374°C
		const float TRIPLE_POINT_PRESSURE = 0.006f; // ~0.006 atm
		const float PRESSURE_TEMP_FACTOR = 30f;
		
		// Water can exist as ice below freezing point
		if (temperature < FREEZING_POINT)
		{
			return true; // Can exist as ice
		}
		
		// Above critical point, water can't exist as a liquid
		if (temperature > CRITICAL_POINT_TEMP)
		{
			return false; // Above critical point
		}
		
		// Minimum pressure required for liquid water
		if (temperature <= BOILING_POINT && pressure >= TRIPLE_POINT_PRESSURE)
		{
			return true; // Can exist as liquid between freezing and boiling at sufficient pressure
		}
		
		// Higher temperatures require higher pressures to maintain liquid state
		float requiredPressure = TRIPLE_POINT_PRESSURE + (temperature - BOILING_POINT) / PRESSURE_TEMP_FACTOR;
		return pressure >= requiredPressure;
	}

	/// <summary>
	/// Calculates base water coverage from the planet's water probability and temperature.
	/// </summary>
	/// <param name="baseChance">Base probability of water from planet type (0-1)</param>
	/// <param name="temperature">Surface temperature in Kelvin</param>
	/// <returns>Initial water coverage before adjustments (0-1)</returns>
	private float CalculateCoverage(float baseWaterContent, float temperature)
	{
		const float BASE_VARIATION = 0.15f;  // ±15% variation
		const float GOLDILOCKS_VARIATION = 0.2f; // ±20% variation
		const float FREEZING_POINT = 273f;
		const float BOILING_POINT = 373f;
		
		// Add some variation to base coverage
		float baseCoverage = Roll.Vary(baseWaterContent, BASE_VARIATION);
		
		// Adjust based on Goldilocks zone (freezing to boiling is ideal for liquid water)
		if (temperature > FREEZING_POINT && temperature < BOILING_POINT)
		{
			baseCoverage = Roll.Vary(baseCoverage, GOLDILOCKS_VARIATION);
			
			// Additional bonus for temperatures close to Earth average (288K)
			float earthLikeFactor = 1f - Math.Abs(temperature - 288f) / 100f;
			baseCoverage *= Math.Max(1f, 1f + 0.1f * earthLikeFactor);
		}
		
		return baseCoverage;
	}

	/// <summary>
	/// Modifies water coverage based on temperature conditions.
	/// Accounts for frozen worlds, hot worlds with water loss, etc.
	/// </summary>
	/// <param name="coverage">Initial water coverage (0-1)</param>
	/// <param name="temperature">Surface temperature in Kelvin</param>
	/// <returns>Modified water coverage (0-1)</returns>
	private float ModifyForTemperature(float coverage, float temperature)
	{
		const float FREEZING_POINT = 273f;
		const float BOILING_POINT = 373f;
		const float TEMPERATURE_DECLINE_RATE = 300f;
		const float ICE_WORLD_FACTOR = 0.3f;
		
		if (temperature < FREEZING_POINT)
		{
			// Frozen world, surface water exists as ice
			// Some liquid water might exist beneath ice sheets or in subsurface oceans
			return coverage * ICE_WORLD_FACTOR;
		}
		
		if (temperature > BOILING_POINT)
		{
			// Hot world, water exists primarily as vapor or is being lost to space
			float temperatureExcess = temperature - BOILING_POINT;
			float retentionFactor = Math.Max(0f, 1f - (temperatureExcess / TEMPERATURE_DECLINE_RATE));
			return coverage * retentionFactor;
		}
		
		return coverage;
	}

	/// <summary>
	/// Modifies water coverage based on atmospheric pressure conditions.
	/// Accounts for pressure effects on water retention and state.
	/// </summary>
	/// <param name="coverage">Initial water coverage (0-1)</param>
	/// <param name="pressure">Atmospheric pressure in Earth atmospheres</param>
	/// <returns>Modified water coverage (0-1)</returns>
	private float ModifyForPressure(float coverage, float pressure)
	{
		const float TRIPLE_POINT_PRESSURE = 0.006f;
		const float HIGH_PRESSURE_THRESHOLD = 100f;
		const float LOW_PRESSURE_FACTOR = 0.1f;
		const float HIGH_PRESSURE_FACTOR = 0.5f;
		
		if (pressure < TRIPLE_POINT_PRESSURE)
		{
			// Too low pressure for liquid water (below triple point)
			// Water would sublimate directly from ice to vapor
			return coverage * LOW_PRESSURE_FACTOR;
		}
		
		if (pressure > HIGH_PRESSURE_THRESHOLD)
		{
			// Very high pressure might cause exotic water states
			// Or indicate gas giant conditions where water behaves differently
			return coverage * HIGH_PRESSURE_FACTOR;
		}
		
		// Higher pressures help retain water, up to a point
		if (pressure > 1f && pressure < 10f) {
			return coverage * Math.Min(1.2f, 1f + (pressure - 1f) / 20f);
		}
		
		return coverage;
	}
}
}
