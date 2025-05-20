using System.Collections.Generic;
using UnityEngine;

namespace Libraries
{
	[System.Serializable]
	public static class AsteroidLibrary
	{
		public enum AsteroidCompositionType
		{
			Carbonaceous,  // C-type: carbon-rich, most common
			Stony,         // S-type: silicate/nickel-iron mix
			Metallic,      // M-type: nickel-iron
			Basaltic,      // V-type: basaltic composition
			Icy,          // D-type: ice and organics
			Mixed         // Mix of various compositions
		}

		public class AsteroidTypeData
		{
			public (float min, float max) DensityRange { get; set; } // g/cm³
			public (float min, float max) AlbedoRange { get; set; }
			public Dictionary<PlanetLibrary.ResourceType, float> ResourceProbabilities { get; set; }
		}

		public static readonly Dictionary<AsteroidCompositionType, AsteroidTypeData> AsteroidData =
			new Dictionary<AsteroidCompositionType, AsteroidTypeData>
		{
			{ AsteroidCompositionType.Carbonaceous, new AsteroidTypeData {
				DensityRange = (1.38f, 2.2f),
				AlbedoRange = (0.03f, 0.1f),
				ResourceProbabilities = new Dictionary<PlanetLibrary.ResourceType, float> {
					{ PlanetLibrary.ResourceType.Water, 0.6f },
					{ PlanetLibrary.ResourceType.Organics, 0.8f },
					{ PlanetLibrary.ResourceType.Metals, 0.3f },
					{ PlanetLibrary.ResourceType.RareMetals, 0.1f }
				}
			}},
			{ AsteroidCompositionType.Stony, new AsteroidTypeData {
				DensityRange = (2.2f, 3.8f),
				AlbedoRange = (0.1f, 0.22f),
				ResourceProbabilities = new Dictionary<PlanetLibrary.ResourceType, float> {
					{ PlanetLibrary.ResourceType.Metals, 0.6f },
					{ PlanetLibrary.ResourceType.RareMetals, 0.2f },
					{ PlanetLibrary.ResourceType.Crystals, 0.4f },
					{ PlanetLibrary.ResourceType.Organics, 0.1f }
				}
			}},
			{ AsteroidCompositionType.Metallic, new AsteroidTypeData {
				DensityRange = (4.7f, 5.5f),
				AlbedoRange = (0.1f, 0.18f),
				ResourceProbabilities = new Dictionary<PlanetLibrary.ResourceType, float> {
					{ PlanetLibrary.ResourceType.Metals, 0.9f },
					{ PlanetLibrary.ResourceType.RareMetals, 0.5f },
					{ PlanetLibrary.ResourceType.Radioactives, 0.3f }
				}
			}},
			{ AsteroidCompositionType.Basaltic, new AsteroidTypeData {
				DensityRange = (2.8f, 3.5f),
				AlbedoRange = (0.2f, 0.4f),
				ResourceProbabilities = new Dictionary<PlanetLibrary.ResourceType, float> {
					{ PlanetLibrary.ResourceType.Metals, 0.5f },
					{ PlanetLibrary.ResourceType.Crystals, 0.7f },
					{ PlanetLibrary.ResourceType.RareMetals, 0.2f }
				}
			}},
			{ AsteroidCompositionType.Icy, new AsteroidTypeData {
				DensityRange = (0.8f, 1.5f),
				AlbedoRange = (0.02f, 0.07f),
				ResourceProbabilities = new Dictionary<PlanetLibrary.ResourceType, float> {
					{ PlanetLibrary.ResourceType.Water, 0.9f },
					{ PlanetLibrary.ResourceType.Gases, 0.6f },
					{ PlanetLibrary.ResourceType.Organics, 0.7f }
				}
			}},
			{ AsteroidCompositionType.Mixed, new AsteroidTypeData {
				DensityRange = (1.8f, 3.5f),
				AlbedoRange = (0.05f, 0.25f),
				ResourceProbabilities = new Dictionary<PlanetLibrary.ResourceType, float> {
					{ PlanetLibrary.ResourceType.Metals, 0.5f },
					{ PlanetLibrary.ResourceType.Water, 0.4f },
					{ PlanetLibrary.ResourceType.Organics, 0.4f },
					{ PlanetLibrary.ResourceType.RareMetals, 0.2f }
				}
			}}
		};

		public static readonly Dictionary<float, AsteroidCompositionType> CompositionDistribution =
			new Dictionary<float, AsteroidCompositionType>
		{
			{ 7500f, AsteroidCompositionType.Carbonaceous }, // 75% chance
			{ 9000f, AsteroidCompositionType.Stony },        // 15% chance
			{ 9500f, AsteroidCompositionType.Metallic },    // 5% chance
			{ 9700f, AsteroidCompositionType.Basaltic },    // 2% chance
			{ 9900f, AsteroidCompositionType.Icy },         // 2% chance
			{ 10000f, AsteroidCompositionType.Mixed }       // 1% chance
		};

		public static readonly (float min, float max) BeltDensityRange = (5000f, 15000f); // asteroids per cubic AU
		public static readonly (float min, float max) BeltThicknessRange = (0.1f, 0.5f); // AU
		public static readonly (float min, float max) AverageObjectSizeRange = (0.0001f, 1000f); // km
	}
}
