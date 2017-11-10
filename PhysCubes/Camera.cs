using System;
using PhysCubes.Utility;
using System.Numerics;

namespace PhysCubes {
	using OpenGL;
	using Walker.Data.Geometry.Speed.Space;

	public class Camera {
		#region Variables

		const float MOVE_SPEED = .5f;
		const float ROT_SPEED = 0.025f;

		public static readonly Vector3F INIT_CAM_ROT = new Vector3F(0, (float) Math.PI, 0);

		public Vector3F forward;
		MatrixStack matStack;

		bool matStackUpdated = false;

		Vector3F position;
		public Vector3F right;
		public Vector3F rotation = INIT_CAM_ROT;
		public Vector3F up;

		#endregion

		public Camera(Vector3F pos) {
			position = pos;
			matStack = new MatrixStack();
			UpdateDir();
		}

		public Vector3F Position {
			get => position;
			set {
				position = value;
				matStackUpdated = false;
			}
		}

		public Vector3F Rotation {
			get => rotation;
			set {
				rotation = value;
				matStackUpdated = false;
			}
		}

		public Matrix4 StackResult {
			get {
				if (!matStackUpdated) {
					matStack.Clear();
					matStack.Push(Matrix4.LookAt(position.ToNet(), (position + forward).ToNet(), up.ToNet()));
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
			forward = new Vector3F(
				(float) (Math.Cos(rotation.x) * Math.Sin(rotation.y)),
				(float) Math.Sin(rotation.x),
				(float) (Math.Cos(rotation.x) * Math.Cos(rotation.y)));
			right = new Vector3F(
				(float) Math.Sin(rotation.y - Math.PI / 2),
				0,
				(float) Math.Cos(rotation.y - Math.PI / 2));
			up = right.Cross(forward);
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

			rotation = new Vector3F(x, y, z);
			UpdateDir();
			matStackUpdated = false;
		}

	}

}