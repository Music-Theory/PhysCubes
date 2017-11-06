using System;

namespace PhysCubes.Utility {
	using Walker.Data.Geometry.Speed.Space;

	public static class PhysMath {

		public static Vector3F Abs(this Vector3F vec) {
			return new Vector3F(Math.Abs(vec.x), Math.Abs(vec.y), Math.Abs(vec.z));
		}

	}
}
