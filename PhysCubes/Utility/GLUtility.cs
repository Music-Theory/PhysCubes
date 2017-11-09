using System;
using System.IO;
using OpenGL;
using System.Numerics;

namespace PhysCubes.Utility {

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

		public static Matrix4 ToGL(this Matrix4x4 mat) {
			return new Matrix4(new [] {
				                          mat.M11, mat.M12, mat.M13, mat.M14,
				                          mat.M21, mat.M22, mat.M23, mat.M24,
				                          mat.M31, mat.M32, mat.M33, mat.M34,
				                          mat.M41, mat.M42, mat.M43, mat.M44
			                          });
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
