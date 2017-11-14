using System;
using System.IO;
using OpenGL;
using System.Numerics;

namespace PhysCubes.Utility {
	using Walker.Data.Geometry.Generic.Plane;
	using Walker.Data.Geometry.Speed.Plane;
	using Walker.Data.Geometry.Speed.Rotation;
	using Walker.Data.Geometry.Speed.Space;
	using Vector2 = OpenGL.Vector2;
	using Vector3 = OpenGL.Vector3;
	using Vector4 = OpenGL.Vector4;

	public static class GLUtility {

		public static readonly Vector3 Right = new Vector3(1, 0, 0);
		public static readonly Vector3 Up = new Vector3(0, 1, 0);
		public static readonly Vector3 Forward = new Vector3(0, 0, 1);

		public static string LoadShaderString(string fileName) {
			StreamReader reader = new StreamReader("Shaders/" + fileName + (fileName.EndsWith(".glsl") ? "" : ".glsl"));

			Console.WriteLine("Loading shader " + fileName + "...");

			return reader.ReadToEnd();
		}

		public static void WriteGLError(string context) { Console.WriteLine(context + ": " + Gl.GetError()); }


		static GLUtility() {
			lineShader = new ShaderProgram(LoadShaderString("simpleVert"), LoadShaderString("simpleFrag"));
			lineVAO = new VAO(lineShader, physLineVec, physLineInd) {DrawMode = BeginMode.Lines};
		}

		public static void Dispose() {
			lineVAO.DisposeChildren = true;
			lineVAO.Dispose();
			lineShader.DisposeChildren = true;
			lineShader.Dispose();
		}

		static ShaderProgram lineShader;

		static VBO<Vector3> physLineVec = new VBO<Vector3>(new[] {
			new Vector3(0, 0, 0),
			new Vector3(0, 0, 1),
		});
		static VBO<int> physLineInd = new VBO<int>(new[] {
			0, 1
		});

		public static VAO lineVAO;

		public static Vector2 ToGL(this Vector2F vec) {
			return new Vector2(vec.x, vec.y);
		}

		public static Vector2 ToGL(this Vector2<float> vec) {
			return new Vector2(vec.X, vec.Y);
		}

		public static Vector3 ToGL(this Vector3F vec) {
			return new Vector3(vec.x, vec.y, vec.z);
		}

		public static Matrix4 ToGLMat(this Vector4F q) {
			return q.QMatrix.ToGL();
		}

		public static Matrix4 ToGL(this Matrix4F mat) {
			return new Matrix4(mat.R1.ToGL(), mat.R2.ToGL(), mat.R3.ToGL(), mat.R4.ToGL());
		}

		public static Vector4 ToGL(this Vector4F vec) {
			return new Vector4(vec.x, vec.y, vec.z, vec.w);
		}

		public static Vector4 Row(this Matrix4x4 mat, int ind) {
			switch (ind) {
				case 0:
					return new Vector4(mat.M11, mat.M12, mat.M13, mat.M14);
				case 1:
					return new Vector4(mat.M21, mat.M22, mat.M23, mat.M24);
				case 2:
					return new Vector4(mat.M31, mat.M32, mat.M33, mat.M34);
				case 3:
					return new Vector4(mat.M41, mat.M42, mat.M43, mat.M44);
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

	}
}
