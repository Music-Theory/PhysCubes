using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenGL;

namespace PhysCubes.Utility {
	public static class GLUtility {

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

	}
}
