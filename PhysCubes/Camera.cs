using System;
using OpenGL;
using PhysCubes.Utility;

namespace PhysCubes {

	public class Camera {
		#region Variables

		const float MOVE_SPEED = .5f;
		const float ROT_SPEED = 0.025f;

		public static readonly Vector3 INIT_CAM_ROT = new Vector3(0, Math.PI, 0);

		public Vector3 forward;
		MatrixStack matStack;

		bool matStackUpdated = false;

		Vector3 position;
		public Vector3 right;
		public Vector3 rotation = INIT_CAM_ROT;
		public Vector3 up;

		#endregion

		public Camera(Vector3 pos) {
			position = pos;
			matStack = new MatrixStack();
			UpdateDir();
		}

		public Vector3 Position {
			get { return position; }
			set {
				position = value;
				matStackUpdated = false;
			}
		}

		public Vector3 Rotation {
			get { return rotation; }
			set {
				rotation = value;
				matStackUpdated = false;
			}
		}

		public Matrix4 StackResult {
			get {
				if (!matStackUpdated) {
					matStack.Clear();
					matStack.Push(Matrix4.LookAt(position, position + forward, up));
					matStack.Push(Program.projMat);
					matStackUpdated = true;
				}
				return matStack.Result;
			}
		}

		public void Refresh() {
			matStackUpdated = false;
			UpdateDir();
		}

		void UpdateDir() {
			forward = new Vector3(
				Math.Cos(rotation.x) * Math.Sin(rotation.y),
				Math.Sin(rotation.x),
				Math.Cos(rotation.x) * Math.Cos(rotation.y));
			right = new Vector3(
				Math.Sin(rotation.y - Math.PI / 2),
				0,
				Math.Cos(rotation.y - Math.PI / 2));
			up = Vector3.Cross(right, forward);
		}

		public void Move(int x, int y, int z) {
			if (z != 0) { position += forward * z * MOVE_SPEED; }
			if (y != 0) { position += up * y * MOVE_SPEED; }
			if (x != 0) { position += right * x * MOVE_SPEED; }
			matStackUpdated = false;
		}

		public void Rotate(float x, float y, float z) {
			x *= ROT_SPEED;
			y *= ROT_SPEED;
			z *= ROT_SPEED;

			x = (float) Math.Max(-Math.PI / 2, Math.Min(Math.PI / 2, rotation.x + x));
			y += rotation.y;
			z += rotation.z;

			rotation = new Vector3(x, y, z);
			UpdateDir();
			matStackUpdated = false;
		}

	}

}