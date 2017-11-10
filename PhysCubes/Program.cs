using System;
using System.Collections.Generic;
using OpenGL;
using PhysCubes.Utility;
using ReturnToGL.Physics;
using ReturnToGL.Rendering;
using static PhysCubes.Utility.GLUtility;

namespace PhysCubes {
	using System.Numerics;
	using SDL2;
	using Walker.Data.Geometry.Generic.Plane;
	using Walker.Data.Geometry.Speed.Plane;
	using Walker.Data.Geometry.Speed.Rotation;
	using Walker.Data.Geometry.Speed.Space;

	static class Program {
		#region Variables

		public static Vector2<int> res = new Vector2<int>(1600, 900);

		public static Matrix4 projMat = Matrix4.CreatePerspectiveFieldOfView(.45f, res.X / res.Y, .1f, 1000f);

		static readonly MatrixStack planeStack = new MatrixStack();

		static readonly Vector3F CAM_POS = new Vector3F(0, 0, 50);
		static readonly Vector3F BOX_POS = new Vector3F(0, 5, 0);

		internal static Camera cam = new Camera(CAM_POS);

		static InputHandler input = new InputHandler();
		public static bool run = true;

		public static IntPtr window;

		static Texture donkeyTex;

		#endregion

		static void Main(string[] args) {
			#region Make Window

			window = SDL.SDL_CreateWindow("PhysCubes", 50, 50, res.X, res.Y, SDL.SDL_WindowFlags.SDL_WINDOW_OPENGL);
			glContext = SDL.SDL_GL_CreateContext(window);
			SDL.SDL_GL_MakeCurrent(window, glContext);
			Console.WriteLine("GL Version: " + Gl.Version());
			WriteGLError("Make Window");

			#endregion

			#region Load Model & Shader


			Physics.boxes.Add(new PhysBox(new PhysState {
				live = false,
				scale = new Vector3F(10, .5f, 10),
				Rotation = new Vector4F(0, 0, 0, 1)
			}));
			Physics.boxes.Add(new PhysBox(new PhysState {
				position = BOX_POS,
				Rotation = Vector4F.QIdentity,
				scale = new Vector3F(1, 1, 1),
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

			Gl.Viewport(0, 0, res.X, res.Y);

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
				texPlane.Program["transform_mat"].SetValue(planeStack.Result * cam.StackResult);
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

		static void WriteMat(Matrix4 mat) {
			int[] lengths = new int[4];
			for (int y = 0; y < 4; y++) {
				Console.Write("[");
				for (int x = 0; x < 4; x++) {
					Vector4 row = mat[y];
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
			planeStack.Push(Matrix4.CreateTranslation(new Vector3(0, 0, 0)));
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

			Vector3F dir = ( box.currState.position - cam.Position ).Normal;
			Vector3F point = //box.currState.position + -dir * box.currState.scale;
				new Vector3F(0,1,1);

			box.ApplyForce(dir, point);
		}

		static void SpinBox(int sign) {
			PhysBox box = Physics.boxes[1];

			//Vector3 dir = cam.forward;
			//Vector3 point =
		}

		internal static void SpawnBox(int x, int y) {
			Vector2<int> mPos = new Vector2<int>(x, y);

			Vector2<float> centerDist = ((res - mPos) / res).Cast<float>();

			// Res / 2

			//dir = dir.Normalize() * 2;

			Vector3F pos = cam.Position + cam.forward * 5 + (-cam.right * centerDist.X + cam.right / 2f) + (cam.up * centerDist.Y - cam.up / 2f);
			Vector3F dir = pos - cam.Position;

			Vector4F rotation = Vector4F.QIdentity;

			PhysBox box = Physics.MakeBox(new PhysState(1) {
				Rotation = rotation,
				live = true,
				position = pos,
				scale = new Vector3F(.5f, .5f, .5f)
			});

			Vector3F forcePoint = box.currState.position;

			box.ApplyForce(dir, forcePoint);
			box.ApplyForce(cam.forward * .125f, pos - new Vector3F(.125f, 0, 1) * box.currState.scale);
			box.ApplyForce(cam.forward * -.125f, pos + new Vector3F(.125f, 0, 1) * box.currState.scale);
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