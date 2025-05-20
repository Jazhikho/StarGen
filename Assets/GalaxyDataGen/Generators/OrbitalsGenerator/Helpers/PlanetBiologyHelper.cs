using System;
using System.Collections.Generic;
using System.Linq;
using Libraries;

/// <summary>
/// Helper class for generating biological characteristics of planets.
/// Handles the generation and properties of potential life forms, biospheres,
/// and the possibility of intelligent life.
/// </summary>
namespace PlanetHelpers
{
public class PlanetBiologyHelper
{
	/// <summary>
	/// Generates biological 
	/// data for a planet based on its habitability and environmental conditions.
	/// </summary>
	/// <param name="habitabilityIndex">Planet's habitability rating (0-1)</param>
	/// <param name="biomeTypes">Array of biome types present on the planet</param>
	/// <param name="age">Age of the planet in billions of years</param>
	/// <param name="magneticFieldStrength">Strength of the planet's magnetic field (0-1)</param>
	/// <returns>BiosphereData containing biological characteristics or null if no life possible</returns>
	public BiologyLibrary.BiosphereData GenerateBiology(
		float habitabilityIndex,
		string[] biomeTypes,
		float age,
		float magneticFieldStrength)
	{
		if (habitabilityIndex < 0.1f)
		{
			return null; // No life possible
		}

		var biosphereData = new BiologyLibrary.BiosphereData
		{
			PresentLifeForms = new List<BiologyLibrary.LifeForm>()
		};

		// Calculate base biodiversity
		biosphereData.Biodiversity = CalculateBiodiversity(habitabilityIndex, biomeTypes);

		// Adjust for planetary age and magnetic field (protects from radiation)
		biosphereData.Biodiversity *= AdjustForAge(age);
		biosphereData.Biodiversity *= AdjustForMagneticField(magneticFieldStrength);

		// Determine present life forms
		DetermineLifeForms(biosphereData, habitabilityIndex);

		// Determine dominant life form
		biosphereData.DominantLifeForm = DetermineDominantLifeForm(biosphereData.PresentLifeForms, habitabilityIndex);

		// Generate special adaptations
		biosphereData.SpecialAdaptations = GenerateSpecialAdaptations(habitabilityIndex);

		// Determine civilization level (if any)
		DetermineCivilization(biosphereData, habitabilityIndex);

		return biosphereData;
	}

	/// <summary>
	/// Calculates biodiversity based on habitability and available biomes.
	/// Higher diversity indicates more complex and varied life forms.
	/// </summary>
	/// <param name="habitabilityIndex">Planet's habitability rating (0-1)</param>
	/// <param name="biomeTypes">Array of biome types present on the planet</param>
	/// <returns>Biodiversity rating (0-1)</returns>
	private float CalculateBiodiversity(float habitabilityIndex, string[] biomeTypes)
	{
		float biodiversity = habitabilityIndex;

		// Adjust based on biomes - more varied biomes support higher biodiversity
		float biomeModifier = 0f;
		foreach (string biomeType in biomeTypes)
		{
			if (Enum.TryParse<PlanetLibrary.BiomeType>(biomeType, out var biome) &&
				BiologyLibrary.BiodiversityModifiers.TryGetValue(biome, out float modifier))
			{
				biomeModifier += modifier;
			}
		}

		if (biomeTypes.Length > 0)
		{
			biomeModifier /= biomeTypes.Length;
		}

		biodiversity *= biomeModifier;
		return Math.Clamp(biodiversity, 0f, 1f);
	}

	/// <summary>
	/// Adjusts biodiversity based on planetary age. Life needs time to evolve,
	/// but very old planets might have declining biodiversity.
	/// </summary>
	/// <param name="age">Age in billions of years</param>
	/// <returns>Age modification factor (0-1)</returns>
	private float AdjustForAge(float age)
	{
		const float TOO_YOUNG_AGE = 0.5f;
		const float SIMPLE_LIFE_AGE = 1f;
		const float PRIME_LIFE_AGE = 3f;
		const float MATURE_AGE = 5f;
		
		const float TOO_YOUNG_FACTOR = 0.2f;
		const float SIMPLE_LIFE_FACTOR = 0.5f;
		const float PRIME_LIFE_FACTOR = 1f;
		const float MATURE_FACTOR = 0.7f;
		const float OLD_FACTOR = 0.4f;
		
		if (age < TOO_YOUNG_AGE) return TOO_YOUNG_FACTOR;
		if (age < SIMPLE_LIFE_AGE) return SIMPLE_LIFE_FACTOR;
		if (age < PRIME_LIFE_AGE) return PRIME_LIFE_FACTOR;
		if (age < MATURE_AGE) return MATURE_FACTOR;
		return OLD_FACTOR;
	}

	/// <summary>
	/// Adjusts biodiversity based on magnetic field strength.
	/// Stronger fields protect from harmful radiation, supporting more complex life.
	/// </summary>
	/// <param name="magneticFieldStrength">Magnetic field strength (0-1)</param>
	/// <returns>Magnetic field modification factor (0.5-1)</returns>
	private float AdjustForMagneticField(float magneticFieldStrength)
	{
		return 0.5f + (magneticFieldStrength * 0.5f);
	}

	/// <summary>
	/// Determines which life forms are present on the planet based on habitability.
	/// More complex life forms require higher habitability indices.
	/// </summary>
	/// <param name="biosphereData">BiosphereData to populate with life forms</param>
	/// <param name="habitabilityIndex">Planet's habitability rating (0-1)</param>
	private void DetermineLifeForms(BiologyLibrary.BiosphereData biosphereData, float habitabilityIndex)
	{
		var lifeForms = Enum.GetValues(typeof(BiologyLibrary.LifeForm)).Cast<BiologyLibrary.LifeForm>();

		foreach (var lifeForm in lifeForms)
		{
			// Different life forms have different habitability requirements
			float threshold = lifeForm switch
			{
				BiologyLibrary.LifeForm.Microbial => 0.1f,
				BiologyLibrary.LifeForm.Plant => 0.3f,
				BiologyLibrary.LifeForm.Aquatic => 0.4f,
				BiologyLibrary.LifeForm.Insectoid => 0.5f,
				BiologyLibrary.LifeForm.Reptilian => 0.6f,
				BiologyLibrary.LifeForm.Avian => 0.7f,
				BiologyLibrary.LifeForm.Mammalian => 0.8f,
				BiologyLibrary.LifeForm.Synthetic => 0.9f,
				_ => 1f
			};

			if (biosphereData.Biodiversity > threshold && Roll.ConditionalProbability(biosphereData.Biodiversity, 0))
			{
				biosphereData.PresentLifeForms.Add(lifeForm);
			}
		}
	}

	/// <summary>
	/// Determines the dominant life form based on present life forms and habitability.
	/// More complex life forms are more likely to dominate in highly habitable environments.
	/// </summary>
	/// <param name="presentLifeForms">List of present life forms</param>
	/// <param name="habitabilityIndex">Planet's habitability rating (0-1)</param>
	/// <returns>The dominant life form type</returns>
	private BiologyLibrary.LifeForm DetermineDominantLifeForm(List<BiologyLibrary.LifeForm> presentLifeForms, float habitabilityIndex)
	{
		if (presentLifeForms.Count == 0)
			return BiologyLibrary.LifeForm.Microbial;

		// Weight the life forms by complexity and randomness
		Dictionary<BiologyLibrary.LifeForm, float> weights = new Dictionary<BiologyLibrary.LifeForm, float>();
		foreach (var lifeForm in presentLifeForms)
		{
			weights[lifeForm] = lifeForm switch
			{
				BiologyLibrary.LifeForm.Synthetic => 10f * habitabilityIndex,
				BiologyLibrary.LifeForm.Mammalian => 8f * habitabilityIndex,
				BiologyLibrary.LifeForm.Avian => 7f * habitabilityIndex,
				BiologyLibrary.LifeForm.Reptilian => 6f * habitabilityIndex,
				BiologyLibrary.LifeForm.Insectoid => 5f * habitabilityIndex,
				BiologyLibrary.LifeForm.Aquatic => 4f * habitabilityIndex,
				BiologyLibrary.LifeForm.Plant => 3f * habitabilityIndex,
				_ => 1f * habitabilityIndex
			} * Roll.FindRange(0.5f, 1.5f);
		}

		return weights.OrderByDescending(w => w.Value).First().Key;
	}

	/// <summary>
	/// Generates special adaptations for life forms based on habitability.
	/// Higher habitability allows for more complex adaptations.
	/// </summary>
	/// <param name="habitabilityIndex">Planet's habitability rating (0-1)</param>
	/// <returns>Array of special adaptations</returns>
	private string[] GenerateSpecialAdaptations(float habitabilityIndex)
	{
		List<string> adaptations = new List<string>();
		
		foreach (var threshold in BiologyLibrary.SpecialAdaptations.Keys.OrderBy(k => k))
		{
			if (habitabilityIndex >= threshold && Roll.ConditionalProbability(0.3f, 0))
			{
				var possibleAdaptations = BiologyLibrary.SpecialAdaptations[threshold];
				var weights = new float[possibleAdaptations.Count];
				for (int i = 0; i < possibleAdaptations.Count; i++)
				{
					weights[i] = 1f; // Equal weight for each adaptation
				}
				
				string adaptation = Roll.Choice(possibleAdaptations.ToArray(), weights);
				if (!adaptations.Contains(adaptation))
					adaptations.Add(adaptation);
			}
		}

		return adaptations.ToArray();
	}

	/// <summary>
	/// Determines civilization level and technological advancement for intelligent life.
	/// Only certain life forms can develop civilization, and high habitability is required.
	/// </summary>
	/// <param name="biosphereData">BiosphereData to update with civilization information</param>
	/// <param name="habitabilityIndex">Planet's habitability rating (0-1)</param>
	private void DetermineCivilization(BiologyLibrary.BiosphereData biosphereData, float habitabilityIndex)
	{
		// Only certain life forms can develop civilization
		if (!new[] {
			BiologyLibrary.LifeForm.Mammalian,
			BiologyLibrary.LifeForm.Reptilian,
			BiologyLibrary.LifeForm.Avian,
			BiologyLibrary.LifeForm.Synthetic
		}.Contains(biosphereData.DominantLifeForm))
		{
			biosphereData.CivilizationLevel = BiologyLibrary.Civilization.None;
			biosphereData.TechnologicalLevel = 0f;
			return;
		}

		// Calculate civilization potential based on habitability and random factors
		float civPotential = habitabilityIndex * Roll.FindRange(0.5f, 1.5f);
		
		foreach (var threshold in BiologyLibrary.CivilizationThresholds.Keys.OrderByDescending(k => k))
		{
			if (civPotential >= threshold)
			{
				biosphereData.CivilizationLevel = BiologyLibrary.CivilizationThresholds[threshold];
				break;
			}
		}

		// Calculate technological level based on civilization level
		biosphereData.TechnologicalLevel = (float)biosphereData.CivilizationLevel / (float)BiologyLibrary.Civilization.PostSingularity;
	}
}
}
