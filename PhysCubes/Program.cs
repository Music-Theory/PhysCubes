using System;
using System.Collections.Generic;
using OpenGL;
using PhysCubes.Utility;
using ReturnToGL.Physics;
using ReturnToGL.Rendering;
using SFML.Audio;
using SFML.Graphics;
using SFML.System;
using SFML.Window;
using static PhysCubes.Utility.GLUtility;
using Texture = OpenGL.Texture;

//using OpenTK.Graphics;

namespace PhysCubes {

	static class Program {
		#region Variables

		public static Vector2 res = new Vector2(1600, 900);
		
		public static Matrix4 projMat = Matrix4.CreatePerspectiveFieldOfView(.45f, res.x / res.y, .1f, 1000f);

		static readonly MatrixStack planeStack = new MatrixStack();

		static readonly Vector3 CAM_POS = new Vector3(0, 0, 50);
		static readonly Vector3 BOX_POS = new Vector3(0, 5, 0);

		static Camera cam = new Camera(CAM_POS);

		static readonly List<Keyboard.Key> pressedKeys = new List<Keyboard.Key>();

		public static RenderWindow window;

		static Music kansas = null;

		static Texture donkeyTex;

		#endregion

		static void Main(string[] args) {
			#region Make Window

			try { kansas = new Music("C:\\Users\\Ash\\Documents\\Documents\\Music\\COWS.ogg"); } catch {
				Console.WriteLine("Could not load music.");
			}

			ContextSettings contextSettings = new ContextSettings {
				DepthBits = 32,
				MajorVersion = 4,
				MinorVersion = 4
			};

			window = new RenderWindow(new VideoMode((uint) res.x, (uint) res.y), "OpenGL", Styles.Default, contextSettings);
			window.SetFramerateLimit(60);

			window.SetActive();

			Console.WriteLine("GL Version: " + window.Settings.MajorVersion + "." + window.Settings.MinorVersion);
			WriteGLError("Make Window");

			window.Closed += OnClosed;
			window.KeyPressed += OnKeyPressed;
			window.KeyReleased += OnKeyReleased;
			window.Resized += OnResized;
			window.MouseButtonPressed += OnMousePressed;
			window.MouseButtonReleased += OnMouseReleased;

			#endregion

			#region Load Model & Shader


			Physics.boxes.Add(new PhysBox(new PhysState {
				live = false,
				scale = new Vector3(10, .5, 10),
				Rotation = new Quaternion(0, 0, 0, 1)
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

			Gl.Viewport(0, 0, (int) res.x, (int) res.y);

			#endregion

			#region Main Loop

			WriteGLError("Begin Loop");

			float textSize = .05f;

			while (window.IsOpen) {
				window.DispatchEvents();

				UpdateKeys();

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
					RenderText.DrawString("CFor" + cam.forward, new Vector2(0, RenderText.GetCharSize(textSize).y * 2), textSize, new Vector4(1, .5, .5, 1));
					RenderText.DrawString("CPos" + cam.Position, new Vector2(0, RenderText.GetCharSize(textSize).y), textSize, new Vector4(1, .5, .5, 1));
					RenderText.DrawString("CRot" + cam.rotation, new Vector2(0, 0), textSize, new Vector4(1, .5, .5, 1));
				}

				window.Display();
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
					string number = row[x] + ( x < 3 ? ", " : "" );
					for (int i = 0; i < lengths[x] - number.Length; i++) { space += " "; }
					Console.Write(number + space);
					lengths[x] = number.Length + space.Length;
				}
				Console.Write("]\n");
			}
		}

		static Texture currTex;

		static void UpdateModelView() {
			planeStack.Clear();
			planeStack.Push(Matrix4.CreateTranslation(new Vector3(0, 0, 0)));
		}

		public static void Reset() {
			cam.Position = CAM_POS;
			cam.Rotation = Camera.INIT_CAM_ROT;
			cam.Refresh();
			kansas?.Stop();
			if (musicActive) {
				kansas?.Play();
			}
		}

		static void OnClosed(object sender, EventArgs e) {
			RenderWindow window = (RenderWindow) sender;
			window.Close();
		}

		static void OnKeyPressed(object sender, KeyEventArgs e) {
			if (!pressedKeys.Contains(e.Code)) { pressedKeys.Add(e.Code); }
			switch (e.Code) {
				case Keyboard.Key.M:
					ToggleMusic();
					break;
				case Keyboard.Key.L:
					ToggleReference();
					break;
				case Keyboard.Key.P:
					Pause();
					break;
				case Keyboard.Key.Num0:
					Reset();
					cam.Position = Vector3.Zero;
					break;
			}
		}

		static bool drawRef = false;

		static void ToggleReference() { drawRef = !drawRef; }

		static bool musicActive = false;

		static void ToggleMusic() {
			if (kansas == null) { return; }
			if (musicActive) {
				kansas.Pause();
			} else {
				kansas.Play();
			}
			musicActive = !musicActive;
		}

		static void OnKeyReleased(object sender, KeyEventArgs e) {
			pressedKeys.Remove(e.Code);
		}

		static bool mousePressed;

		static void OnMousePressed(object sender, MouseButtonEventArgs e) {
			switch (e.Button) {
				case Mouse.Button.Left:
					mousePressed = true;
					break;
				case Mouse.Button.Right:
					PushBox();
					break;
				case Mouse.Button.Middle:
					break;
				case Mouse.Button.XButton1:
					break;
				case Mouse.Button.XButton2:
					break;
				case Mouse.Button.ButtonCount:
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}
		}

		static void OnMouseReleased(object sender, MouseButtonEventArgs mouseButtonEventArgs) { mousePressed = false; }

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

		static void SpawnBox() {
			Vector2i mPos = Mouse.GetPosition(window);

			Vector2 centerDist = new Vector2(res.x - mPos.X, res.y - mPos.Y) / res;

			// Res / 2

			//dir = dir.Normalize() * 2;

			Vector3 pos = cam.Position + cam.forward * 5 + (-cam.right * centerDist.x + cam.right / 2f) + (cam.up * centerDist.y - cam.up / 2f);
			Vector3 dir = pos - cam.Position;

			Quaternion rotation = Quaternion.Identity;

			PhysBox box = Physics.MakeBox(new PhysState(1) {
				Rotation = rotation,
				live = true,
				position = pos,
				scale = new Vector3(.5, .5, .5)
			});

			Vector3 forcePoint = box.currState.position;

			box.ApplyForce(dir, forcePoint);
			box.ApplyForce(cam.forward * .125f, pos - new Vector3(.125, 0, 1) * box.currState.scale);
			box.ApplyForce(cam.forward * -.125f, pos + new Vector3(.125, 0, 1) * box.currState.scale);
			box.currState.live = !paused;
			box.RefreshInit();
		}

		static void SwapTex() { currTex = currTex == PhysBox.physTex ? donkeyTex : PhysBox.physTex; }

		static bool paused = false;
		
		static void Pause() {
			paused = !paused;
			for (int i = 1; i < Physics.boxes.Count; i++) {
				PhysBox b = Physics.boxes[i];
				b.currState.live = !paused;
			}
		}

		static int mouseFrames = 0;

		static void UpdateKeys() {
			if (mousePressed && mouseFrames > 15) {
				mouseFrames = 0;
				SpawnBox();
			} else { mouseFrames++; }
			foreach (Keyboard.Key key in pressedKeys) {
				switch (key) {
					case Keyboard.Key.Escape:
						for (int i = Physics.boxes.Count - 1; i > 1; i--) { Physics.boxes.RemoveAt(i); }
						break;
					case Keyboard.Key.A:
						cam.Move(-1, 0, 0);
						break;
					case Keyboard.Key.D:
						cam.Move(1, 0, 0);
						break;
					case Keyboard.Key.E:
						cam.Move(0, 1, 0);
						break;
					case Keyboard.Key.R:
						Reset();
						break;
					case Keyboard.Key.Space:
						Physics.ResetBoxes();
						break;
					case Keyboard.Key.S:
						cam.Move(0, 0, -1);
						break;
					case Keyboard.Key.T:
						SwapTex();
						break;
					case Keyboard.Key.W:
						cam.Move(0, 0, 1);
						break;
					case Keyboard.Key.Q:
						cam.Move(0, -1, 0);
						break;
					case Keyboard.Key.Numpad2:
						cam.Rotate(-1, 0, 0);
						break;
					case Keyboard.Key.Numpad4:
						cam.Rotate(0, 1, 0);
						break;
					case Keyboard.Key.Numpad6:
						cam.Rotate(0, -1, 0);
						break;
					case Keyboard.Key.Numpad7:
						cam.Rotate(0, 0, 1);
						break;
					case Keyboard.Key.Numpad8:
						cam.Rotate(1, 0, 0);
						break;
					case Keyboard.Key.Numpad9:
						cam.Rotate(0, 0, -1);
						break;
				}
			}
		}

		static void OnResized(object sender, SizeEventArgs e) {
			projMat = Matrix4.CreatePerspectiveFieldOfView(.45f, (float) e.Width / e.Height, .1f, 1000f);

			res.x = e.Width;
			res.y = e.Height;

			// this is assuming that y < x
			RenderText.textSquareFactor = new Vector2(res.y / res.x, 1);

			UpdateModelView();
			cam.Refresh();

			Gl.Viewport(0, 0, (int) e.Width, (int) e.Height);
		}

	}

}