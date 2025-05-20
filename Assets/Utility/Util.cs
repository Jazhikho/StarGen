using System;
using UnityEngine;

namespace Libraries
{
    public static class Utils
    {
        #region Constants

        #region Physical Constants
        public const float G = 6.67430e-11f; // Gravitational constant in m³/kg/s²
        public const float STEFAN_BOLTZMANN = 5.670374419e-8f; // Stefan-Boltzmann constant in W/m²/K⁴
        public const float UNIVERSAL_GAS_CONSTANT = 8.314462618f; // Universal gas constant in J/mol/K
        #endregion

        #region Earth Constants
        public const float EARTH_MASS = 5.972e24f; // Earth's mass in kg
        public const float EARTH_RADIUS = 6371f; // Earth's radius in km
        public const float EARTH_RADIUS_TO_METERS = 6371000f; // Earth's radius in meters
        public const float EARTH_GRAVITY = 9.81f; // Earth's surface gravity in m/s²
        public const float EARTH_ALBEDO = 0.306f; // Earth's albedo
        public const float EARTH_GREENHOUSE_EFFECT = 33f; // Earth's greenhouse effect in K
        public const float EARTH_ATMOSPHERIC_PRESSURE = 1.01325e5f; // Earth's atmospheric pressure in Pa
        public const float EARTH_ATMOSPHERIC_DENSITY = 1.225f; // Earth's atmospheric density in kg/m³
        #endregion

        #region Solar Constants
        public const float SOLAR_MASS = 1.989e30f; // Solar mass in kg
        public const float SOLAR_RADIUS = 696340f; // Solar radius in km
        public const float SOLAR_LUMINOSITY = 3.828e26f; // Solar luminosity in W
        public const float SOLAR_MASS_TO_EARTH_MASS = 333000f; // Solar mass in Earth masses
        public const float SOLAR_RADIUS_TO_EARTH_RADIUS = 109.2f; // Solar radius in Earth radii
        public const float KM_TO_SOLAR_RADIUS = 1f / 696340f; // Kilometers to solar radii
        public const float SOLAR_RADIUS_TO_AU = 0.00465f; // Solar radius in AU
        #endregion

        #region Distance Constants
        public const float AU = 149597870.7f; // Astronomical Unit in km
        public const float AU_TO_METERS = 149597870700f; // AU in meters
        public const float AU_TO_EARTH_RADIUS = 23481.0f; // AU to Earth radii
        public const float EARTH_RADIUS_TO_AU = 1f / 23481.0f; // Earth radii to AU
        public const float AU_TO_SOLAR_RADIUS = 215.032f; // AU to solar radii
        #endregion

        #region Time Constants
        public const float SECONDS_PER_DAY = 86400f; // Seconds in a day
        public const float EARTH_DAY = 1f; // Earth's rotation period in Earth days
        public const float EARTH_HOUR = 24f; // Hours in an Earth day
        public const float EARTH_YEAR = 365.256f; // Earth's orbital period in Earth days
        #endregion
        #endregion

        #region Conversions
        #region Mass Conversions
        public static float SolarToEarthMass(float solarMass) => solarMass * SOLAR_MASS_TO_EARTH_MASS;
        public static float EarthToSolarMass(float earthMass) => earthMass / SOLAR_MASS_TO_EARTH_MASS;
        #endregion
        
        #region Radius Conversions
        public static float SolarToEarthRadius(float solarRadius) => solarRadius * SOLAR_RADIUS_TO_EARTH_RADIUS;
        public static float EarthToSolarRadius(float earthRadius) => earthRadius / SOLAR_RADIUS_TO_EARTH_RADIUS;
        public static float KmToSolarRadius(float kilometers) => kilometers * KM_TO_SOLAR_RADIUS;
        #endregion
        
        #region Distance Conversions
        public static float AuToKm(float au) => au * AU;
        public static float KmToAu(float km) => km / AU;
        public static float EarthRadiiToKm(float earthRadii) => earthRadii * EARTH_RADIUS;
        #endregion
        
        #region Time Conversions
        public static float DaysToSeconds(float days) => days * SECONDS_PER_DAY;
        public static float SecondsToDays(float seconds) => seconds / SECONDS_PER_DAY;
        #endregion
        #endregion

        #region Orbital Mechanics
        public static float OrbitalPeriod(float semiMajorAxisAU, float starMassInSolar, float planetMassInEarth = 0)
        {
            float planetMassInSolar = EarthToSolarMass(planetMassInEarth);
            float totalMassInSolar = starMassInSolar + planetMassInSolar;
            
            return (float)Math.Sqrt(Math.Pow(semiMajorAxisAU, 3) / totalMassInSolar);
        }

        /// <summary>
        /// Calculates semi-major axis from orbital period using Kepler's Third Law.
        /// </summary>
        public static float SemiMajorAxisFromPeriod(float periodInYears, float starMassInSolar)
        {
            return (float)Math.Pow(periodInYears * periodInYears * starMassInSolar, 1f/3f);
        }

        /// <summary>
        /// Calculates orbital velocity in km/s.
        /// </summary>
        public static float OrbitalVelocity(float semiMajorAxisAU, float starMassInSolar)
        {
            float earthVelocity = 29.78f; // km/s
            return earthVelocity * (float)Math.Sqrt(starMassInSolar / semiMajorAxisAU);
        }

        #region Hill Sphere Calculations
        /// <summary>
        /// Calculates Hill sphere radius for any body orbiting a primary
        /// </summary>
        /// <param name="primaryMass">Mass of the primary body in appropriate units</param>
        /// <param name="orbitingMass">Mass of the orbiting body in the same units</param>
        /// <param name="semiMajorAxis">Orbital distance in appropriate units</param>
        /// <param name="eccentricity">Orbital eccentricity (0-1)</param>
        /// <returns>Hill sphere radius in the same units as semiMajorAxis</returns>
        public static float HillSphereRadius(float primaryMass, float orbitingMass, float semiMajorAxis, float eccentricity = 0)
        {
            // Standard Hill sphere formula: a * cbrt(m / (3 * M)) * (1 - e)
            return semiMajorAxis * (1 - eccentricity) * (float)Math.Pow(orbitingMass / (3 * primaryMass), 1.0f/3.0f);
        }

        /// <summary>
        /// Calculates Hill sphere radius for a planet around a star
        /// </summary>
        /// <param name="planetMassInEarth">Planet mass in Earth masses</param>
        /// <param name="starMassInSolar">Star mass in solar units</param>
        /// <param name="semiMajorAxisAU">Orbital distance in AU</param>
        /// <param name="eccentricity">Orbital eccentricity (default: 0)</param>
        /// <returns>Hill sphere radius in AU</returns>
        public static float PlanetaryHillSphere(float planetMassInEarth, float starMassInSolar, float semiMajorAxisAU, float eccentricity = 0)
        {
            // Convert masses to same units (Earth masses)
            float starMassInEarth = SolarToEarthMass(starMassInSolar);
            
            return HillSphereRadius(starMassInEarth, planetMassInEarth, semiMajorAxisAU, eccentricity);
        }

        /// <summary>
        /// Calculates Hill sphere radius for a star in a binary system
        /// </summary>
        /// <param name="starMassInSolar">Mass of the star in solar units</param>
        /// <param name="companionMassInSolar">Mass of the companion in solar units</param>
        /// <param name="separationAU">Binary separation in AU</param>
        /// <param name="eccentricity">Binary eccentricity (default: 0)</param>
        /// <returns>Hill sphere radius in AU</returns>
        public static float StellarHillSphere(float starMassInSolar, float companionMassInSolar, float separationAU, float eccentricity = 0)
        {
            return HillSphereRadius(companionMassInSolar, starMassInSolar, separationAU, eccentricity);
        }

        /// <summary>
        /// Calculates Hill sphere radius for a moon around a planet
        /// </summary>
        /// <param name="moonMassInEarth">Moon mass in Earth masses</param>
        /// <param name="planetMassInEarth">Planet mass in Earth masses</param>
        /// <param name="semiMajorAxis">Orbital distance in planet radii</param>
        /// <param name="eccentricity">Orbital eccentricity (default: 0)</param>
        /// <returns>Hill sphere radius in planet radii</returns>
        public static float LunarHillSphere(float moonMassInEarth, float planetMassInEarth, float semiMajorAxis, float eccentricity = 0)
        {
            return HillSphereRadius(planetMassInEarth, moonMassInEarth, semiMajorAxis, eccentricity);
        }
        #endregion

        /// <summary>
        /// Calculates the orbital period of a moon in Earth days.
        /// </summary>
        public static float MoonOrbitalPeriod(float semiMajorAxis, float planetMassInEarth, float moonMassInEarth = 0)
        {
            // Using Kepler's Third Law adapted for moons
            // Convert distances to meters and masses to kg
            float distanceInM = semiMajorAxis * EARTH_RADIUS_TO_METERS;
            float planetMassInKg = planetMassInEarth * EARTH_MASS;
            float moonMassInKg = moonMassInEarth * EARTH_MASS;
            
            // Calculate orbital period in seconds
            float periodInSec = (float)(2 * Math.PI * Math.Sqrt(
                Math.Pow(distanceInM, 3) / (G * (planetMassInKg + moonMassInKg))
            ));
            
            // Convert to days
            return periodInSec / SECONDS_PER_DAY;
        }

        /// <summary>
        /// Calculates the maximum stable orbital distance for a moon.
        /// </summary>
        public static float MaximumStableMoonOrbit(float planetMassInEarth, float starMassInSolar, float semiMajorAxisAU, float hillSphereModifier = 0.4f)
        {
            // Hill sphere radius in AU
            float hillRadiusAU = HillSphereRadius(planetMassInEarth, starMassInSolar, semiMajorAxisAU);
            
            // Moons are typically stable within ~0.4-0.5 of Hill radius
            float stableRadiusAU = hillRadiusAU * hillSphereModifier;
            
            // Convert planet Earth masses to radius in Earth radii (assuming Earth density)
            float planetRadiusInEarth = CalculateRadius(planetMassInEarth, 1.0f);
            
            // Convert AU to planet radii
            return stableRadiusAU * AU_TO_METERS / (planetRadiusInEarth * EARTH_RADIUS_TO_METERS);
        }

        /// <summary>
        /// Calculates orbital inclination stability threshold based on Kozai mechanism.
        /// </summary>
        public static float MaxStableInclination(float semiMajorAxis, float maxStableOrbit)
        {
            // Based on Kozai mechanism - inclinations approaching 39.2° become unstable
            // Closer orbits can support higher inclinations
            float stabilityFactor = 1.0f - (semiMajorAxis / maxStableOrbit);
            return 39.2f + (stabilityFactor * 30f);
        }
        #endregion

        #region Rotation Calculations
        /// <summary>
        /// Calculates the initial rotation period of a body in Earth days.
        /// </summary>
        public static float CalculateInitialRotation(float massInEarth, float radiusInEarth, float formationDistance)
        {
            // Base rotation period on mass and radius
            float basePeriod = (float)Math.Sqrt(massInEarth / (radiusInEarth * radiusInEarth * radiusInEarth));
            
            // Adjust for formation distance (bodies form faster closer to star)
            float distanceFactor = (float)Math.Pow(formationDistance, 0.5f);
            
            // Use Roll instead of Random for deterministic results
            float randomFactor = 0.8f + Roll.FindRange(0f, 0.4f);
            
            return basePeriod * distanceFactor * randomFactor;
        }

        /// <summary>
        /// Calculates the effect of tidal forces on rotation period over time.
        /// </summary>
        public static float CalculateTidalRotationEffect(
            float initialPeriod,
            float planetMass,
            float planetRadius,
            float orbitalDistance,
            float starMass,
            float systemAge = 5.0f)
        {
            const float Q = 100f; // Tidal quality factor
            const float k2 = 0.3f; // Love number
            
            // Convert to SI units
            float planetMassInKg = planetMass * EARTH_MASS;
            float planetRadiusInM = planetRadius * EARTH_RADIUS_TO_METERS;
            float orbitalDistanceInM = orbitalDistance * AU_TO_METERS;
            float starMassInKg = starMass * SOLAR_MASS;
            
            // Calculate tidal torque
            float tidalTorque = (3f * G * starMassInKg * planetMassInKg * 
                                planetRadiusInM * planetRadiusInM * planetRadiusInM * k2) /
                                (2f * Q * orbitalDistanceInM * orbitalDistanceInM * orbitalDistanceInM);
            
            // Calculate moment of inertia
            float momentOfInertia = 0.4f * planetMassInKg * planetRadiusInM * planetRadiusInM;
            
            // Calculate time in seconds
            float timeInSeconds = systemAge * 1e9f * SECONDS_PER_DAY * 365.25f;
            
            // Calculate final rotation period
            float finalPeriod = initialPeriod * (float)Math.Exp(-tidalTorque * timeInSeconds / momentOfInertia);
            
            return finalPeriod;
        }

        /// <summary>
        /// Calculates the rotation period of a moon in Earth days.
        /// </summary>
        public static float CalculateMoonRotationPeriod(
            float moonMass,
            float moonRadius,
            float orbitalPeriod,
            float parentMass,
            float orbitalDistance,
            float systemAge = 4.5f)
        {
            // Calculate initial rotation period
            float initialPeriod = CalculateInitialRotation(moonMass, moonRadius, orbitalDistance);
            
            // Calculate tidal effect
            float finalPeriod = CalculateTidalRotationEffect(
                initialPeriod,
                moonMass,
                moonRadius,
                orbitalDistance * moonRadius * EARTH_RADIUS / AU_TO_METERS,
                parentMass / SOLAR_MASS_TO_EARTH_MASS,
                systemAge
            );
            
            // Ensure rotation period doesn't exceed orbital period
            return Math.Min(finalPeriod, orbitalPeriod);
        }
        #endregion
        
        #region Planetary Physics
        /// <summary>
        /// Calculates planetary radius in Earth radii based on mass and density.
        /// </summary>
        public static float CalculateRadius(float massInEarth, float densityRelativeToEarth)
        {
            // R = cbrt(M/ρ), where Earth values are normalized to 1
            return (float)Math.Pow(massInEarth / densityRelativeToEarth, 1.0f / 3.0f);
        }
        
        #region Roche Limit Calculations
        /// <summary>
        /// Generic Roche limit calculation for any two bodies
        /// </summary>
        /// <param name="primaryMass">Mass of the primary body</param>
        /// <param name="secondaryMass">Mass of the secondary body</param>
        /// <param name="primaryRadius">Radius of the primary body</param>
        /// <param name="secondaryDensity">Density of secondary body (if known)</param>
        /// <returns>Roche limit distance in same units as primaryRadius</returns>
        private static float CalculateRocheLimitGeneric(float primaryMass, float secondaryMass, float primaryRadius, float secondaryDensity = 0)
        {
            if (secondaryDensity > 0)
            {
                // Roche limit = primaryRadius * 2.44 * cbrt(primaryDensity / secondaryDensity)
                float primaryDensity = primaryMass / (4f/3f * (float)Math.PI * primaryRadius * primaryRadius * primaryRadius);
                return primaryRadius * 2.44f * (float)Math.Pow(primaryDensity / secondaryDensity, 1.0f/3.0f);
            }
            else
            {
                // Simplified Roche limit assuming equal densities: 2.44 * R
                // If densities are equal, we can simplify using mass ratio instead
                return primaryRadius * 2.44f * (float)Math.Pow(primaryMass / (primaryMass + secondaryMass), 1.0f/3.0f);
            }
        }

        /// <summary>
        /// Calculates Roche limit for a star in a binary system
        /// </summary>
        /// <param name="starMassInSolar">Mass of the star in solar units</param>
        /// <param name="companionMassInSolar">Mass of the companion in solar units</param>
        /// <param name="starRadiusInSolar">Radius of the star in solar radii</param>
        /// <returns>Roche limit in solar radii</returns>
        public static float StellarRocheLimit(float starMassInSolar, float companionMassInSolar, float starRadiusInSolar)
        {
            return CalculateRocheLimitGeneric(starMassInSolar, companionMassInSolar, starRadiusInSolar);
        }

        /// <summary>
        /// Calculates Roche limit for a planet around a star
        /// </summary>
        /// <param name="starMassInSolar">Mass of the star in solar units</param>
        /// <param name="starRadiusInSolar">Radius of the star in solar radii</param>
        /// <param name="planetMassInEarth">Mass of the planet in Earth masses (optional, default 1)</param>
        /// <returns>Roche limit in solar radii</returns>
        public static float PlanetaryRocheLimit(float starMassInSolar, float starRadiusInSolar, float planetMassInEarth = 1.0f)
        {
            float planetMassInSolar = EarthToSolarMass(planetMassInEarth);
            return CalculateRocheLimitGeneric(starMassInSolar, planetMassInSolar, starRadiusInSolar);
        }

        /// <summary>
        /// Calculates Roche limit for a moon around a planet
        /// </summary>
        /// <param name="planetMassInEarth">Mass of the planet in Earth masses</param>
        /// <param name="planetRadiusInEarth">Radius of the planet in Earth radii</param>
        /// <param name="moonMassInEarth">Mass of the moon in Earth masses (optional, default lunar mass)</param>
        /// <returns>Roche limit in Earth radii</returns>
        public static float LunarRocheLimit(float planetMassInEarth, float planetRadiusInEarth, float moonMassInEarth = 0.0123f)
        {
            return CalculateRocheLimitGeneric(planetMassInEarth, moonMassInEarth, planetRadiusInEarth);
        }

        /// <summary>
        /// Converts Roche limit from star-centric units to AU
        /// </summary>
        /// <param name="rocheLimitInSolarRadii">Roche limit in solar radii</param>
        /// <returns>Roche limit in AU</returns>
        public static float RocheLimitToAU(float rocheLimitInSolarRadii)
        {
            return rocheLimitInSolarRadii * SOLAR_RADIUS_TO_AU;
        }
        #endregion
        #endregion
    }
}