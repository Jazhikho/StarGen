using System;
using System.Collections.Generic;
using UnityEngine;

namespace SimpleKeplerOrbits.SystemBuilder
{
    [Serializable]
    public class CelestialBodyData
    {
        public string name;
        public CelestialBodyType bodyType;
        public float mass;
        public float scale;
        public Color color;
        
        // Orbital parameters
        public float semiMajorAxis;
        public float eccentricity;
        public float inclination;
        public float argumentOfPerifocus;
        public float longitudeOfAscendingNode;
        public float meanAnomaly;
        
        // Children
        [SerializeReference]
        public List<CelestialBodyData> satellites = new List<CelestialBodyData>();
    }
    
    public enum CelestialBodyType
    {
        Barycenter,
        Star,
        Planet,
        Moon,
        Asteroid
    }
    
    [Serializable]
    public class StarSystemData
    {
        public string systemName;
        public float gravityConstant = 100f;
        public CelestialBodyData rootBody;
    }
}