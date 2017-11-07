using System;
using PhysCubes.Utility;
using System.Numerics;

namespace PhysCubes {

	public class Camera {
		#region Variables

		const float MOVE_SPEED = .5f;
		const float ROT_SPEED = 0.025f;

		public static readonly Vector3 INIT_CAM_ROT = new Vector3(0, (float) Math.PI, 0);

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
			get => position;
			set {
				position = value;
				matStackUpdated = false;
			}
		}

		public Vector3 Rotation {
			get => rotation;
			set {
				rotation = value;
				matStackUpdated = false;
			}
		}

		public Matrix4x4 StackResult {
			get {
				if (!matStackUpdated) {
					matStack.Clear();
					matStack.Push(Matrix4x4.CreateLookAt(position, position + forward, up));
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
				(float) (Math.Cos(rotation.X) * Math.Sin(rotation.Y)),
				(float) Math.Sin(rotation.X),
				(float) (Math.Cos(rotation.X) * Math.Cos(rotation.Y)));
			right = new Vector3(
				(float) Math.Sin(rotation.Y - Math.PI / 2),
				0,
				(float) Math.Cos(rotation.Y - Math.PI / 2));
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

			x = (float) Math.Max(-Math.PI / 2, Math.Min(Math.PI / 2, rotation.X + x));
			y += rotation.Y;
			z += rotation.Z;

			rotation = new Vector3(x, y, z);
			UpdateDir();
			matStackUpdated = false;
		}

	}

}