namespace ReturnToGL.Rendering {
	using System.Collections.Generic;
	using System.Numerics;
	using OpenGL;
	using PhysCubes;
	using PhysCubes.Utility;
	using Walker.Data.Geometry.Generic.Plane;

	public class RenderText {


		public static Vector2<float> textSquareFactor = new Vector2<float>((float) Program.res.X / Program.res.Y, 1);

		public static Vector2<float> GetCharSize(float scale) {
			return textSquareFactor * (Program.res.Cast<float>() / 2) * scale;
		}

		public static Vector2<float> GetStringSize(string str, float scale) {
			Vector2<float> charSize = GetCharSize(scale);
			return new Vector2<float>(charSize.X * str.Length, charSize.Y);
		}

		public static void DrawString(string str, Vector2<float> screenCoords, float size, Vector4 color) {
			Vector2<float> scale = textSquareFactor * size;
			float charWidth = GetCharSize(size).X;
			for (int i = 0; i < str.Length; i++) {
				char c = str[i];
				DrawChar(c, screenCoords + new Vector2<float>(charWidth * i, 0), size, color);
			}
		}

		public static void DrawChar(char c, Vector2<float> screenCoords, float size, Vector4 color) {
			VAO vao = new VAO(textShader, textQuad, charUVs[GetCharCode(c)], charVecIndices);
			Vector2<float> translation = (screenCoords - (Program.res.Cast<float>() / 2)) * 2 / Program.res.Cast<float>();
			// changing the coordinates to be from -1 to 1 with the origin in the center of the screen
			Vector2<float> scale = textSquareFactor * size;
			vao.Program.Use();
			Gl.BindTexture(font);
			vao.Program["translation"].SetValue(translation.ToNet());
			vao.Program["scale"].SetValue(scale.ToNet());
			vao.Program["color"].SetValue(color.ToNet());
			vao.DrawMode = BeginMode.Triangles;
			vao.Draw();
			vao.Dispose();
		}

		static RenderText() {
			font = new Texture("Inconsolata16.png");
			textShader = new ShaderProgram(GLUtility.LoadShaderString("stringVert"), GLUtility.LoadShaderString("stringFrag"));
			charUVs = new VBO<Vector2>[(int) (font.Size.Width / CHAR_SIZE.X * (font.Size.Height / CHAR_SIZE.Y))];
			PopulateUVArray();
		}

		public static void Dispose() {
			charVecIndices.Dispose();
			textQuad.Dispose();
			foreach (VBO<Vector2> charUV in charUVs) {
				charUV.Dispose();
			}
			font.Dispose();
			textShader.DisposeChildren = true;
			textShader.Dispose();
		}

		static ShaderProgram textShader;

		static VBO<Vector3> textQuad = new VBO<Vector3>(new [] {
			new Vector3(0, 0, 0), new Vector3(1, 0, 0), new Vector3(1, 1, 0), new Vector3(0, 1, 0)
		});

		static VBO<int> charVecIndices = new VBO<int>(new [] {
			0, 1, 2,
			2, 3, 0
		});

		static Texture font;

		public static readonly Vector2 CHAR_SIZE = new Vector2(8, 16);

		static VBO<Vector2>[] charUVs;

		// 112 is the amount of characters in Inconsolata16.bmp

		static int GetCharCode(char c) {
			if (char.IsLetter(c)) {
				// this relies on letters being ordered sequentially
				int shift = char.ToLower(c) - 'a';
				char a = char.IsLower(c) ? 'a' : 'A';
				return charIndices[a] + shift;
			}
			return charIndices.ContainsKey(c) ? charIndices[c] : charIndices['?'];
		}

		static Dictionary<char, int> charIndices = new Dictionary<char, int> {
			{' ', 0},
			{'!', 1},
			{'"', 2},
			{'\'', 7},
			{'(', 8},
			{')', 9},
			{',', 12},
			{'-', 13},
			{'.', 14},
			{'0', 16},
			{'1', 17},
			{'2', 18},
			{'3', 19},
			{'4', 20},
			{'5', 21},
			{'6', 22},
			{'7', 23},
			{'8', 24},
			{'9', 25},
			{':', 26},
			{'?', 31},
			{'A', 33},
			{'[', 59},
			{']', 61},
			{'a', 65},
			{'{', 91},
			{'}', 93}
		};

		static void PopulateUVArray() {
			int charsPerRow = (int) (font.Size.Width / CHAR_SIZE.X);
			int charsPerColumn = (int) (font.Size.Height / CHAR_SIZE.Y);
			for (int y = 0; y < charsPerColumn; y++) {
				Vector2 yVals = new Vector2((float)(charsPerColumn - y - 1) / charsPerColumn, (float)(charsPerColumn - y) / charsPerColumn);
				for (int x = 0; x < charsPerRow; x++) {

					Vector2 xVals = new Vector2((float)x / charsPerRow, (float)(x + 1) / charsPerRow);

					Vector2 bL = new Vector2(xVals.X, yVals.X),
						bR = new Vector2(xVals.Y, yVals.X),
						tR = new Vector2(xVals.Y, yVals.Y),
						tL = new Vector2(xVals.X, yVals.Y);

					charUVs[x + y * charsPerRow] = new VBO<Vector2>(new [] {
						bL, bR, tR, tL
					});
				}
			}
		}

	}
}
