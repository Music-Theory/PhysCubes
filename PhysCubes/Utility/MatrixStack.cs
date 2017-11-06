using System.Collections.Generic;
using System.Linq;

namespace PhysCubes.Utility {
	using Walker.Data.Geometry.Speed.Rotation;

	public class MatrixStack {

		List<Matrix4F> matrices = new List<Matrix4F>();

		Matrix4F result = Matrix4F.Identity;
		bool updated = true;

		public Matrix4F Result {
			get {
				if (!updated) { UpdateResult(); }
				return result;
			}
		}

		public int Count => matrices.Count;

		public void Push(Matrix4F mat) {
			matrices.Add(mat);
			updated = false;
		}

		public Matrix4F Pop() {
			if (matrices.Count < 1) { return Matrix4F.Identity; }
			Matrix4F mat = matrices.Last();
			matrices.RemoveAt(matrices.Count - 1);
			updated = false;
			return mat;
		}

		public void Clear() {
			matrices.Clear();
			updated = false;
		}

		public Matrix4F Peek() { return matrices.Count < 1 ? Matrix4F.Identity : matrices.Last(); }

		void UpdateResult() {
			result = Matrix4F.Identity;
			foreach (Matrix4F mat in matrices) { result *= mat; }
			updated = true;
		}


	}
}
