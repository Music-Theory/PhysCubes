namespace ReturnToGL.Physics {
	using System.Numerics;

	public class Spring {

		float length;

		float kVal;

		public static Vector3 ForceWithDamping(float k, Vector3 pos, float b, Vector3 relVel) {
			return -k * pos - b * relVel;
		}



	}
}
