using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenGL;

namespace PhysCubes.Utility {
	public static class PhysMath {

		public static Vector3 Abs(Vector3 vec) {
			return new Vector3(Math.Abs(vec.x), Math.Abs(vec.y), Math.Abs(vec.z));
		}

	}
}
