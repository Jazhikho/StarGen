using System.Collections.Generic;
using UnityEngine;

namespace Libraries
{
    [System.Serializable]
    public static class PlanetLibrary
    {
        [System.Serializable]
        public enum PlanetType
        {
            Metian,         // Mercury-like worlds
            Menoetian,      // Barren terrestrials
            Promethean,     // Arid terrestrials
            Tethysian,      // Mesic terrestrials
            Gaian,          // Aquatic terrestrials
            Oceanian,       // Oceanic terrestrials
            Phoeboan,       // Ice dwarfs (renamed from Phoebean)
            Rhean,          // Super-Earths inside frost line
            Dionean,        // Icy Super-Earths
            Criusian,       // Hot mini-Neptunes
            Theian,         // Cold mini-Neptunes
            Iapetian,       // Ice giants
            Helian,         // Helium planets (renamed from Phoebian)
            Lelantian,      // Carbon planets
            Hyperion,       // Hot gas giants
            Atlantean,      // Cold gas giants
            Vulcanian,      // Extreme volcanic/epistellar planets
            Cronusian,      // Stripped gas giant cores
            Asterian        // Asteroid belt objects
        }

        [System.Serializable]
        public enum ChemistryType
        {
            Silicate,       // Silicon-oxygen based
            Carbon,         // Carbon-dominated
            Iron,           // Iron-dominated
            Water,          // Water-dominated
            Hydrogen,       // Hydrogen-dominated
            Helium,         // Helium-dominated
            Ammonia,        // Ammonia-based
            Sulfur,         // Sulfur-dominated
            Methane         // Methane-dominated
        }

        [System.Serializable]
        public enum BiomeType
        {
            // — Terrestrial Biomes —
            RockyPlains,         // flat, barren rock
            HighPlains,          // high-altitude grasslands
            LowMountains,        // low-altitude mountains
            HighMountains,       // high-relief, orogenic belts
            Desert,              // arid sand/rock
            Grassland,           // savanna/steppe
            Woodland,            // open-canopy forests
            DenseForest,         // closed-canopy forests
            Rainforest,          // wet, tropical/subtropical
            Wetlands,            // marshes, swamps

            // — Polar Biomes —
            Tundra,              // permafrost meadows
            IceSheet,            // continental ice caps
            GlacialPlains,       // glacial outwash/till

            // — Oceanic Biomes —
            CoastalZone,         // shallow littoral
            CoralReef,           // biogenic carbonate ridges
            PelagicZone,         // open-ocean surface
            AbyssalPlain,        // deep-ocean floor
            HydrothermalFields,  // deep-sea vents & seeps

            // - Other Aquatic Biomes -
            FreshwaterLake,      // freshwater
            BrackishLake,        // brackish water
            FreshwaterRiver,     // freshwater river
            SaltWaterRiver,      // saltwater river

            // — Volcanic & Geothermal —
            LavaFields,          // recent flows
            VolcanicPlateau,     // shield/trapp terrain
            AcidSpringFields,    // sulfuric/acid pools
            CryovolcanicFields,  // icy‐volcanic ejecta

            // — Gas & Ice Giant Layers —
            CloudBands,          // alternating belts/zones
            ExoticCloudDeck,     // high-altitude hazes
            MethaneLakeRegion,   // hydrocarbon seas/shores
            AmmoniaCloudLayer,   // ammonia‐rich deck

            // — Small-Body Surfaces —
            RegolithPlains,      // dusty/impact-gardens
            MetallicCrust,       // exposed metal cores
            CrystallineVeins     // mineral‐crystal outcrops
        }

        [System.Serializable]
        public enum ResourceType
        {
            Water, Metals, RareMetals, Radioactives, Organics, Gases, Crystals, ExoticMatter
        }

        [System.Serializable]
        public class AtmosphereTemplate
        {
            public Dictionary<string, float> Composition { get; set; }
            public (float min, float max) PressureRange { get; set; }
        }

        [System.Serializable]
        public class PlanetTypeData
        {
            // Basic physical properties
            public (float min, float max) MassRange { get; set; }
            public (float min, float max) DensityRange { get; set; }
            public (float min, float max) AlbedoRange { get; set; }
            public (float min, float max) WaterContentRange { get; set; }
            
            // Temperature and atmosphere
            public float TemperatureModifier { get; set; }
            public float AtmosphereProbability { get; set; }
            public float AtmospherePressureMod { get; set; }
            
            // Chemistry and water
            public ChemistryType[] ChemistryTypes { get; set; }
            
            // Geography
            public BiomeType[] PossibleBiomes { get; set; }
            
            // Geology
            public (float min, float max) TectonicActivityRange { get; set; }
            public (float min, float max) VolcanicActivityRange { get; set; }
            public float MagneticFieldModifier { get; set; }
            public float RingProbability { get; set; }
            
            // Resources
            public Dictionary<ResourceType, float> ResourceProbabilities { get; set; }
            
            // Frost line data
            public bool InsideFrostLine { get; set; }
        }

        public static readonly Dictionary<PlanetType, PlanetTypeData> PlanetData = 
            new Dictionary<PlanetType, PlanetTypeData>
        {
            { PlanetType.Metian, new PlanetTypeData {
                MassRange = (0.01f, 0.1f),
                DensityRange = (0.9f, 1.3f),
                AlbedoRange = (0.1f, 0.2f),
                WaterContentRange = (0f, 0.01f),
                TemperatureModifier = 1.3f,
                AtmosphereProbability = 0.1f,
                AtmospherePressureMod = 0.1f,
                ChemistryTypes = new[] { ChemistryType.Iron, ChemistryType.Silicate },
                PossibleBiomes = new[] { BiomeType.RegolithPlains, BiomeType.RockyPlains },
                TectonicActivityRange = (0.1f, 0.3f),
                VolcanicActivityRange = (0.1f, 0.4f),
                MagneticFieldModifier = 0.1f,
                RingProbability = 0.01f,
                InsideFrostLine = true,
                ResourceProbabilities = new Dictionary<ResourceType, float> {
                    { ResourceType.Metals, 0.8f },
                    { ResourceType.RareMetals, 0.3f }
                }
            }},
            
            { PlanetType.Menoetian, new PlanetTypeData {
                MassRange = (0.1f, 1.0f),
                DensityRange = (0.8f, 1.2f),
                AlbedoRange = (0.1f, 0.3f),
                WaterContentRange = (0f, 0.01f),
                TemperatureModifier = 1.1f,
                AtmosphereProbability = 0.3f,
                AtmospherePressureMod = 0.2f,
                ChemistryTypes = new[] { ChemistryType.Silicate, ChemistryType.Iron },
                PossibleBiomes = new[] { BiomeType.RegolithPlains, BiomeType.RockyPlains,
                    BiomeType.Desert },
                TectonicActivityRange = (0.2f, 0.5f),
                VolcanicActivityRange = (0.2f, 0.5f),
                MagneticFieldModifier = 0.3f,
                RingProbability = 0.01f,
                InsideFrostLine = true,
                ResourceProbabilities = new Dictionary<ResourceType, float> {
                    { ResourceType.Metals, 0.7f },
                    { ResourceType.RareMetals, 0.2f },
                    { ResourceType.Radioactives, 0.1f }
                }
            }},
            
            { PlanetType.Promethean, new PlanetTypeData {
                MassRange = (0.3f, 1.5f),
                DensityRange = (0.8f, 1.1f),
                AlbedoRange = (0.15f, 0.3f),
                WaterContentRange = (0.01f, 0.25f),
                TemperatureModifier = 1.05f,
                AtmosphereProbability = 0.7f,
                AtmospherePressureMod = 0.6f,
                ChemistryTypes = new[] { ChemistryType.Silicate },
                PossibleBiomes = new[] { BiomeType.Desert, BiomeType.HighMountains,
                    BiomeType.LowMountains, BiomeType.RockyPlains },
                TectonicActivityRange = (0.3f, 0.7f),
                VolcanicActivityRange = (0.3f, 0.6f),
                MagneticFieldModifier = 0.5f,
                RingProbability = 0.05f,
                InsideFrostLine = true,
                ResourceProbabilities = new Dictionary<ResourceType, float> {
                    { ResourceType.Water, 0.3f },
                    { ResourceType.Metals, 0.8f },
                    { ResourceType.RareMetals, 0.3f }
                }
            }},
            
            { PlanetType.Tethysian, new PlanetTypeData {
                MassRange = (0.5f, 1.5f),
                DensityRange = (0.8f, 1.1f),
                AlbedoRange = (0.2f, 0.35f),
                WaterContentRange = (0.26f, 0.5f),
                TemperatureModifier = 1.0f,
                AtmosphereProbability = 0.9f,
                AtmospherePressureMod = 0.8f,
                ChemistryTypes = new[] { ChemistryType.Silicate, ChemistryType.Water },
                PossibleBiomes = new[] { BiomeType.Grassland, BiomeType.Desert,
                    BiomeType.HighMountains, BiomeType.LowMountains, BiomeType.RockyPlains,
                    BiomeType.Woodland },
                TectonicActivityRange = (0.4f, 0.8f),
                VolcanicActivityRange = (0.3f, 0.7f),
                MagneticFieldModifier = 0.8f,
                RingProbability = 0.05f,
                InsideFrostLine = true,
                ResourceProbabilities = new Dictionary<ResourceType, float> {
                    { ResourceType.Water, 0.6f },
                    { ResourceType.Metals, 0.7f },
                    { ResourceType.Organics, 0.5f }
                }
            }},
            
            { PlanetType.Gaian, new PlanetTypeData {
                MassRange = (0.5f, 1.5f),
                DensityRange = (0.8f, 1.1f),
                AlbedoRange = (0.25f, 0.35f),
                WaterContentRange = (0.5f, 0.9f),
                TemperatureModifier = 1.0f,
                AtmosphereProbability = 0.95f,
                AtmospherePressureMod = 1.0f,
                ChemistryTypes = new[] { ChemistryType.Silicate, ChemistryType.Water },
                PossibleBiomes = new[] { BiomeType.RockyPlains, BiomeType.HighPlains,
                    BiomeType.LowMountains, BiomeType.HighMountains, BiomeType.Desert,
                    BiomeType.Grassland, BiomeType.Woodland, BiomeType.Rainforest,
                    BiomeType.DenseForest, BiomeType.Wetlands, BiomeType.Tundra,
                    BiomeType.IceSheet, BiomeType.GlacialPlains, BiomeType.CoastalZone,
                    BiomeType.CoralReef, BiomeType.PelagicZone, BiomeType.AbyssalPlain,
                    BiomeType.HydrothermalFields, BiomeType.FreshwaterLake,
                    BiomeType.BrackishLake, BiomeType.FreshwaterRiver, BiomeType.SaltWaterRiver
                },
                TectonicActivityRange = (0.5f, 0.9f),
                VolcanicActivityRange = (0.4f, 0.8f),
                MagneticFieldModifier = 1.0f,
                RingProbability = 0.01f,
                InsideFrostLine = true,
                ResourceProbabilities = new Dictionary<ResourceType, float> {
                    { ResourceType.Water, 0.9f },
                    { ResourceType.Metals, 0.7f },
                    { ResourceType.Organics, 0.9f }
                }
            }},
            
            { PlanetType.Oceanian, new PlanetTypeData {
                MassRange = (0.8f, 5.0f),
                DensityRange = (0.6f, 0.9f),
                AlbedoRange = (0.25f, 0.35f),
                WaterContentRange = (0.9f, 1.0f),
                TemperatureModifier = 1.0f,
                AtmosphereProbability = 0.95f,
                AtmospherePressureMod = 1.2f,
                ChemistryTypes = new[] { ChemistryType.Water, ChemistryType.Silicate },
                PossibleBiomes = new[] { BiomeType.CoralReef, BiomeType.PelagicZone,
                    BiomeType.AbyssalPlain, BiomeType.HydrothermalFields,
                    BiomeType.IceSheet, BiomeType.CoastalZone },
                TectonicActivityRange = (0.3f, 0.6f),
                VolcanicActivityRange = (0.2f, 0.5f),
                MagneticFieldModifier = 0.8f,
                RingProbability = 0.05f,
                InsideFrostLine = true,
                ResourceProbabilities = new Dictionary<ResourceType, float> {
                    { ResourceType.Water, 1.0f },
                    { ResourceType.Organics, 0.8f },
                    { ResourceType.Gases, 0.3f }
                }
            }},
            
            { PlanetType.Phoeboan, new PlanetTypeData {
                MassRange = (0.1f, 1.0f),
                DensityRange = (0.6f, 0.9f),
                AlbedoRange = (0.4f, 0.6f),
                WaterContentRange = (0.6f, 0.9f),
                TemperatureModifier = 0.7f,
                AtmosphereProbability = 0.3f,
                AtmospherePressureMod = 0.2f,
                ChemistryTypes = new[] { ChemistryType.Water, ChemistryType.Ammonia, ChemistryType.Methane },
                PossibleBiomes = new[] { BiomeType.IceSheet, BiomeType.Tundra, BiomeType.GlacialPlains },
                TectonicActivityRange = (0.1f, 0.3f),
                VolcanicActivityRange = (0.0f, 0.2f),
                MagneticFieldModifier = 0.2f,
                RingProbability = 0.05f,
                InsideFrostLine = false,
                ResourceProbabilities = new Dictionary<ResourceType, float> {
                    { ResourceType.Water, 0.9f },
                    { ResourceType.Gases, 0.3f },
                    { ResourceType.Crystals, 0.2f }
                }
            }},
            
            { PlanetType.Rhean, new PlanetTypeData {
                MassRange = (1.5f, 10.0f),
                DensityRange = (0.8f, 1.2f),
                AlbedoRange = (0.2f, 0.35f),
                WaterContentRange = (0.26f, 0.9f),
                TemperatureModifier = 1.1f,
                AtmosphereProbability = 0.95f,
                AtmospherePressureMod = 2.0f,
                ChemistryTypes = new[] { ChemistryType.Silicate, ChemistryType.Water },
                PossibleBiomes = new[] {  BiomeType.RockyPlains, BiomeType.HighPlains,
                    BiomeType.LowMountains, BiomeType.HighMountains, BiomeType.Desert,
                    BiomeType.Grassland, BiomeType.Woodland, BiomeType.Rainforest,
                    BiomeType.DenseForest, BiomeType.Wetlands, BiomeType.Tundra,
                    BiomeType.IceSheet, BiomeType.GlacialPlains, BiomeType.CoastalZone,
                    BiomeType.CoralReef, BiomeType.PelagicZone, BiomeType.AbyssalPlain,
                    BiomeType.HydrothermalFields, BiomeType.FreshwaterLake,
                    BiomeType.BrackishLake, BiomeType.FreshwaterRiver, BiomeType.SaltWaterRiver
                },
                TectonicActivityRange = (0.6f, 1.0f),
                VolcanicActivityRange = (0.5f, 0.9f),
                MagneticFieldModifier = 1.5f,
                RingProbability = 0.1f,
                InsideFrostLine = true,
                ResourceProbabilities = new Dictionary<ResourceType, float> {
                    { ResourceType.Water, 0.8f },
                    { ResourceType.Metals, 0.8f },
                    { ResourceType.RareMetals, 0.4f },
                    { ResourceType.Organics, 0.7f }
                }
            }},
            
            { PlanetType.Dionean, new PlanetTypeData {
                MassRange = (1.5f,  10.0f),
                DensityRange = (0.6f, 0.9f),
                AlbedoRange = (0.4f, 0.6f),
                WaterContentRange = (0.7f, 0.95f),
                TemperatureModifier = 0.8f,
                AtmosphereProbability = 0.8f,
                AtmospherePressureMod = 1.5f,
                ChemistryTypes = new[] { ChemistryType.Water, ChemistryType.Methane, ChemistryType.Ammonia },
                PossibleBiomes = new[] {  BiomeType.HighMountains, BiomeType.Tundra,
                    BiomeType.IceSheet, BiomeType.GlacialPlains, BiomeType.PelagicZone,
                    BiomeType.AbyssalPlain, BiomeType.HydrothermalFields
                },
                TectonicActivityRange = (0.2f, 0.5f),
                VolcanicActivityRange = (0.1f, 0.4f),
                MagneticFieldModifier = 1.0f,
                RingProbability = 0.2f,
                InsideFrostLine = false,
                ResourceProbabilities = new Dictionary<ResourceType, float> {
                    { ResourceType.Water, 0.9f },
                    { ResourceType.Gases, 0.6f },
                    { ResourceType.Organics, 0.5f },
                    { ResourceType.Crystals, 0.3f }
                }
            }},
            
            { PlanetType.Criusian, new PlanetTypeData {
                MassRange = (2.0f, 10.0f),
                DensityRange = (0.3f, 0.6f),
                AlbedoRange = (0.2f, 0.4f),
                WaterContentRange = (0.4f, 0.7f),
                TemperatureModifier = 1.2f,
                AtmosphereProbability = 1.0f,
                AtmospherePressureMod = 10.0f,
                ChemistryTypes = new[] { ChemistryType.Hydrogen, ChemistryType.Helium, ChemistryType.Methane },
                PossibleBiomes = new[] { BiomeType.CloudBands, BiomeType.ExoticCloudDeck },
                TectonicActivityRange = (0.0f, 0.0f),
                VolcanicActivityRange = (0.0f, 0.0f),
                MagneticFieldModifier = 2.0f,
                RingProbability = 0.2f,
                InsideFrostLine = true,
                ResourceProbabilities = new Dictionary<ResourceType, float> {
                    { ResourceType.Gases, 0.9f },
                    { ResourceType.ExoticMatter, 0.1f }
                }
            }},
            
            { PlanetType.Theian, new PlanetTypeData {
                MassRange = (2.0f, 10.0f),
                DensityRange = (0.3f, 0.6f),
                AlbedoRange = (0.3f, 0.5f),
                WaterContentRange = (0.5f, 0.8f),
                TemperatureModifier = 0.8f,
                AtmosphereProbability = 1.0f,
                AtmospherePressureMod = 8.0f,
                ChemistryTypes = new[] { ChemistryType.Hydrogen, ChemistryType.Methane, ChemistryType.Ammonia },
                PossibleBiomes = new[] { BiomeType.AmmoniaCloudLayer, BiomeType.MethaneLakeRegion },
                TectonicActivityRange = (0.0f, 0.0f),
                VolcanicActivityRange = (0.0f, 0.0f),
                MagneticFieldModifier = 1.8f,
                RingProbability = 0.3f,
                InsideFrostLine = false,
                ResourceProbabilities = new Dictionary<ResourceType, float> {
                    { ResourceType.Gases, 0.9f },
                    { ResourceType.ExoticMatter, 0.1f }
                }
            }},
            
            { PlanetType.Iapetian, new PlanetTypeData {
                MassRange = (10.0f, 30.0f),
                DensityRange = (0.2f, 0.4f),
                AlbedoRange = (0.3f, 0.5f),
                WaterContentRange = (0.6f, 0.9f),
                TemperatureModifier = 0.7f,
                AtmosphereProbability = 1.0f,
                AtmospherePressureMod = 30.0f,
                ChemistryTypes = new[] { ChemistryType.Hydrogen, ChemistryType.Methane, ChemistryType.Ammonia },
                PossibleBiomes = new[] { BiomeType.AmmoniaCloudLayer, BiomeType.MethaneLakeRegion, 
                    BiomeType.CryovolcanicFields, BiomeType.IceSheet },
                TectonicActivityRange = (0.0f, 0.0f),
                VolcanicActivityRange = (0.0f, 0.0f),
                MagneticFieldModifier = 5.0f,
                RingProbability = 0.6f,
                InsideFrostLine = false,
                ResourceProbabilities = new Dictionary<ResourceType, float> {
                    { ResourceType.Gases, 1.0f },
                    { ResourceType.ExoticMatter, 0.2f }
                }
            }},
            
            { PlanetType.Helian, new PlanetTypeData { 
                MassRange = (20.0f, 60.0f),
                DensityRange = (0.1f, 0.2f),
                AlbedoRange = (0.1f, 0.3f),
                WaterContentRange = (0.0f, 0.1f),
                TemperatureModifier = 1.0f,
                AtmosphereProbability = 1.0f,
                AtmospherePressureMod = 50.0f,
                ChemistryTypes = new[] { ChemistryType.Helium, ChemistryType.Hydrogen },
                PossibleBiomes = new[] { BiomeType.ExoticCloudDeck },
                TectonicActivityRange = (0.0f, 0.0f),
                VolcanicActivityRange = (0.0f, 0.0f),
                MagneticFieldModifier = 4.0f,
                RingProbability = 0.5f,
                InsideFrostLine = false,
                ResourceProbabilities = new Dictionary<ResourceType, float> {
                    { ResourceType.Gases, 1.0f },
                    { ResourceType.ExoticMatter, 0.3f }
                }
            }},
            
            { PlanetType.Lelantian, new PlanetTypeData {
                MassRange = (0.5f, 8.0f),
                DensityRange = (1.1f, 1.5f),
                AlbedoRange = (0.1f, 0.3f),
                WaterContentRange = (0.0f, 0.1f),
                TemperatureModifier = 1.1f,
                AtmosphereProbability = 0.7f,
                AtmospherePressureMod = 0.8f,
                ChemistryTypes = new[] { ChemistryType.Carbon, ChemistryType.Silicate },
                PossibleBiomes = new[] { BiomeType.RockyPlains, BiomeType.CrystallineVeins },
                TectonicActivityRange = (0.3f, 0.7f),
                VolcanicActivityRange = (0.3f, 0.7f),
                MagneticFieldModifier = 0.7f,
                RingProbability = 0.05f,
                InsideFrostLine = true,
                ResourceProbabilities = new Dictionary<ResourceType, float> {
                    { ResourceType.Metals, 0.6f },
                    { ResourceType.Crystals, 0.8f },
                    { ResourceType.RareMetals, 0.4f }
                }
            }},
            
            { PlanetType.Hyperion, new PlanetTypeData {
                MassRange = (30.0f, 300.0f),
                DensityRange = (0.1f, 0.3f),
                AlbedoRange = (0.15f, 0.25f),
                WaterContentRange = (0.0f, 0.1f),
                TemperatureModifier = 1.3f,
                AtmosphereProbability = 1.0f,
                AtmospherePressureMod = 100.0f,
                ChemistryTypes = new[] { ChemistryType.Hydrogen, ChemistryType.Helium },
                PossibleBiomes = new[] { BiomeType.CloudBands, BiomeType.ExoticCloudDeck },
                TectonicActivityRange = (0.0f, 0.0f),
                VolcanicActivityRange = (0.0f, 0.0f),
                MagneticFieldModifier = 8.0f,
                RingProbability = 0.4f,
                InsideFrostLine = true,
                ResourceProbabilities = new Dictionary<ResourceType, float> {
                    { ResourceType.Gases, 1.0f },
                    { ResourceType.ExoticMatter, 0.2f }
                }
            }},
            
            { PlanetType.Atlantean, new PlanetTypeData {
                MassRange = (50.0f, 500.0f),
                DensityRange = (0.1f, 0.3f),
                AlbedoRange = (0.2f, 0.3f),
                WaterContentRange = (0.0f, 0.1f),
                TemperatureModifier = 0.9f,
                AtmosphereProbability = 1.0f,
                AtmospherePressureMod = 150.0f,
                ChemistryTypes = new[] { ChemistryType.Hydrogen, ChemistryType.Helium, ChemistryType.Methane },
                PossibleBiomes = new[] { BiomeType.CloudBands },
                TectonicActivityRange = (0.0f, 0.0f),
                VolcanicActivityRange = (0.0f, 0.0f),
                MagneticFieldModifier = 10.0f,
                RingProbability = 0.8f,
                InsideFrostLine = false,
                ResourceProbabilities = new Dictionary<ResourceType, float> {
                    { ResourceType.Gases, 1.0f },
                    { ResourceType.ExoticMatter, 0.25f }
                }
            }},
            
            { PlanetType.Vulcanian, new PlanetTypeData {
                MassRange = (0.3f, 2.0f),
                DensityRange = (0.9f, 1.3f),
                AlbedoRange = (0.1f, 0.2f),
                WaterContentRange = (0.0f, 0.05f),
                TemperatureModifier = 1.4f,
                AtmosphereProbability = 0.9f,
                AtmospherePressureMod = 2.0f,
                ChemistryTypes = new[] { ChemistryType.Silicate, ChemistryType.Sulfur },
                PossibleBiomes = new[] { BiomeType.LavaFields, BiomeType.VolcanicPlateau,
                    BiomeType.AcidSpringFields },
                TectonicActivityRange = (0.8f, 1.0f),
                VolcanicActivityRange = (0.9f, 1.0f),
                MagneticFieldModifier = 0.6f,
                RingProbability = 0.05f,
                InsideFrostLine = true,
                ResourceProbabilities = new Dictionary<ResourceType, float> {
                    { ResourceType.Metals, 0.8f },
                    { ResourceType.RareMetals, 0.5f },
                    { ResourceType.Radioactives, 0.3f }
                }
            }},
            
            { PlanetType.Cronusian, new PlanetTypeData {
                MassRange = (5.0f, 30.0f),
                DensityRange = (0.8f, 1.3f),
                AlbedoRange = (0.1f, 0.2f),
                WaterContentRange = (0.0f, 0.05f),
                TemperatureModifier = 1.35f,
                AtmosphereProbability = 0.5f,
                AtmospherePressureMod = 0.5f,
                ChemistryTypes = new[] { ChemistryType.Iron, ChemistryType.Silicate },
                PossibleBiomes = new[] { BiomeType.LavaFields},
                TectonicActivityRange = (0.6f, 0.9f),
                VolcanicActivityRange = (0.7f, 1.0f),
                MagneticFieldModifier = 2.5f,
                RingProbability = 0.1f,
                InsideFrostLine = true,
                ResourceProbabilities = new Dictionary<ResourceType, float> {
                    { ResourceType.Metals, 0.9f },
                    { ResourceType.RareMetals, 0.6f },
                    { ResourceType.Radioactives, 0.4f }
                }
            }},
            
            { PlanetType.Asterian, new PlanetTypeData {
                MassRange = (0.00001f, 0.01f),
                DensityRange = (0.2f, 0.6f),
                AlbedoRange = (0.05f, 0.25f),
                WaterContentRange = (0.0f, 0.1f),
                TemperatureModifier = 1.0f,
                AtmosphereProbability = 0.0f,
                AtmospherePressureMod = 0.0f,
                ChemistryTypes = new[] { ChemistryType.Silicate, ChemistryType.Iron },
                PossibleBiomes = new[] { BiomeType.MetallicCrust, BiomeType.CrystallineVeins,
                    BiomeType.RegolithPlains },
                TectonicActivityRange = (0.0f, 0.0f),
                VolcanicActivityRange = (0.0f, 0.0f),
                MagneticFieldModifier = 0.0f,
                RingProbability = 0.0f,
                InsideFrostLine = true, // can be either inside or outside
                ResourceProbabilities = new Dictionary<ResourceType, float> {
                    { ResourceType.Metals, 0.8f },
                    { ResourceType.RareMetals, 0.4f },
                    { ResourceType.Radioactives, 0.2f }
                }
            }}
        };

        public static readonly Dictionary<ChemistryType, AtmosphereTemplate> AtmosphereTemplates = 
            new Dictionary<ChemistryType, AtmosphereTemplate>
        {
            { ChemistryType.Silicate, new AtmosphereTemplate {
                Composition = new Dictionary<string, float> {
                    { "Nitrogen", 78f },
                    { "Oxygen", 21f },
                    { "Argon",  0.9f },
                    { "Carbon Dioxide", 0.1f }
                },
                PressureRange = (0.5f, 2f)
            }},
            { ChemistryType.Carbon, new AtmosphereTemplate {
                Composition = new Dictionary<string, float> {
                    { "Carbon Dioxide", 95f },
                    { "Nitrogen", 3f },
                    { "Argon", 1.5f },
                    { "Sulfur Dioxide", 0.5f }
                },
                PressureRange = (50f, 100f)
            }},
            { ChemistryType.Hydrogen, new AtmosphereTemplate {
                Composition = new Dictionary<string, float> {
                    { "Hydrogen", 90f },
                    { "Helium", 9f },
                    { "Methane", 1f }
                },
                PressureRange = (500f, 1000f)
            }},
            { ChemistryType.Helium, new AtmosphereTemplate {
                Composition = new Dictionary<string, float> {
                    { "Helium", 85f },
                    { "Hydrogen", 14f },
                    { "Neon", 1f }
                },
                PressureRange = (300f, 600f)
            }},
            { ChemistryType.Water, new AtmosphereTemplate {
                Composition = new Dictionary<string, float> {
                    { "Nitrogen", 75f },
                    { "Oxygen", 24f },
                    { "Water Vapor", 1f }
                },
                PressureRange = (0.8f, 1.2f)
            }},
            { ChemistryType.Ammonia, new AtmosphereTemplate {
                Composition = new Dictionary<string, float> {
                    { "Ammonia", 75f },
                    { "Methane", 15f },
                    { "Hydrogen", 10f }
                },
                PressureRange = (0.1f, 1f)
            }},
            { ChemistryType.Methane, new AtmosphereTemplate {
                Composition = new Dictionary<string, float> {
                    { "Methane", 80f },
                    { "Nitrogen", 15f },
                    { "Hydrogen", 5f }
                },
                PressureRange = (0.5f, 5f)
            }},
            { ChemistryType.Iron, new AtmosphereTemplate {
                Composition = new Dictionary<string, float> {
                    { "Sulfur Dioxide", 40f },
                    { "Carbon Dioxide", 30f },
                    { "Nitrogen", 30f }
                },
                PressureRange = (0.1f, 0.5f)
            }},
            { ChemistryType.Sulfur, new AtmosphereTemplate {
                Composition = new Dictionary<string, float> {
                    { "Sulfur Dioxide", 85f },
                    { "Carbon Dioxide", 10f },
                    { "Hydrogen Sulfide", 5f }
                },
                PressureRange = (1f, 10f)
            }}
        };
    }
}