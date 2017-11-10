using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace PhysCubes.Utility {
	using OpenGL;

	public class MatrixStack {

		List<Matrix4> matrices = new List<Matrix4>();

		Matrix4 result = Matrix4.Identity;
		bool updated = true;

		public Matrix4 Result {
			get {
				if (!updated) { UpdateResult(); }
				return result;
			}
		}

		public int Count => matrices.Count;

		public void Push(Matrix4 mat) {
			matrices.Add(mat);
			updated = false;
		}

		public Matrix4 Pop() {
			if (matrices.Count < 1) { return Matrix4.Identity; }
			Matrix4 mat = matrices.Last();
			matrices.RemoveAt(matrices.Count - 1);
			updated = false;
			return mat;
		}

		public void Clear() {
			matrices.Clear();
			updated = false;
		}

		public Matrix4 Peek() { return matrices.Count < 1 ? Matrix4.Identity : matrices.Last(); }

		void UpdateResult() {
			result = Matrix4.Identity;
			foreach (Matrix4 mat in matrices) { result *= mat; }
			updated = true;
		}


	}
}
