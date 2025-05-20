using UnityEngine;
using System;
using System.Collections.Generic;
using Libraries;

[Serializable]
public class PopulationStructure
{
	public enum PopulationSize
	{
		Outpost,        // < 1,000
		Village,        // < 10,000
		Town,           // < 100,000
		City,           // < 1,000,000
		Metropolis,     // < 10,000,000
		Province,       // < 100,000,000
		Region,         // < 1,000,000,000
		Continental,    // < 10,000,000,000
		GlobalPop      // > 10,000,000,000
	}

	public enum TechnologyLevel
	{
		PreIndustrial,  // Agriculture, basic tools
		Industrial,     // Steam power, basic manufacturing
		Atomic,         // Nuclear power, early computers
		Digital,        // Information age, advanced computing
		InterPlanetary, // Can travel within star system
		InterStellar,   // Can travel between stars
		PostSingular    // Advanced AI, transhumanism
	}

	public enum GovernmentType
	{
		Anarchy,
		Tribalism,
		Monarchy,
		Oligarchy,
		Republic,
		Democracy,
		Theocracy,
		Corporatocracy,
		TechnocraticMeritocracy,
		HiveMind,
		AIGovernance,
		MilitaryDictatorship
	}

	public enum LawLevel
	{
		NoLaw,          // Complete anarchy
		Minimal,        // Basic protection laws only
		Moderate,       // Standard legal framework
		Strict,         // Highly regulated
		Authoritarian,  // Heavy restrictions
		Totalitarian    // Complete control
	}

	public enum EconomicSystem
	{
		Subsistence,
		Barter,
		Mercantile,
		Capitalism,
		Socialism,
		Communism,
		PostScarcity,
		ResourceBased
	}

	public enum CulturalDiversity
	{
		Monoculture,
		LowDiversity,
		ModerateDiversity,
		HighDiversity,
		GlobalMelting
	}

	public enum ConflictLevel
	{
		Peace,
		CivilUnrest,
		LocalConflicts,
		RegionalWars,
		GlobalConflict,
		Apocalyptic
	}

	[Serializable]
	public class Population
	{
		public string ID;
		public long PopulationCount;
		public PopulationSize PopulationCategory;
		public TechnologyLevel TechLevel;
		public GovernmentType Government;
		public LawLevel Law;
		public EconomicSystem Economy;
		public CulturalDiversity Culture;
		public ConflictLevel Conflict;

		// Capacity properties
		public long PlanetaryCapacity;
		public float CapacityUsage; // 0 to 1 (current population / capacity)
		public bool ExceedingCapacity;

		// Demographics - Serializable entries for dictionaries
		[Serializable]
		public class AgeDistributionEntry
		{
			public string AgeGroup;
			public float Percentage;
			
			public AgeDistributionEntry(string ageGroup, float percentage)
			{
				AgeGroup = ageGroup;
				Percentage = percentage;
			}
		}
		
		[SerializeField]
		private List<AgeDistributionEntry> _ageDistributionList = new List<AgeDistributionEntry>();
		
		[NonSerialized]
		private Dictionary<string, float> _ageDistribution = new Dictionary<string, float>();
		
		public float GrowthRate;
		public float BirthRate;
		public float DeathRate;
		public float MigrationRate;

		// Infrastructure
		public float UrbanizationLevel; // 0 to 1
		public float InfrastructureQuality; // 0 to 1
		public float EducationLevel; // 0 to 1
		public float HealthcareLevel; // 0 to 1

		// Economy
		public float EconomicOutput;
		public float ResourceConsumption;
		
		[Serializable]
		public class EconomicSectorEntry
		{
			public string Sector;
			public float Percentage;
			
			public EconomicSectorEntry(string sector, float percentage)
			{
				Sector = sector;
				Percentage = percentage;
			}
		}
		
		[SerializeField]
		private List<EconomicSectorEntry> _economicSectorsList = new List<EconomicSectorEntry>();
		
		[NonSerialized]
		private Dictionary<string, float> _economicSectors = new Dictionary<string, float>();
		
		public float TradeVolume;

		// Political
		public string LeaderTitle;
		
		[SerializeField]
		public List<string> MajorFactions = new List<string>();
		
		[Serializable]
		public class PoliticalStabilityEntry
		{
			public string Factor;
			public float Value;
			
			public PoliticalStabilityEntry(string factor, float value)
			{
				Factor = factor;
				Value = value;
			}
		}
		
		[SerializeField]
		private List<PoliticalStabilityEntry> _politicalStabilityList = new List<PoliticalStabilityEntry>();
		
		[NonSerialized]
		private Dictionary<string, float> _politicalStability = new Dictionary<string, float>();
		
		public float CorruptionLevel; // 0 to 1

		// Culture
		[SerializeField]
		public List<string> MajorLanguages = new List<string>();
		
		[SerializeField]
		public List<string> MajorReligions = new List<string>();
		
		public float CulturalUnity; // 0 to 1
		
		[SerializeField]
		public List<string> CulturalTraits = new List<string>();

		// Space Capability
		public bool SpaceCapable;
		public int ColoniesCount;
		public float SpaceInfrastructure; // 0 to 1
		public bool InterstellarCapable;
		public float SpaceCapabilityLevel;

		// Parent Reference
		public string ParentBodyID;
		public string ParentSystemID;

		public Population()
		{
			ID = Guid.NewGuid().ToString();
			_ageDistributionList = new List<AgeDistributionEntry>();
			_economicSectorsList = new List<EconomicSectorEntry>();
			MajorFactions = new List<string>();
			_politicalStabilityList = new List<PoliticalStabilityEntry>();
			MajorLanguages = new List<string>();
			MajorReligions = new List<string>();
			CulturalTraits = new List<string>();
			
			_ageDistribution = new Dictionary<string, float>();
			_economicSectors = new Dictionary<string, float>();
			_politicalStability = new Dictionary<string, float>();
		}
		
		#region Age Distribution Methods
		
		public void AddAgeDistribution(string ageGroup, float percentage)
		{
			_ageDistribution[ageGroup] = percentage;
			
			// Update the serializable list
			var existing = _ageDistributionList.Find(a => a.AgeGroup == ageGroup);
			if (existing != null)
			{
				existing.Percentage = percentage;
			}
			else
			{
				_ageDistributionList.Add(new AgeDistributionEntry(ageGroup, percentage));
			}
		}
		
		public Dictionary<string, float> GetAgeDistribution()
		{
			RebuildAgeDistributionDictionary();
			return _ageDistribution;
		}
		
		private void RebuildAgeDistributionDictionary()
		{
			_ageDistribution.Clear();
			foreach (var entry in _ageDistributionList)
			{
				_ageDistribution[entry.AgeGroup] = entry.Percentage;
			}
		}
		
		#endregion
		
		#region Economic Sectors Methods
		
		public void AddEconomicSector(string sector, float percentage)
		{
			_economicSectors[sector] = percentage;
			
			// Update the serializable list
			var existing = _economicSectorsList.Find(e => e.Sector == sector);
			if (existing != null)
			{
				existing.Percentage = percentage;
			}
			else
			{
				_economicSectorsList.Add(new EconomicSectorEntry(sector, percentage));
			}
		}
		
		public Dictionary<string, float> GetEconomicSectors()
		{
			RebuildEconomicSectorsDictionary();
			return _economicSectors;
		}
		
		private void RebuildEconomicSectorsDictionary()
		{
			_economicSectors.Clear();
			foreach (var entry in _economicSectorsList)
			{
				_economicSectors[entry.Sector] = entry.Percentage;
			}
		}
		
		#endregion
		
		#region Political Stability Methods
		
		public void AddPoliticalStabilityFactor(string factor, float value)
		{
			_politicalStability[factor] = value;
			
			// Update the serializable list
			var existing = _politicalStabilityList.Find(p => p.Factor == factor);
			if (existing != null)
			{
				existing.Value = value;
			}
			else
			{
				_politicalStabilityList.Add(new PoliticalStabilityEntry(factor, value));
			}
		}
		
		public Dictionary<string, float> GetPoliticalStability()
		{
			RebuildPoliticalStabilityDictionary();
			return _politicalStability;
		}
		
		private void RebuildPoliticalStabilityDictionary()
		{
			_politicalStability.Clear();
			foreach (var entry in _politicalStabilityList)
			{
				_politicalStability[entry.Factor] = entry.Value;
			}
		}
		
		#endregion
	}
}