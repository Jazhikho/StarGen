using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public abstract class OrbitalBody
{
    // Basic Physical Properties
    public string ID;
    public float Mass; // In Earth masses
    public float Radius; // In Earth radii
    public float Density; // In Earth densities
    public float SurfaceGravity; // In Earth g's
    public float RotationPeriod; // In Earth days
    public bool TidallyLocked;
    public float AxialTilt; // In degrees

    // Orbital Parameters
    public float Eccentricity;
    public float Inclination; // In degrees
    public Vector3 Position; // Position in the system

    // Atmospheric and Surface Properties
    public bool HasAtmosphere;
    public float AtmosphericPressure; // In Earth atmospheres
    [SerializeField]
    public Dictionary<string, float> AtmosphericComposition = new Dictionary<string, float>();
    public float SurfaceTemperature; // In Kelvin
    public float DaytimeTemperature;
    public float NighttimeTemperature;
    public string[] BiomeTypes = new string[0];
    public float Albedo;

    // Geological Properties
    public float TectonicActivity;  // 0 to 1
    public float VolcanicActivity;  // 0 to 1
    public float MagneticFieldStrength;  // 0 to 1
    public string PlanetType;
    public float WaterCoverage; // 0 to 1

    // Habitability
    public bool IsHabitable;
    public float HabitabilityIndex;
    public bool IsTerraformable;
    public float TerraformingIndex;
    public string[] TerraformingChallenges = new string[0];

    // The Population structure class will be used directly once updated
    public PopulationStructure.Population Population;
    // public BiosphereStructure Biosphere;
    
    // Serializable alternative for Dictionary<string, float> for resources
    [Serializable]
    public class ResourceEntry
    {
        public string ResourceName;
        public float Amount;
        
        public ResourceEntry(string name, float amount)
        {
            ResourceName = name;
            Amount = amount;
        }
    }
    
    [SerializeField]
    private List<ResourceEntry> _resourcesList = new List<ResourceEntry>();
    
    // Runtime dictionary for efficient lookups
    [NonSerialized]
    private Dictionary<string, float> _resources = new Dictionary<string, float>();
    
    public float ResourceValue;

    protected OrbitalBody()
    {
        ID = System.Guid.NewGuid().ToString();
        _resourcesList = new List<ResourceEntry>();
        _resources = new Dictionary<string, float>();
        AtmosphericComposition = new Dictionary<string, float>();
    }
    
    /// <summary>
    /// Add a resource to this body
    /// </summary>
    /// <param name="resourceName">Resource name</param>
    /// <param name="amount">Resource amount</param>
    public void AddResource(string resourceName, float amount)
    {
        _resources[resourceName] = amount;
        
        // Update the serializable list
        var existing = _resourcesList.Find(r => r.ResourceName == resourceName);
        if (existing != null)
        {
            existing.Amount = amount;
        }
        else
        {
            _resourcesList.Add(new ResourceEntry(resourceName, amount));
        }
    }
    
    /// <summary>
    /// Get the amount of a specific resource
    /// </summary>
    /// <param name="resourceName">Resource name</param>
    /// <returns>Amount of the resource, or 0 if not found</returns>
    public float GetResourceAmount(string resourceName)
    {
        if (_resources.ContainsKey(resourceName))
        {
            return _resources[resourceName];
        }
        return 0f;
    }
    
    /// <summary>
    /// Get all resources as a dictionary
    /// </summary>
    /// <returns>Dictionary of resource names and amounts</returns>
    public Dictionary<string, float> GetResources()
    {
        RebuildResourcesDictionary();
        return _resources;
    }
    
    /// <summary>
    /// Rebuild the resources dictionary from the serializable list
    /// </summary>
    public void RebuildResourcesDictionary()
    {
        _resources.Clear();
        foreach (var entry in _resourcesList)
        {
            _resources[entry.ResourceName] = entry.Amount;
        }
    }
}

[Serializable]
public class Planet : OrbitalBody
{
    // Planet-specific properties
    public float SemiMajorAxis; // In AU
    public float OrbitalPeriod; // In Earth years

    // Parent references
    public string ParentStarID;  // ID of parent star for later lookup
    public string ParentSystemID;  // ID of parent system for later lookup
    public string ParentBinaryPairID; // ID of parent binary pair (for circumbinary planets)
    
    // Structural Components
    [SerializeField]
    public List<Moon> Moons = new List<Moon>();
    
    public bool HasRings;
    public RingSystem Rings;
    
    [Serializable]
    public class RingSystem
    {
        public enum RingComplexity
        {
            None,
            Simple,      // One main ring
            Moderate,    // 2-3 distinct rings
            Complex      // Multiple rings with gaps and variations
        }

        public bool HasRings;
        public RingComplexity Complexity;
        public float InnerRadius;  // In planet radii
        public float OuterRadius;  // In planet radii
        public float TotalMass;    // In Earth moon masses
        public float Opacity;      // 0-1
        
        // To handle serialization of the composition dictionary
        [Serializable]
        public class CompositionEntry
        {
            public string Material;
            public float Percentage;
            
            public CompositionEntry(string material, float percentage)
            {
                Material = material;
                Percentage = percentage;
            }
        }
        
        [SerializeField]
        private List<CompositionEntry> _compositionList = new List<CompositionEntry>();
        
        [NonSerialized]
        private Dictionary<string, float> _composition = new Dictionary<string, float>();
        
        [SerializeField]
        public List<RingGap> Gaps = new List<RingGap>();
        
        [Serializable]
        public class RingGap
        {
            public float InnerRadius;  // In planet radii
            public float OuterRadius;  // In planet radii
            public string Name;
        }
        
        public RingSystem()
        {
            HasRings = false;
            Complexity = RingComplexity.None;
            _compositionList = new List<CompositionEntry>();
            Gaps = new List<RingGap>();
        }
        
        /// <summary>
        /// Add material to the ring composition
        /// </summary>
        /// <param name="material">Material name</param>
        /// <param name="percentage">Percentage of the material</param>
        public void AddComposition(string material, float percentage)
        {
            _composition[material] = percentage;
            
            // Update the serializable list
            var existing = _compositionList.Find(c => c.Material == material);
            if (existing != null)
            {
                existing.Percentage = percentage;
            }
            else
            {
                _compositionList.Add(new CompositionEntry(material, percentage));
            }
        }
        
        /// <summary>
        /// Get the composition dictionary
        /// </summary>
        /// <returns>Dictionary of material names and percentages</returns>
        public Dictionary<string, float> GetComposition()
        {
            RebuildCompositionDictionary();
            return _composition;
        }
        
        /// <summary>
        /// Rebuild the composition dictionary from the serializable list
        /// </summary>
        public void RebuildCompositionDictionary()
        {
            _composition.Clear();
            foreach (var entry in _compositionList)
            {
                _composition[entry.Material] = entry.Percentage;
            }
        }
    }

    public Planet() : base()
    {
        Rings = new RingSystem();
        Moons = new List<Moon>();
    }

    /// <summary>
    /// Adds a moon to this planet
    /// </summary>
    /// <param name="moon">The moon to add</param>
    public void AddMoon(Moon moon)
    {
        moon.ParentPlanetID = this.ID;
        Moons.Add(moon);
    }
    
    /// <summary>
    /// Finds a moon by ID
    /// </summary>
    /// <param name="moonID">The ID of the moon to find</param>
    /// <returns>The found moon or null if not found</returns>
    public Moon FindMoonByID(string moonID)
    {
        return Moons.Find(m => m.ID == moonID);
    }
}

[Serializable]
public class Moon : OrbitalBody
{
    // Moon-specific properties
    public float OrbitalDistance; // In parent planet radii
    public float OrbitalPeriod; // In Earth days
    
    // Tidal Properties
    public float TidalHeating; // In W/m²
    public float OrbitalResonance; // Ratio with other moons
    
    // Parent Reference
    public string ParentPlanetID;
    public string ParentSystemID;

    public Moon() : base()
    {
    }
}

[Serializable]
public class AsteroidBelt
{
    // Belt Properties
    public string ID;
    public float InnerRadius; // In AU
    public float OuterRadius; // In AU
    public float Thickness; // In AU
    public float AverageDensity; // Objects per cubic AU
    public float TotalMass; // In Earth masses

    // Parent References
    public string ParentSystemID;
    public string ParentStarID;
    public string ParentBinaryPairID;

    // Additional Properties
    public float AverageObjectSize; // In kilometers
    public bool HasDwarfPlanets;
    public float EconomicValue;
    public bool NavigationHazard;
    
    // To handle serialization of the composition dictionaries
    [Serializable]
    public class MaterialEntry
    {
        public string Material;
        public float Percentage;
        
        public MaterialEntry(string material, float percentage)
        {
            Material = material;
            Percentage = percentage;
        }
    }
    
    [Serializable]
    public class ResourceEntry
    {
        public string Resource;
        public float Concentration;
        
        public ResourceEntry(string resource, float concentration)
        {
            Resource = resource;
            Concentration = concentration;
        }
    }
    
    [SerializeField]
    private List<MaterialEntry> _materialCompositionList = new List<MaterialEntry>();
    
    [SerializeField]
    private List<ResourceEntry> _resourceConcentrationList = new List<ResourceEntry>();
    
    [NonSerialized]
    private Dictionary<string, float> _materialComposition = new Dictionary<string, float>();
    
    [NonSerialized]
    private Dictionary<string, float> _resourceConcentration = new Dictionary<string, float>();

    // Notable Objects
    [SerializeField]
    public List<AsteroidBeltObject> NotableObjects = new List<AsteroidBeltObject>();

    [Serializable]
    public class AsteroidBeltObject
    {
        public string ID;
        public float Radius; // In kilometers
        public float OrbitalDistance; // In AU
        public float OrbitalAngle; // In radians
        public bool IsObjectOfInterest;
        
        // Serializable list for composition
        [SerializeField]
        private List<MaterialEntry> _compositionList = new List<MaterialEntry>();
        
        [NonSerialized]
        private Dictionary<string, float> _composition = new Dictionary<string, float>();

        public AsteroidBeltObject()
        {
            ID = Guid.NewGuid().ToString();
            _compositionList = new List<MaterialEntry>();
        }
        
        /// <summary>
        /// Add material to the object's composition
        /// </summary>
        /// <param name="material">Material name</param>
        /// <param name="percentage">Percentage of the material</param>
        public void AddComposition(string material, float percentage)
        {
            _composition[material] = percentage;
            
            // Update the serializable list
            var existing = _compositionList.Find(c => c.Material == material);
            if (existing != null)
            {
                existing.Percentage = percentage;
            }
            else
            {
                _compositionList.Add(new MaterialEntry(material, percentage));
            }
        }
        
        /// <summary>
        /// Get the composition dictionary
        /// </summary>
        /// <returns>Dictionary of material names and percentages</returns>
        public Dictionary<string, float> GetComposition()
        {
            RebuildCompositionDictionary();
            return _composition;
        }
        
        /// <summary>
        /// Rebuild the composition dictionary from the serializable list
        /// </summary>
        public void RebuildCompositionDictionary()
        {
            _composition.Clear();
            foreach (var entry in _compositionList)
            {
                _composition[entry.Material] = entry.Percentage;
            }
        }
    }

    public AsteroidBelt()
    {
        ID = Guid.NewGuid().ToString();
        NotableObjects = new List<AsteroidBeltObject>();
        _materialCompositionList = new List<MaterialEntry>();
        _resourceConcentrationList = new List<ResourceEntry>();
    }
    
    /// <summary>
    /// Add material to the belt's composition
    /// </summary>
    /// <param name="material">Material name</param>
    /// <param name="percentage">Percentage of the material</param>
    public void AddMaterialComposition(string material, float percentage)
    {
        _materialComposition[material] = percentage;
        
        // Update the serializable list
        var existing = _materialCompositionList.Find(c => c.Material == material);
        if (existing != null)
        {
            existing.Percentage = percentage;
        }
        else
        {
            _materialCompositionList.Add(new MaterialEntry(material, percentage));
        }
    }
    
    /// <summary>
    /// Add resource to the belt's resource concentration
    /// </summary>
    /// <param name="resource">Resource name</param>
    /// <param name="concentration">Concentration of the resource</param>
    public void AddResourceConcentration(string resource, float concentration)
    {
        _resourceConcentration[resource] = concentration;
        
        // Update the serializable list
        var existing = _resourceConcentrationList.Find(r => r.Resource == resource);
        if (existing != null)
        {
            existing.Concentration = concentration;
        }
        else
        {
            _resourceConcentrationList.Add(new ResourceEntry(resource, concentration));
        }
    }
    
    /// <summary>
    /// Get the material composition dictionary
    /// </summary>
    /// <returns>Dictionary of material names and percentages</returns>
    public Dictionary<string, float> GetMaterialComposition()
    {
        RebuildMaterialCompositionDictionary();
        return _materialComposition;
    }
    
    /// <summary>
    /// Get the resource concentration dictionary
    /// </summary>
    /// <returns>Dictionary of resource names and concentrations</returns>
    public Dictionary<string, float> GetResourceConcentration()
    {
        RebuildResourceConcentrationDictionary();
        return _resourceConcentration;
    }
    
    /// <summary>
    /// Rebuild the material composition dictionary from the serializable list
    /// </summary>
    public void RebuildMaterialCompositionDictionary()
    {
        _materialComposition.Clear();
        foreach (var entry in _materialCompositionList)
        {
            _materialComposition[entry.Material] = entry.Percentage;
        }
    }
    
    /// <summary>
    /// Rebuild the resource concentration dictionary from the serializable list
    /// </summary>
    public void RebuildResourceConcentrationDictionary()
    {
        _resourceConcentration.Clear();
        foreach (var entry in _resourceConcentrationList)
        {
            _resourceConcentration[entry.Resource] = entry.Concentration;
        }
    }
    
    /// <summary>
    /// Add a notable object to the belt
    /// </summary>
    /// <param name="obj">The object to add</param>
    public void AddNotableObject(AsteroidBeltObject obj)
    {
        NotableObjects.Add(obj);
    }
}