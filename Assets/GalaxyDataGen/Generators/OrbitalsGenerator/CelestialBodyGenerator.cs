using System;
using System.Collections.Generic;
using UnityEngine;
using Libraries;
using PlanetHelpers;

/// <summary>
/// Abstract base class for generating celestial bodies with common properties.
/// Provides common generation logic while allowing specialized implementations.
/// </summary>
/// <typeparam name="T">The type of orbital body to generate</typeparam>
public abstract class CelestialBodyGenerator<T> where T : OrbitalBody, new()
{
    protected PlanetGeologyHelper geologyHelper = new PlanetGeologyHelper();
    protected PlanetAtmosphereHelper atmosphereHelper = new PlanetAtmosphereHelper();
    protected PlanetHydrologyHelper hydrologyHelper = new PlanetHydrologyHelper();
    protected PlanetGeographyHelper geographyHelper = new PlanetGeographyHelper();
    protected PlanetResourceHelper resourceHelper = new PlanetResourceHelper();
    protected PlanetHabitabilityHelper habitabilityHelper = new PlanetHabitabilityHelper();
    protected PlanetBiologyHelper biologyHelper = new PlanetBiologyHelper();
    protected EntityRegistry registry;
    
    /// <summary>
    /// Initializes the generator with the entity registry.
    /// </summary>
    /// <param name="registry">Registry for entity lookup and registration</param>
    public CelestialBodyGenerator(EntityRegistry registry = null)
    {
        this.registry = registry ?? EntityRegistry.Instance;
    }

    /// <summary>
    /// Generates a celestial body with basic physical properties.
    /// </summary>
    /// <param name="mass">Mass in Earth masses</param>
    /// <param name="orbitalDistance">Distance from parent body in appropriate units</param>
    /// <param name="zone">Orbital zone classification</param>
    /// <param name="eccentricity">Orbital eccentricity</param>
    /// <returns>A new orbital body with basic properties defined</returns>
    protected T GenerateBasicProperties(
        float mass, 
        float orbitalDistance, 
        ZoneType zone, 
        float eccentricity,
        StarStructure parentStar)  // Add parent star parameter
    {
        var body = new T();
        
        // Generate unique ID
        body.ID = Guid.NewGuid().ToString();
        
        // Set basic physical properties
        body.Mass = mass;
        body.Eccentricity = eccentricity;

        // Now we can properly determine planet type with full stellar information
        body.PlanetType = OrbitData.DeterminePlanetType(
            mass,
            0, // We'll calculate density after we know the type
            zone,
            parentStar.Luminosity,
            orbitalDistance
        );
        
        var bodyTypeEnum = (PlanetLibrary.PlanetType)Enum.Parse(typeof(PlanetLibrary.PlanetType), body.PlanetType);
        
        // Calculate density and radius
        body.Density = CalculateDensity(bodyTypeEnum, mass);
        body.Radius = Utils.CalculateRadius(mass, body.Density);

        // Set albedo based on body type
        body.Albedo = CalculateAlbedo(bodyTypeEnum);

        // Calculate initial rotation period for geology
        float initialRotationPeriod = Utils.CalculateInitialRotation(mass, body.Radius, orbitalDistance);
        
        // Generate geology with proper values
        var geologyData = geologyHelper.GenerateGeology(
            bodyTypeEnum,
            mass, 
            body.Density, 
            Astro.PlanetaryTemperature(parentStar.Luminosity, orbitalDistance, body.Albedo),
            initialRotationPeriod
        );

        // Store geology data
        body.TectonicActivity = geologyData.TectonicActivity;
        body.VolcanicActivity = geologyData.VolcanicActivity;
        body.MagneticFieldStrength = geologyData.MagneticFieldStrength;

        // Register the body immediately to enable lookups
        RegisterBody(body);
        
        return body;
    }
    
    /// <summary>
    /// Registers the body with the appropriate registry method based on type.
    /// </summary>
    protected virtual void RegisterBody(T body)
    {
        if (body is Planet planet)
            registry.RegisterPlanet(planet);
        else if (body is Moon moon)
            registry.RegisterMoon(moon);
    }
    
    /// <summary>
	/// Finalizes a celestial body's properties after its parent relationship is established.
	/// </summary>
	/// <param name="body">The body to finalize</param>
	/// <param name="parentLuminosity">Luminosity of the parent star in solar units</param>
	/// <param name="distanceFromStar">Distance from star in AU</param>
	protected void FinalizeCelestialBody(T body, float parentLuminosity, float distanceFromStar)
	{
		var bodyTypeEnum = (PlanetLibrary.PlanetType)Enum.Parse(typeof(PlanetLibrary.PlanetType), body.PlanetType);

        // Calculate temperature now that we know distance from star
        float albedo = body.Albedo;
        body.SurfaceTemperature = Astro.PlanetaryTemperature(
            parentLuminosity,
            distanceFromStar,
            albedo
        );

        // Calculate surface temperature with atmospheric effects
        if (IsGasGiant(bodyTypeEnum))
        {
            // Gas giants have higher surface temperatures due to compression
            const float GAS_GIANT_TEMP_MULTIPLIER = 1.5f;
            body.SurfaceTemperature = body.SurfaceTemperature * GAS_GIANT_TEMP_MULTIPLIER;
        }

        // Calculate day/night temperature range
        var tempRange = Astro.DayNightTemperatures(
            body.SurfaceTemperature,
            body.RotationPeriod,
            body.HasAtmosphere,
            body.AtmosphericPressure,
            body.TidallyLocked
        );
        body.DaytimeTemperature = tempRange.dayTemp;
        body.NighttimeTemperature = tempRange.nightTemp;

		// Atmosphere
		var atmosphereData = atmosphereHelper.GenerateAtmosphere(
			bodyTypeEnum,
			body.Mass,
			body.SurfaceGravity,
			body.SurfaceTemperature
		);
		body.HasAtmosphere = atmosphereData.hasAtmosphere;
		body.AtmosphericPressure = atmosphereData.pressure;
		body.AtmosphericComposition = atmosphereData.composition;

		// Hydrology
		body.WaterCoverage = hydrologyHelper.CalculateWaterCoverage(
			bodyTypeEnum,
			body.SurfaceTemperature,
			body.AtmosphericPressure
		);

		// Geography
		body.BiomeTypes = geographyHelper.DetermineBiomes(
            bodyTypeEnum,
            body.WaterCoverage,
            body.SurfaceTemperature,
            body.AtmosphericPressure
        );

		// Resources
		var geologyData = new PlanetGeologyHelper.GeologyData
		{
			TectonicActivity = body.TectonicActivity,
			VolcanicActivity = body.VolcanicActivity,
			MagneticFieldStrength = body.MagneticFieldStrength
		};
		Dictionary<string, float> resources = resourceHelper.GenerateResources(bodyTypeEnum, body.Mass, geologyData);

		foreach (var kvp in resources)
		{
			body.AddResource(kvp.Key, kvp.Value);
		}

		body.ResourceValue = resourceHelper.CalculateResourceValue(resources);

		// Habitability
		body.HabitabilityIndex = habitabilityHelper.CalculateHabitabilityIndex(
			bodyTypeEnum,
			body.SurfaceTemperature,
			body.AtmosphericPressure,
			body.AtmosphericComposition,
			body.WaterCoverage,
			geologyData.MagneticFieldStrength
		);
		body.IsHabitable = body.HabitabilityIndex > 0.5f;

		// Biology (if habitable)
		const float LIFE_PROBABILITY = 0.15f;
        if (body.IsHabitable && Roll.ConditionalProbability(LIFE_PROBABILITY, 0))
        {
            // Generate biological data
            const float DEFAULT_SYSTEM_AGE = 5.0f;
            var biosphereData = biologyHelper.GenerateBiology(
                body.HabitabilityIndex,
                body.BiomeTypes,
                DEFAULT_SYSTEM_AGE,
                geologyData.MagneticFieldStrength
            );

            // Store biosphere data if properties exist
            // body.BiosphereType = biosphereData.type;
            // body.BioDiversity = biosphereData.diversity;
            // body.BiosphereComplexity = biosphereData.complexity;
        }

		// Terraformability
		var terraformabilityHelper = new PlanetTerraformabilityHelper();
		var (terraformable, index, challenges) = terraformabilityHelper.EvaluateTerraformability(body);
		body.IsTerraformable = terraformable;
		body.TerraformingIndex = index;
		body.TerraformingChallenges = challenges;

		// Update the registry with the finalized body
		RegisterBody(body);
	}

    private bool IsGasGiant(PlanetLibrary.PlanetType planetType)
    {
        return planetType == PlanetLibrary.PlanetType.Hyperion || 
            planetType == PlanetLibrary.PlanetType.Atlantean ||
            planetType == PlanetLibrary.PlanetType.Criusian ||
            planetType == PlanetLibrary.PlanetType.Theian ||
            planetType == PlanetLibrary.PlanetType.Iapetian ||
            planetType == PlanetLibrary.PlanetType.Helian;
    }
    
    /// <summary>
    /// Calculates the density of a celestial body based on its type and mass.
    /// </summary>
    /// <param name="bodyType">Type classification of the body</param>
    /// <param name="mass">Mass in Earth masses</param>
    /// <returns>Density relative to Earth</returns>
    protected float CalculateDensity(PlanetLibrary.PlanetType bodyType, float mass)
    {
        var densityRange = PlanetLibrary.PlanetData[bodyType].DensityRange;
        float average = (densityRange.min + densityRange.max) / 2f;
        float variation = (densityRange.max - densityRange.min) / average;
        return Roll.Vary(average, variation / 2f);
    }
    
    /// <summary>
    /// Calculates the albedo (reflectivity) of a celestial body based on its type.
    /// </summary>
    /// <param name="bodyType">Type classification of the body</param>
    /// <returns>Albedo value (0-1)</returns>
    protected float CalculateAlbedo(PlanetLibrary.PlanetType bodyType)
    {
        var albedoRange = PlanetLibrary.PlanetData[bodyType].AlbedoRange;
        float average = (albedoRange.min + albedoRange.max) / 2f;
        float variation = (albedoRange.max - albedoRange.min) / average;
        return Roll.Vary(average, variation / 2f);
    }
    
    /// <summary>
    /// Calculates the rotation properties of a celestial body.
    /// </summary>
    /// <param name="bodyMass">Mass in Earth masses</param>
    /// <param name="bodyRadius">Radius in Earth radii</param>
    /// <param name="orbitalDistance">Orbital distance in appropriate units</param>
    /// <param name="parentMass">Mass of parent body</param>
    /// <param name="isAroundStar">Whether the body orbits a star</param>
    /// <param name="systemAge">Age of the system in billions of years</param>
    /// <returns>Rotation period and tidal locking status</returns>
    protected (float rotationPeriod, bool tidallyLocked) CalculateRotation(
        float bodyMass, 
        float bodyRadius,
        float orbitalDistance, 
        float parentMass, 
        bool isAroundStar,
        float systemAge = 5.0f)
    {
        // Calculate initial rotation period
        float rotationPeriod = Utils.CalculateInitialRotation(bodyMass, bodyRadius, orbitalDistance);
        
        // Calculate tidal effects
        if (isAroundStar)
        {
            rotationPeriod = Utils.CalculateTidalRotationEffect(
                rotationPeriod,
                bodyMass,
                bodyRadius,
                orbitalDistance,
                parentMass,
                systemAge
            );
        }
        else
        {
            rotationPeriod = Utils.CalculateMoonRotationPeriod(
                bodyMass,
                bodyRadius,
                rotationPeriod,
                parentMass,
                orbitalDistance,
                systemAge
            );
        }
        
        // Check for tidal locking
        bool tidallyLocked = rotationPeriod <= 0.01f || rotationPeriod >= 999f;
        
        return (rotationPeriod, tidallyLocked);
    }
    
    /// <summary>
    /// Applies variation to a value using Roll utility.
    /// </summary>
    protected float ApplyVariation(float baseValue, float variationFactor = 0.1f)
    {
        return Roll.Vary(baseValue, variationFactor);
    }
}