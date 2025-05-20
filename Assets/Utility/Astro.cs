using System;
using UnityEngine;
using System.Collections.Generic;

namespace Libraries
{
    /// <summary>
    /// Provides consolidated astronomical utility functions for planetary, atmospheric,
    /// and temperature calculations.
    /// </summary>
    public static class Astro
    {
        #region Atmospheric Calculations

        /// <summary>
        /// Determines if a planet can retain a specific gas at its current temperature.
        /// </summary>
        /// <param name="gravity">Surface gravity in Earth g's</param>
        /// <param name="temperature">Temperature in Kelvin</param>
        /// <param name="molecularWeight">Molecular weight of the gas</param>
        /// <returns>True if the gas can be retained</returns>
        public static bool CanRetainGas(float gravity, float temperature, float molecularWeight)
        {
            // Jeans escape parameter
            float escapeParameter = molecularWeight * gravity * Utils.EARTH_GRAVITY *
                                   Utils.EARTH_RADIUS / (Utils.UNIVERSAL_GAS_CONSTANT * temperature);

            // Higher escape parameter means better retention
            return escapeParameter > 10f;
        }

        /// <summary>
        /// Calculates base atmospheric pressure for a planet.
        /// </summary>
        /// <param name="mass">Planet mass in Earth masses</param>
        /// <param name="gravity">Surface gravity in Earth g's</param>
        /// <param name="temperature">Temperature in Kelvin</param>
        /// <param name="volatileContent">Volatile content modifier (0-1)</param>
        /// <returns>Atmospheric pressure in Earth atmospheres</returns>
        public static float AtmosphericPressure(float mass, float gravity, float temperature, float volatileContent)
        {
            // Base pressure scales with mass and gravity
            float basePressure = mass * gravity * volatileContent;

            // Temperature affects pressure (higher temperature = higher pressure)
            float tempFactor = temperature / 288f; // 288K is Earth's average temperature

            return basePressure * tempFactor;
        }

        /// <summary>
        /// Calculates atmospheric greenhouse effect.
        /// </summary>
        /// <param name="pressure">Pressure in Earth atmospheres</param>
        /// <param name="composition">Atmospheric composition</param>
        /// <returns>Greenhouse effect factor (Earth = 1.0)</returns>
        public static float GreenhouseEffect(float pressure, Dictionary<string, float> composition)
        {
            float greenhouseFactor = 1.0f;

            // CO2 is a strong greenhouse gas
            if (composition.ContainsKey("CO2"))
                greenhouseFactor += composition["CO2"] * 2.0f;

            // H2O is also a strong greenhouse gas
            if (composition.ContainsKey("H2O"))
                greenhouseFactor += composition["H2O"] * 1.5f;

            // Pressure affects greenhouse effect
            greenhouseFactor *= (float)Math.Pow(pressure, 0.5f);

            return greenhouseFactor;
        }
        #endregion

        #region Temperature Calculations

        /// <summary>
        /// Calculates blackbody temperature in Kelvin.
        /// </summary>
        /// <param name="stellarLuminosityInSolar">Stellar luminosity in solar units</param>
        /// <param name="albedo">Surface albedo (reflection coefficient)</param>
        /// <param name="distanceInAU">Distance from star in AU</param>
        /// <returns>Blackbody temperature in Kelvin</returns>
        public static float BlackbodyTemperature(float stellarLuminosityInSolar, float albedo, float distanceInAU)
        {
            float earthTemp = 255f; // K, Earth's blackbody temp
            return earthTemp * (float)Math.Pow(stellarLuminosityInSolar / (distanceInAU * distanceInAU), 0.25)
                            * (float)Math.Pow((1 - albedo) / (1 - Utils.EARTH_ALBEDO), 0.25);
        }

        /// <summary>
        /// Calculates a planet's surface temperature including greenhouse effects.
        /// </summary>
        /// <param name="stellarLuminosity">Star's luminosity in solar units</param>
        /// <param name="orbitalDistance">Distance from star in AU</param>
        /// <param name="albedo">Planet's albedo (reflectivity)</param>
        /// <param name="greenhouseFactor">Greenhouse effect strength (Earth = 1.0)</param>
        /// <returns>Surface temperature in Kelvin</returns>
        public static float PlanetaryTemperature(float stellarLuminosity, float orbitalDistance, float albedo, float greenhouseFactor = 1.0f)
        {
            float baseTemp = BlackbodyTemperature(stellarLuminosity, albedo, orbitalDistance);

            // Add greenhouse effect (Earth = 33K greenhouse effect)
            float greenhouseEffect = Utils.EARTH_GREENHOUSE_EFFECT * greenhouseFactor;

            return baseTemp + greenhouseEffect;
        }

        /// <summary>
        /// Calculates day and night temperatures for a planet.
        /// </summary>
        /// <param name="equilibriumTempK">Equilibrium temperature in Kelvin</param>
        /// <param name="rotationPeriod">Rotation period in Earth days</param>
        /// <param name="hasAtmosphere">Whether the planet has an atmosphere</param>
        /// <param name="atmosphericDensity">Atmospheric density relative to Earth</param>
        /// <param name="tidallyLocked">Whether the planet is tidally locked</param>
        /// <returns>A tuple containing (dayTemp, nightTemp) in Kelvin</returns>
        public static (float dayTemp, float nightTemp) DayNightTemperatures(
            float equilibriumTempK,
            float rotationPeriod,
            bool hasAtmosphere,
            float atmosphericDensity = 1.0f,
            bool tidallyLocked = false)
        {
            if (tidallyLocked)
            {
                // Tidal locking creates extreme temperature gradients
                float dayTemp = equilibriumTempK * 1.5f;
                float nightTemp = equilibriumTempK * 0.3f;

                // Atmosphere moderates this difference
                if (hasAtmosphere)
                {
                    // More atmosphere = less difference
                    float moderationFactor = Math.Min(1f, atmosphericDensity);
                    dayTemp = equilibriumTempK * (1.5f - 0.3f * moderationFactor);
                    nightTemp = equilibriumTempK * (0.3f + 0.4f * moderationFactor);
                }

                return (dayTemp, nightTemp);
            }
            else
            {
                // Calculate diurnal temperature range
                float dayLength = rotationPeriod;

                // Calculate thermal inertia - how quickly it heats/cools
                float thermalInertia = hasAtmosphere ? Math.Min(1f, atmosphericDensity) : 0.1f;

                // Fast rotation and high thermal inertia reduce temperature swings
                float swingFactor = (float)Math.Sqrt(dayLength) / (1f + 10f * thermalInertia);
                float tempSwing = equilibriumTempK * 0.2f * swingFactor;

                return (equilibriumTempK + tempSwing, Math.Max(10f, equilibriumTempK - tempSwing));
            }
        }
        #endregion

        #region Magnetic Field Calculations

        /// <summary>
        /// Calculates magnetic field strength relative to Earth's.
        /// </summary>
        /// <param name="massInEarth">Mass in Earth masses</param>
        /// <param name="radiusInEarth">Radius in Earth radii</param>
        /// <param name="rotationPeriod">Rotation period in Earth days</param>
        /// <param name="coreDensity">Core density relative to Earth's core</param>
        /// <param name="temperatureK">Internal temperature in Kelvin</param>
        /// <returns>Magnetic field strength (Earth = 1.0)</returns>
        public static float MagneticFieldStrength(
            float massInEarth,
            float radiusInEarth,
            float rotationPeriod,
            float coreDensity = 1.0f,
            float temperatureK = 6000f)
        {
            // Core dynamo is driven by rotation, conductive material, and heat
            
            // Estimate core size (rough approximation)
            float coreRadiusRatio = 0.5f * (float)Math.Pow(massInEarth / radiusInEarth, 0.25f);
            
            // Core factors
            float coreMassFactor = massInEarth * coreRadiusRatio * coreRadiusRatio * coreRadiusRatio * coreDensity;
            
            // Rotation factor (faster rotation = stronger field)
            float rotationFactor = Math.Min(2.0f, 1.0f / rotationPeriod);
            
            // Temperature factor (needs to be hot enough, but not too hot)
            float tempFactor = Math.Min(1.0f, Math.Max(0.0f, (temperatureK - 1000f) / 5000f));
            
            // Combine factors - simplified dynamo model
            float magneticField = coreMassFactor * rotationFactor * tempFactor / radiusInEarth;
            
            // Normalize to Earth value
            return magneticField / 0.7f;  // Scaling factor to normalize Earth = 1.0
        }
        #endregion
    }
}