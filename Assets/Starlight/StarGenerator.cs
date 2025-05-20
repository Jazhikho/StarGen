using System.Collections.Generic;
using UnityEngine;

namespace Starlight
{
    [RequireComponent(typeof(StarManager))]
    public class StarGenerator : MonoBehaviour
    {
        [Tooltip("Radius of a sphere in which to place stars")]
        public float size = 5000f;
        
        [Tooltip("Number of stars to generate")]
        public int starCount = 10000;
        
        [Tooltip("Random seed used for star generation")]
        public int seed = 1234;
        
        [Tooltip("If enabled, a Sol-like star will be placed at origin (0,0,0)")]
        public bool generateAtOrigin = false;
        
        [Tooltip("Generate stars on start")]
        public bool generateOnStart = true;
        
        private StarManager _starManager;
        private bool _initialized = false;

        // Star classification categories
        private class RangeF
        {
            public float Min;
            public float Max;

            public RangeF(float min, float max)
            {
                Min = min;
                Max = max;
            }

            public float Sample(float value)
            {
                return (Max - Min) * value + Min;
            }
        }

        private class StarClass
        {
            public int Weight;
            public string StellarClass;
            public RangeF TempRange;
            public RangeF LuminosityRange;
            public RangeF MassRange;

            public StarClass(int weight, string stellarClass, 
                             RangeF tempRange, RangeF luminosityRange, RangeF massRange)
            {
                Weight = weight;
                StellarClass = stellarClass;
                TempRange = tempRange;
                LuminosityRange = luminosityRange;
                MassRange = massRange;
            }

            public Dictionary<string, float> Sample(float value)
            {
                return new Dictionary<string, float> {
                    { "stellar_class", 0 }, // Using 0 as placeholder for string value
                    { "temp", TempRange.Sample(value) },
                    { "luminosity", LuminosityRange.Sample(value) },
                    { "mass", MassRange.Sample(value) }
                };
            }

            public Star GetStar(Vector3 position, float value)
            {
                var p = Sample(value);
                // B and O-class stars are obscenely bright, so spawn them further away than other stars.
                position *= Mathf.Max(1.0f, p["luminosity"] / 400f);
                return new Star(position, p["luminosity"], p["temp"]);
            }
        }

        // Star classification data
        private readonly StarClass _classO = new StarClass(
            1,
            "O",
            new RangeF(30_000, 60_000),
            new RangeF(30_000, 60_000),
            new RangeF(16, 32)
        );

        private readonly StarClass _classB = new StarClass(
            13,
            "B",
            new RangeF(10_000, 30_000),
            new RangeF(25, 30_000),
            new RangeF(2.1f, 16)
        );

        private readonly StarClass _classA = new StarClass(
            60,
            "A",
            new RangeF(7500, 10_000),
            new RangeF(5, 25),
            new RangeF(1.4f, 2.1f)
        );

        private readonly StarClass _classF = new StarClass(
            3_00,
            "F",
            new RangeF(6000, 7500),
            new RangeF(1.5f, 5),
            new RangeF(1.04f, 1.4f)
        );

        private readonly StarClass _classG = new StarClass(
            7_60,
            "G",
            new RangeF(5200, 6000),
            new RangeF(.6f, 1.50f),
            new RangeF(0.8f, 1.04f)
        );

        private readonly StarClass _classK = new StarClass(
            12_10,
            "K",
            new RangeF(3700, 5200),
            new RangeF(0.08f, 0.6f),
            new RangeF(0.45f, 0.8f)
        );

        private readonly StarClass _classM = new StarClass(
            76_45,
            "M",
            new RangeF(2400, 3700),
            new RangeF(0.01f, 0.08f),
            new RangeF(0.08f, 0.45f)
        );

        private readonly List<StarClass> _starTable;

        public StarGenerator()
        {
            _starTable = new List<StarClass> {
                _classO,
                _classB,
                _classA,
                _classF,
                _classG,
                _classK,
                _classM
            };
        }

        private void Awake()
        {
            _starManager = GetComponent<StarManager>();
        }

        private void Start()
        {
            if (generateOnStart && !_initialized)
            {
                GenerateStars();
            }
        }

        /// <summary>
        /// Generate a random position within a sphere of given radius
        /// </summary>
        private Vector3 SampleSphere(System.Random random, float radius)
        {
            Vector3 pos;
            do
            {
                pos = new Vector3(
                    (float)(random.NextDouble() * 2.0 - 1.0),
                    (float)(random.NextDouble() * 2.0 - 1.0),
                    (float)(random.NextDouble() * 2.0 - 1.0)
                );
            } while (pos.sqrMagnitude > 1.0f);
            
            return pos * radius;
        }

        /// <summary>
        /// Pick a random star classification based on weighted frequency
        /// </summary>
        private StarClass RandomCategory(System.Random random)
        {
            int sum = 0;
            foreach (var category in _starTable)
            {
                sum += category.Weight;
            }

            int weight = random.Next(1, sum);

            sum = 0;
            foreach (var category in _starTable)
            {
                int prev = sum;
                sum += category.Weight;
                if (weight <= sum)
                {
                    return category;
                }
            }

            return _starTable[_starTable.Count - 1]; // Default to M-class if something goes wrong
        }

        /// <summary>
        /// Generate stars and apply them to the star manager
        /// </summary>
        public void GenerateStars()
        {
            if (_starManager == null)
            {
                _starManager = GetComponent<StarManager>();
                if (_starManager == null)
                {
                    Debug.LogError("StarManager component is required");
                    return;
                }
            }

            System.Random random = new System.Random(seed);
            List<Star> stars = new List<Star>();

            // Add the Sun at the origin if requested
            if (generateAtOrigin)
            {
                stars.Add(_classG.GetStar(Vector3.zero, 0.5f));
            }

            // Generate the rest of the stars
            for (int i = 0; i < starCount; i++)
            {
                StarClass category = RandomCategory(random);
                if (category != null)
                {
                    stars.Add(category.GetStar(
                        SampleSphere(random, size),
                        (float)random.NextDouble()
                    ));
                }
            }

            _starManager.SetStarList(stars);
            _initialized = true;
        }

        /// <summary>
        /// Regenerate stars with the current settings
        /// </summary>
        [ContextMenu("Regenerate Stars")]
        public void RegenerateStars()
        {
            GenerateStars();
        }
    }
}
