using System.Collections.Generic;

namespace Libraries
{
	public static class SystemLibrary
	{
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
}
