using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenGL;

namespace PhysCubes.Utility {
	public static class Utility {

		public static string LoadShaderString(string fileName) {
			StreamReader reader = new StreamReader("Shaders/" + fileName + (fileName.EndsWith(".glsl") ? "" : ".glsl"));

			Console.WriteLine("Loading shader " + fileName + "...");

			return reader.ReadToEnd();
		}

		public static void WriteGLError(string context) { Console.WriteLine(context + ": " + Gl.GetError()); }

	}
}
