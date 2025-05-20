using System;
using System.Linq;
using Libraries;

public class PlanetGeologyHelper
{
    /// <summary>
    /// Container class for geological data of a celestial body.
    /// </summary>
    public class GeologyData
    {
        /// <summary>Level of tectonic activity (0-1)</summary>
        public float TectonicActivity { get; set; }
        
        /// <summary>Level of volcanic activity (0-1)</summary>
        public float VolcanicActivity { get; set; }
        
        /// <summary>Strength of magnetic field relative to Earth (0-1)</summary>
        public float MagneticFieldStrength { get; set; }
        
        /// <summary>Whether the body has a ring system</summary>
        public bool HasRings { get; set; }
    }

    /// <summary>
    /// Generates geological properties for a celestial body based on its physical characteristics.
    /// </summary>
    /// <param name="planetType">Type classification of the planet</param>
    /// <param name="mass">Mass in Earth masses</param>
    /// <param name="radius">Radius in Earth radii</param>
    /// <param name="surfaceTemperature">Surface temperature in Kelvin</param>
    /// <param name="rotationPeriod">Rotation period in Earth days</param>
    /// <returns>Geological data for the celestial body</returns>
    public GeologyData GenerateGeology(
        PlanetLibrary.PlanetType planetType,
        float mass,
        float radius,
        float surfaceTemperature,
        float rotationPeriod)
    {
        var planetData = PlanetLibrary.PlanetData[planetType];
        
        var geology = new GeologyData();
        
        // Calculate density from mass and radius
        float density = mass / (float)Math.Pow(radius, 3);
        
        // Tectonic activity
        geology.TectonicActivity = CalculateTectonicActivity(
            planetType,
            mass,
            surfaceTemperature
        );
        
        // Volcanic activity
        geology.VolcanicActivity = CalculateVolcanicActivity(
            planetType,
            geology.TectonicActivity,
            surfaceTemperature
        );
        
        // Magnetic field - use Astro calculations
        float coreDensity = CalculateCoreDensity(geology.TectonicActivity, geology.VolcanicActivity);
        float coreTemp = CalculateCoreTemperature(surfaceTemperature, mass, geology.TectonicActivity);

        float magneticField = Astro.MagneticFieldStrength(mass, radius, rotationPeriod, coreDensity, coreTemp);
        geology.MagneticFieldStrength = magneticField;
            
        // Ring system
        geology.HasRings = DetermineHasRings(planetType);
        
        return geology;
    }

    /// <summary>
    /// Calculates core density based on geological activity.
    /// </summary>
    private float CalculateCoreDensity(float tectonicActivity, float volcanicActivity)
    {
        // Active geology suggests a differentiated, dense core
        float activityFactor = (tectonicActivity + volcanicActivity) / 2f;
        return 0.5f + (activityFactor * 0.7f); // Range from 0.5 to 1.2
    }

    /// <summary>
    /// Estimates core temperature based on surface temperature, mass, and activity.
    /// </summary>
    private float CalculateCoreTemperature(float surfaceTemp, float mass, float tectonicActivity)
    {
        // Base temperature gradient based on mass
        float massGradient = (float)Math.Pow(mass, 0.5f) * 3000f;
        
        // Activity modifier
        float activityMod = 1f + (tectonicActivity * 0.5f);
        
        // Core temperature is surface temperature plus gradient
        return surfaceTemp + (massGradient * activityMod);
    }

    /// <summary>
    /// Calculates tectonic activity level based on planet type, mass, and temperature.
    /// </summary>
    /// <param name="planetType">Type classification of the planet</param>
    /// <param name="mass">Mass in Earth masses</param>
    /// <param name="temperature">Surface temperature in Kelvin</param>
    /// <returns>Tectonic activity level (0-1)</returns>
    private float CalculateTectonicActivity(PlanetLibrary.PlanetType planetType, float mass, float temperature)
    {
        var planetData = PlanetLibrary.PlanetData[planetType];
        
        // Gas giants and similar types have no tectonic activity
        if (new[] {
            PlanetLibrary.PlanetType.Criusian,    // Hot mini-Neptune
            PlanetLibrary.PlanetType.Theian,      // Cold mini-Neptune
            PlanetLibrary.PlanetType.Iapetian,    // Ice giant
            PlanetLibrary.PlanetType.Helian,      // Helium planet
            PlanetLibrary.PlanetType.Hyperion,    // Hot gas giant
            PlanetLibrary.PlanetType.Atlantean,   // Cold gas giant
            PlanetLibrary.PlanetType.Asterian     // Asteroid
        }.Contains(planetType))
        {
            return 0f;
        }

        // Start with a random value within the planet type's range
        float baseActivity = Roll.FindRange(
            planetData.TectonicActivityRange.min,
            planetData.TectonicActivityRange.max
        );
        
        // Modify by mass (bigger planets have more heat)
        float massFactor = Math.Min(1f, mass * 0.5f);
        baseActivity = Roll.Vary(baseActivity * massFactor, 0.1f);
        
        // Modify by temperature (hot planets have more active tectonics)
        float tempModifier = 1f;
        if (temperature > 600f)
            tempModifier = 1.5f;
        else if (temperature < 200f)
            tempModifier = 0.5f;
            
        baseActivity *= tempModifier;
            
        return Math.Clamp(baseActivity, 0f, 1f);
    }

    /// <summary>
    /// Calculates volcanic activity level based on planet type, tectonic activity, and temperature.
    /// </summary>
    /// <param name="planetType">Type classification of the planet</param>
    /// <param name="tectonicActivity">Tectonic activity level (0-1)</param>
    /// <param name="temperature">Surface temperature in Kelvin</param>
    /// <returns>Volcanic activity level (0-1)</returns>
    private float CalculateVolcanicActivity(PlanetLibrary.PlanetType planetType, float tectonicActivity, float temperature)
    {
        var planetData = PlanetLibrary.PlanetData[planetType];
        
        // Start with a random value within the planet type's range
        float baseActivity = Roll.FindRange(
            planetData.VolcanicActivityRange.min,
            planetData.VolcanicActivityRange.max
        );
        
        // Higher tectonic activity means more volcanism
        baseActivity = Roll.Vary(baseActivity * (tectonicActivity + 0.5f), 0.1f);
        
        // Very hot planets tend to have more volcanism
        if (temperature > 700f)
            baseActivity *= 2f;
            
        return Math.Clamp(baseActivity, 0f, 1f);
    }

    /// <summary>
    /// Determines whether a planet has rings based on its type and ring probability.
    /// </summary>
    /// <param name="planetType">Type classification of the planet</param>
    /// <returns>True if the planet has rings</returns>
    private bool DetermineHasRings(PlanetLibrary.PlanetType planetType)
    {
        var planetData = PlanetLibrary.PlanetData[planetType];
        return Roll.ConditionalProbability(planetData.RingProbability, 0);
    }
}