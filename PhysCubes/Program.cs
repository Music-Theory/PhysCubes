using System;
using System.Numerics;
using System.Collections.Generic;
using OpenGL;
using PhysCubes.Utility;
using ReturnToGL.Physics;
using ReturnToGL.Rendering;
using static PhysCubes.Utility.GLUtility;

namespace PhysCubes {
	using System.Numerics;
	using OpenGL.Objects;
	using SDL2;

	static class Program {

		#region Variables

		public static Vector<int> res = new Vector<int>(new [] {1600, 900});

		public static Matrix4x4 projMat = Matrix4x4.CreatePerspectiveFieldOfView(.45f, res[0] / res[1], .1f, 1000f);

		static readonly MatrixStack planeStack = new MatrixStack();

		static readonly Vector3 CAM_POS = new Vector3(0, 0, 50);
		static readonly Vector3 BOX_POS = new Vector3(0, 5, 0);

		internal static Camera cam = new Camera(CAM_POS);

		static InputHandler input = new InputHandler();
		public static bool run = true;

		public static IntPtr window;

		static Texture donkeyTex;

		#endregion

		static void Main(string[] args) {
			#region Make Window

			window = SDL.SDL_CreateWindow("PhysCubes", 50, 50, res[0], res[0], SDL.SDL_WindowFlags.SDL_WINDOW_OPENGL);
			glContext = SDL.SDL_GL_CreateContext(window);
			SDL.SDL_GL_MakeCurrent(window, glContext);
			Console.WriteLine("GL Version: " + Gl);
			WriteGLError("Make Window");

			#endregion

			#region Load Model & Shader


			Physics.boxes.Add(new PhysBox(new PhysState {
				live = false,
				scale = new Vector3(10, .5f, 10),
				Rotation = new System.Numerics.Quaternion(0, 0, 0, 1)
			}));
			Physics.boxes.Add(new PhysBox(new PhysState {
				position = BOX_POS,
				Rotation = Quaternion.Identity,
				scale = new Vector3(1, 1, 1),
				Mass = 1,
				live = true
			}));

			#region Tex Plane

			// Make Tex Plane
			VBO<Vector3> planeVerts = new VBO<Vector3>(new[] {
				new Vector3(-10, 0, 10), new Vector3(10, 0, 10), new Vector3(10, 0, -10), new Vector3(-10, 0, -10),
				new Vector3(-10, 0, 10), new Vector3(10, 0, 10), new Vector3(10, 0, -10), new Vector3(-10, 0, -10)
			});
			VBO<Vector2> planeUV = new VBO<Vector2>(new[] {
				new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1),
				new Vector2(0, 0), new Vector2(1, 0), new Vector2(1, 1), new Vector2(0, 1)
				// lol
			});
			VBO<int> planeIndices = new VBO<int>(new[] {
				0, 1, 2, 2, 3, 0,
				2, 1, 0, 0, 3, 2
			});
			VAO texPlane = new VAO(PhysBox.physShader, planeVerts, planeUV, planeIndices);
			WriteGLError("Make Tex Plane");

			#endregion

			// Load Textures

			Texture indexTex = new Texture("Checker.png");
			Gl.BindTexture(indexTex);
			WriteGLError("Load Texture: Checker");

			donkeyTex = new Texture("DonkeyCube.bmp");
			Gl.BindTexture(donkeyTex);

			currTex = PhysBox.physTex;

			// Finish
			UpdateModelView();
			cam.Refresh();

			#endregion

			#region Set Up GL

			Gl.ClearDepth(1);
			Gl.ClearColor(0, 0, 0, 1);

			Gl.Enable(EnableCap.DepthTest);
			Gl.Enable(EnableCap.CullFace);
			Gl.Enable(EnableCap.Blend);
			Gl.BlendFunc(BlendingFactorSrc.SrcAlpha, BlendingFactorDest.OneMinusSrcAlpha);
			Gl.DepthMask(true);

			Gl.Viewport(0, 0, res[0], res[1]);

			#endregion

			#region Main Loop

			WriteGLError("Begin Loop");

			float textSize = .05f;

			while (run) {

				input.Update();

				Physics.UpdateLiving();

				Gl.Clear(ClearBufferMask.DepthBufferBit | ClearBufferMask.ColorBufferBit);



				for (int i = 0; i < Physics.boxes.Count; i++) {
					PhysBox box = Physics.boxes[i];
					box.Draw(cam, currTex);
					if (drawRef && i > 0) { box.DrawPhysics(cam); }
				}

				texPlane.Program.Use();
				texPlane.Program["transform_mat"].SetValue((planeStack.Result * cam.StackResult).ToGL());
				Gl.BindTexture(indexTex);
				texPlane.Draw();

				//RenderCrosshair();


				if (drawRef) {
					RenderText.DrawString("CFor" + cam.forward, new Vector2(0, RenderText.GetCharSize(textSize).Y * 2), textSize, new Vector4(1, .5f, .5f, 1));
					RenderText.DrawString("CPos" + cam.Position, new Vector2(0, RenderText.GetCharSize(textSize).Y), textSize, new Vector4(1, .5f, .5f, 1));
					RenderText.DrawString("CRot" + cam.rotation, new Vector2(0, 0), textSize, new Vector4(1, .5f, .5f, 1));
				}
			}

			#endregion

			#region Disposal

			// Dispose Plane
			indexTex.Dispose();
			texPlane.DisposeChildren = true;
			texPlane.Dispose();

			// Dispose Tex
			donkeyTex.Dispose();
			PhysBox.StaticDispose();

			GLUtility.Dispose();
			RenderText.Dispose();

			#endregion
		}

		static void WriteMat(Matrix4x4 mat) {
			int[] lengths = new int[4];
			for (int y = 0; y < 4; y++) {
				Console.Write("[");
				for (int x = 0; x < 4; x++) {
					Vector4 row = mat.Row(y);
					string space = "";
					string number = row.Get(x) + ( x < 3 ? ", " : "" );
					for (int i = 0; i < lengths[x] - number.Length; i++) { space += " "; }
					Console.Write(number + space);
					lengths[x] = number.Length + space.Length;
				}
				Console.Write("]\n");
			}
		}

		static Texture currTex;

		internal static void UpdateModelView() {
			planeStack.Clear();
			planeStack.Push(Matrix4x4.CreateTranslation(new Vector3(0, 0, 0)));
		}

		public static void Reset() {
			cam.Position = CAM_POS;
			cam.Rotation = Camera.INIT_CAM_ROT;
			cam.Refresh();
		}

		static bool drawRef = false;

		internal static void ToggleReference() { drawRef = !drawRef; }

		static void PushBox() {
			PhysBox box = Physics.boxes[1];

			Vector3 dir = ( box.currState.position - cam.Position ).Normalize();
			Vector3 point = //box.currState.position + -dir * box.currState.scale;
				new Vector3(0,1,1);

			box.ApplyForce(dir, point);
		}

		static void SpinBox(int sign) {
			PhysBox box = Physics.boxes[1];

			//Vector3 dir = cam.forward;
			//Vector3 point =
		}

		internal static void SpawnBox(int x, int y) {
			Vector<int> mPos = new Vector<int>(new [] {x, y});

			Vector2 centerDist = new Vector2(res[0] - mPos[0], res[1] - mPos[1]);
			centerDist.X *= 1f / res[0];
			centerDist.Y *= 1f / res[1];

			// Res / 2

			//dir = dir.Normalize() * 2;

			Vector3 pos = cam.Position + cam.forward * 5 + (-cam.right * centerDist.X + cam.right / 2f) + (cam.up * centerDist.Y - cam.up / 2f);
			Vector3 dir = pos - cam.Position;

			System.Numerics.Quaternion rotation = Quaternion.Identity;

			PhysBox box = Physics.MakeBox(new PhysState(1) {
				Rotation = rotation,
				live = true,
				position = pos,
				scale = new Vector3(.5f, .5f, .5f)
			});

			Vector3 forcePoint = box.currState.position;

			box.ApplyForce(dir, forcePoint);
			box.ApplyForce(cam.forward * .125f, pos - new Vector3(.125f, 0, 1) * box.currState.scale);
			box.ApplyForce(cam.forward * -.125f, pos + new Vector3(.125f, 0, 1) * box.currState.scale);
			box.currState.live = !paused;
			box.RefreshInit();
		}

		internal static void SwapTex() { currTex = currTex == PhysBox.physTex ? donkeyTex : PhysBox.physTex; }

		static bool paused = false;

		internal static void Pause() {
			paused = !paused;
			for (int i = 1; i < Physics.boxes.Count; i++) {
				PhysBox b = Physics.boxes[i];
				b.currState.live = !paused;
			}
		}

		static int mouseFrames = 0;
		static IntPtr glContext;

	}

}