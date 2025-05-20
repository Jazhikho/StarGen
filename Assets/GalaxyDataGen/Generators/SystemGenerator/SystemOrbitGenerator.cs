using UnityEngine;
using System;
using System.Collections.Generic;
using Libraries;

/// <summary>
/// Generates orbits and planets within star systems.
/// </summary>
public class SystemOrbitGenerator
{
	private OrbitGenerator _orbitGenerator = new OrbitGenerator();
	private PlanetGenerator _planetGenerator = new PlanetGenerator();
	private AsteroidBeltGenerator _asteroidGenerator = new AsteroidBeltGenerator();

	/// <summary>
	/// Generates orbits and planets for a star system.
	/// </summary>
	/// <param name="system">The star system to generate orbits for</param>
	public void GenerateSystemOrbits(StarSystem system)
	{
		// Process all stars directly
		foreach (var star in system.Stars)
		{
			star.Zones = ZoneHelper.CalculateOrbitalZones(star);
			star.CalculateHabitableZone();
			star.CalculateFrostLine();
			
			// Generate orbits for standalone stars
			// For stars without binary pair associations
			if (!IsPartOfBinaryPair(star.ID, system))
			{
				star.Orbits = _orbitGenerator.GenerateCircumstellarOrbits(
					star, 
					0f,
					float.MaxValue  // Maximum separation - no companion
				);
				GeneratePlanets(star, system);
			}
		}
		
		// Process binary pairs
		foreach (var pair in system.BinaryPairs)
		{
			ProcessBinaryPair(pair, system);
		}
	}
	
	/// <summary>
	/// Check if a star is part of a binary pair
	/// </summary>
	private bool IsPartOfBinaryPair(string starID, StarSystem system)
	{
		foreach (var pair in system.BinaryPairs)
		{
			if ((pair.PrimaryType == "Star" && pair.PrimaryID == starID) ||
				(pair.SecondaryType == "Star" && pair.SecondaryID == starID))
			{
				return true;
			}
		}
		return false;
	}
	
	/// <summary>
	/// Process a binary pair
	/// </summary>
	private void ProcessBinaryPair(StarSystem.BinaryPair pair, StarSystem system)
	{
		// Get references to the components
		object primary = GetComponent(pair.PrimaryID, pair.PrimaryType, system);
		object secondary = GetComponent(pair.SecondaryID, pair.SecondaryType, system);
		
		if (primary == null || secondary == null)
		{
			Debug.LogWarning($"Could not find components for binary pair {pair.ID}");
			return;
		}
		
		float primaryMass = GetTotalMass(primary, system);
		float secondaryMass = GetTotalMass(secondary, system);
		float separation = pair.SeparationDistance;
		
		// Generate orbits for primary star considering secondary's influence
		if (primary is StarStructure primaryStar)
		{
			primaryStar.Orbits = _orbitGenerator.GenerateCircumstellarOrbits(
				primaryStar,
				secondaryMass,
				separation
			);
			GeneratePlanets(primaryStar, system);
		}
		
		// Generate orbits for secondary star considering primary's influence
		if (secondary is StarStructure secondaryStar)
		{
			secondaryStar.Orbits = _orbitGenerator.GenerateCircumstellarOrbits(
				secondaryStar,
				primaryMass,
				separation
			);
			GeneratePlanets(secondaryStar, system);
		}

		// Generate circumbinary orbits if applicable
		if (pair.CircumbinaryZones != null)
		{
			// Get radii for Roche limit calculations
			float primaryRadius = GetComponentRadius(primary, system);
			float secondaryRadius = GetComponentRadius(secondary, system);
			
			pair.CircumbinaryOrbits = _orbitGenerator.GenerateCircumbinaryOrbits(
				pair.CircumbinaryZones,
				primaryMass,
				secondaryMass,
				separation,
				primaryRadius,
				secondaryRadius,
				GetTotalLuminosity(pair, system)
			);
			
			GenerateCircumbinaryPlanets(pair, system);
		}
	}
	
	/// <summary>
	/// Gets a component from the system by ID and type
	/// </summary>
	private object GetComponent(string id, string type, StarSystem system)
	{
		if (type == "Star")
		{
			return system.FindStarByID(id);
		}
		else if (type == "BinaryPair")
		{
			return system.FindBinaryPairByID(id);
		}
		return null;
	}
	
	/// <summary>
	/// Gets the radius of a component (star or binary pair).
	/// </summary>
	/// <param name="component">The component to get the radius of</param>
	/// <returns>The radius in solar radii</returns>
	private float GetComponentRadius(object component, StarSystem system)
	{
		switch (component)
		{
			case StarStructure star:
				return star.Radius;
			case StarSystem.BinaryPair pair:
				object primary = GetComponent(pair.PrimaryID, pair.PrimaryType, system);
				object secondary = GetComponent(pair.SecondaryID, pair.SecondaryType, system);
				
				float primaryRadius = primary != null ? GetComponentRadius(primary, system) : 0;
				float secondaryRadius = secondary != null ? GetComponentRadius(secondary, system) : 0;
				
				return Mathf.Max(primaryRadius, secondaryRadius);
			default:
				return 0;
		}
	}
	
	/// <summary>
	/// Generates planets for a star based on its calculated orbits.
	/// </summary>
	/// <param name="star">The star to generate planets for</param>
	private void GeneratePlanets(StarStructure star, StarSystem system)
	{
		foreach (var orbit in star.Orbits)
		{
			if (orbit.PlanetType == PlanetLibrary.PlanetType.Asterian)
			{
				AsteroidBelt belt = _asteroidGenerator.GenerateAsteroidBelt(
					$"{star.ID}-ABelt-{star.AsteroidBelts.Count + 1}",
					orbit.Distance * 0.9f,
					orbit.Distance * 1.1f,
					star.Zones.GetZoneType(orbit.Distance)
				);
				star.AddAsteroidBelt(belt);
			}
			else
			{
				ZoneType zone = star.Zones.GetZoneType(orbit.Distance);
				Planet planet = _planetGenerator.GeneratePlanet(
					orbit.Mass,
					orbit.Distance,
					zone,
					star,
					orbit.Eccentricity
				);

				planet.Inclination = orbit.Inclination;
				planet.HasRings = orbit.HasMoons && Roll.Dice(1, 100) <= 20;

				star.AddPlanet(planet);
			}
		}
	}
	
	/// <summary>
	/// Generates planets that orbit around both stars in a binary pair.
	/// </summary>
	/// <param name="pair">The binary pair</param>
	private void GenerateCircumbinaryPlanets(StarSystem.BinaryPair pair, StarSystem system)
	{
		// Create a "virtual star" with the combined properties of the binary
		StarStructure virtualStar = new StarStructure
		{
			Mass = GetTotalMass(pair, system),
			Luminosity = GetTotalLuminosity(pair, system),
			Zones = pair.CircumbinaryZones,
			Radius = GetComponentRadius(pair, system),
			Temperature = GetEffectiveTemperature(pair, system)
		};

		foreach (var orbit in pair.CircumbinaryOrbits)
		{
			if (orbit.PlanetType == PlanetLibrary.PlanetType.Asterian)
			{
				AsteroidBelt belt = _asteroidGenerator.GenerateAsteroidBelt(
					$"CB-ABelt-{pair.CircumbinaryAsteroidBelts.Count + 1}",
					orbit.Distance * 0.9f,
					orbit.Distance * 1.1f,
					pair.CircumbinaryZones.GetZoneType(orbit.Distance)
				);
				pair.AddCircumbinaryAsteroidBelt(belt);
			}
			else
			{
				ZoneType zone = pair.CircumbinaryZones.GetZoneType(orbit.Distance);
				Planet planet = _planetGenerator.GeneratePlanet(
					orbit.Mass,
					orbit.Distance,
					zone,
					virtualStar,
					orbit.Eccentricity
				);

				planet.Inclination = orbit.Inclination;
				planet.HasRings = orbit.HasMoons && UnityEngine.Random.value <= 0.2f;

				pair.AddCircumbinaryPlanet(planet);
			}
		}
	}

	/// <summary>
	/// Calculates the total luminosity of all stars in a binary pair.
	/// </summary>
	/// <param name="component">The component to calculate luminosity for</param>
	/// <returns>The total luminosity in solar luminosities</returns>
	private float GetTotalLuminosity(object component, StarSystem system)
	{
		switch (component)
		{
			case StarStructure star:
				return star.Luminosity;
			case StarSystem.BinaryPair pair:
				float luminosity = 0f;
				
				object primary = GetComponent(pair.PrimaryID, pair.PrimaryType, system);
				object secondary = GetComponent(pair.SecondaryID, pair.SecondaryType, system);
				
				if (primary != null)
					luminosity += GetTotalLuminosity(primary, system);
				
				if (secondary != null)
					luminosity += GetTotalLuminosity(secondary, system);
				
				return luminosity;
			default:
				return 0;
		}
	}
	
	/// <summary>
	/// Calculates the effective temperature as perceived by a distant object
	/// (like a circumbinary planet). Uses luminosity-weighted average.
	/// </summary>
	private float GetEffectiveTemperature(object component, StarSystem system)
	{
		switch (component)
		{
			case StarStructure star:
				return star.Temperature;
			case StarSystem.BinaryPair pair:
				object primary = GetComponent(pair.PrimaryID, pair.PrimaryType, system);
				object secondary = GetComponent(pair.SecondaryID, pair.SecondaryType, system);
				
				float primaryLum = primary != null ? GetTotalLuminosity(primary, system) : 0;
				float secondaryLum = secondary != null ? GetTotalLuminosity(secondary, system) : 0;
				float totalLum = primaryLum + secondaryLum;
				
				if (totalLum == 0) return 0;
				
				// Luminosity-weighted average temperature
				float primaryTemp = primary != null ? GetEffectiveTemperature(primary, system) : 0;
				float secondaryTemp = secondary != null ? GetEffectiveTemperature(secondary, system) : 0;
				
				return (primaryTemp * primaryLum + secondaryTemp * secondaryLum) / totalLum;
				
			default:
				return 0;
		}
	}
	
	/// <summary>
	/// Gets the total mass of a component (star or binary pair).
	/// </summary>
	/// <param name="component">The component to get the mass of</param>
	/// <returns>The total mass in solar masses</returns>
	private float GetTotalMass(object component, StarSystem system)
	{
		switch (component)
		{
			case StarStructure star:
				return star.Mass;
			case StarSystem.BinaryPair pair:
				float mass = 0f;

				object primary = GetComponent(pair.PrimaryID, pair.PrimaryType, system);
				object secondary = GetComponent(pair.SecondaryID, pair.SecondaryType, system);

				if (primary != null)
					mass += GetTotalMass(primary, system);

				if (secondary != null)
					mass += GetTotalMass(secondary, system);

				return mass;
			default:
				return 0;
		}
	}
}
