using System;
using UnityEngine;
using System.Collections.Generic;
using Libraries;

/// <summary>
/// Helper class for generating and calculating properties specific to moons.
/// </summary>
namespace PlanetHelpers
{
	/// <summary>
	/// Handles the generation and distribution of moon systems around planets.
	/// </summary>
	public class MoonSystemGenerator
	{
		private const float MIN_MASS_RATIO = 0.001f;  // Minimum moon-to-planet mass ratio
		private const float MAX_MASS_RATIO = 0.1f;    // Maximum moon-to-planet mass ratio
		
		/// <summary>
		/// Calculates the total mass available for a planet's moon system.
		/// </summary>
		/// <param name="planetMass">Mass of parent planet in Earth masses</param>
		/// <param name="planetType">Type classification of parent planet</param>
		/// <param name="zone">Orbital zone of the parent planet</param>
		/// <returns>Available mass for moons in Earth masses</returns>
		public float CalculatePotentialMoonSystemMass(float planetMass, PlanetLibrary.PlanetType planetType, ZoneType zone)
		{
			// Constants for mass ratio calculations
			const float REFERENCE_PLANET_MASS = 100f; // Earth masses
			const float BASE_RATIO_MODIFIER = 0.1f;
			
			// Larger planets have smaller available mass ratios for moons
			float maxRatio = Mathf.Lerp(MAX_MASS_RATIO, MIN_MASS_RATIO, planetMass / REFERENCE_PLANET_MASS);
			
			// Use Roll.Vary for the ratio - center at 50% of max
			float averageRatio = maxRatio * 0.5f;
			float availableMassRatio = Roll.Vary(averageRatio, 0.8f); // ±80% variation
			availableMassRatio = Mathf.Clamp(availableMassRatio, BASE_RATIO_MODIFIER * maxRatio, maxRatio);
			
			float totalAvailableMass = planetMass * availableMassRatio;

			// Get minimum possible moon mass for this zone
			float minMoonMass = GetMinimumPossibleMoonMass(zone);

			// If we can't even make one minimum mass moon, return 0
			if (totalAvailableMass < minMoonMass)
				return 0;

			return totalAvailableMass;
		}

		/// <summary>
		/// Represents orbital parameters for a moon.
		/// </summary>
		public struct MoonOrbitData
		{
			public float mass;
			public float orbitalDistance;
			public float eccentricity;
		}

		/// <summary>
		/// Determines the orbits and distribution of moons around a planet.
		/// </summary>
		/// <param name="totalMoonMass">Total mass available for moons</param>
		/// <param name="planetMass">Mass of parent planet in Earth masses</param>
		/// <param name="planetRadius">Radius of parent planet in Earth radii</param>
		/// <param name="planetOrbitDistance">Planet's orbital distance in AU</param>
		/// <param name="starMass">Parent star mass in solar masses</param>
		/// <param name="planetType">Type classification of parent planet</param>
		/// <param name="zone">Orbital zone classification</param>
		/// <returns>List of moon orbital parameters</returns>
		public List<MoonOrbitData> DetermineMoonOrbits(
			float totalMoonMass,
			float planetMass,
			float planetRadius,
			float planetOrbitDistance,
			float starMass,
			PlanetLibrary.PlanetType planetType,
			ZoneType zone)
		{
			var moons = new List<MoonOrbitData>();
			var formationData = MoonLibrary.MoonFormationParameters[planetType];
			float minMoonMass = GetMinimumPossibleMoonMass(zone);
			float remainingMass = totalMoonMass;
			
			// Calculate orbital limits using proper utility methods
			float rocheLimit = Utils.LunarRocheLimit(planetMass, planetRadius, minMoonMass);
			
			// Use PlanetaryHillSphere and convert to planet radii
			float hillSphereAU = Utils.PlanetaryHillSphere(planetMass, starMass, planetOrbitDistance);
			float hillSphereInPlanetRadii = hillSphereAU * Utils.AU / (planetRadius * Utils.EARTH_RADIUS);
			float maxStableOrbit = hillSphereInPlanetRadii * formationData.HillSphereModifier;

			float currentOrbitDistance = rocheLimit * 1.5f; // Start beyond Roche limit

			while (remainingMass >= minMoonMass && 
				moons.Count < formationData.MaxMoons && 
				currentOrbitDistance < maxStableOrbit)
			{
				// Allocate mass for this moon using Roll.Vary
				const float MASS_ALLOCATION_CENTER = 0.4f; // Center at 40% of remaining
				const float MASS_VARIATION = 0.6f; // ±60% variation
				
				float massFraction = Roll.Vary(MASS_ALLOCATION_CENTER, MASS_VARIATION);
				massFraction = Mathf.Clamp(massFraction, 0.1f, 1.0f);
				float moonMass = remainingMass * massFraction;
				
				// Ensure moon mass is at least the minimum required
				if (moonMass < minMoonMass)
					moonMass = minMoonMass;

				// Use Roll.Vary for eccentricity
				const float BASE_ECCENTRICITY = 0.05f;
				const float ECCENTRICITY_VARIATION = 0.8f;
				float eccentricity = Roll.Vary(BASE_ECCENTRICITY, ECCENTRICITY_VARIATION);
				eccentricity = Mathf.Clamp(eccentricity, 0.001f, 0.1f);

				moons.Add(new MoonOrbitData
				{
					mass = moonMass,
					orbitalDistance = currentOrbitDistance,
					eccentricity = eccentricity
				});

				remainingMass -= moonMass;
				currentOrbitDistance *= MoonLibrary.MoonConstants.MIN_SEPARATION_FACTOR;
			}
			return moons;
		}

		/// <summary>
		/// Gets the minimum possible mass for a moon based on its orbital zone.
		/// </summary>
		/// <param name="zone">Orbital zone classification</param>
		/// <returns>Minimum moon mass in Earth masses</returns>
		public float GetMinimumPossibleMoonMass(ZoneType zone)
		{
			// These represent the limit of what we consider a "moon" vs smaller debris
			// Based on real solar system examples:
			const float INNER_ZONE_MIN = 0.00001f;    // ~ Phobos/Deimos mass
			const float HABITABLE_ZONE_MIN = 0.0001f;  // ~ Small asteroid mass
			const float OUTER_ZONE_MIN = 0.001f;       // ~ Larger asteroid mass
			
			return zone switch
			{
				ZoneType.Inner => INNER_ZONE_MIN,
				ZoneType.Habitable => HABITABLE_ZONE_MIN,
				ZoneType.Outer => OUTER_ZONE_MIN,
				_ => HABITABLE_ZONE_MIN
			};
		}
	}
}
