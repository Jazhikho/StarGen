using System;
using Libraries;

/// <summary>
/// Provides helper methods for calculating stellar properties and evolution.
/// </summary>
public static class StarHelper
{	
    /// <summary>
    /// Calculates radius based on luminosity and temperature using Stefan-Boltzmann law.
    /// </summary>
    public static float CalculateRadius(float luminosity, float temperature, StellarEvolutionStage stage = StellarEvolutionStage.V)
    {
        // R ∝ √(L/T⁴), normalized to solar values
        float radius = (float)Math.Sqrt(luminosity / Math.Pow(temperature / 5778f, 4f));
        
        // Apply a sanity check based on evolutionary stage
        float maxRadius;
        switch (stage)
        {
            case StellarEvolutionStage.I: // Supergiants can be enormous
                maxRadius = 1500f;
                break;
            case StellarEvolutionStage.II: // Bright giants
                maxRadius = 500f;
                break;
            case StellarEvolutionStage.III: // Giants
                maxRadius = 200f;
                break;
            case StellarEvolutionStage.IV: // Subgiants
                maxRadius = 50f;
                break;
            default: // Main sequence, white dwarfs, etc.
                maxRadius = 20f;
                break;
        }
        
        return Math.Min(radius, maxRadius);
    }

    /// <summary>
    /// Calculates luminosity based on mass, spectral type, and evolution stage.
    /// </summary>
    public static float CalculateLuminosity(float mass, string type, StellarEvolutionStage stage = StellarEvolutionStage.V)
    {
        // Base luminosity for main sequence stars
        float luminosity;
        
        // Handle special types first
        if (type == "I") // White dwarf
            luminosity = mass * 0.001f; // Very low luminosity
        else if (type == "W") // Wolf-Rayet
            luminosity = (float)Math.Pow(mass, 3.0f) * 1000f; // Very high luminosity
        else if (type == "C") // Carbon star
            luminosity = (float)Math.Pow(mass, 3.0f) * 10f; // Higher than normal
        else if (type == "N") // Neutron star
            luminosity = 0.001f; // Minimal luminosity unless pulsar
        else if (type == "S") // Black hole
            luminosity = 0f; // No intrinsic luminosity
        else if (type == "O" || type == "B")
            luminosity = (float)Math.Pow(mass, 3.5f);
        else if (type == "A" || type == "F")
            luminosity = (float)Math.Pow(mass, 4.0f);
        else if (type == "G" || type == "K")
            luminosity = (float)Math.Pow(mass, 4.5f);
        else if (type == "M")
            luminosity = (float)Math.Pow(mass, 2.3f);
        else if (type == "L" || type == "T" || type == "Y") // Brown dwarfs
            luminosity = mass * mass * 0.01f; // Much less luminous
        else // Default fallback
            luminosity = (float)Math.Pow(mass, 3.5f);
        
        // Adjust based on evolutionary stage (skip for special types)
        if (type != "I" && type != "W" && type != "N" && type != "S")
        {
            switch (stage)
            {
                case StellarEvolutionStage.IV: // Subgiant
                    luminosity *= 2.5f;
                    break;
                case StellarEvolutionStage.III: // Giant
                    luminosity *= 10.0f;
                    break;
                case StellarEvolutionStage.II: // Bright giant
                    luminosity *= 30.0f;
                    break;
                case StellarEvolutionStage.I: // Supergiant
                    luminosity *= 100.0f;
                    break;
            }
        }
        
        return luminosity;
    }

    /// <summary>
    /// Calculates stellar rotation period in Earth days.
    /// </summary>
    public static float CalculateRotationPeriod(StarStructure star)
    {
        // Rotation varies greatly by spectral type and age
        float basePeriod;
        
        // Special types first
        if (star.SpectralClass.StartsWith("I")) // White dwarf
        {
            return Roll.FindRange(0.1f, 1.0f); // Very fast rotation
        }
        else if (star.SpectralClass.StartsWith("N")) // Neutron star
        {
            return Roll.FindRange(0.0001f, 0.01f) / 24.0f; // Seconds to hours
        }
        else if (star.SpectralClass.StartsWith("S")) // Black hole
        {
            return 0.0f; // No meaningful rotation period
        }
        else if (star.SpectralClass.StartsWith("W")) // Wolf-Rayet
        {
            return Roll.FindRange(0.5f, 5.0f); // Very fast rotation
        }
        
        // Main sequence and brown dwarf stars
        if (star.SpectralClass.StartsWith("O") || star.SpectralClass.StartsWith("B"))
            basePeriod = Roll.FindRange(0.5f, 3.0f); // Fast rotators
        else if (star.SpectralClass.StartsWith("A"))
            basePeriod = Roll.FindRange(1.0f, 5.0f);
        else if (star.SpectralClass.StartsWith("F"))
            basePeriod = Roll.FindRange(3.0f, 10.0f);
        else if (star.SpectralClass.StartsWith("G"))
            basePeriod = Roll.FindRange(15.0f, 30.0f); // Sun-like
        else if (star.SpectralClass.StartsWith("K"))
            basePeriod = Roll.FindRange(20.0f, 40.0f);
        else if (star.SpectralClass.StartsWith("M"))
            basePeriod = Roll.FindRange(30.0f, 100.0f);
        else if (star.SpectralClass.StartsWith("L"))
            basePeriod = Roll.FindRange(10.0f, 50.0f); // Faster than M due to less magnetic braking
        else if (star.SpectralClass.StartsWith("T"))
            basePeriod = Roll.FindRange(5.0f, 30.0f);
        else if (star.SpectralClass.StartsWith("Y"))
            basePeriod = Roll.FindRange(1.0f, 20.0f);
        else
            basePeriod = Roll.FindRange(20.0f, 40.0f); // Default case

        // Stars slow down as they age - modified to use exponential scaling
        float ageFactor = (float)Math.Pow(1.0f + star.Age, 0.4f);

        // Evolution stage affects rotation
        switch (star.StellarEvolutionStage)
        {
            case StellarEvolutionStage.VII: // White dwarf
                return Roll.FindRange(0.1f, 1.0f); // Very fast rotation
            case StellarEvolutionStage.VI: // Brown dwarf
                return basePeriod * 0.5f; // Faster rotation
            case StellarEvolutionStage.IV: // Subgiant
                ageFactor *= 2.0f; // Slower rotation
                break;
            case StellarEvolutionStage.III: // Giant
                ageFactor *= 5.0f; // Much slower rotation
                break;
            case StellarEvolutionStage.II: // Bright giant
                ageFactor *= 10.0f; // Very slow rotation
                break;
            case StellarEvolutionStage.I: // Supergiant
                ageFactor *= 20.0f; // Extremely slow rotation
                break;
        }

        return basePeriod * ageFactor;
    }

    /// <summary>
    /// Calculates age based on stellar type and evolution stage using library data.
    /// </summary>
    public static float CalculateAgeForStar(StarStructure star)
    {
        // Get the age range from the library
        var spectralType = star.SpectralClass.Substring(0, 1);
        var range = StarLibrary.SpectralRanges[spectralType];
        
        float minAge = range.MinAge;
        float maxAge = range.MaxAge;
        float age = 0f;
        
        // Adjust age range based on evolutionary stage
        switch (star.StellarEvolutionStage)
        {
            case StellarEvolutionStage.VII: // White dwarf
                // White dwarfs are remnants of old stars
                age = Roll.FindRange(maxAge * 0.8f, maxAge);
                break;
                
            case StellarEvolutionStage.VI: // Brown dwarf
                // Brown dwarfs can be any age
                age = Roll.FindRange(minAge, maxAge);
                break;
                
            case StellarEvolutionStage.V: // Main sequence
                // Full range but weighted toward younger stars
                float targetAge = minAge + (maxAge - minAge) * Roll.FindRange(0.1f, 0.7f);
                age = targetAge;
                break;
                
            case StellarEvolutionStage.IV: // Subgiant
                // Upper half of range
                age = Roll.FindRange(minAge + (maxAge - minAge) * 0.5f, maxAge * 0.9f);
                break;
                
            case StellarEvolutionStage.III: // Giant
                // Upper third of range
                age = Roll.FindRange(minAge + (maxAge - minAge) * 0.67f, maxAge * 0.95f);
                break;
                
            case StellarEvolutionStage.II: // Bright giant
                // Upper quarter of range
                age = Roll.FindRange(minAge + (maxAge - minAge) * 0.75f, maxAge * 0.98f);
                break;
                
            case StellarEvolutionStage.I: // Supergiant
                // Near maximum age
                age = Roll.FindRange(maxAge * 0.9f, maxAge);
                break;
        }

        // Cap age at universe age (13.8 Gyr)
        age = Math.Min(age, 13.8f);
        
        // Ensure proper minimums for planet formation
        if (age < 0.01f && star.StellarEvolutionStage >= StellarEvolutionStage.V)
            age = 0.01f; // 10 million years minimum for habitable planets
            
        return age;
    }

    /// <summary>
    /// Applies natural variations and special characteristics to stellar properties.
    /// </summary>
    public static void ApplyNaturalVariations(StarStructure star)
    {
        // Base variations to physical properties
        star.Temperature = Roll.Vary(star.Temperature, 0.05f);
        star.Luminosity = Roll.Vary(star.Luminosity, 0.1f);
        star.Radius = Roll.Vary(star.Radius, 0.05f);

        // Get the spectral type (just the letter part)
        string spectralType = star.SpectralClass.Substring(0, 1);
        var range = StarLibrary.SpectralRanges[spectralType];
        
        // Variable star probability depends on type
        float variableChance = 0.05f; // 5% base chance
        if (spectralType == "M" || spectralType == "K")
            variableChance = 0.1f; // More common in cool stars
        else if (spectralType == "O" || spectralType == "B")
            variableChance = 0.15f; // Also common in hot stars
        
        star.IsVariableStar = Roll.FindRange(0f, 1f) <= variableChance;
        
        // Star spots probability
        float spotChance = 0.15f; // 15% base chance
        if (spectralType == "G" || spectralType == "K" || spectralType == "M")
            spotChance = 0.3f; // More common in cooler stars
        
        star.HasStarSpots = Roll.FindRange(0f, 1f) <= spotChance;
        
        // Stellar flares probability
        // Use the HasFlares property if available, otherwise calculate based on type
        if (range.HasFlares.HasValue)
        {
            star.HasStellarFlares = range.HasFlares.Value;
        }
        else
        {
            float flareChance = 0.08f; // 8% base chance
            if (spectralType == "M") flareChance = 0.25f; // M dwarfs are very flare-active
            else if (spectralType == "K") flareChance = 0.15f;
            
            star.HasStellarFlares = Roll.FindRange(0f, 1f) <= flareChance;
        }
    }
}