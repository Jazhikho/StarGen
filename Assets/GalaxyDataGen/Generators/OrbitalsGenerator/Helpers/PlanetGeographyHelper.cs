using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using Libraries;

public class PlanetGeographyHelper
{
	/// <summary>
	/// Determines the biomes present on a planet based on its physical characteristics.
	/// </summary>
	/// <param name="planetType">Type classification of the planet</param>
	/// <param name="waterCoverage">Fraction of surface covered by liquid water (0-1)</param>
	/// <param name="surfaceTemperature">Average surface temperature in Kelvin</param>
	/// <param name="atmosphericPressure">Atmospheric pressure in Earth atmospheres</param>
	/// <returns>Array of biome type names present on the planet</returns>
	public string[] DetermineBiomes(
		PlanetLibrary.PlanetType planetType,
		float waterCoverage,
		float surfaceTemperature,
		float atmosphericPressure)
	{
		// Get possible biomes for this planet type from the library data
		var possibleBiomes = new List<PlanetLibrary.BiomeType>(
			PlanetLibrary.PlanetData[planetType].PossibleBiomes);

		// Early return for no biomes
		if (possibleBiomes.Count == 0)
			return Array.Empty<string>();

		// Filter based on water coverage
		if (waterCoverage < 0.1f)
		{
			possibleBiomes.RemoveAll(b => b == PlanetLibrary.BiomeType.DenseForest ||
										b == PlanetLibrary.BiomeType.Rainforest ||
										b == PlanetLibrary.BiomeType.Wetlands ||
										b == PlanetLibrary.BiomeType.IceSheet ||
										b == PlanetLibrary.BiomeType.CoastalZone ||
										b == PlanetLibrary.BiomeType.CoralReef ||
										b == PlanetLibrary.BiomeType.PelagicZone ||
										b == PlanetLibrary.BiomeType.AbyssalPlain ||
										b == PlanetLibrary.BiomeType.HydrothermalFields ||
										b == PlanetLibrary.BiomeType.CryovolcanicFields);
		}

		// Filter based on temperature ranges
		if (surfaceTemperature < 273f) // Below freezing
		{
			possibleBiomes.RemoveAll(b => b == PlanetLibrary.BiomeType.Rainforest ||
										b == PlanetLibrary.BiomeType.Desert ||
										b == PlanetLibrary.BiomeType.Grassland ||
										b == PlanetLibrary.BiomeType.Wetlands ||
										b == PlanetLibrary.BiomeType.FreshwaterLake ||
										b == PlanetLibrary.BiomeType.BrackishLake ||
										b == PlanetLibrary.BiomeType.SaltWaterRiver ||
										b == PlanetLibrary.BiomeType.FreshwaterRiver ||
										b == PlanetLibrary.BiomeType.LavaFields ||
										b == PlanetLibrary.BiomeType.VolcanicPlateau ||
										b == PlanetLibrary.BiomeType.AcidSpringFields);
		}
		else if (surfaceTemperature > 373f) // Above boiling
		{
			possibleBiomes.RemoveAll(b => b == PlanetLibrary.BiomeType.IceSheet ||
										b == PlanetLibrary.BiomeType.CryovolcanicFields ||
										b == PlanetLibrary.BiomeType.CoastalZone ||
										b == PlanetLibrary.BiomeType.CoralReef ||
										b == PlanetLibrary.BiomeType.PelagicZone ||
										b == PlanetLibrary.BiomeType.FreshwaterLake ||
										b == PlanetLibrary.BiomeType.Rainforest ||
										b == PlanetLibrary.BiomeType.DenseForest ||
										b == PlanetLibrary.BiomeType.Wetlands ||
										b == PlanetLibrary.BiomeType.Tundra ||
										b == PlanetLibrary.BiomeType.GlacialPlains);
		}
		// Filter based on atmospheric pressure
		if (atmosphericPressure < 0.1f)
		{
			possibleBiomes.RemoveAll(b => b == PlanetLibrary.BiomeType.Rainforest ||
										b == PlanetLibrary.BiomeType.DenseForest ||
										b == PlanetLibrary.BiomeType.Wetlands ||
										b == PlanetLibrary.BiomeType.Grassland ||
										b == PlanetLibrary.BiomeType.Woodland);
		}

		return possibleBiomes.ConvertAll(b => b.ToString()).ToArray();
	}
}