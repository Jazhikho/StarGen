using UnityEngine;
using System;
using System.Collections.Generic;
using Libraries;

/// <summary>
/// Generates stars with realistic properties based on stellar distributions.
/// </summary>
public class StarGenerator
{
    private EntityRegistry registry;
    
    public StarGenerator(EntityRegistry registry = null)
    {
        this.registry = registry ?? EntityRegistry.Instance;
    }

    /// <summary>
    /// Generates a new star with realistic properties.
    /// </summary>
    /// <returns>A newly generated star</returns>
    public StarStructure GenerateStar()
    {
        StarStructure star = new StarStructure();
        star.ID = Guid.NewGuid().ToString();

        // First determine spectral type based on empirical distribution
        string spectralType = Roll.Seek(StarLibrary.SpectralTypeDistribution);

        // Special handling for exotic stars and brown dwarfs
        if (spectralType == "I" || spectralType == "W" || spectralType == "N" || 
            spectralType == "S" || spectralType == "L" || spectralType == "T" || 
            spectralType == "Y" || spectralType == "C")
            return GenerateSpecialStar(star, spectralType);

        // For normal spectral types, determine subclass
        string subclass = Roll.Seek(StarLibrary.SubclassDistributions[spectralType]);
        star.SpectralClass = $"{spectralType}{subclass}";

        // Get mass range for this spectral type and determine specific mass
        var range = StarLibrary.SpectralRanges[spectralType];
        int subclassNum = 5; // Convert to int for calculations
        if (int.TryParse(subclass, out int parsedSubclass))
        {
            subclassNum = parsedSubclass;
        }
    
        star.Mass = CalculateMassFromSpectralType(spectralType, subclassNum, range);

        // Determine evolutionary stage based on spectral type
        star.StellarEvolutionStage = Roll.Seek(StarLibrary.LuminosityDistributions[spectralType]);

        // Calculate base physical properties using StarHelper
        star.Temperature = CalculateTemperature(spectralType, subclassNum, range);
        star.Luminosity = StarHelper.CalculateLuminosity(star.Mass, spectralType, star.StellarEvolutionStage);
        star.Radius = StarHelper.CalculateRadius(star.Luminosity, star.Temperature, star.StellarEvolutionStage);

        // Calculate age using StarHelper
        star.Age = StarHelper.CalculateAgeForStar(star);

        // Add natural variations using StarHelper
        StarHelper.ApplyNaturalVariations(star);

        // Calculate rotation period using StarHelper
        star.RotationPeriod = StarHelper.CalculateRotationPeriod(star);

        return star;
    }

    /// <summary>
    /// Calculates mass based on spectral type and subclass.
    /// </summary>
    private float CalculateMassFromSpectralType(string type, int subclass, StarLibrary.StarTypeRange range)
    {
        // Calculate where in the mass range this subclass falls
        float massRange = range.MaxMass - range.MinMass;
        float position = subclass / 9.0f; // Convert 0-9 subclass to 0-1 position
        // 0 = largest/hottest, 9 = smallest/coolest
        return range.MaxMass - (position * massRange);
    }

    /// <summary>
    /// Calculates temperature based on spectral type and subclass.
    /// </summary>
    private float CalculateTemperature(string type, int subclass, StarLibrary.StarTypeRange range)
    {
        float tempRange = range.MaxTemp - range.MinTemp;
        float position = subclass / 9.0f; // 0 = hottest, 9 = coolest
        return range.MaxTemp - (position * tempRange);
    }
    
    /// <summary>
    /// Generates a special type of star (white dwarf, brown dwarf, etc.)
    /// </summary>
    private StarStructure GenerateSpecialStar(StarStructure star, string spectralClass)
    {
        var range = StarLibrary.SpectralRanges[spectralClass];

        // Generate subclass for special types if available
        string subclass = "";
        if (StarLibrary.SubclassDistributions.ContainsKey(spectralClass))
        {
            subclass = Roll.Seek(StarLibrary.SubclassDistributions[spectralClass]);
        }
        
        // Set spectral class with subclass if applicable
        star.SpectralClass = subclass.Length > 0 ? spectralClass + subclass : spectralClass;

        // Set basic physical properties based on range
        star.Mass = Roll.FindRange(range.MinMass, range.MaxMass);
        
        // Determine evolutionary stage
        if (StarLibrary.LuminosityDistributions.ContainsKey(spectralClass))
        {
            star.StellarEvolutionStage = Roll.Seek(StarLibrary.LuminosityDistributions[spectralClass]);
        }
        else
        {
            star.StellarEvolutionStage = range.DefaultStage;
        }
        
        // For stellar types where we want more control over physical properties
        if (spectralClass == "I" || spectralClass == "N" || spectralClass == "S")
        {
            // Use direct values from range for these extreme objects
            star.Temperature = Roll.FindRange(range.MinTemp, range.MaxTemp);
            star.Luminosity = Roll.FindRange(range.MinLuminosity, range.MaxLuminosity);
            star.Radius = (range.MinRadius == range.MaxRadius)
                ? range.MinRadius
                : Roll.FindRange(range.MinRadius, range.MaxRadius);
        }
        else
        {
            // For other special types, use helper functions
            // Convert subclass to numeric if needed for calculation functions
            if (!string.IsNullOrEmpty(subclass) && int.TryParse(subclass, out int parsedSubclass))
            {
                int subclassNum = parsedSubclass;
                star.Temperature = CalculateTemperature(spectralClass, subclassNum, range);
            }
            else
            {
                // For letter subclasses or no subclass, use midpoint temperature
                star.Temperature = (range.MinTemp + range.MaxTemp) / 2f;
            }
            
            star.Luminosity = StarHelper.CalculateLuminosity(star.Mass, spectralClass, star.StellarEvolutionStage);
            star.Radius = StarHelper.CalculateRadius(star.Luminosity, star.Temperature, star.StellarEvolutionStage);
        }
        
        // Calculate age using StarHelper
        star.Age = StarHelper.CalculateAgeForStar(star);
        
        // Apply natural variations using StarHelper
        StarHelper.ApplyNaturalVariations(star);
        
        // Special characteristics from the range
        if (range.HasFlares.HasValue)
            star.HasStellarFlares = range.HasFlares.Value;

        if (range.CanBePulsar)
            star.IsPulsar = Roll.FindRange(0f, 1f) < 0.02f; // 2% chance
            
        // Calculate rotation period using StarHelper
        star.RotationPeriod = StarHelper.CalculateRotationPeriod(star);
        
        return star;
    }
}