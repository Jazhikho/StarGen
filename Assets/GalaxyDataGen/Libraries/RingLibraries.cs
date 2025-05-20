using System.Collections.Generic;
using UnityEngine;

namespace Libraries
{
	[System.Serializable]
	public static class RingLibrary
	{
		public class RingTypeData
		{
			public (float min, float max) MassRatio { get; set; }  // Ratio of planet mass
			public (float min, float max) OpacityRange { get; set; }
			public Dictionary<string, (float min, float max)> CompositionRanges { get; set; }
			public float ComplexityProbability { get; set; }  // Chance of having complex rings
		}

		public static readonly Dictionary<PlanetLibrary.PlanetType, RingTypeData> RingData = 
			new Dictionary<PlanetLibrary.PlanetType, RingTypeData>
		{
			// Gas Giants - Most likely to have rings
			{ PlanetLibrary.PlanetType.Hyperion, new RingTypeData { // Hot gas giant
				MassRatio = (0.00005f, 0.0005f),
				OpacityRange = (0.2f, 0.5f),
				ComplexityProbability = 0.3f,
				CompositionRanges = new Dictionary<string, (float min, float max)> {
					{ "Silicates", (0.5f, 0.7f) },
					{ "Metals", (0.2f, 0.4f) },
					{ "Organics", (0.05f, 0.15f) },
					{ "Water Ice", (0.01f, 0.05f) }
				}
			}},
			{ PlanetLibrary.PlanetType.Atlantean, new RingTypeData { // Cold gas giant
				MassRatio = (0.00001f, 0.0001f),
				OpacityRange = (0.3f, 0.7f),
				ComplexityProbability = 0.5f,
				CompositionRanges = new Dictionary<string, (float min, float max)> {
					{ "Water Ice", (0.4f, 0.6f) },
					{ "Silicates", (0.2f, 0.3f) },
					{ "Organics", (0.1f, 0.2f) },
					{ "Metals", (0.05f, 0.15f) }
				}
			}},
			
			// Ice Giants
			{ PlanetLibrary.PlanetType.Iapetian, new RingTypeData {
				MassRatio = (0.000005f, 0.00005f),
				OpacityRange = (0.3f, 0.6f),
				ComplexityProbability = 0.4f,
				CompositionRanges = new Dictionary<string, (float min, float max)> {
					{ "Water Ice", (0.5f, 0.7f) },
					{ "Silicates", (0.15f, 0.25f) },
					{ "Organics", (0.1f, 0.2f) },
					{ "Metals", (0.02f, 0.08f) }
				}
			}},
			
			// Helium planets
			{ PlanetLibrary.PlanetType.Helian, new RingTypeData {
				MassRatio = (0.000015f, 0.00015f),
				OpacityRange = (0.3f, 0.8f),
				ComplexityProbability = 0.6f,
				CompositionRanges = new Dictionary<string, (float min, float max)> {
					{ "Helium Compounds", (0.4f, 0.6f) },
					{ "Water Ice", (0.2f, 0.3f) },
					{ "Silicates", (0.1f, 0.2f) },
					{ "Metals", (0.05f, 0.15f) }
				}
			}},
			
			// Mini-Neptunes
			{ PlanetLibrary.PlanetType.Criusian, new RingTypeData { // Hot mini-Neptune
				MassRatio = (0.000003f, 0.00003f),
				OpacityRange = (0.3f, 0.6f),
				ComplexityProbability = 0.4f,
				CompositionRanges = new Dictionary<string, (float min, float max)> {
					{ "Water Ice", (0.6f, 0.8f) },
					{ "Frozen Volatiles", (0.1f, 0.2f) },
					{ "Silicates", (0.1f, 0.2f) }
				}
			}},
			{ PlanetLibrary.PlanetType.Theian, new RingTypeData { // Cold mini-Neptune
				MassRatio = (0.000001f, 0.00001f),
				OpacityRange = (0.1f, 0.3f),
				ComplexityProbability = 0.1f,
				CompositionRanges = new Dictionary<string, (float min, float max)> {
					{ "Silicates", (0.6f, 0.8f) },
					{ "Metals", (0.15f, 0.25f) },
					{ "Sulfur Compounds", (0.05f, 0.15f) }
				}
			}},
			
			// Super-Earths
			{ PlanetLibrary.PlanetType.Rhean, new RingTypeData {
				MassRatio = (0.000002f, 0.00002f),
				OpacityRange = (0.2f, 0.4f),
				ComplexityProbability = 0.2f,
				CompositionRanges = new Dictionary<string, (float min, float max)> {
					{ "Silicates", (0.4f, 0.6f) },
					{ "Water Ice", (0.2f, 0.4f) },
					{ "Metals", (0.1f, 0.2f) }
				}
			}},
			{ PlanetLibrary.PlanetType.Dionean, new RingTypeData { // Icy Super-Earth
				MassRatio = (0.000003f, 0.00003f),
				OpacityRange = (0.2f, 0.5f),
				ComplexityProbability = 0.3f,
				CompositionRanges = new Dictionary<string, (float min, float max)> {
					{ "Water Ice", (0.5f, 0.7f) },
					{ "Silicates", (0.2f, 0.3f) },
					{ "Organics", (0.1f, 0.2f) }
				}
			}},
			
			// Terrestrial worlds - Less likely to have rings
			{ PlanetLibrary.PlanetType.Gaian, new RingTypeData {
				MassRatio = (0.000001f, 0.00001f),
				OpacityRange = (0.1f, 0.3f),
				ComplexityProbability = 0.1f,
				CompositionRanges = new Dictionary<string, (float min, float max)> {
					{ "Silicates", (0.5f, 0.7f) },
					{ "Water Ice", (0.2f, 0.3f) },
					{ "Metals", (0.1f, 0.2f) }
				}
			}},
			{ PlanetLibrary.PlanetType.Oceanian, new RingTypeData {
				MassRatio = (0.000005f, 0.00005f),
				OpacityRange = (0.2f, 0.5f),
				ComplexityProbability = 0.3f,
				CompositionRanges = new Dictionary<string, (float min, float max)> {
					{ "Water Ice", (0.5f, 0.7f) },
					{ "Silicates", (0.2f, 0.3f) },
					{ "Organics", (0.1f, 0.2f) }
				}
			}},
			{ PlanetLibrary.PlanetType.Tethysian, new RingTypeData { // Mesic terrestrial
				MassRatio = (0.0000008f, 0.000008f),
				OpacityRange = (0.1f, 0.25f),
				ComplexityProbability = 0.08f,
				CompositionRanges = new Dictionary<string, (float min, float max)> {
					{ "Silicates", (0.5f, 0.7f) },
					{ "Water Ice", (0.15f, 0.25f) },
					{ "Metals", (0.1f, 0.2f) }
				}
			}},
			{ PlanetLibrary.PlanetType.Promethean, new RingTypeData { // Arid terrestrial
				MassRatio = (0.0000006f, 0.000006f),
				OpacityRange = (0.1f, 0.2f),
				ComplexityProbability = 0.05f,
				CompositionRanges = new Dictionary<string, (float min, float max)> {
					{ "Silicates", (0.7f, 0.85f) },
					{ "Metals", (0.15f, 0.25f) },
					{ "Water Ice", (0.01f, 0.05f) }
				}
			}},
			{ PlanetLibrary.PlanetType.Menoetian, new RingTypeData { // Barren terrestrial
				MassRatio = (0.0000005f, 0.000005f),
				OpacityRange = (0.1f, 0.2f),
				ComplexityProbability = 0.1f,
				CompositionRanges = new Dictionary<string, (float min, float max)> {
					{ "Silicates", (0.6f, 0.8f) },
					{ "Water Ice", (0.1f, 0.2f) },
					{ "Metals", (0.1f, 0.2f) }
				}
			}},
			{ PlanetLibrary.PlanetType.Metian, new RingTypeData { // Mercury-like
				MassRatio = (0.0000003f, 0.000003f),
				OpacityRange = (0.1f, 0.15f),
				ComplexityProbability = 0.03f,
				CompositionRanges = new Dictionary<string, (float min, float max)> {
					{ "Silicates", (0.7f, 0.85f) },
					{ "Metals", (0.15f, 0.3f) }
				}
			}},
			
			// Dwarf planets and special types
			{ PlanetLibrary.PlanetType.Phoeboan, new RingTypeData { // Ice dwarf
				MassRatio = (0.0000002f, 0.000002f),
				OpacityRange = (0.1f, 0.2f),
				ComplexityProbability = 0.05f,
				CompositionRanges = new Dictionary<string, (float min, float max)> {
					{ "Water Ice", (0.7f, 0.85f) },
					{ "Silicates", (0.1f, 0.2f) },
					{ "Organics", (0.05f, 0.1f) }
				}
			}},
			{ PlanetLibrary.PlanetType.Vulcanian, new RingTypeData { // Extreme volcanic
				MassRatio = (0.0000001f, 0.000001f),
				OpacityRange = (0.1f, 0.2f),
				ComplexityProbability = 0.02f,
				CompositionRanges = new Dictionary<string, (float min, float max)> {
					{ "Silicates", (0.5f, 0.7f) },
					{ "Sulfur Compounds", (0.2f, 0.3f) },
					{ "Metals", (0.1f, 0.2f) }
				}
			}},
			{ PlanetLibrary.PlanetType.Cronusian, new RingTypeData { // Stripped gas giant core
				MassRatio = (0.00002f, 0.0002f),
				OpacityRange = (0.4f, 0.9f),
				ComplexityProbability = 0.7f,
				CompositionRanges = new Dictionary<string, (float min, float max)> {
					{ "Water Ice", (0.6f, 0.8f) },
					{ "Silicates", (0.1f, 0.2f) },
					{ "Organics", (0.05f, 0.15f) },
					{ "Metals", (0.01f, 0.05f) }
				}
			}},
			{ PlanetLibrary.PlanetType.Lelantian, new RingTypeData { // Carbon planet
				MassRatio = (0.000002f, 0.00002f),
				OpacityRange = (0.2f, 0.4f),
				ComplexityProbability = 0.2f,
				CompositionRanges = new Dictionary<string, (float min, float max)> {
					{ "Carbon Compounds", (0.5f, 0.7f) },
					{ "Silicates", (0.2f, 0.3f) },
					{ "Metals", (0.1f, 0.2f) }
				}
			}}
			// Asterian (asteroid) doesn't have rings by definition
		};

		public static class RingConstants
		{
			public const float BASE_RING_CHANCE = 0.1f;      // Base 10% chance for non-listed planet types
			public const float ROCHE_MULTIPLIER = 2.44f;     // Basic Roche limit multiplier
			public const float MIN_GAP_WIDTH = 0.01f;        // Minimum gap width as fraction of total ring width
			public const float MAX_GAP_WIDTH = 0.05f;        // Maximum gap width as fraction of total ring width
			public const int MIN_GAPS = 2;                   // Minimum number of gaps in complex systems
			public const int MAX_GAPS = 6;                   // Maximum number of gaps in complex systems
		}
	}
}