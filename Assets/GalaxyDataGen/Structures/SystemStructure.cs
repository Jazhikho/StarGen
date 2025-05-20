using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// Represents a star system with one or more stars and their planetary bodies.
/// </summary>
[Serializable]
public class StarSystem
{
    /// <summary>3D position of the system in light years</summary>
    public Vector3 Position;
    
    /// <summary>Unique identifier for the system</summary>
    public string ID;
    
    /// <summary>Reference to the parent sector ID (for later lookup)</summary>
    public string ParentSectorID;

    /// <summary>Total population of the system (sentient beings)</summary>
    public int SystemPopulation;

    /// <summary>Codes for system features</summary>
    public List<string> SystemFeatures = new List<string>();
    
    /// <summary>The binary hierarchy that represents the structure of stars in this system</summary>
    public List<BinaryPair> BinaryPairs = new List<BinaryPair>();
    
    /// <summary>List of all stars in this system for direct access</summary>
    public List<StarStructure> Stars = new List<StarStructure>();
    
    /// <summary>ID of the root binary pair (if any)</summary>
    public string RootBinaryPairID;
    
    /// <summary>
    /// Constructor sets up a new system with a unique ID
    /// </summary>
    public StarSystem()
    {
        ID = Guid.NewGuid().ToString();
        SystemFeatures = new List<string>();
        BinaryPairs = new List<BinaryPair>();
        Stars = new List<StarStructure>();
    }
    
    /// <summary>
    /// Direct reference to the primary star of the system.
    /// </summary>
    public StarStructure PrimaryStar 
    { 
        get 
        {
            if (Stars.Count > 0)
            {
                return Stars[0];
            }
            return null;
        }
    }
    
    /// <summary>
    /// Gets all stars in this system.
    /// </summary>
    public List<StarStructure> GetAllStars()
    {
        return Stars;
    }
    
    /// <summary>
    /// Adds a star to this system and updates relationships
    /// </summary>
    /// <param name="star">The star to add</param>
    public void AddStar(StarStructure star)
    {
        star.ParentSystemID = this.ID;
        Stars.Add(star);
    }
    
    /// <summary>
    /// Adds a binary pair to this system
    /// </summary>
    /// <param name="pair">The binary pair to add</param>
    public void AddBinaryPair(BinaryPair pair)
    {
        pair.ParentSystemID = this.ID;
        BinaryPairs.Add(pair);
    }
    
    /// <summary>
    /// Sets up the root binary pair
    /// </summary>
    /// <param name="pairID">ID of the root binary pair</param>
    public void SetRootBinaryPair(string pairID)
    {
        RootBinaryPairID = pairID;
    }
    
    /// <summary>
    /// Gets the root binary pair
    /// </summary>
    /// <returns>The root binary pair or null if not found</returns>
    public BinaryPair GetRootBinaryPair()
    {
        if (!string.IsNullOrEmpty(RootBinaryPairID))
        {
            return BinaryPairs.Find(p => p.ID == RootBinaryPairID);
        }
        return null;
    }
    
    /// <summary>
    /// Finds a star by ID
    /// </summary>
    /// <param name="starID">The ID of the star to find</param>
    /// <returns>The found star or null if not found</returns>
    public StarStructure FindStarByID(string starID)
    {
        return Stars.Find(s => s.ID == starID);
    }
    
    /// <summary>
    /// Finds a binary pair by ID
    /// </summary>
    /// <param name="pairID">The ID of the binary pair to find</param>
    /// <returns>The found binary pair or null if not found</returns>
    public BinaryPair FindBinaryPairByID(string pairID)
    {
        return BinaryPairs.Find(p => p.ID == pairID);
    }
    
    /// <summary>
    /// Represents a pair of celestial objects (stars or other binary pairs) in a binary relationship.
    /// </summary>
    [Serializable]
    public class BinaryPair
    {
        /// <summary>Unique identifier</summary>
        public string ID;
        
        /// <summary>ID of the parent system</summary>
        public string ParentSystemID;
        
        /// <summary>ID of the primary object (reference to a star or another binary pair)</summary>
        public string PrimaryID;
        
        /// <summary>Type of the primary object (Star or BinaryPair)</summary>
        public string PrimaryType;
        
        /// <summary>ID of the secondary object (reference to a star or another binary pair)</summary>
        public string SecondaryID;
        
        /// <summary>Type of the secondary object (Star or BinaryPair)</summary>
        public string SecondaryType;
        
        /// <summary>Center of mass between the two objects</summary>
        public Vector3 Barycenter;
        
        /// <summary>Separation distance between primary and secondary in AU</summary>
        public float SeparationDistance;
        
        /// <summary>Semi-major axis of primary's orbit</summary>
        public float PrimaryOrbitRadius;
        
        /// <summary>Semi-major axis of secondary's orbit</summary>
        public float SecondaryOrbitRadius;
        
        /// <summary>Orbital period of the binary pair in Earth years</summary>
        public float OrbitalPeriod;
        
        /// <summary>Orbital zones around both objects where planets can form</summary>
        public OrbitalZones CircumbinaryZones;
        
        /// <summary>Orbits that can exist around both objects</summary>
        public List<OrbitGenerator.Orbit> CircumbinaryOrbits = new List<OrbitGenerator.Orbit>();
        
        /// <summary>Planets that orbit around both objects (circumbinary)</summary>
        public List<Planet> CircumbinaryPlanets = new List<Planet>();
        
        /// <summary>Asteroid belts that orbit around both objects (circumbinary)</summary>
        public List<AsteroidBelt> CircumbinaryAsteroidBelts = new List<AsteroidBelt>();
        
        /// <summary>
        /// Constructor initializes with a unique ID
        /// </summary>
        public BinaryPair()
        {
            ID = Guid.NewGuid().ToString();
            CircumbinaryOrbits = new List<OrbitGenerator.Orbit>();
            CircumbinaryPlanets = new List<Planet>();
            CircumbinaryAsteroidBelts = new List<AsteroidBelt>();
        }
        
        /// <summary>
        /// Gets all stars contained in this binary pair hierarchy.
        /// </summary>
        /// <param name="system">The parent star system</param>
        /// <returns>A list of all stars in the hierarchy</returns>
        public List<StarStructure> GetAllStars(StarSystem system)
        {
            var stars = new List<StarStructure>();
            
            // Add stars from primary
            if (PrimaryType == "Star")
            {
                var primaryStar = system.FindStarByID(PrimaryID);
                if (primaryStar != null)
                {
                    stars.Add(primaryStar);
                }
            }
            else if (PrimaryType == "BinaryPair")
            {
                var primaryPair = system.FindBinaryPairByID(PrimaryID);
                if (primaryPair != null)
                {
                    stars.AddRange(primaryPair.GetAllStars(system));
                }
            }
            
            // Add stars from secondary
            if (SecondaryType == "Star")
            {
                var secondaryStar = system.FindStarByID(SecondaryID);
                if (secondaryStar != null)
                {
                    stars.Add(secondaryStar);
                }
            }
            else if (SecondaryType == "BinaryPair")
            {
                var secondaryPair = system.FindBinaryPairByID(SecondaryID);
                if (secondaryPair != null)
                {
                    stars.AddRange(secondaryPair.GetAllStars(system));
                }
            }
            
            return stars;
        }
        
        /// <summary>
        /// Sets the primary object
        /// </summary>
        /// <param name="id">ID of the object</param>
        /// <param name="type">Type of the object (Star or BinaryPair)</param>
        public void SetPrimary(string id, string type)
        {
            PrimaryID = id;
            PrimaryType = type;
        }
        
        /// <summary>
        /// Sets the secondary object
        /// </summary>
        /// <param name="id">ID of the object</param>
        /// <param name="type">Type of the object (Star or BinaryPair)</param>
        public void SetSecondary(string id, string type)
        {
            SecondaryID = id;
            SecondaryType = type;
        }
        
        /// <summary>
        /// Adds a circumbinary planet
        /// </summary>
        /// <param name="planet">The planet to add</param>
        public void AddCircumbinaryPlanet(Planet planet)
        {
            planet.ParentBinaryPairID = this.ID;
            CircumbinaryPlanets.Add(planet);
        }
        
        /// <summary>
        /// Adds a circumbinary asteroid belt
        /// </summary>
        /// <param name="belt">The asteroid belt to add</param>
        public void AddCircumbinaryAsteroidBelt(AsteroidBelt belt)
        {
            belt.ParentBinaryPairID = this.ID;
            CircumbinaryAsteroidBelts.Add(belt);
        }
    }
}