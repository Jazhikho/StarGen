using System.Collections.Generic;
using UnityEngine;

namespace Libraries
{
	[System.Serializable]
	public static class MoonLibrary
	{
		public class MoonFormationData
		{
			public (float min, float max) OrbitDistanceRange { get; set; } // In planet radii
			public (float min, float max) MassRange { get; set; } // In Earth masses
			public float TidalHeatingFactor { get; set; } // Multiplier for tidal heating calculations
			public float ResonanceProbability { get; set; } // Chance of forming resonant orbits
			public bool PrefersTidalLocking { get; set; }
			public int MaxMoons { get; set; } // Maximum number of major moons
			public float RocheLimit { get; set; } // In planet radii
			public float HillSphereModifier { get; set; } // Fraction of Hill sphere where moons are stable
		}

		// Data for moon formation based on parent planet type
		public static readonly Dictionary<PlanetLibrary.PlanetType, MoonFormationData> MoonFormationParameters = 
			new Dictionary<PlanetLibrary.PlanetType, MoonFormationData>
		{
			// Gas Giants - Many moons
			{ PlanetLibrary.PlanetType.Hyperion, new MoonFormationData { // Hot gas giant
				OrbitDistanceRange = (5f, 80f),
				MassRange = (0.005f, 0.05f),
				TidalHeatingFactor = 0.2f,
				ResonanceProbability = 0.2f,
				PrefersTidalLocking = true,
				MaxMoons = 3,
				RocheLimit = 2.5f,
				HillSphereModifier = 0.3f
			}},
			{ PlanetLibrary.PlanetType.Atlantean, new MoonFormationData { // Cold gas giant
				OrbitDistanceRange = (5f, 100f),
				MassRange = (0.01f, 0.1f),
				TidalHeatingFactor = 0.6f,
				ResonanceProbability = 0.4f,
				PrefersTidalLocking = true,
				MaxMoons = 6,
				RocheLimit = 2.7f,
				HillSphereModifier = 0.5f
			}},
			
			// Ice Giants
			{ PlanetLibrary.PlanetType.Iapetian, new MoonFormationData {
				OrbitDistanceRange = (5f, 80f),
				MassRange = (0.005f, 0.05f),
				TidalHeatingFactor = 0.5f,
				ResonanceProbability = 0.35f,
				PrefersTidalLocking = true,
				MaxMoons = 4,
				RocheLimit = 2.5f,
				HillSphereModifier = 0.45f
			}},
			
			// Helium Planets
			{ PlanetLibrary.PlanetType.Helian, new MoonFormationData {
				OrbitDistanceRange = (6f, 90f),
				MassRange = (0.01f, 0.15f),
				TidalHeatingFactor = 0.7f,
				ResonanceProbability = 0.45f,
				PrefersTidalLocking = true,
				MaxMoons = 5,
				RocheLimit = 2.6f,
				HillSphereModifier = 0.5f
			}},
			
			// Mini-Neptunes
			{ PlanetLibrary.PlanetType.Criusian, new MoonFormationData { // Hot mini-Neptune
				OrbitDistanceRange = (4f, 30f),
				MassRange = (0.0005f, 0.01f),
				TidalHeatingFactor = 0.15f,
				ResonanceProbability = 0.15f,
				PrefersTidalLocking = true,
				MaxMoons = 1,
				RocheLimit = 2.1f,
				HillSphereModifier = 0.3f
			}},
			{ PlanetLibrary.PlanetType.Theian, new MoonFormationData { // Cold mini-Neptune
				OrbitDistanceRange = (3f, 25f),
				MassRange = (0.0001f, 0.005f),
				TidalHeatingFactor = 0.3f,
				ResonanceProbability = 0.1f,
				PrefersTidalLocking = true,
				MaxMoons = 2,
				RocheLimit = 2.2f,
				HillSphereModifier = 0.2f
			}},
			
			// Super-Earths
			{ PlanetLibrary.PlanetType.Rhean, new MoonFormationData {
				OrbitDistanceRange = (5f, 40f),
				MassRange = (0.005f, 0.05f),
				TidalHeatingFactor = 0.4f,
				ResonanceProbability = 0.3f,
				PrefersTidalLocking = true,
				MaxMoons = 3,
				RocheLimit = 2.3f,
				HillSphereModifier = 0.4f
			}},
			{ PlanetLibrary.PlanetType.Dionean, new MoonFormationData { // Icy Super-Earth
				OrbitDistanceRange = (4f, 35f),
				MassRange = (0.003f, 0.03f),
				TidalHeatingFactor = 0.35f,
				ResonanceProbability = 0.25f,
				PrefersTidalLocking = true,
				MaxMoons = 2,
				RocheLimit = 2.2f,
				HillSphereModifier = 0.35f
			}},
			
			// Terrestrial Worlds
			{ PlanetLibrary.PlanetType.Gaian, new MoonFormationData {
				OrbitDistanceRange = (10f, 60f),
				MassRange = (0.001f, 0.02f),
				TidalHeatingFactor = 0.3f,
				ResonanceProbability = 0.2f,
				PrefersTidalLocking = true,
				MaxMoons = 2,
				RocheLimit = 2.44f,
				HillSphereModifier = 0.4f
			}},
			{ PlanetLibrary.PlanetType.Oceanian, new MoonFormationData {
				OrbitDistanceRange = (8f, 50f),
				MassRange = (0.001f, 0.015f),
				TidalHeatingFactor = 0.35f,
				ResonanceProbability = 0.25f,
				PrefersTidalLocking = true,
				MaxMoons = 2,
				RocheLimit = 2.3f,
				HillSphereModifier = 0.35f
			}},
			{ PlanetLibrary.PlanetType.Tethysian, new MoonFormationData { // Mesic terrestrial
				OrbitDistanceRange = (7f, 45f),
				MassRange = (0.0008f, 0.012f),
				TidalHeatingFactor = 0.25f,
				ResonanceProbability = 0.2f,
				PrefersTidalLocking = true,
				MaxMoons = 2,
				RocheLimit = 2.3f,
				HillSphereModifier = 0.35f
			}},
			{ PlanetLibrary.PlanetType.Promethean, new MoonFormationData { // Arid terrestrial
				OrbitDistanceRange = (5f, 30f),
				MassRange = (0.0005f, 0.008f),
				TidalHeatingFactor = 0.2f,
				ResonanceProbability = 0.15f,
				PrefersTidalLocking = true,
				MaxMoons = 1,
				RocheLimit = 2.1f,
				HillSphereModifier = 0.25f
			}},
			{ PlanetLibrary.PlanetType.Menoetian, new MoonFormationData { // Barren terrestrial
				OrbitDistanceRange = (3f, 15f),
				MassRange = (0.00005f, 0.001f),
				TidalHeatingFactor = 0.05f,
				ResonanceProbability = 0.05f,
				PrefersTidalLocking = true,
				MaxMoons = 1,
				RocheLimit = 1.8f,
				HillSphereModifier = 0.15f
			}},
			{ PlanetLibrary.PlanetType.Metian, new MoonFormationData { // Mercury-like
				OrbitDistanceRange = (2f, 8f),
				MassRange = (0.00001f, 0.0005f),
				TidalHeatingFactor = 0.02f,
				ResonanceProbability = 0.02f,
				PrefersTidalLocking = true,
				MaxMoons = 1,
				RocheLimit = 1.5f,
				HillSphereModifier = 0.1f
			}},
			
			// Ice Dwarfs
			{ PlanetLibrary.PlanetType.Phoeboan, new MoonFormationData {
				OrbitDistanceRange = (3f, 20f),
				MassRange = (0.0001f, 0.001f),
				TidalHeatingFactor = 0.1f,
				ResonanceProbability = 0.05f,
				PrefersTidalLocking = true,
				MaxMoons = 1,
				RocheLimit = 2.0f,
				HillSphereModifier = 0.2f
			}},
			
			// Special Types
			{ PlanetLibrary.PlanetType.Lelantian, new MoonFormationData { // Carbon planet
				OrbitDistanceRange = (4f, 35f),
				MassRange = (0.001f, 0.02f),
				TidalHeatingFactor = 0.25f,
				ResonanceProbability = 0.2f,
				PrefersTidalLocking = true,
				MaxMoons = 2,
				RocheLimit = 2.2f,
				HillSphereModifier = 0.3f
			}},
			{ PlanetLibrary.PlanetType.Vulcanian, new MoonFormationData { // Extreme volcanic
				OrbitDistanceRange = (3f, 12f),
				MassRange = (0.00005f, 0.0008f),
				TidalHeatingFactor = 0.03f,
				ResonanceProbability = 0.03f,
				PrefersTidalLocking = true,
				MaxMoons = 1,
				RocheLimit = 1.6f,
				HillSphereModifier = 0.12f
			}},
			{ PlanetLibrary.PlanetType.Cronusian, new MoonFormationData { // Stripped gas giant core
				OrbitDistanceRange = (5f, 60f),
				MassRange = (0.01f, 0.1f),
				TidalHeatingFactor = 0.8f,
				ResonanceProbability = 0.5f,
				PrefersTidalLocking = true,
				MaxMoons = 4,
				RocheLimit = 2.8f,
				HillSphereModifier = 0.45f
			}}
			// Note: Asterian (asteroids) don't have moons in this model
		};

		// Constants for moon formation and stability
		public static class MoonConstants
		{
			public const float MIN_SEPARATION_FACTOR = 3.5f; // Minimum separation between moons in Hill radii
			public const float CRITICAL_TIDAL_FORCE = 0.025f; // Critical tidal force for significant heating
			public const float MAX_STABLE_INCLINATION = 30.0f; // Maximum stable inclination in degrees
			public const float RESONANCE_TOLERANCE = 0.01f; // Tolerance for resonance calculations
		}
	}
}