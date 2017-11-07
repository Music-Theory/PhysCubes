using System;
using System.Numerics;

namespace PhysCubes.Utility {

	public static class PhysMath {

		public static Vector3 Abs(this Vector3 vec) {
			return new Vector3(Math.Abs(vec.X), Math.Abs(vec.Y), Math.Abs(vec.Z));
		}

	}
}
