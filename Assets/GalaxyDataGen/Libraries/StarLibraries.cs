using System.Collections.Generic;

namespace Libraries
{
	/// <summary>
	/// Contains stellar data and distributions for star generation.
	/// </summary>
	public static class StarLibrary
	{
		/// <summary>
		/// Empirical distribution of spectral types based on real stellar catalog data.
		/// </summary>
		public static readonly Dictionary<float, string> SpectralTypeDistribution = new Dictionary<float, string>
	{
		{ 428.66f, "M" },    // 4.29% 
		{ 429.18f, "L" },    // 0.052%, Brown Dwarf
		{ 429.49f, "T" },    // 0.031%, Brown Dwarf
		{ 429.60f, "Y" },    // 0.011%, Brown Dwarf
		{ 2816.00f, "K" },   // 23.86%
		{ 4875.84f, "G" },   // 20.60%
		{ 7216.23f, "F" },   // 23.40%
		{ 8909.36f, "A" },   // 16.93%
		{ 9850.73f, "B" },   // 9.41% 
		{ 9874.73f, "W" },   // 0.24%, Wolf Rayat Star
		{ 9898.73f, "O" },   // 0.24%
		{ 9925.85f, "C" },   // 0.27%, Carbon Star
		{ 9978.00f, "I" },   // 0.52%, White Dwarf
		{ 9998.86f, "N" },   // 0.21%, Neutron Star
		{ 10000.0f, "S" }    // 0.011%, Black Hole
	};

		/// <summary>
		/// Spectral subclass distributions for each spectral type.
		/// </summary>
		public static readonly Dictionary<string, Dictionary<float, string>> SubclassDistributions = new Dictionary<string, Dictionary<float, string>>
		{
			{ "O", new Dictionary<float, string>
				{
					{ 0.4f, "4" },
					{ 7.7f, "5" },
					{ 16.9f, "6" },
					{ 31.5f, "7" },
					{ 48.4f, "8" },
					{ 100.0f, "9" }
				}
			},
			{ "B", new Dictionary<float, string>
				{
					{ 3.1f, "0" },
					{ 7.5f, "1" },
					{ 14.7f, "2" },
					{ 20.8f, "3" },
					{ 22.7f, "4" },
					{ 30.9f, "5" },
					{ 33.9f, "6" },
					{ 37.8f, "7" },
					{ 61.5f, "8" },
					{ 100.0f, "9" }
				}
			},
			{ "A", new Dictionary<float, string>
				{
					{ 37.1f, "0" },
					{ 43.6f, "1" },
					{ 64.2f, "2" },
					{ 75.2f, "3" },
					{ 77.3f, "4" },
					{ 86.3f, "5" },
					{ 87.7f, "6" },
					{ 90.5f, "7" },
					{ 92.4f, "8" },
					{ 100.0f, "9" }
				}
			},
			{ "F", new Dictionary<float, string>
				{
					{ 16.4f, "0" },
					{ 16.7f, "1" },
					{ 31.1f, "2" },
					{ 39.7f, "3" },
					{ 40.4f, "4" },
					{ 65.2f, "5" },
					{ 71.3f, "6" },
					{ 76.7f, "7" },
					{ 98.7f, "8" },
					{ 100.0f, "9" }
				}
			},
			{ "G", new Dictionary<float, string>
				{
					{ 24.2f, "0" },
					{ 27.1f, "1" },
					{ 32.1f, "2" },
					{ 38.3f, "3" },
					{ 38.6f, "4" },
					{ 72.5f, "5" },
					{ 77.9f, "6" },
					{ 78.3f, "7" },
					{ 97.9f, "8" },
					{ 100.0f, "9" }
				}
			},
			{ "K", new Dictionary<float, string>
				{
					{ 42.9f, "0" },
					{ 53.8f, "1" },
					{ 75.1f, "2" },
					{ 81.6f, "3" },
					{ 85.1f, "4" },
					{ 97.3f, "5" },
					{ 97.4f, "6" },
					{ 99.2f, "7" },
					{ 99.7f, "8" },
					{ 100.0f, "9" }
				}
			},
			{ "M", new Dictionary<float, string>
				{
					{ 30.8f, "0" },
					{ 44.8f, "1" },
					{ 62.8f, "2" },
					{ 72.4f, "3" },
					{ 78.7f, "4" },
					{ 83.8f, "5" },
					{ 86.0f, "6" },
					{ 86.9f, "7" },
					{ 87.8f, "8" },
					{ 100.0f, "9" }
				}
			},
			// Wolf-Rayet subclasses (WN, WC, WO)
			{ "W", new Dictionary<float, string>
				{
					{ 60.0f, "N" },  // Nitrogen-rich
					{ 95.0f, "C" },  // Carbon-rich
					{ 100.0f, "O" }  // Oxygen-rich (very rare)
				}
			},

			// Carbon star subclasses
			{ "C", new Dictionary<float, string>
				{
					{ 70.0f, "-N" },
					{ 85.0f, "-J" },
					{ 95.0f, "-H" },
					{ 100.0f, "-R" }
				}
			},

			// White Dwarf subclasses
			{ "I", new Dictionary<float, string>
				{
					{ 60.0f, "DA" },  // Hydrogen atmosphere
					{ 75.0f, "DB" },  // Helium atmosphere
					{ 85.0f, "DC" },  // Continuous spectrum
					{ 92.0f, "DO" },  // Ionized helium
					{ 97.0f, "DZ" },  // Metal-rich
					{ 100.0f, "DQ" }  // Carbon-rich
				}
			}
		};

		/// <summary>
		/// Corrected luminosity class distribution by spectral type, including assumed main sequence stars.
		/// </summary>
		public static readonly Dictionary<string, Dictionary<float, StellarEvolutionStage>> LuminosityDistributions =
			new Dictionary<string, Dictionary<float, StellarEvolutionStage>>
		{
			{ "O", new Dictionary<float, StellarEvolutionStage>
				{
					{ 16.5f, StellarEvolutionStage.I },
					{ 20.7f, StellarEvolutionStage.II },
					{ 28.0f, StellarEvolutionStage.III },
					{ 30.7f, StellarEvolutionStage.IV },
					{ 100.0f, StellarEvolutionStage.V }
				}
			},
			{ "B", new Dictionary<float, StellarEvolutionStage>
				{
					{ 4.4f, StellarEvolutionStage.I },
					{ 9.3f, StellarEvolutionStage.II },
					{ 21.0f, StellarEvolutionStage.III },
					{ 33.7f, StellarEvolutionStage.IV },
					{ 100.0f, StellarEvolutionStage.V }
				}
			},
			{ "A", new Dictionary<float, StellarEvolutionStage>
				{
					{ 0.6f, StellarEvolutionStage.I },
					{ 1.4f, StellarEvolutionStage.II },
					{ 4.5f, StellarEvolutionStage.III },
					{ 12.9f, StellarEvolutionStage.IV },
					{ 100.0f, StellarEvolutionStage.V }
				}
			},
			{ "F", new Dictionary<float, StellarEvolutionStage>
				{
					{ 0.6f, StellarEvolutionStage.I },
					{ 1.3f, StellarEvolutionStage.II },
					{ 3.2f, StellarEvolutionStage.III },
					{ 11.0f, StellarEvolutionStage.IV },
					{ 100.0f, StellarEvolutionStage.V }
				}
			},
			{ "G", new Dictionary<float, StellarEvolutionStage>
				{
					{ 0.7f, StellarEvolutionStage.I },
					{ 2.0f, StellarEvolutionStage.II },
					{ 17.7f, StellarEvolutionStage.III },
					{ 26.0f, StellarEvolutionStage.IV },
					{ 100.0f, StellarEvolutionStage.V }
				}
			},
			{ "K", new Dictionary<float, StellarEvolutionStage>
				{
					{ 0.3f, StellarEvolutionStage.I },
					{ 1.5f, StellarEvolutionStage.II },
					{ 41.4f, StellarEvolutionStage.III },
					{ 43.8f, StellarEvolutionStage.IV },
					{ 100.0f, StellarEvolutionStage.V }
				}
			},
			{ "M", new Dictionary<float, StellarEvolutionStage>
				{
					{ 1.3f, StellarEvolutionStage.I },
					{ 2.7f, StellarEvolutionStage.II },
					{ 44.3f, StellarEvolutionStage.III },
					{ 44.4f, StellarEvolutionStage.IV },
					{ 100.0f, StellarEvolutionStage.V }
				}
			},
			{ "W", new Dictionary<float, StellarEvolutionStage>
				{
					{ 100.0f, StellarEvolutionStage.I }  // Most WR stars are considered supergiants
				}
			},

			{ "C", new Dictionary<float, StellarEvolutionStage>
				{
					{ 15.0f, StellarEvolutionStage.I },  // Some are supergiants
					{ 30.0f, StellarEvolutionStage.II },  // Some are bright giants
					{ 100.0f, StellarEvolutionStage.III }  // Most are giants
				}
			},

			{ "I", new Dictionary<float, StellarEvolutionStage>
				{
					{ 100.0f, StellarEvolutionStage.VII }  // White dwarfs don't have luminosity classes
				}
			},

			{ "L", new Dictionary<float, StellarEvolutionStage>
				{
					{ 100.0f, StellarEvolutionStage.VI }  // Brown dwarfs are considered dwarf stars
				}
			},

			{ "T", new Dictionary<float, StellarEvolutionStage>
				{
					{ 100.0f, StellarEvolutionStage.VI }
				}
			},

			{ "Y", new Dictionary<float, StellarEvolutionStage>
				{
					{ 100.0f, StellarEvolutionStage.VI }
				}
			},

			{ "N", new Dictionary<float, StellarEvolutionStage>
				{
					{ 100.0f, StellarEvolutionStage.V }  // No luminosity class for neutron stars
				}
			},

			{ "S", new Dictionary<float, StellarEvolutionStage>
				{
					{ 100.0f, StellarEvolutionStage.V }  // No luminosity class for black holes
				}
			}
		};

		/// <summary>
		/// Mass distributions for special stellar types.
		/// </summary>
		public static readonly Dictionary<string, Dictionary<float, int>> SpecialMassDistributions = new Dictionary<string, Dictionary<float, int>>
		{
			{ "I", new Dictionary<float, int>
				{
					{ 0.2f, 100 },
					{ 0.3f, 300 },
					{ 0.4f, 1110 },
					{ 0.5f, 3540 },
					{ 0.6f, 6780 },
					{ 0.7f, 8600 },
					{ 0.8f, 9410 },
					{ 0.9f, 9710 },
					{ 1.0f, 9870 },
					{ 1.1f, 9950 },
					{ 1.2f, 10000 }
				}
			}
		};


		/// <summary>
		/// Physical characteristics ranges for each spectral type.
		/// </summary>
		public static readonly Dictionary<string, StarTypeRange> SpectralRanges = new Dictionary<string, StarTypeRange>
		{
			{ "O", new StarTypeRange {
				MinMass = 16.0f, MaxMass = 150.0f,
				MinTemp = 30000f, MaxTemp = 50000f,
				MinLuminosity = 30000f, MaxLuminosity = 1000000f,
				MinRadius = 6f, MaxRadius = 15f,
				MinAge = 1f, MaxAge = 10f,
				DefaultStage = StellarEvolutionStage.V
			}},
			{ "B", new StarTypeRange {
				MinMass = 2.1f, MaxMass = 16.0f,
				MinTemp = 10000f, MaxTemp = 30000f,
				MinLuminosity = 25f, MaxLuminosity = 30000f,
				MinRadius = 1.8f, MaxRadius = 6.6f,
				MinAge = 10f, MaxAge = 100f,
				DefaultStage = StellarEvolutionStage.V
			}},
			{ "A", new StarTypeRange {
				MinMass = 1.4f, MaxMass = 2.1f,
				MinTemp = 7500f, MaxTemp = 10000f,
				MinLuminosity = 5f, MaxLuminosity = 25f,
				MinRadius = 1.4f, MaxRadius = 2.4f,
				MinAge = 100f, MaxAge = 1000f,
				DefaultStage = StellarEvolutionStage.V
			}},
			{ "F", new StarTypeRange {
				MinMass = 1.04f, MaxMass = 1.4f,
				MinTemp = 6000f, MaxTemp = 7500f,
				MinLuminosity = 1.5f, MaxLuminosity = 5f,
				MinRadius = 1.15f, MaxRadius = 1.4f,
				MinAge = 1000f, MaxAge = 2500f,
				DefaultStage = StellarEvolutionStage.V
			}},
			{ "G", new StarTypeRange {
				MinMass = 0.8f, MaxMass = 1.04f,
				MinTemp = 5200f, MaxTemp = 6000f,
				MinLuminosity = 0.6f, MaxLuminosity = 1.5f,
				MinRadius = 0.96f, MaxRadius = 1.15f,
				MinAge = 2500f, MaxAge = 10000f,
				DefaultStage = StellarEvolutionStage.V
			}},
			{ "K", new StarTypeRange {
				MinMass = 0.45f, MaxMass = 0.8f,
				MinTemp = 3700f, MaxTemp = 5200f,
				MinLuminosity = 0.08f, MaxLuminosity = 0.6f,
				MinRadius = 0.7f, MaxRadius = 0.96f,
				MinAge = 5000f, MaxAge = 12000f,
				DefaultStage = StellarEvolutionStage.V
			}},
			{ "M", new StarTypeRange {
				MinMass = 0.08f, MaxMass = 0.45f,
				MinTemp = 2400f, MaxTemp = 3700f,
				MinLuminosity = 0.0001f, MaxLuminosity = 0.08f,
				MinRadius = 0.1f, MaxRadius = 0.7f,
				MinAge = 10000f, MaxAge = 13000f,
				DefaultStage = StellarEvolutionStage.V
			}},
			{ "I", new StarTypeRange {
				MinMass = 0.17f, MaxMass = 1.4f,
				MinTemp = 8000f, MaxTemp = 40000f,
				MinLuminosity = 0.0001f, MaxLuminosity = 0.1f,
				MinRadius = 0.01f, MaxRadius = 0.01f,
				MinAge = 5f, MaxAge = 13.5f,
				DefaultStage = StellarEvolutionStage.VII,
				CanBePulsar = true
			}},
			{ "W", new StarTypeRange {
				MinMass = 10.0f, MaxMass = 50.0f,
				MinTemp = 30000f, MaxTemp = 200000f,
				MinLuminosity = 100000f, MaxLuminosity = 1000000f,
				MinRadius = 3f, MaxRadius = 20f,
				MinAge = 1f, MaxAge = 5f,
				DefaultStage = StellarEvolutionStage.I,
				HasFlares = true
			}},
			{ "C", new StarTypeRange {
				MinMass = 0.8f, MaxMass = 10.0f,
				MinTemp = 2500f, MaxTemp = 5000f,
				MinLuminosity = 1.0f, MaxLuminosity = 10000f,
				MinRadius = 25f, MaxRadius = 500f,
				MinAge = 5f, MaxAge = 12f,
				DefaultStage = StellarEvolutionStage.III
			}},
			{ "L", new StarTypeRange {
				MinMass = 0.013f, MaxMass = 0.08f,
				MinTemp = 1300f, MaxTemp = 2400f,
				MinLuminosity = 0.000001f, MaxLuminosity = 0.0001f,
				MinRadius = 0.08f, MaxRadius = 0.15f,
				MinAge = 1f, MaxAge = 13f,
				DefaultStage = StellarEvolutionStage.VI
			}},
			{ "T", new StarTypeRange {
				MinMass = 0.004f, MaxMass = 0.013f,
				MinTemp = 800f, MaxTemp = 1300f,
				MinLuminosity = 0.000000001f, MaxLuminosity = 0.000001f,
				MinRadius = 0.05f, MaxRadius = 0.08f,
				MinAge = 1f, MaxAge = 13800f,
				DefaultStage = StellarEvolutionStage.VI
			}},
			{ "Y", new StarTypeRange {
				MinMass = 0.001f, MaxMass = 0.004f,
				MinTemp = 300f, MaxTemp = 800f,
				MinLuminosity = 0.000000000001f, MaxLuminosity = 0.000000001f,
				MinRadius = 0.01f, MaxRadius = 0.05f,
				MinAge = 1f, MaxAge = 13800f,
				DefaultStage = StellarEvolutionStage.VI
			}},
			{ "N", new StarTypeRange { // Neutron Star
				MinMass = 1.4f, MaxMass = 3.0f,
				MinTemp = 500000f, MaxTemp = 3000000f,  // Hot, but not zero
				MinLuminosity = 0.00001f, MaxLuminosity = 10.0f,
				MinRadius = 0.00001f, MaxRadius = 0.00002f,  // 10-20 km
				MinAge = 0.01f, MaxAge = 10000f,
				DefaultStage = StellarEvolutionStage.V,
				CanBePulsar = true
			}},
			{ "S", new StarTypeRange { // Black Hole
				MinMass = 3.0f, MaxMass = 100.0f,  // Wider mass range
				MinTemp = 0f, MaxTemp = 0f,
				MinLuminosity = 0f, MaxLuminosity = 0f,
				MinRadius = 0.0001f, MaxRadius = 0.0001f,
				MinAge = 0.01f, MaxAge = 13800f,  // Can be very old
				DefaultStage = StellarEvolutionStage.V
		}}
		};


		/// <summary>
		/// Defines the mass and physical property ranges for a stellar type.
		/// </summary>
		public class StarTypeRange
		{
			public float MinMass;
			public float MaxMass;
			public float MinTemp;
			public float MaxTemp;
			public float MinLuminosity;
			public float MaxLuminosity;
			public float MinRadius;
			public float MaxRadius;
			public float MinAge;
			public float MaxAge;
			public StellarEvolutionStage DefaultStage;
			public bool? HasFlares = null;
			public bool CanBePulsar = false;
		}


		/// <summary>
		/// Distribution of number of stars in a system.
		/// </summary>
		public static readonly Dictionary<float, int> NumberDistribution = new Dictionary<float, int>
		{
			{ 5400.0f, 1 },
			{ 8800.0f, 2 },
			{ 9600.0f, 3 },
			{ 9900.0f, 4 },
			{ 9950.0f, 5 },
			{ 9980.0f, 6 },
			{ 9990.0f, 7 },
			{ 9995.0f, 8 },
			{ 9998.0f, 9 },
			{ 10000.0f, 10 }
		};
	}

	/// <summary>
	/// Defines the evolutionary stages of stars.
	/// </summary>
	public enum StellarEvolutionStage
	{
		VII,  // White dwarf
		VI,   // Brown dwarf
		V,    // Main sequence
		IV,   // Subgiant
		III,  // Giant
		II,   // Bright giant
		I,     // Supergiant
	}
}
