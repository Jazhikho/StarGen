using UnityEngine;
using System;

/// <summary>
/// Helper class for calculating galaxy structure and density modifications.
/// </summary>
public static class GalaxyStructureHelper
{
	/// <summary>
	/// Applies galaxy structure modifications to the star density based on position and galaxy type.
	/// </summary>
	/// <param name="sectorCoords">Relative coordinates of the sector from galactic center</param>
	/// <param name="parsecCoords">Coordinates of the parsec within the sector</param>
	/// <param name="baseDensity">Base star density to modify</param>
	/// <param name="galaxyType">Galaxy type (0=uniform, 1=spiral, 2=elliptical, 3=irregular)</param>
	/// <param name="sectorSizeX">Galaxy size in X dimension (sectors)</param>
	/// <param name="sectorSizeY">Galaxy size in Y dimension (sectors)</param>
	/// <param name="sectorSizeZ">Galaxy size in Z dimension (sectors)</param>
	/// <param name="parsecsPerSector">Number of parsecs per sector dimension</param>
	/// <returns>Modified star density</returns>
	public static float ApplyGalaxyStructure(
		Vector3Int sectorCoords, 
		Vector3 parsecCoords, 
		float baseDensity,
		int galaxyType,
		int sectorSizeX,
		int sectorSizeY,
		int sectorSizeZ,
		int parsecsPerSector)
	{
		// Default is uniform distribution (no modification)
		if (galaxyType == 0)
			return baseDensity;
			
		// Get global coordinates in parsecs from galactic center
		Vector3 globalPos = new Vector3(
			sectorCoords.x * parsecsPerSector + parsecCoords.x,
			sectorCoords.y * parsecsPerSector + parsecCoords.y,
			sectorCoords.z * parsecsPerSector + parsecCoords.z
		);
		
		// Distance from galaxy center on the XY plane and in 3D
		float distanceXY = (float)Math.Sqrt(globalPos.x * globalPos.x + globalPos.y * globalPos.y);
		float distance3D = globalPos.magnitude;
		
		// Maximum distance from center to edge of the galaxy (in parsecs)
		float maxDistance = (float)Math.Sqrt(Math.Pow(sectorSizeX * parsecsPerSector / 2, 2) + 
										   Math.Pow(sectorSizeY * parsecsPerSector / 2, 2) + 
										   Math.Pow(sectorSizeZ * parsecsPerSector / 2, 2));
		
		// Normalized distance (0 at center, 1 at max distance)
		float normalizedDistance = distance3D / maxDistance;
		
		switch (galaxyType)
		{
			case 1: // Spiral galaxy
				// Calculate angle in the XY plane
				float angle = (float)Math.Atan2(globalPos.y, globalPos.x);
				
				// Spiral arm factor (creates density waves)
				float armFactor = (float)Math.Sin(angle * 4 + distanceXY * 0.1f);
				
				// Higher density in spiral arms and galactic core
				float spiral = 1.0f + 0.5f * (armFactor > 0 ? armFactor : 0) - 0.5f * normalizedDistance;
				
				// Height falloff (thinner at edges)
				float heightFactor = 1.0f - 0.8f * Math.Abs(globalPos.z) / (sectorSizeZ * parsecsPerSector / 4.0f);
				
				return baseDensity * Math.Max(0.1f, spiral * heightFactor);
				
			case 2: // Elliptical galaxy
				// Density decreases from center based on 3D distance
				float elliptical = 1.0f - 0.9f * normalizedDistance;
				return baseDensity * Math.Max(0.1f, elliptical);
				
			case 3: // Irregular galaxy
				// Random density pockets
				float irregFactor = (float)Math.Sin(globalPos.x * 0.1f) * (float)Math.Cos(globalPos.y * 0.11f) * (float)Math.Sin(globalPos.z * 0.13f);
				float irregular = 1.0f + 0.8f * irregFactor - 0.6f * normalizedDistance;
				return baseDensity * Math.Max(0.1f, irregular);
				
			default:
				return baseDensity;
		}
	}
} 
