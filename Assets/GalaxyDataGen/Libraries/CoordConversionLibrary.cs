using UnityEngine;
using System;

namespace Libraries
{
	[System.Serializable]
	public static class CoordConversion
	{
		private const float ParsecsPerSector = 10f;
		private const float LightYearsPerParsec = 3.26f;
		private const float SectorSizeLY = ParsecsPerSector * LightYearsPerParsec;

		/// <summary>
		/// Converts spherical galactic coordinates to a Cartesian position in light-years.
		/// </summary>
		public static Vector3 SphericalToCartesian(float r, float thetaDeg, float phiDeg)
		{
			float theta = thetaDeg * Mathf.Deg2Rad;
			float phi = phiDeg * Mathf.Deg2Rad;

			float x = r * Mathf.Cos(phi) * Mathf.Cos(theta);
			float y = r * Mathf.Cos(phi) * Mathf.Sin(theta);
			float z = r * Mathf.Sin(phi);

			return new Vector3(x, y, z);
		}

		/// <summary>
		/// Converts a Cartesian position (in light-years) to sector coordinates.
		/// </summary>
		public static Vector3Int CartesianToSectorCoords(Vector3 positionLY)
		{
			return new Vector3Int(
				Mathf.FloorToInt(positionLY.x / SectorSizeLY),
				Mathf.FloorToInt(positionLY.y / SectorSizeLY),
				Mathf.FloorToInt(positionLY.z / SectorSizeLY)
			);
		}

		/// <summary>
		/// Computes spherical coordinates (r, θ, φ) from a Cartesian position in light-years.
		/// θ is the azimuthal angle (longitude) in the x-y plane from the x-axis with range [0,360).
		/// φ is the polar angle (latitude) from the x-y plane with range [-90,90].
		/// </summary>
		public static (float r, float thetaDeg, float phiDeg) CartesianToSpherical(Vector3 position)
		{
			// Calculate distance from origin (r)
			float r = position.magnitude;
			
			// Avoid division by zero for points at the origin
			if (r < 0.00001f)
				return (0, 0, 0);
			
			// Calculate azimuthal angle (θ) in the x-y plane (0° at positive x-axis)
			float thetaRad = Mathf.Atan2(position.y, position.x);
			float thetaDeg = thetaRad * Mathf.Rad2Deg;
			
			// Convert to 0-360 range
			if (thetaDeg < 0)
				thetaDeg += 360.0f;
			
			// Calculate polar angle (φ) from the x-y plane (0° at x-y plane)
			float xyDistance = Mathf.Sqrt(position.x * position.x + position.y * position.y);
			float phiRad = Mathf.Atan2(position.z, xyDistance);
			float phiDeg = phiRad * Mathf.Rad2Deg;

			return (r, thetaDeg, phiDeg);
		}

		/// <summary>
		/// Gets the Cartesian position of the center of a sector.
		/// </summary>
		public static Vector3 SectorCenterPosition(Vector3Int coords)
		{
			return new Vector3(
				(coords.x + 0.5f) * SectorSizeLY,
				(coords.y + 0.5f) * SectorSizeLY,
				(coords.z + 0.5f) * SectorSizeLY
			);
		}
	}
}
