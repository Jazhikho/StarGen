using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

/// <summary>
/// Provides randomization utilities and distribution functions for procedural generation.
/// </summary>
public static class Roll
{
    // Thread-safe random generators
    private static readonly ThreadLocal<System.Random> systemRandom = 
        new ThreadLocal<System.Random>(() => new System.Random(Interlocked.Increment(ref randomSeed)));
    
    // Thread synchronization
    private static int randomSeed = Environment.TickCount;
    private static readonly object seedLock = new object();
    
    // Caches for sorted keys
    private static readonly ConcurrentDictionary<int, List<float>> sortedKeysCache = 
        new ConcurrentDictionary<int, List<float>>();

    /// <summary>
    /// Sets the random seed for deterministic generation
    /// </summary>
    public static void SetSeed(int seed)
    {
        lock (seedLock)
        {
            UnityEngine.Random.InitState(seed);
            randomSeed = seed;
            systemRandom.Value = new System.Random(seed);
            
            // Clear caches when seed changes
            sortedKeysCache.Clear();
        }
    }

    /// <summary>
    /// Rolls dice with the specified number of dice, sides per die, and modifier.
    /// </summary>
    /// <param name="number">Number of dice to roll (default: 3)</param>
    /// <param name="sides">Number of sides per die (default: 6)</param>
    /// <param name="modifier">Value to add to the total (default: 0)</param>
    /// <param name="low">Minimum possible result (default: int.MinValue)</param>
    /// <param name="high">Maximum possible result (default: int.MaxValue)</param>
    /// <returns>The sum of all dice plus modifier, clamped between low and high</returns>
    public static int Dice(int number = 3, int sides = 6, int modifier = 0, int low = int.MinValue, int high = int.MaxValue)
    {
        if (number <= 0 || sides <= 0) return 0;

        int total = 0;
        lock (seedLock)
        {
            for (int i = 0; i < number; i++)
            {
                total += UnityEngine.Random.Range(1, sides + 1);
            }
        }
        total += modifier;

        return Mathf.Clamp(total, low, high);
    }
    
    /// <summary>
    /// Generates a random integer between 1 and 10000, used for high-precision distribution sampling.
    /// </summary>
    /// <returns>A random integer between 1 and 10000</returns>
    public static int Distribution()
    {
        lock (seedLock)
        {
            return UnityEngine.Random.Range(1, 10001);
        }
    }

    /// <summary>
    /// Varies a value by a random factor within the specified percentage range.
    /// </summary>
    /// <param name="amount">The base value to vary</param>
    /// <param name="factor">The maximum percentage of variation (default: 0.05 or 5%)</param>
    /// <returns>The original value adjusted by a random factor</returns>
    public static float Vary(float amount, float factor = 0.05f)
    {
        float fudge;
        lock (seedLock)
        {
            fudge = UnityEngine.Random.Range(1 - factor, 1 + factor);
        }
        return amount * fudge;
    }

    /// <summary>
    /// Randomly selects an item from options array based on weighted probabilities.
    /// </summary>
    /// <typeparam name="T">Type of items in the options array</typeparam>
    /// <param name="options">Array of possible options</param>
    /// <param name="probabilities">Array of probability weights for each option</param>
    /// <returns>A randomly selected item from the options array</returns>
    /// <exception cref="ArgumentException">Thrown when options and probabilities arrays have different lengths</exception>
    public static T Choice<T>(T[] options, float[] probabilities)
    {
        if (options.Length != probabilities.Length)
            throw new ArgumentException("Options and probabilities must have the same length");

        float total = probabilities.Sum();
        float randomValue;
        
        lock (seedLock)
        {
            randomValue = UnityEngine.Random.Range(0f, total);
        }
        
        float cumulativeProb = 0;

        for (int i = 0; i < options.Length; i++)
        {
            cumulativeProb += probabilities[i];
            if (randomValue <= cumulativeProb)
            {
                return options[i];
            }
        }

        return options[options.Length - 1];
    }

    /// <summary>
    /// Finds a value in a dictionary where the key is greater than or equal to the search value.
    /// Uses Dice() as the default search value if none is provided.
    /// </summary>
    /// <typeparam name="T">Type of values in the dictionary</typeparam>
    /// <param name="dictionary">Dictionary with float keys and values of type T</param>
    /// <param name="searchValue">Value to search for in the dictionary keys (default: result of Dice())</param>
    /// <returns>The value associated with the first key that is >= the search value</returns>
    public static T Search<T>(Dictionary<float, T> dictionary, float? searchValue = null)
    {
        if (searchValue == null)
            searchValue = Dice();

        var sortedKeys = GetSortedKeys(dictionary);

        foreach (float key in sortedKeys)
        {
            if (searchValue <= key)
            {
                return dictionary[key];
            }
        }

        return dictionary[sortedKeys[sortedKeys.Count - 1]];
    }
    
    /// <summary>
    /// Finds a value in a dictionary where the key is greater than or equal to the search value.
    /// Uses Distribution() as the default search value if none is provided, allowing for finer-grained selection.
    /// </summary>
    /// <typeparam name="T">Type of values in the dictionary</typeparam>
    /// <param name="dictionary">Dictionary with float keys and values of type T</param>
    /// <param name="searchValue">Value to search for in the dictionary keys (default: result of Distribution())</param>
    /// <returns>The value associated with the first key that is >= the search value</returns>
    public static T Seek<T>(Dictionary<float, T> dictionary, float? searchValue = null)
    {
        if (searchValue == null)
            searchValue = Distribution();

        var sortedKeys = GetSortedKeys(dictionary);

        foreach (float key in sortedKeys)
        {
            if (searchValue <= key)
            {
                return dictionary[key];
            }
        }

        return dictionary[sortedKeys[sortedKeys.Count - 1]];
    }
    
    /// <summary>
    /// Gets or creates a cached sorted keys list for the dictionary
    /// </summary>
    private static List<float> GetSortedKeys<T>(Dictionary<float, T> dictionary)
    {
        int dictionaryHashCode = dictionary.GetHashCode();
        
        return sortedKeysCache.GetOrAdd(dictionaryHashCode, _ => {
            lock (seedLock)  // Lock during the sort operation
            {
                return dictionary.Keys.OrderBy(k => k).ToList();
            }
        });
    }
    
    /// <summary>
    /// Generates a random float value between the specified minimum and maximum values.
    /// </summary>
    /// <param name="min">Minimum possible value</param>
    /// <param name="max">Maximum possible value</param>
    /// <returns>A random float between min and max</returns>
    public static float FindRange(float min, float max)
    {
        lock (seedLock)
        {
            return UnityEngine.Random.Range(min, max);
        }
    }
    
    /// <summary>
    /// Generates a random value using a Gaussian (normal) distribution.
    /// </summary>
    /// <param name="mean">The mean (average) of the distribution</param>
    /// <param name="stdDev">The standard deviation of the distribution</param>
    /// <returns>A random value from a normal distribution with the specified mean and standard deviation</returns>
    public static float GaussianDistribution(float mean, float stdDev)
    {
        // Box-Muller transform - thread-safe through ThreadLocal
        float u1 = 1.0f - (float)systemRandom.Value.NextDouble(); // uniform(0,1] random doubles
        float u2 = 1.0f - (float)systemRandom.Value.NextDouble();
        float randStdNormal = (float)(Math.Sqrt(-2.0 * Math.Log(u1)) *
                                      Math.Sin(2.0 * Math.PI * u2)); // random normal(0,1)
        return mean + stdDev * randStdNormal; // random normal(mean,stdDev)
    }
    
    /// <summary>
    /// Determines if an event occurs based on a base probability modified by a factor.
    /// </summary>
    /// <param name="baseProbability">Base probability of the event (0-1)</param>
    /// <param name="modifier">Modifier that adjusts the probability (-1 to 1)</param>
    /// <returns>True if the event occurs, false otherwise</returns>
    public static bool ConditionalProbability(float baseProbability, float modifier)
    {
        // Clamp inputs
        baseProbability = Mathf.Clamp01(baseProbability);
        modifier = Mathf.Clamp(modifier, -1f, 1f);

        // Adjust probability
        float adjustedProbability = baseProbability + (modifier * (modifier > 0 ? 
                                                      (1 - baseProbability) : 
                                                      baseProbability));

        // Roll against adjusted probability
        float randomValue;
        lock (seedLock)
        {
            randomValue = UnityEngine.Random.value;
        }
        
        return randomValue < adjustedProbability;
    }
}