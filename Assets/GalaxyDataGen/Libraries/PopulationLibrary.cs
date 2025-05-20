using System.Collections.Generic;
using UnityEngine;

namespace Libraries
{
	[System.Serializable]
	public static class PopulationLibrary
	{
		[System.Serializable]
		public class PopulationSizeData
		{
			public long MinPopulation { get; set; }
			public long MaxPopulation { get; set; }
			public int TechLevelModifier { get; set; }
			public float InfrastructureRequirement { get; set; }
			public float UrbanizationTendency { get; set; }
		}

		[System.Serializable]
		public class PlanetaryCapacityFactors
		{
			public float BaseCapacity { get; set; } // Base population capacity per Earth radius
			public float WaterModifier { get; set; } // Multiplier based on water availability
			public float AtmosphereModifier { get; set; } // Multiplier based on atmosphere
			public float GravityModifier { get; set; } // Multiplier based on gravity
			public float TemperatureModifier { get; set; } // Multiplier based on temperature
			public float TechnologyMultiplier { get; set; } // How much tech can increase capacity
		}

		[System.Serializable]
		public class TechnologyLevelData
		{
			public float BaseGrowthRate { get; set; }
			public float HealthCareBonus { get; set; }
			public float EducationBonus { get; set; }
			public float EconomicProductivityModifier { get; set; }
			public bool SpaceCapable { get; set; }
			public bool InterstellarCapable { get; set; }
			public float ResourceConsumptionRate { get; set; }
			public float InfrastructureCapability { get; set; }
			public float CapacityTechMultiplier { get; set; } // How much this tech level increases capacity
		}

		[System.Serializable]
		public class GovernmentTypeData
		{
			public float StabilityModifier { get; set; }
			public float EconomicEfficiencyModifier { get; set; }
			public float ResearchSpeedModifier { get; set; }
			public float CorruptionTendency { get; set; }
			public float LawLevelTendency { get; set; }
			public string[] PossibleLeaderTitles { get; set; }
			public float TradeVolumeModifier { get; set; }
			public float CulturalDiversityModifier { get; set; }
			public float CapacityEfficiencyModifier { get; set; } // How efficiently this gov uses space
		}

		public static Dictionary<PopulationStructure.PopulationSize, PopulationSizeData> PopulationSizes = 
			new Dictionary<PopulationStructure.PopulationSize, PopulationSizeData>
		{
			{ PopulationStructure.PopulationSize.Outpost, new PopulationSizeData {
				MinPopulation = 10,
				MaxPopulation = 999,
				TechLevelModifier = 0,
				InfrastructureRequirement = 0.2f,
				UrbanizationTendency = 0.9f
			}},
			{ PopulationStructure.PopulationSize.Village, new PopulationSizeData {
				MinPopulation = 1000,
				MaxPopulation = 9999,
				TechLevelModifier = -1,
				InfrastructureRequirement = 0.3f,
				UrbanizationTendency = 0.1f
			}},
			{ PopulationStructure.PopulationSize.Town, new PopulationSizeData {
				MinPopulation = 10000,
				MaxPopulation = 99999,
				TechLevelModifier = 0,
				InfrastructureRequirement = 0.4f,
				UrbanizationTendency = 0.3f
			}},
			{ PopulationStructure.PopulationSize.City, new PopulationSizeData {
				MinPopulation = 100000,
				MaxPopulation = 999999,
				TechLevelModifier = 1,
				InfrastructureRequirement = 0.6f,
				UrbanizationTendency = 0.7f
			}},
			{ PopulationStructure.PopulationSize.Metropolis, new PopulationSizeData {
				MinPopulation = 1000000,
				MaxPopulation = 9999999,
				TechLevelModifier = 2,
				InfrastructureRequirement = 0.8f,
				UrbanizationTendency = 0.9f
			}},
			{ PopulationStructure.PopulationSize.Province, new PopulationSizeData {
				MinPopulation = 10000000,
				MaxPopulation = 99999999,
				TechLevelModifier = 1,
				InfrastructureRequirement = 0.7f,
				UrbanizationTendency = 0.5f
			}},
			{ PopulationStructure.PopulationSize.Region, new PopulationSizeData {
				MinPopulation = 100000000,
				MaxPopulation = 999999999,
				TechLevelModifier = 0,
				InfrastructureRequirement = 0.7f,
				UrbanizationTendency = 0.6f
			}},
			{ PopulationStructure.PopulationSize.Continental, new PopulationSizeData {
				MinPopulation = 1000000000,
				MaxPopulation = 9999999999,
				TechLevelModifier = 0,
				InfrastructureRequirement = 0.8f,
				UrbanizationTendency = 0.7f
			}},
			{ PopulationStructure.PopulationSize.GlobalPop, new PopulationSizeData {
				MinPopulation = 10000000000,
				MaxPopulation = 100000000000,
				TechLevelModifier = 1,
				InfrastructureRequirement = 0.9f,
				UrbanizationTendency = 0.8f
			}}};

		public static Dictionary<PopulationStructure.TechnologyLevel, TechnologyLevelData> TechnologyLevels = 
			new Dictionary<PopulationStructure.TechnologyLevel, TechnologyLevelData>
		{
			{ PopulationStructure.TechnologyLevel.PreIndustrial, new TechnologyLevelData {
				BaseGrowthRate = 0.01f,
				HealthCareBonus = 0.1f,
				EducationBonus = 0.1f,
				EconomicProductivityModifier = 0.2f,
				SpaceCapable = false,
				InterstellarCapable = false,
				ResourceConsumptionRate = 0.1f,
				InfrastructureCapability = 0.2f,
				CapacityTechMultiplier = 1.0f
			}},
			{ PopulationStructure.TechnologyLevel.Industrial, new TechnologyLevelData {
				BaseGrowthRate = 0.02f,
				HealthCareBonus = 0.3f,
				EducationBonus = 0.4f,
				EconomicProductivityModifier = 0.5f,
				SpaceCapable = false,
				InterstellarCapable = false,
				ResourceConsumptionRate = 0.5f,
				InfrastructureCapability = 0.5f,
				CapacityTechMultiplier = 1.5f
			}},
			{ PopulationStructure.TechnologyLevel.Atomic, new TechnologyLevelData {
				BaseGrowthRate = 0.015f,
				HealthCareBonus = 0.6f,
				EducationBonus = 0.7f,
				EconomicProductivityModifier = 1.0f,
				SpaceCapable = true,
				InterstellarCapable = false,
				ResourceConsumptionRate = 1.0f,
				InfrastructureCapability = 0.7f,
				CapacityTechMultiplier = 2.0f
			}},
			{ PopulationStructure.TechnologyLevel.Digital, new TechnologyLevelData {
				BaseGrowthRate = 0.01f,
				HealthCareBonus = 0.8f,
				EducationBonus = 0.9f,
				EconomicProductivityModifier = 1.5f,
				SpaceCapable = true,
				InterstellarCapable = false,
				ResourceConsumptionRate = 1.5f,
				InfrastructureCapability = 0.9f,
				CapacityTechMultiplier = 3.0f
			}},
			{ PopulationStructure.TechnologyLevel.InterPlanetary, new TechnologyLevelData {
				BaseGrowthRate = 0.015f,
				HealthCareBonus = 0.9f,
				EducationBonus = 0.95f,
				EconomicProductivityModifier = 2.0f,
				SpaceCapable = true,
				InterstellarCapable = false,
				ResourceConsumptionRate = 2.0f,
				InfrastructureCapability = 1.0f,
				CapacityTechMultiplier = 5.0f
			}},
			{ PopulationStructure.TechnologyLevel.InterStellar, new TechnologyLevelData {
				BaseGrowthRate = 0.02f,
				HealthCareBonus = 0.95f,
				EducationBonus = 0.98f,
				EconomicProductivityModifier = 3.0f,
				SpaceCapable = true,
				InterstellarCapable = true,
				ResourceConsumptionRate = 3.0f,
				InfrastructureCapability = 1.2f,
				CapacityTechMultiplier = 10.0f
			}},
			{ PopulationStructure.TechnologyLevel.PostSingular, new TechnologyLevelData {
				BaseGrowthRate = 0.005f,
				HealthCareBonus = 1.0f,
				EducationBonus = 1.0f,
				EconomicProductivityModifier = 5.0f,
				SpaceCapable = true,
				InterstellarCapable = true,
				ResourceConsumptionRate = 1.0f,
				InfrastructureCapability = 2.0f,
				CapacityTechMultiplier = 20.0f
			}}
		};

		public static Dictionary<PopulationStructure.GovernmentType, GovernmentTypeData> GovernmentTypes = 
			new Dictionary<PopulationStructure.GovernmentType, GovernmentTypeData>
		{
			{ PopulationStructure.GovernmentType.Anarchy, new GovernmentTypeData {
				StabilityModifier = -0.5f,
				EconomicEfficiencyModifier = 0.5f,
				ResearchSpeedModifier = 0.7f,
				CorruptionTendency = 0.2f,
				LawLevelTendency = 0.0f,
				PossibleLeaderTitles = new[] { "None", "Placeholder", "Figurehead" },
				TradeVolumeModifier = 0.7f,
				CulturalDiversityModifier = 1.2f,
				CapacityEfficiencyModifier = 0.7f
			}},
			{ PopulationStructure.GovernmentType.Tribalism, new GovernmentTypeData {
				StabilityModifier = 0.2f,
				EconomicEfficiencyModifier = 0.3f,
				ResearchSpeedModifier = 0.4f,
				CorruptionTendency = 0.4f,
				LawLevelTendency = 0.3f,
				PossibleLeaderTitles = new[] { "Chief", "Elder", "Shaman" },
				TradeVolumeModifier = 0.3f,
				CulturalDiversityModifier = 0.8f,
				CapacityEfficiencyModifier = 0.6f
			}},
			{ PopulationStructure.GovernmentType.Monarchy, new GovernmentTypeData {
				StabilityModifier = 0.6f,
				EconomicEfficiencyModifier = 0.7f,
				ResearchSpeedModifier = 0.8f,
				CorruptionTendency = 0.5f,
				LawLevelTendency = 0.7f,
				PossibleLeaderTitles = new[] { "King", "Queen", "Emperor", "Sultan", "Prince" },
				TradeVolumeModifier = 0.9f,
				CulturalDiversityModifier = 0.7f,
				CapacityEfficiencyModifier = 0.9f
			}},
			{ PopulationStructure.GovernmentType.Oligarchy, new GovernmentTypeData {
				StabilityModifier = 0.4f,
				EconomicEfficiencyModifier = 1.1f,
				ResearchSpeedModifier = 0.9f,
				CorruptionTendency = 0.7f,
				LawLevelTendency = 0.8f,
				PossibleLeaderTitles = new[] { "Chairman", "Council Chair", "Speaker" },
				TradeVolumeModifier = 1.3f,
				CulturalDiversityModifier = 0.6f,
				CapacityEfficiencyModifier = 0.8f
			}},
			{ PopulationStructure.GovernmentType.Republic, new GovernmentTypeData {
				StabilityModifier = 0.7f,
				EconomicEfficiencyModifier = 0.9f,
				ResearchSpeedModifier = 1.0f,
				CorruptionTendency = 0.4f,
				LawLevelTendency = 0.6f,
				PossibleLeaderTitles = new[] { "President", "Chancellor", "Premier" },
				TradeVolumeModifier = 1.1f,
				CulturalDiversityModifier = 0.9f,
				CapacityEfficiencyModifier = 1.0f
			}},
			{ PopulationStructure.GovernmentType.Democracy, new GovernmentTypeData {
				StabilityModifier = 0.8f,
				EconomicEfficiencyModifier = 1.0f,
				ResearchSpeedModifier = 1.2f,
				CorruptionTendency = 0.3f,
				LawLevelTendency = 0.5f,
				PossibleLeaderTitles = new[] { "Prime Minister", "President", "Premier" },
				TradeVolumeModifier = 1.2f,
				CulturalDiversityModifier = 1.1f,
				CapacityEfficiencyModifier = 1.1f
			}},
			{ PopulationStructure.GovernmentType.Theocracy, new GovernmentTypeData {
				StabilityModifier = 0.6f,
				EconomicEfficiencyModifier = 0.6f,
				ResearchSpeedModifier = 0.5f,
				CorruptionTendency = 0.6f,
				LawLevelTendency = 0.9f,
				PossibleLeaderTitles = new[] { "High Priest", "Prophet", "Holy One", "Patriarch" },
				TradeVolumeModifier = 0.8f,
				CulturalDiversityModifier = 0.3f,
				CapacityEfficiencyModifier = 0.8f
			}},
			{ PopulationStructure.GovernmentType.Corporatocracy, new GovernmentTypeData {
				StabilityModifier = 0.5f,
				EconomicEfficiencyModifier = 1.5f,
				ResearchSpeedModifier = 1.3f,
				CorruptionTendency = 0.8f,
				LawLevelTendency = 0.6f,
				PossibleLeaderTitles = new[] { "CEO", "Chairman", "Executive Director" },
				TradeVolumeModifier = 1.7f,
				CulturalDiversityModifier = 0.5f,
				CapacityEfficiencyModifier = 1.2f
			}},
			{ PopulationStructure.GovernmentType.TechnocraticMeritocracy, new GovernmentTypeData {
				StabilityModifier = 0.7f,
				EconomicEfficiencyModifier = 1.2f,
				ResearchSpeedModifier = 1.5f,
				CorruptionTendency = 0.2f,
				LawLevelTendency = 0.6f,
				PossibleLeaderTitles = new[] { "Chief Scientist", "Minister of Science", "Director General" },
				TradeVolumeModifier = 1.0f,
				CulturalDiversityModifier = 0.7f,
				CapacityEfficiencyModifier = 1.3f
			}},
			{ PopulationStructure.GovernmentType.HiveMind, new GovernmentTypeData {
				StabilityModifier = 1.0f,
				EconomicEfficiencyModifier = 1.8f,
				ResearchSpeedModifier = 1.4f,
				CorruptionTendency = 0.0f,
				LawLevelTendency = 1.0f,
				PossibleLeaderTitles = new[] { "Overmind", "Central Consciousness", "Collective" },
				TradeVolumeModifier = 0.5f,
				CulturalDiversityModifier = 0.0f,
				CapacityEfficiencyModifier = 2.0f
			}},
			{ PopulationStructure.GovernmentType.AIGovernance, new GovernmentTypeData {
				StabilityModifier = 0.9f,
				EconomicEfficiencyModifier = 1.6f,
				ResearchSpeedModifier = 2.0f,
				CorruptionTendency = 0.1f,
				LawLevelTendency = 0.8f,
				PossibleLeaderTitles = new[] { "Central AI", "Governing Intelligence", "Primary System" },
				TradeVolumeModifier = 1.4f,
				CulturalDiversityModifier = 0.6f,
				CapacityEfficiencyModifier = 1.5f
			}},
			{ PopulationStructure.GovernmentType.MilitaryDictatorship, new GovernmentTypeData {
				StabilityModifier = 0.4f,
				EconomicEfficiencyModifier = 0.7f,
				ResearchSpeedModifier = 0.8f,
				CorruptionTendency = 0.7f,
				LawLevelTendency = 0.9f,
				PossibleLeaderTitles = new[] { "General", "Supreme Commander", "Marshall" },
				TradeVolumeModifier = 0.6f,
				CulturalDiversityModifier = 0.4f,
				CapacityEfficiencyModifier = 0.9f
			}}
		};

		public static Dictionary<PopulationStructure.LawLevel, float> LawLevelStability = 
			new Dictionary<PopulationStructure.LawLevel, float>
		{
			{ PopulationStructure.LawLevel.NoLaw, -0.3f },
			{ PopulationStructure.LawLevel.Minimal, 0.1f },
			{ PopulationStructure.LawLevel.Moderate, 0.4f },
			{ PopulationStructure.LawLevel.Strict, 0.5f },
			{ PopulationStructure.LawLevel.Authoritarian, 0.3f },
			{ PopulationStructure.LawLevel.Totalitarian, 0.1f }
		};

		// Fixed: Changed dictionary key type from int to float
		public static Dictionary<float, PopulationStructure.PopulationSize> PopulationThresholds = 
			new Dictionary<float, PopulationStructure.PopulationSize>
		{
			{ 1000f, PopulationStructure.PopulationSize.Outpost },
			{ 10000f, PopulationStructure.PopulationSize.Village },
			{ 100000f, PopulationStructure.PopulationSize.Town },
			{ 1000000f, PopulationStructure.PopulationSize.City },
			{ 10000000f, PopulationStructure.PopulationSize.Metropolis },
			{ 100000000f, PopulationStructure.PopulationSize.Province },
			{ 1000000000f, PopulationStructure.PopulationSize.Region },
			{ 10000000000f, PopulationStructure.PopulationSize.Continental },
			{ 100000000000f, PopulationStructure.PopulationSize.GlobalPop }
		};

		public static readonly Dictionary<string, float> StandardAgeDistribution = 
			new Dictionary<string, float>
		{
			{ "Child (0-14)", 20f },
			{ "Young Adult (15-29)", 25f },
			{ "Adult (30-59)", 40f },
			{ "Senior (60+)", 15f }
		};

		public static readonly Dictionary<string, float> StandardEconomicSectors = 
			new Dictionary<string, float>
		{
			{ "Agriculture", 10f },
			{ "Industry", 30f },
			{ "Services", 50f },
			{ "Government", 10f }
		};

		public static readonly string[] CulturalTraitList = new string[]
		{
			"Xenophilic", "Xenophobic", "Militaristic", "Pacifist", "Materialistic", 
			"Spiritualistic", "Authoritarian", "Egalitarian", "Traditionalist", "Progressive",
			"Environmental", "Industrious", "Artistic", "Scientific", "Religious",
			"Mercantile", "Exploratory", "Isolationist", "Communal", "Individualistic"
		};

		public static readonly string[] MajorLanguageList = new string[]
		{
			"Standard", "Colonial", "Trade Pidgin", "Academic", "Military", "Religious",
			"Ancient", "Synthetic", "Sign", "Mathematical"
		};

		public static readonly string[] MajorReligionList = new string[]
		{
			"Secular Humanism", "Neo-Buddhism", "Cyber-Christianity", "Quantum Mysticism",
			"Gaia Worship", "Machine Cult", "Ancestor Veneration", "Cosmic Harmony",
			"Scientific Deism", "Universal Consciousness"
		};

		public static PlanetaryCapacityFactors DefaultCapacityFactors = new PlanetaryCapacityFactors
		{
			BaseCapacity = 5000000000f, // 5 billion for Earth-sized planet
			WaterModifier = 1.0f,
			AtmosphereModifier = 1.0f,
			GravityModifier = 1.0f,
			TemperatureModifier = 1.0f,
			TechnologyMultiplier = 1.0f
		};
	}
}