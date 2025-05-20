using System;
using System.Collections.Generic;
using UnityEngine;
using Libraries;

/// <summary>
/// Generates orbits for planets around stars and binary systems.
/// </summary>
public class OrbitGenerator
{
    /// <summary>Represents a single orbital path.</summary>
    public class Orbit
    {
        public float Distance { get; set; }
        public float Mass { get; set; }
        public PlanetLibrary.PlanetType PlanetType { get; set; }
        public bool HasMoons { get; set; }
        public float Eccentricity { get; set; }
        public float Inclination { get; set; }
        public float Temperature { get; set; }  // Added to store calculated temperature
    }

    public const float MAXDISTANCEMULTI = 40f;

    /// <summary>
    /// Generates circumstellar orbits (planets around a single star).
    /// </summary>
    public List<Orbit> GenerateCircumstellarOrbits(
        StarStructure star,
        float companionMass,
        float starSeparation)
    {
        // Calculate star's Hill sphere (gravitational dominance region)
        float hillRadius = CalculateHillRadiusForStar(star.Mass, companionMass, starSeparation);

        // Get minimum stable distance based on Roche limit
        float rocheLimit = Utils.StellarRocheLimit(star.Mass, companionMass, star.Radius);

        // Convert Roche limit from Earth radii to AU
        float rocheLimitAU = rocheLimit / Utils.AU_TO_EARTH_RADIUS;

        // Effective minimum distance is the greater of Roche limit or inner zone
        float minDistance = Mathf.Max(rocheLimitAU, star.Zones.EpistellarInner);

        // Maximum stable distance is where Hill spheres start to overlap
        // In a binary system, planets only stable to ~1/3 of Hill radius
        float maxDistance = hillRadius * (companionMass > 0.001f ? 0.3f : 0.9f);

        // Generate orbits only in valid range
        return GenerateOrbitSet(minDistance, maxDistance, star.Zones, star.Mass, star.Radius, false, star.Luminosity);
    }

    /// <summary>
    /// Generates circumbinary orbits (planets around both stars).
    /// </summary>
    public List<Orbit> GenerateCircumbinaryOrbits(
        OrbitalZones zones,
        float starMass1,
        float starMass2,
        float separation,
        float star1Radius,
        float star2Radius,
        float totalLuminosity)
    {
        var orbits = new List<Orbit>();

        if (starMass1 <= 0 || starMass2 <= 0 || separation <= 0)
            return orbits;

        float totalMass = starMass1 + starMass2;

        // Rule 3a: Check if Hill spheres overlap
        float hill1 = CalculateHillRadiusForStar(starMass1, starMass2, separation);
        float hill2 = CalculateHillRadiusForStar(starMass2, starMass1, separation);
        bool hillSpheresOverlap = (hill1 + hill2) > separation;

        if (!hillSpheresOverlap)
            return orbits; // No circumbinary planets possible

        // Rule 3b: Orbit radius > max separation of stars
        // Add 50% safety margin to ensure stability
        float minStableDistance = separation * 2.5f;

        // Rule 3c: Outside both stars' Roche limits
        float roche1 = Utils.StellarRocheLimit(starMass1, starMass2, star1Radius);
        float roche2 = Utils.StellarRocheLimit(starMass2, starMass1, star2Radius);
        float maxRocheLimit = Mathf.Max(roche1, roche2);

        // Effective minimum is the maximum of all constraints
        float minDistance = Mathf.Max(minStableDistance, Mathf.Max(maxRocheLimit, zones.EpistellarInner));

        // Maximum distance is the effective circumbinary Hill sphere
        // For long-term stability, usually ~1/5 of the combined Hill radius
        float maxDistance = Mathf.Min(hill1, hill2) * 0.2f;

        // If min > max, no stable orbits possible
        if (minDistance < maxDistance)
        {
            return GenerateOrbitSet(minDistance, maxDistance, zones, totalMass, star1Radius, true, totalLuminosity);
        }

        return orbits;
    }

    /// <summary>
    /// Calculates Hill radius for a star relative to a companion.
    /// </summary>
    private float CalculateHillRadiusForStar(float starMass, float companionMass, float separation)
    {
        // Handle case where companion is very small or missing (e.g. single star)
        if (companionMass < 0.001f) return Mathf.Pow(starMass, 0.5f) * MAXDISTANCEMULTI;
        
        return Utils.StellarHillSphere(starMass, companionMass, separation);
    }

    /// <summary>
    /// Generates a set of orbits within specified distance boundaries.
    /// </summary>
    private List<Orbit> GenerateOrbitSet(
        float minDistance,
        float maxDistance,
        OrbitalZones zones,
        float stellarMass,
        float stellarRadius,
        bool isCircumbinary,
        float stellarLuminosity)  // Added parameter for temperature calculation
    {
        var orbits = new List<Orbit>();

        // Generate potential orbits using Titius-Bode law
        List<float> potentialOrbits = GenerateTitiusBodeOrbits(minDistance, maxDistance, isCircumbinary);

        // Check each potential orbit for stability and populate if valid
        foreach (float distance in potentialOrbits)
        {
            // Check if orbit formation is possible at this distance
            if (IsOrbitViable(distance, orbits, stellarRadius, isCircumbinary))
            {
                var orbit = GenerateOrbit(distance, zones, stellarMass, stellarLuminosity);  // Pass luminosity
                if (orbit != null)
                {
                    // Circumbinary orbits tend to have lower eccentricity
                    if (isCircumbinary)
                        orbit.Eccentricity *= 0.6f;

                    orbits.Add(orbit);
                }
            }
        }
        return orbits;
    }

    /// <summary>
    /// Generates a series of potential orbital distances using the Titius-Bode law.
    /// </summary>
    private List<float> GenerateTitiusBodeOrbits(float minDistance, float maxDistance, bool isCircumbinary)
    {
        var orbits = new List<float>();

        // Titius-Bode law parameter: a = 0.4 + 0.3 * 2^n AU
        float titiusBodeA = 0.4f;
        float titiusBodeB = 0.3f;

        // Adjust for circumbinary systems
        if (isCircumbinary)
        {
            titiusBodeA *= 1.5f;
            titiusBodeB *= 1.5f;
        }

        // Start at a reasonable n value
        float logArg = Mathf.Max(0.01f, minDistance - titiusBodeA);
        float n = Mathf.Log(logArg, 2f) - Mathf.Log(titiusBodeB, 2f);
        n = Mathf.Ceil(n); // Start from next integer

        // Limit to 15 potential orbits maximum
        int orbitCount = 0;
        int maxOrbits = 15;

        while (orbitCount < maxOrbits)
        {
            float distance = CalculateTitiusBodeDistance(titiusBodeA, titiusBodeB, n);

            if (distance > maxDistance)
                break;

            if (distance >= minDistance)
            {
                orbits.Add(distance);
                orbitCount++;
            }

            n += 1.0f;
        }

        return orbits;
    }

    /// <summary>
    /// Calculates an orbital distance using modified Titius-Bode formula.
    /// </summary>
    private float CalculateTitiusBodeDistance(float a, float b, float n)
    {
        return a + b * Mathf.Pow(2, n);
    }

    /// <summary>
    /// Determines if a planet or asteroid belt can form at the given orbital distance.
    /// </summary>
    private bool IsOrbitViable(float distance, List<Orbit> existingOrbits, float stellarRadius, bool isCircumbinary)
    {
        // Planets can't form inside the stellar radius (converted to AU)
        float stellarRadiusInAU = stellarRadius * Utils.SOLAR_RADIUS_TO_AU;
        if (distance <= stellarRadiusInAU * 1.1f)
        {
            return false;
        }

        // Optimize Hill stability check - only check against the most recent 2 orbits
        if (existingOrbits.Count > 0)
        {
            // Only check against the most recent orbit for performance
            int checkCount = Mathf.Min(existingOrbits.Count, 2);
            for (int i = existingOrbits.Count - 1; i >= existingOrbits.Count - checkCount; i--)
            {
                var previousOrbit = existingOrbits[i];

                // Calculate Hill radius
                float previousHillRadius = distance * Mathf.Pow(previousOrbit.Mass / 332946f, 1.0f / 3.0f);

                // Minimum separation should be at least 8 Hill radii for stability
                float minSeparation = previousHillRadius * 8.0f;
                float actualSeparation = Mathf.Abs(distance - previousOrbit.Distance);

                if (actualSeparation < minSeparation)
                    return false;
            }
        }

        float baseChance = 0.7f;
        if (isCircumbinary)
            baseChance *= 0.6f; // Slightly harder to form around binaries
        
        // Adjust probability based on distance
        if (distance > 50 * stellarRadius) // in AU, multiplied by factor of stellarRadius
            baseChance *= 0.8f;
        else if (distance < 0.5f * stellarRadius) // in AU, multiplied by factor of stellarRadius
            baseChance *= 0.9f;

        return Roll.FindRange(0f, 1f) <= baseChance;
    }

    /// <summary>
    /// Generates an orbit with appropriate parameters.
    /// </summary>
    private Orbit GenerateOrbit(float distance, OrbitalZones zones, float stellarMass, float stellarLuminosity)
    {
        var zoneType = zones.GetZoneType(distance);
        
        // Calculate temperature at this distance
        float temperature = OrbitData.CalculateEquilibriumTemperature(stellarLuminosity, distance);

        // Decide if this should be an asteroid belt
        // Adjust probability based on zone
        float asteroidProbability = zoneType switch
        {
            ZoneType.Inner => 0.05f,
            ZoneType.Habitable => 0.01f,
            ZoneType.Outer => 0.15f,
            ZoneType.FarOuter => 0.20f,
            _ => 0.10f
        };

        bool isAsteroidBelt = Roll.FindRange(0f, 1f) < asteroidProbability;

        float mass = isAsteroidBelt ? 
            Roll.FindRange(0.001f, 0.1f) : 
            Roll.Vary(GenerateMassForZone(zoneType));

        PlanetLibrary.PlanetType planetType = PlanetLibrary.PlanetType.Asterian;

        // Create orbit object - fix PlanetType assignment if it's not an asteroid belt
        if (!isAsteroidBelt)
        {
            // Calculate density for planet classification
            float density = OrbitData.EstimatePlanetDensity(mass, zoneType);
            
            string typeString = OrbitData.DeterminePlanetType(mass, density, zoneType, stellarLuminosity, distance);
            if (Enum.TryParse(typeString, out PlanetLibrary.PlanetType parsedType))
            {
                planetType = parsedType;
            }
        }

        // Create and return orbit
        return new Orbit
        {
            Distance = distance,
            Mass = mass,
            PlanetType = planetType,
            HasMoons = OrbitData.DetermineHasMoons(mass, stellarMass),
            Eccentricity = OrbitData.GenerateEccentricity(isAsteroidBelt),
            Inclination = OrbitData.GenerateInclination(),
            Temperature = temperature  // Store calculated temperature
        };
    }

    /// <summary>
    /// Determines appropriate mass for a planet based on zone.
    /// </summary>
    private float GenerateMassForZone(ZoneType zone)
    {
        var roll = Roll.Dice(1, 100);
        
        // Define mass ranges for each zone with comments
        switch (zone)
        {
            case ZoneType.Epistellar:
                // Very hot, usually rocky worlds; rare gas giants
                if (roll <= 50)
                    return Roll.Vary(Roll.Dice(1, 6, 3) * 0.2f); // 0.8-3.6 EM: Small rocky
                else if (roll <= 80)
                    return Roll.Vary(Roll.Dice(1, 6, 0) * 0.6f); // 0.6-3.6 EM: Larger rocky
                else
                    return Roll.Vary(Roll.Dice(1, 6, 0) * 3f);   // 3-18 EM: Rare sub-Neptunes

            case ZoneType.Inner:
                // Rocky worlds, occasional sub-Neptunes
                if (roll <= 70)
                    return Roll.Vary(Roll.Dice(1, 6, 0) * 0.3f); // 0.3-1.8 EM: Typical rocky
                else if (roll <= 90)
                    return Roll.Vary(Roll.Dice(1, 6, 0) * 0.8f); // 0.8-4.8 EM: Large rocky
                else
                    return Roll.Vary(Roll.Dice(1, 6, 3) * 1.0f); // 4-9 EM: Sub-Neptunes

            case ZoneType.Habitable:
                // Varied masses, sweet spot for rocky worlds
                if (roll <= 60)
                    return Roll.Vary(Roll.Dice(1, 6, 0) * 0.4f + 0.4f); // 0.8-2.8 EM: Earth-like
                else if (roll <= 85)
                    return Roll.Vary(Roll.Dice(1, 6, 2) * 0.6f);        // 1.8-6.6 EM: Super-Earths
                else
                    return Roll.Vary(Roll.Dice(1, 6, 5) * 1.2f);        // 7.2-13.2 EM: Mini-Neptunes

            case ZoneType.Outer:
                // Gas or ice giants
                if (roll <= 40)
                    return Roll.Vary(Roll.Dice(1, 6, 10) * 2.5f); // 32.5-55 EM: Saturn-class
                else if (roll <= 70)
                    return Roll.Vary(Roll.Dice(1, 6, 15) * 8f);   // 128-208 EM: Neptune-class
                else
                    return Roll.Vary(Roll.Dice(1, 6, 20) * 16f);  // 336-416 EM: Jupiter-class

            case ZoneType.FarOuter:
                // Small icy worlds or distant giants
                if (roll <= 60)
                    return Roll.Vary(Roll.Dice(1, 6, 0) * 0.15f + 0.05f); // 0.2-0.95 EM: Pluto-like
                else if (roll <= 80)
                    return Roll.Vary(Roll.Dice(1, 6, 3) * 0.8f);          // 3.2-7.2 EM: Large ice dwarfs
                else if (roll <= 95)
                    return Roll.Vary(Roll.Dice(1, 6, 5) * 3.0f);          // 18-33 EM: Ice giants
                else
                    return Roll.Vary(Roll.Dice(2, 6, 20) * 5.0f);         // 60-80 EM: Distant gas giants

            default:
                return Roll.Vary(1.0f); // Earth mass fallback
        }
    }
}