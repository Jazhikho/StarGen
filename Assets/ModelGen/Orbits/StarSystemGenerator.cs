using UnityEngine;

namespace SimpleKeplerOrbits.SystemBuilder
{
    public class StarSystemGenerator : MonoBehaviour
    {
        [Header("Generator Settings")]
        [SerializeField] private StarSystemManager systemManager;
        [SerializeField] private ExampleSystemType exampleToGenerate = ExampleSystemType.SimpleSolarSystem;
        
        public enum ExampleSystemType
        {
            SimpleSolarSystem,
            BinaryStarSystem,
            ComplexHierarchicalSystem,
            EarthMoonSystem,
            JovianSystem
        }
        
        private void Start()
        {
            if (systemManager == null)
                systemManager = GetComponent<StarSystemManager>();
                
            GenerateExampleSystem(exampleToGenerate);
        }
        
        [ContextMenu("Generate Example System")]
        public void GenerateSelectedExample()
        {
            GenerateExampleSystem(exampleToGenerate);
        }
        
        public void GenerateExampleSystem(ExampleSystemType systemType)
        {
            StarSystemData systemData = null;
            
            switch (systemType)
            {
                case ExampleSystemType.SimpleSolarSystem:
                    systemData = CreateSimpleSolarSystem();
                    break;
                case ExampleSystemType.BinaryStarSystem:
                    systemData = CreateBinaryStarSystem();
                    break;
                case ExampleSystemType.ComplexHierarchicalSystem:
                    systemData = CreateComplexHierarchicalSystem();
                    break;
                case ExampleSystemType.EarthMoonSystem:
                    systemData = CreateEarthMoonSystem();
                    break;
                case ExampleSystemType.JovianSystem:
                    systemData = CreateJovianSystem();
                    break;
            }
            
            if (systemData != null && systemManager != null)
            {
                systemManager.GenerateSystem(systemData);
            }
        }
        
        #region Example System Generators
        
        private StarSystemData CreateSimpleSolarSystem()
        {
            var system = new StarSystemData
            {
                systemName = "Simple Solar System",
                gravityConstant = 100f,
                rootBody = new CelestialBodyData
                {
                    name = "Sun",
                    bodyType = CelestialBodyType.Star,
                    mass = 10000f,
                    scale = 10f,
                    color = Color.yellow
                }
            };
            
            // Add Earth with Moon
            var earth = new CelestialBodyData
            {
                name = "Earth",
                bodyType = CelestialBodyType.Planet,
                mass = 100f,
                scale = 3f,
                color = Color.blue,
                semiMajorAxis = 150f,
                eccentricity = 0.0167f,
                inclination = 0f,
                meanAnomaly = 0f
            };
            
            earth.satellites.Add(new CelestialBodyData
            {
                name = "Moon",
                bodyType = CelestialBodyType.Moon,
                mass = 10f,
                scale = 1f,
                color = Color.gray,
                semiMajorAxis = 20f,
                eccentricity = 0.0549f,
                inclination = 5.145f,
                meanAnomaly = 0f
            });
            
            // Add Mars
            system.rootBody.satellites.Add(new CelestialBodyData
            {
                name = "Mars",
                bodyType = CelestialBodyType.Planet,
                mass = 80f,
                scale = 2.5f,
                color = new Color(0.8f, 0.4f, 0.2f),
                semiMajorAxis = 230f,
                eccentricity = 0.0934f,
                inclination = 1.85f,
                meanAnomaly = 180f
            });
            
            system.rootBody.satellites.Add(earth);
            
            return system;
        }
        
        private StarSystemData CreateBinaryStarSystem()
        {
            var system = new StarSystemData
            {
                systemName = "Binary Star System",
                gravityConstant = 100f,
                rootBody = new CelestialBodyData
                {
                    name = "Barycenter",
                    bodyType = CelestialBodyType.Barycenter,
                    mass = 20000f
                }
            };
            
            // Star A (larger)
            var starA = new CelestialBodyData
            {
                name = "Star A",
                bodyType = CelestialBodyType.Star,
                mass = 12000f,
                scale = 12f,
                color = Color.yellow,
                semiMajorAxis = 50f,
                eccentricity = 0.2f,
                meanAnomaly = 0f
            };
            
            // Planet orbiting Star A
            starA.satellites.Add(new CelestialBodyData
            {
                name = "Planet A1",
                bodyType = CelestialBodyType.Planet,
                mass = 100f,
                scale = 3f,
                color = new Color(0.2f, 0.6f, 0.8f),
                semiMajorAxis = 100f,
                eccentricity = 0.05f,
                inclination = 2f,
                meanAnomaly = 45f
            });
            
            // Star B (smaller)
            var starB = new CelestialBodyData
            {
                name = "Star B",
                bodyType = CelestialBodyType.Star,
                mass = 8000f,
                scale = 8f,
                color = new Color(1f, 0.8f, 0.6f),
                semiMajorAxis = 75f,
                eccentricity = 0.2f,
                meanAnomaly = 180f
            };
            
            system.rootBody.satellites.Add(starA);
            system.rootBody.satellites.Add(starB);
            
            return system;
        }
        
        private StarSystemData CreateComplexHierarchicalSystem()
        {
            var system = new StarSystemData
            {
                systemName = "Complex Hierarchical System",
                gravityConstant = 100f,
                rootBody = new CelestialBodyData
                {
                    name = "System Barycenter",
                    bodyType = CelestialBodyType.Barycenter,
                    mass = 50000f
                }
            };
            
            // Binary pair AB
            var barycenterAB = new CelestialBodyData
            {
                name = "Barycenter AB",
                bodyType = CelestialBodyType.Barycenter,
                mass = 25000f,
                semiMajorAxis = 500f,
                eccentricity = 0.3f,
                meanAnomaly = 0f
            };
            
            // Star System A
            var systemA = new CelestialBodyData
            {
                name = "System A",
                bodyType = CelestialBodyType.Barycenter,
                mass = 15000f,
                semiMajorAxis = 100f,
                eccentricity = 0.1f,
                meanAnomaly = 0f
            };
            
            var starA = new CelestialBodyData
            {
                name = "Star A",
                bodyType = CelestialBodyType.Star,
                mass = 15000f,
                scale = 10f,
                color = new Color(1f, 0.9f, 0.8f)
            };
            
            // Add planets to Star A
            starA.satellites.Add(CreatePlanetWithMoons("A1", 100f, 0.05f, 2));
            starA.satellites.Add(CreatePlanetWithMoons("A2", 200f, 0.1f, 0));
            
            systemA.satellites.Add(starA);
            
            // Star System B
            var systemB = new CelestialBodyData
            {
                name = "System B",
                bodyType = CelestialBodyType.Barycenter,
                mass = 10000f,
                semiMajorAxis = 150f,
                eccentricity = 0.1f,
                meanAnomaly = 180f
            };
            
            var starB = new CelestialBodyData
            {
                name = "Star B",
                bodyType = CelestialBodyType.Star,
                mass = 10000f,
                scale = 8f,
                color = new Color(0.8f, 0.9f, 1f)
            };
            
            starB.satellites.Add(CreatePlanetWithMoons("B1", 80f, 0.02f, 1));
            systemB.satellites.Add(starB);
            
            barycenterAB.satellites.Add(systemA);
            barycenterAB.satellites.Add(systemB);
            
            // Binary pair CD (similar structure)
            var barycenterCD = new CelestialBodyData
            {
                name = "Barycenter CD",
                bodyType = CelestialBodyType.Barycenter,
                mass = 25000f,
                semiMajorAxis = 500f,
                eccentricity = 0.3f,
                meanAnomaly = 180f
            };
            
            // Add systems C and D (simplified for brevity)
            barycenterCD.satellites.Add(CreateSimpleStarSystem("C", 12000f));
            barycenterCD.satellites.Add(CreateSimpleStarSystem("D", 8000f));
            
            system.rootBody.satellites.Add(barycenterAB);
            system.rootBody.satellites.Add(barycenterCD);
            
            return system;
        }
        
        private StarSystemData CreateEarthMoonSystem()
        {
            var system = new StarSystemData
            {
                systemName = "Earth-Moon System",
                gravityConstant = 1f,
                rootBody = new CelestialBodyData
                {
                    name = "Earth",
                    bodyType = CelestialBodyType.Planet,
                    mass = 5.972e24f,
                    scale = 6.371f,
                    color = new Color(0.2f, 0.4f, 0.8f)
                }
            };
            
            system.rootBody.satellites.Add(new CelestialBodyData
            {
                name = "Moon",
                bodyType = CelestialBodyType.Moon,
                mass = 7.34767309e22f,
                scale = 1.737f,
                color = Color.gray,
                semiMajorAxis = 384.4f,
                eccentricity = 0.0549f,
                inclination = 5.145f,
                meanAnomaly = 0f
            });
            
            return system;
        }
        
        private StarSystemData CreateJovianSystem()
        {
            var system = new StarSystemData
            {
                systemName = "Jovian System",
                gravityConstant = 100f,
                rootBody = new CelestialBodyData
                {
                    name = "Jupiter",
                    bodyType = CelestialBodyType.Planet,
                    mass = 5000f,
                    scale = 8f,
                    color = new Color(0.8f, 0.6f, 0.4f)
                }
            };
            
            // Galilean moons
            system.rootBody.satellites.Add(new CelestialBodyData
            {
                name = "Io",
                bodyType = CelestialBodyType.Moon,
                mass = 50f,
                scale = 1.5f,
                color = new Color(1f, 0.9f, 0.6f),
                semiMajorAxis = 40f,
                eccentricity = 0.0041f,
                inclination = 0.04f,
                meanAnomaly = 0f
            });
            
            system.rootBody.satellites.Add(new CelestialBodyData
            {
                name = "Europa",
                bodyType = CelestialBodyType.Moon,
                mass = 40f,
                scale = 1.3f,
                color = new Color(0.8f, 0.9f, 1f),
                semiMajorAxis = 60f,
                eccentricity = 0.009f,
                inclination = 0.47f,
                meanAnomaly = 90f
            });
            
            system.rootBody.satellites.Add(new CelestialBodyData
            {
                name = "Ganymede",
                bodyType = CelestialBodyType.Moon,
                mass = 60f,
                scale = 2f,
                color = new Color(0.6f, 0.5f, 0.4f),
                semiMajorAxis = 100f,
                eccentricity = 0.0013f,
                inclination = 0.18f,
                meanAnomaly = 180f
            });
            
            system.rootBody.satellites.Add(new CelestialBodyData
            {
                name = "Callisto",
                bodyType = CelestialBodyType.Moon,
                mass = 55f,
                scale = 1.8f,
                color = new Color(0.4f, 0.3f, 0.3f),
                semiMajorAxis = 150f,
                eccentricity = 0.0074f,
                inclination = 0.19f,
                meanAnomaly = 270f
            });
            
            return system;
        }
        
        #endregion
        
        #region Helper Methods
        
        private CelestialBodyData CreatePlanetWithMoons(string planetName, float semiMajorAxis, 
            float eccentricity, int moonCount)
        {
            var planet = new CelestialBodyData
            {
                name = $"Planet {planetName}",
                bodyType = CelestialBodyType.Planet,
                mass = 100f,
                scale = 3f,
                color = new Color(Random.Range(0.3f, 0.8f), Random.Range(0.3f, 0.8f), Random.Range(0.3f, 0.8f)),
                semiMajorAxis = semiMajorAxis,
                eccentricity = eccentricity,
                inclination = Random.Range(-5f, 5f),
                meanAnomaly = Random.Range(0f, 360f)
            };
            
            for (int i = 1; i <= moonCount; i++)
            {
                planet.satellites.Add(new CelestialBodyData
                {
                    name = $"Moon {planetName}-{i}",
                    bodyType = CelestialBodyType.Moon,
                    mass = 10f,
                    scale = 1f,
                    color = Color.gray,
                    semiMajorAxis = 15f + i * 10f,
                    eccentricity = Random.Range(0f, 0.1f),
                    inclination = Random.Range(-10f, 10f),
                    meanAnomaly = Random.Range(0f, 360f)
                });
            }
            
            return planet;
        }
        
        private CelestialBodyData CreateSimpleStarSystem(string name, float starMass)
        {
            var system = new CelestialBodyData
            {
                name = $"System {name}",
                bodyType = CelestialBodyType.Barycenter,
                mass = starMass,
                semiMajorAxis = 150f,
                eccentricity = 0.1f,
                meanAnomaly = Random.Range(0f, 360f)
            };
            
            var star = new CelestialBodyData
            {
                name = $"Star {name}",
                bodyType = CelestialBodyType.Star,
                mass = starMass,
                scale = starMass / 1000f,
                color = Color.Lerp(Color.red, Color.white, starMass / 20000f)
            };
            
            star.satellites.Add(CreatePlanetWithMoons($"{name}1", 100f, 0.05f, 1));
            system.satellites.Add(star);
            
            return system;
        }
        
        #endregion
    }
}