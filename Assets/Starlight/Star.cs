using UnityEngine;

namespace Starlight
{
    /// <summary>
    /// Represents a single star in the starfield
    /// </summary>
    [System.Serializable]
    public class Star
    {
        /// <summary>
        /// Position of the star in 3D space
        /// </summary>
        public Vector3 Position;
        
        /// <summary>
        /// Luminosity of the star, relative to the luminosity of the Sun (L☉)
        /// </summary>
        public float Luminosity;
        
        /// <summary>
        /// Temperature of the star, in Kelvin
        /// </summary>
        public float Temperature;
        
        /// <summary>
        /// Create a new star
        /// </summary>
        /// <param name="position">3D position in world space units</param>
        /// <param name="luminosity">Luminosity in solar luminosity (L☉). Approximately 1.0 for Sol</param>
        /// <param name="temperature">Effective temperature in Kelvin. Approximately 5778 for Sol</param>
        public Star(Vector3 position, float luminosity, float temperature)
        {
            Position = position;
            Luminosity = luminosity;
            Temperature = temperature;
        }
        
        /// <summary>
        /// Copy constructor
        /// </summary>
        public Star(Star other)
        {
            Position = other.Position;
            Luminosity = other.Luminosity;
            Temperature = other.Temperature;
        }
    }
}
