using System.Collections.Generic;
using UnityEngine;

namespace Starlight
{
    /// <summary>
    /// Class to hold and manage star data for a starfield
    /// </summary>
    [System.Serializable]
    public class StarfieldData
    {
        [SerializeField]
        private List<Star> stars = new List<Star>();
        
        /// <summary>
        /// Get the list of stars
        /// </summary>
        public List<Star> Stars => stars;
        
        /// <summary>
        /// Number of stars in the starfield
        /// </summary>
        public int Count => stars.Count;
        
        /// <summary>
        /// Add a star to the starfield
        /// </summary>
        public void AddStar(Star star)
        {
            stars.Add(star);
        }
        
        /// <summary>
        /// Add a collection of stars to the starfield
        /// </summary>
        public void AddStars(IEnumerable<Star> newStars)
        {
            stars.AddRange(newStars);
        }
        
        /// <summary>
        /// Clear all stars from the starfield
        /// </summary>
        public void Clear()
        {
            stars.Clear();
        }
        
        /// <summary>
        /// Create a new starfield data object
        /// </summary>
        public StarfieldData()
        {
            stars = new List<Star>();
        }
        
        /// <summary>
        /// Create a new starfield data object with the given stars
        /// </summary>
        public StarfieldData(IEnumerable<Star> initialStars)
        {
            stars = new List<Star>(initialStars);
        }
        
        /// <summary>
        /// Create a copy of this starfield data
        /// </summary>
        public StarfieldData Copy()
        {
            StarfieldData copy = new StarfieldData();
            foreach (var star in stars)
            {
                copy.AddStar(new Star(star));
            }
            return copy;
        }
    }
}
