using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenGL;
using PhysCubes;
using PhysCubes.Utility;

namespace ReturnToGL.Rendering {
	public class RenderText {


		public static Vector2 textSquareFactor = new Vector2(Program.res.y / Program.res.x, 1);

		public static Vector2 GetCharSize(float scale) { return textSquareFactor * ( Program.res / 2 ) * scale; }

		public static Vector2 GetStringSize(string str, float scale) {
			Vector2 charSize = GetCharSize(scale);
			return new Vector2(charSize.x * str.Length, charSize.y);
		}

		public static void DrawString(string str, Vector2 screenCoords, float size, Vector4 color) {
			Vector2 scale = textSquareFactor * size;
			float charWidth = GetCharSize(size).x;
			for (int i = 0; i < str.Length; i++) {
				char c = str[i];
				DrawChar(c, screenCoords + new Vector2(charWidth * i, 0), size, color);
			}
		}

		public static void DrawChar(char c, Vector2 screenCoords, float size, Vector4 color) {
			VAO vao = new VAO(textShader, textQuad, charUVs[GetCharCode(c)], charVecIndices);
			Vector2 translation = ( screenCoords - Program.res / 2 ) * 2 / Program.res; // changing the coordinates to be from -1 to 1 with the origin in the center of the screen
			Vector2 scale = textSquareFactor * size;
			vao.Program.Use();
			Gl.BindTexture(font);
			vao.Program["translation"].SetValue(translation);
			vao.Program["scale"].SetValue(scale);
			vao.Program["color"].SetValue(color);
			vao.DrawMode = BeginMode.Triangles;
			vao.Draw();
			vao.Dispose();
		}

		static RenderText() {
			font = new Texture("Inconsolata16.png");
			textShader = new ShaderProgram(GLUtility.LoadShaderString("stringVert"), GLUtility.LoadShaderString("stringFrag"));
			charUVs = new VBO<Vector2>[(int) ( (font.Size.Width / CHAR_SIZE.x) * (font.Size.Height / CHAR_SIZE.y) )];
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
				char a = ( char.IsLower(c) ? 'a' : 'A' );
				return charIndices[a] + shift;
			} 
			return charIndices.ContainsKey(c) ? charIndices[c] : charIndices['?'];
		}

		static Dictionary<char, int> charIndices = new Dictionary<char, int>() {
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
			int charsPerRow = (int) ( font.Size.Width / CHAR_SIZE.x );
			int charsPerColumn = (int) ( font.Size.Height / CHAR_SIZE.y );
			for (int y = 0; y < charsPerColumn; y++) {
				Vector2 yVals = new Vector2((float)(charsPerColumn - y - 1) / charsPerColumn, (float)(charsPerColumn - y) / charsPerColumn);
				for (int x = 0; x < charsPerRow; x++) {

					Vector2 xVals = new Vector2((float)x / charsPerRow, (float)(x + 1) / charsPerRow);

					Vector2 bL = new Vector2(xVals.x, yVals.x),
						bR = new Vector2(xVals.y, yVals.x),
						tR = new Vector2(xVals.y, yVals.y),
						tL = new Vector2(xVals.x, yVals.y);

					charUVs[x + y * charsPerRow] = new VBO<Vector2>(new [] {
						bL, bR, tR, tL
					});
				}
			}
		}

	}
}
