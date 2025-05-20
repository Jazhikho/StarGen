using System.Collections.Generic;
using UnityEngine;

namespace Libraries
{
	[System.Serializable]
	public static class BiologyLibrary
	{
		public enum LifeForm
		{
			Microbial,
			Plant,
			Aquatic,
			Insectoid,
			Reptilian,
			Avian,
			Mammalian,
			Synthetic
		}

		public enum Civilization
		{
			None,
			Primitive,
			Industrial,
			Spacefaring,
			PostSingularity
		}

		public class BiosphereData
		{
			public float Biodiversity { get; set; }
			public LifeForm DominantLifeForm { get; set; }
			public List<LifeForm> PresentLifeForms { get; set; }
			public Civilization CivilizationLevel { get; set; }
			public float TechnologicalLevel { get; set; }
			public string[] SpecialAdaptations { get; set; }
		}

		public static readonly Dictionary<PlanetLibrary.BiomeType, float> BiodiversityModifiers = 
			new Dictionary<PlanetLibrary.BiomeType, float>
		{
			// Terrestrial Biomes
			{ PlanetLibrary.BiomeType.RockyPlains, 0.3f },
			{ PlanetLibrary.BiomeType.HighPlains, 0.5f },
			{ PlanetLibrary.BiomeType.LowMountains, 0.4f },
			{ PlanetLibrary.BiomeType.HighMountains, 0.2f },
			{ PlanetLibrary.BiomeType.Desert, 0.3f },
			{ PlanetLibrary.BiomeType.Grassland, 0.7f },
			{ PlanetLibrary.BiomeType.Woodland, 0.9f },
			{ PlanetLibrary.BiomeType.DenseForest, 1.1f },
			{ PlanetLibrary.BiomeType.Rainforest, 1.5f },
			{ PlanetLibrary.BiomeType.Wetlands, 1.3f },
			
			// Polar Biomes
			{ PlanetLibrary.BiomeType.Tundra, 0.4f },
			{ PlanetLibrary.BiomeType.IceSheet, 0.2f },
			{ PlanetLibrary.BiomeType.GlacialPlains, 0.3f },
			
			// Oceanic Biomes
			{ PlanetLibrary.BiomeType.CoastalZone, 1.2f },
			{ PlanetLibrary.BiomeType.CoralReef, 1.4f },
			{ PlanetLibrary.BiomeType.PelagicZone, 1.0f },
			{ PlanetLibrary.BiomeType.AbyssalPlain, 0.5f },
			{ PlanetLibrary.BiomeType.HydrothermalFields, 0.7f },
			
			// Other Aquatic Biomes
			{ PlanetLibrary.BiomeType.FreshwaterLake, 1.1f },
			{ PlanetLibrary.BiomeType.BrackishLake, 0.9f },
			{ PlanetLibrary.BiomeType.FreshwaterRiver, 1.0f },
			{ PlanetLibrary.BiomeType.SaltWaterRiver, 0.8f },
			
			// Volcanic & Geothermal
			{ PlanetLibrary.BiomeType.LavaFields, 0.1f },
			{ PlanetLibrary.BiomeType.VolcanicPlateau, 0.2f },
			{ PlanetLibrary.BiomeType.AcidSpringFields, 0.3f },
			{ PlanetLibrary.BiomeType.CryovolcanicFields, 0.2f },
			
			// Gas & Ice Giant Layers
			{ PlanetLibrary.BiomeType.CloudBands, 0.05f },
			{ PlanetLibrary.BiomeType.ExoticCloudDeck, 0.1f },
			{ PlanetLibrary.BiomeType.MethaneLakeRegion, 0.2f },
			{ PlanetLibrary.BiomeType.AmmoniaCloudLayer, 0.1f },
			
			// Small-Body Surfaces
			{ PlanetLibrary.BiomeType.RegolithPlains, 0.05f },
			{ PlanetLibrary.BiomeType.MetallicCrust, 0.01f },
			{ PlanetLibrary.BiomeType.CrystallineVeins, 0.2f }
		};

		public static readonly Dictionary<float, List<string>> SpecialAdaptations =
			new Dictionary<float, List<string>>
		{
			{ 0.2f, new List<string> { 
				"Extremophile", 
				"Chemosynthetic",
				"Radiation-resistant",
				"Vacuum-tolerant",
				"Pressure-adapted"
			}},
			{ 0.4f, new List<string> { 
				"Bioluminescent", 
				"Silicon-based", 
				"Hivemind",
				"Sulfur-metabolizing",
				"Cryostasis-capable",
				"High-gravity adapted"
			}},
			{ 0.6f, new List<string> { 
				"Regenerative", 
				"Telepathic", 
				"Metamorphic",
				"Electromagnetic-sensitive",
				"Multi-organism symbiosis",
				"Atmospheric-filtration"
			}},
			{ 0.8f, new List<string> { 
				"Symbiotic", 
				"Nanite-infused", 
				"Phase-shifting",
				"Quantum-entangled",
				"Collective consciousness",
				"Radiotrophs"
			}},
			{ 1.0f, new List<string> { 
				"Energy-manipulating", 
				"Dimensional", 
				"Transcendent",
				"Quantum computing biology",
				"Reality-altering",
				"Post-physical"
			}}
		};

		public static readonly Dictionary<float, Civilization> CivilizationThresholds =
			new Dictionary<float, Civilization>
		{
			{ 0.0f, Civilization.None },
			{ 0.5f, Civilization.Primitive },
			{ 0.7f, Civilization.Industrial },
			{ 0.9f, Civilization.Spacefaring },
			{ 0.95f, Civilization.PostSingularity }
		};
	}
}