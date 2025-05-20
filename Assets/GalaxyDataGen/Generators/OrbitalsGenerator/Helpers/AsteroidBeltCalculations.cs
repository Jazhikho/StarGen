using System;
using UnityEngine;

namespace Libraries
{
    /// <summary>
    /// Provides functions for calculating asteroid belt properties and dynamics.
    /// </summary>
    public static class AsteroidBeltCalculations
    {
        /// <summary>
        /// Calculates the volume of an asteroid belt in cubic AU.
        /// </summary>
        /// <param name="innerRadius">Inner radius in AU</param>
        /// <param name="outerRadius">Outer radius in AU</param>
        /// <param name="thickness">Thickness in AU</param>
        /// <returns>Volume in cubic AU</returns>
        public static float CalculateBeltVolume(float innerRadius, float outerRadius, float thickness)
        {
            return (float)(Math.PI * (Math.Pow(outerRadius, 2) - Math.Pow(innerRadius, 2)) * thickness);
        }

        /// <summary>
        /// Calculates the total mass of an asteroid belt in Earth masses.
        /// </summary>
        /// <param name="density">Average density in kg/m³</param>
        /// <param name="volume">Volume in cubic AU</param>
        /// <param name="avgObjectSize">Average object size in meters</param>
        /// <returns>Total mass in Earth masses</returns>
        public static float CalculateBeltMass(float density, float volume, float avgObjectSize)
        {
            float volumeInM3 = volume * (float)Math.Pow(Utils.AU_TO_METERS, 3);
            float totalMass = density * volumeInM3;
            return totalMass / Utils.EARTH_MASS;
        }

        /// <summary>
        /// Calculates the orbital velocity of asteroids in the belt in km/s.
        /// </summary>
        /// <param name="innerRadius">Inner radius in AU</param>
        /// <param name="outerRadius">Outer radius in AU</param>
        /// <param name="stellarMass">Mass of the central star in solar masses</param>
        /// <returns>Average orbital velocity in km/s</returns>
        public static float AsteroidBeltVelocity(float innerRadius, float outerRadius, float stellarMass)
        {
            float avgRadius = (innerRadius + outerRadius) / 2;
            return Utils.OrbitalVelocity(avgRadius, stellarMass);
        }

        /// <summary>
        /// Calculates the frequency of collisions in the asteroid belt.
        /// </summary>
        /// <param name="density">Number density of asteroids per cubic AU</param>
        /// <param name="avgRadius">Average radius of asteroids in meters</param>
        /// <param name="relativeVelocity">Relative velocity between asteroids in km/s</param>
        /// <returns>Collision frequency per asteroid per year</returns>
        public static float CollisionFrequency(float density, float avgRadius, float relativeVelocity)
        {
            float crossSection = (float)(Math.PI * Math.Pow(avgRadius * 2, 2));
            float velocityInMS = relativeVelocity * 1000f; // Convert km/s to m/s
            float volumePerYear = velocityInMS * crossSection * (Utils.SECONDS_PER_DAY * 365.25f);
            float volumeInAU = volumePerYear / (float)Math.Pow(Utils.AU_TO_METERS, 3);
            return density * volumeInAU;
        }
    }
} 