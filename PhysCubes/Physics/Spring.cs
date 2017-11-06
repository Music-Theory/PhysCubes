namespace ReturnToGL.Physics {
	using Walker.Data.Geometry.Speed.Space;

	public class Spring {

		float length;

		float kVal;

		public static Vector3F ForceWithDamping(float k, Vector3F pos, float b, Vector3F relVel) {
			return -k * pos - b * relVel;
		}



	}
}
