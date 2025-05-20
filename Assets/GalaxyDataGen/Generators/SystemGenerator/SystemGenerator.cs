using UnityEngine;
using System;
using System.Collections.Generic;
using Libraries;

/// <summary>
/// Generates star systems with stars, planets, and other celestial bodies.
/// </summary>
public class SystemGenerator
{
	private StarGenerator _starGenerator = new StarGenerator();
	private SystemOrbitGenerator _systemOrbitGenerator;
	private EntityRegistry _registry;

	/// <summary>
	/// Initializes a new instance of the SystemGenerator class.
	/// </summary>
	public SystemGenerator(EntityRegistry registry = null)
	{
		_systemOrbitGenerator = new SystemOrbitGenerator();
		_registry = registry ?? EntityRegistry.Instance;
	}

	/// <summary>
	/// Generates a complete star system with stars, planets, and other bodies.
	/// </summary>
	/// <param name="system">The star system to populate</param>
	public void GenerateSystem(StarSystem system)
	{
		// Generate stars
		int starCount = Roll.Seek(SystemLibrary.NumberDistribution);
		List<StarStructure> stars = new List<StarStructure>();
		
		for (int i = 0; i < starCount; i++)
		{
			StarStructure newStar = _starGenerator.GenerateStar();
			newStar.ParentSystemID = system.ID;
			_registry.RegisterStar(newStar);
			stars.Add(newStar);
			system.AddStar(newStar);
		}

		// Organize stars into hierarchical structure using the adapted helper
		string rootPairID = SystemArrangementHelper.OrganizeStarHierarchy(stars, system);
		
		// Only set root pair if we actually have one (multi-star systems)
		if (rootPairID != null)
		{
			system.SetRootBinaryPair(rootPairID);
		}

		// Register the system and all its components
		_registry.RegisterSystem(system);
		
		// Calculate orbital zones and generate planets
		CalculateSystemZones(system);
		_systemOrbitGenerator.GenerateSystemOrbits(system);
	}
	
	/// <summary>
	/// Calculates the orbital zones for all components in a system.
	/// </summary>
	/// <param name="system">The star system to calculate zones for</param>
	private void CalculateSystemZones(StarSystem system)
	{
		// Calculate zones for individual stars
		foreach (var star in system.Stars)
		{
			star.Zones = ZoneHelper.CalculateOrbitalZones(star);
		}
		
		// Handle binary pair zones
		foreach (var pair in system.BinaryPairs)
		{
			CalculateBinaryZones(system, pair);
		}
	}

	/// <summary>
	/// Calculates zones for a binary pair
	/// </summary>
	private void CalculateBinaryZones(StarSystem system, StarSystem.BinaryPair pair)
	{
		// Calculate circumbinary zones
		pair.CircumbinaryZones = new OrbitalZones();

		// Get the component objects
		object primary = GetComponentFromID(pair.PrimaryID, pair.PrimaryType, system);
		object secondary = GetComponentFromID(pair.SecondaryID, pair.SecondaryType, system);

		// If both components are stars, calculate their combined zones
		if (primary is StarStructure primaryStar && secondary is StarStructure secondaryStar)
		{
			// Binary separation affects zones
			if (pair.SeparationDistance < 10f)
			{
				// Close binary - combine properties for circumbinary calculation
				float combinedLuminosity = primaryStar.Luminosity + secondaryStar.Luminosity;
				float combinedMass = primaryStar.Mass + secondaryStar.Mass;

				// Create a temporary star to calculate combined zones
				StarStructure combinedStar = new StarStructure
				{
					Luminosity = combinedLuminosity,
					Mass = combinedMass,
					Temperature = Mathf.Max(primaryStar.Temperature, secondaryStar.Temperature),
					Radius = Mathf.Max(primaryStar.Radius, secondaryStar.Radius)
				};

				// Calculate zones for the combined star
				pair.CircumbinaryZones = ZoneHelper.CalculateOrbitalZones(combinedStar);

				// Adjust individual star zones (making them non-applicable)
				ZoneHelper.AdjustZonesForBinaryInterference(primaryStar.Zones, pair.SeparationDistance);
				ZoneHelper.AdjustZonesForBinaryInterference(secondaryStar.Zones, pair.SeparationDistance);
			}
			else
			{
				// Wide binary - each star has its own zones, plus circumbinary zones
				pair.CircumbinaryZones = ZoneHelper.CalculateCircumbinaryZones(pair, system);

				// Still adjust individual zones for interference
				ZoneHelper.AdjustZonesForBinaryInterference(primaryStar.Zones, pair.SeparationDistance);
				ZoneHelper.AdjustZonesForBinaryInterference(secondaryStar.Zones, pair.SeparationDistance);
			}
		}
		else
		{
			// More complex hierarchies - simplified implementation
			// Just calculate some basic circumbinary zones
			pair.CircumbinaryZones = new OrbitalZones
			{
				SystemLimit = pair.SeparationDistance * 5f,  // Estimate
				HabitableZoneInner = float.NaN,              // Not applicable
				HabitableZoneOuter = float.NaN               // Not applicable
			};
		}
	}
		
	private object GetComponentFromID(string id, string type, StarSystem system)
	{
		if (type == "Star")
			return _registry.GetStar(id) ?? system.FindStarByID(id);

		if (type == "BinaryPair")
			return system.FindBinaryPairByID(id);

		return null;
	}
}
