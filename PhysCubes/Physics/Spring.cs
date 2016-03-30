using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenGL;
using static PhysCubes.Utility.PhysMath;

namespace ReturnToGL.Physics {
	public class Spring {

		float length;

		float kVal;

		public static Vector3 ForceWithDamping(float k, Vector3 pos, float b, Vector3 relVel) {
			return -k * pos - b * relVel;
		}



	}
}
