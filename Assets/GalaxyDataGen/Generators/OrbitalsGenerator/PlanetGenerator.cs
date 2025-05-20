using System;
using Libraries;
using PlanetHelpers;

/// <summary>
/// Specialized generator for planet bodies orbiting a star.
/// </summary>
public class PlanetGenerator : CelestialBodyGenerator<Planet>
{
    private RingGenerator ringGenerator;
    private MoonGenerator moonGenerator;
    private MoonSystemGenerator moonSystemGenerator;

    /// <summary>
    /// Creates a new planet generator with the specified registry.
    /// </summary>
    public PlanetGenerator(EntityRegistry registry = null) : base(registry)
    {
        ringGenerator = new RingGenerator();
        moonGenerator = new MoonGenerator(registry);
        moonSystemGenerator = new MoonSystemGenerator();
    }

    /// <summary>
    /// Generates a planet based on its physical characteristics and parent star.
    /// </summary>
    /// <param name="mass">Planet mass in Earth masses</param>
    /// <param name="orbitalDistance">Orbital distance in AU</param>
    /// <param name="zone">Orbital zone classification</param>
    /// <param name="parentStar">Parent star</param>
    /// <param name="eccentricity">Orbital eccentricity</param>
    /// <returns>A fully generated planet</returns>
    public Planet GeneratePlanet(
        float mass, 
        float orbitalDistance, 
        ZoneType zone, 
        StarStructure parentStar, 
        float eccentricity)
    {
        // Generate basic properties common to all celestial bodies
        Planet planet = GenerateBasicProperties(mass, orbitalDistance, zone, eccentricity, parentStar);
        
        // Set planet-specific properties
        planet.SemiMajorAxis = orbitalDistance;
        planet.ParentStarID = parentStar.ID;
        
        // Set parent system ID
        if (!string.IsNullOrEmpty(parentStar.ParentSystemID))
        {
            planet.ParentSystemID = parentStar.ParentSystemID;
        }
        
        // Calculate rotation and orbital properties
        var rotationData = CalculateRotation(
            mass, 
            planet.Radius,
            orbitalDistance, 
            parentStar.Mass,
            true // is orbiting a star
        );
        planet.RotationPeriod = rotationData.rotationPeriod;
        planet.TidallyLocked = rotationData.tidallyLocked;
        
        // Calculate orbital period
        planet.OrbitalPeriod = Utils.OrbitalPeriod(orbitalDistance, parentStar.Mass, planet.Mass);

        // Calculate axial tilt (except for tidally locked planets)
        if (!planet.TidallyLocked)
        {
            const float BASE_AXIAL_TILT = 23.5f; // Earth-like
            const float TILT_VARIATION = 0.5f;
            planet.AxialTilt = Roll.Vary(BASE_AXIAL_TILT, TILT_VARIATION);
            planet.AxialTilt = Math.Clamp(planet.AxialTilt, 0f, 45f);
        }
        else
        {
            planet.AxialTilt = 0f; // Tidally locked planets have minimal tilt
        }

        // Finalize common properties now that we have the parent star
        FinalizeCelestialBody(planet, parentStar.Luminosity, orbitalDistance);
        
        // Generate planet-specific features: rings and moons
        GeneratePlanetSpecificFeatures(planet, parentStar);

        return planet;
    }
    
    /// <summary>
    /// Generates planet-specific features like rings and moons.
    /// </summary>
    /// <param name="planet">The planet to add features to</param>
    /// <param name="parentStar">The parent star</param>
    private void GeneratePlanetSpecificFeatures(Planet planet, StarStructure parentStar)
    {
        // Generate rings if appropriate
        ringGenerator.GenerateRings(planet);
        
        // Calculate available mass for moons
        var planetType = (PlanetLibrary.PlanetType)Enum.Parse(typeof(PlanetLibrary.PlanetType), planet.PlanetType);
        var zoneType = parentStar.Zones.GetZoneType(planet.SemiMajorAxis);
        
        float moonSystemMass = planet.Mass;
        if (planet.HasRings && planet.Rings != null)
        {
            moonSystemMass -= planet.Rings.TotalMass;
        }
        
        float availableMoonMass = moonSystemGenerator.CalculatePotentialMoonSystemMass(
            moonSystemMass, 
            planetType, 
            zoneType
        );

        if (availableMoonMass > 0)
        {
            var moonSystem = moonSystemGenerator.DetermineMoonOrbits(
                availableMoonMass,
                planet.Mass,
                planet.Radius,
                planet.SemiMajorAxis,
                parentStar.Mass,
                planetType,
                zoneType
            );

            // Generate each moon based on the calculated orbits
            foreach (var moonData in moonSystem)
            {
                var moon = moonGenerator.GenerateMoon(
                    moonData.mass,
                    moonData.orbitalDistance,
                    planet,
                    moonData.eccentricity,
                    zoneType
                );
                planet.AddMoon(moon);
            }
        }
    }
    
    /// <summary>
    /// Override registration to specifically handle planets
    /// </summary>
    protected override void RegisterBody(Planet planet)
    {
        registry.RegisterPlanet(planet);
    }
}