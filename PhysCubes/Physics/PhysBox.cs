using System;
using OpenGL;
using PhysCubes;
using PhysCubes.Utility;
using System.Numerics;

namespace ReturnToGL.Physics {

	public struct PhysState {

		public const float MIN_MASS = 0.0001f;

		Quaternion rotation;
		public Vector3 position;
		public Vector3 scale;

		Vector3 linMomentum;
		Vector3 angMomentum;

		Vector3 linearVel;
		Vector3 angularVel;
		Quaternion spin;

		float mass;
		float inertiaTensor;

		public bool live;
		bool updated;

		public PhysState(float mass = 1) {
			rotation = Quaternion.Identity;
			position = Vector3.Zero;
			scale = Vector3.One;

			linMomentum = Vector3.Zero;
			angMomentum = Vector3.Zero;

			linearVel = Vector3.Zero;
			angularVel = Vector3.Zero;
			spin = Vector4.Zero;

			this.mass = 1;
			inertiaTensor = mass * scale.X * scale.X * 1 / 6;
			live = true;

			updated = false;
		}

		public Quaternion Rotation {
			get {
				Update();
				return rotation;
			}
			set {
				rotation = value;
				updated = false;
			}
		}

		public Vector3 LinMomentum {
			get => linMomentum;
			set {
				linMomentum = value;
				updated = false;
			}
		}

		public Vector3 AngMomentum {
			get => angMomentum;
			set {
				angMomentum = value;
				updated = false;
			}
		}

		public float Mass {
			get => mass;
			set {
				mass = Math.Max(MIN_MASS, value); // because 0 mass would break things
				updated = false;
			}
		}

		public Vector3 LinearVel {
			get {
				Update();
				return linearVel;
			}
		}

		public Vector3 AngularVel {
			get {
				Update();
				return angularVel;
			}
		}

		public Vector4 Spin {
			get {
				Update();
				return spin;
			}
		}

		public float InertiaTensor {
			get {
				Update();
				return inertiaTensor;
			}
		}

		void Update() {
			if (!updated) { Recalculate(); }
		}

		void Recalculate() {
			linearVel = linMomentum / mass;
			inertiaTensor = mass * scale.X * scale.X * 1 / 6;
			angularVel = angMomentum / inertiaTensor;
			rotation = Quaternion.Normalize(rotation);
			spin = new Quaternion(angularVel, 0) * .5f * rotation;
			updated = true;
		}

	}

	public class PhysBox {

		public PhysState initState;
		public PhysState prevState;
		public PhysState currState;

		public PhysState InterState => Physics.Interpolate(prevState, currState, Physics.currAlpha);

		public AxisAlignedBoundingBox bBox;

		bool interStackUpdated = false;
		MatrixStack interStack = new MatrixStack();

		public Texture texture = physTex;

		public Matrix4x4 InterStackRes {
			get {
				if (!interStackUpdated) {
					PhysState s = InterState;
					interStack.Clear();
					interStack.Push(Matrix4x4.CreateScale(s.scale));
					interStack.Push(Matrix4x4.CreateFromQuaternion(s.Rotation));
					interStack.Push(Matrix4x4.CreateTranslation(s.position));
					interStackUpdated = true;
				}
				return interStack.Result;
			}
		}

		#region Constructor

		public PhysBox(Vector3 pos, Quaternion rot, Vector3 sca, Vector3 linMom, Vector3 angMom, bool live) {
			initState = new PhysState() {
				position = pos,
				Rotation = rot,
				scale = sca,
				LinMomentum = linMom,
				live = live
			};
			prevState = initState;
			currState = initState;

			bBox = new AxisAlignedBoundingBox(new Vector3(-1, -1, -1), new Vector3(1, 1, 1));

			bBox.Transform(InterStackRes);

			UpdateBoundingBox();
		}

		public PhysBox(Vector3 pos) : this(pos, Quaternion.Identity, Vector3.One, Vector3.Zero, Vector3.Zero, true) { }
		public PhysBox(Vector3 pos, Vector3 sca) : this(pos, Quaternion.Identity, sca, Vector3.Zero, Vector3.Zero, true) { }
		public PhysBox(Vector3 pos, Vector3 sca, Vector3 vel) : this(pos, Quaternion.Identity, sca, vel, Vector3.Zero, true) { }
		public PhysBox(Vector3 pos, bool live) : this(pos, Quaternion.Identity, Vector3.One, Vector3.Zero, Vector3.Zero, live) { }
		public PhysBox(Vector3 pos, Vector3 sca, bool live) : this(pos, Quaternion.Identity, sca, Vector3.Zero, Vector3.Zero, live) { }

		public PhysBox(PhysState init) {
			initState = init;
			prevState = init;
			currState = init;

			bBox = new AxisAlignedBoundingBox(new Vector3(-1, -1, -1), new Vector3(1, 1, 1));
			bBox.Transform(InterStackRes);
			UpdateBoundingBox();
		}

		#endregion

		public void RefreshInit() { initState = currState; }

		public void ApplyForce(Vector3 force, Vector3 pos) {
			currState.LinMomentum += force;
			currState.AngMomentum += Vector3.Cross(force, pos - currState.position);
		}

		public Vector3 GetVelOfPoint(Vector3 point) {
			return currState.LinearVel + Vector3.Cross(currState.AngularVel, point - currState.position);
		}

		public void Refresh() {
			interStackUpdated = false;
			UpdateBoundingBox();
		}

		void UpdateBoundingBox() {
			//bBox.Transform(StackRes);
			bBox.Translate(currState.position - bBox.Center);
		}

		public void Draw(Camera cam, Texture tex = null) {
			physCube.Program.Use();
			physCube.Program["transform_mat"].SetValue(InterStackRes * cam.StackResult);
			Gl.BindTexture(tex ?? texture);
			physCube.Draw();
		}

		public void DrawPhysics(Camera cam) {
			GLUtility.lineVAO.Program.Use();
			MatrixStack lineStack = new MatrixStack();
			PhysState s = currState;
			Matrix4x4 trans = Matrix4x4.CreateTranslation(s.position);

			GLUtility.lineVAO.Program["color"].SetValue(new Vector3(.5f, .5f, 1));
			lineStack.Push(Matrix4x4.CreateScale(new Vector3(4, 4, 4) * s.scale));
			lineStack.Push(Matrix4x4.CreateFromQuaternion(s.Rotation));
			lineStack.Push(trans);
			GLUtility.lineVAO.Program["transform_mat"].SetValue(lineStack.Result * cam.StackResult);
			GLUtility.lineVAO.Draw();

			GLUtility.lineVAO.Program["color"].SetValue(new Vector3(.5f, 1, .5f));
			lineStack.Clear();
			lineStack.Push(Matrix4F.CreateScaling(new Vector3(4, 4, 4) * s.scale));
			lineStack.Push((new Vector4((float) Math.PI / 2f, Vector3.Right) * s.Rotation).QMatrix);
			lineStack.Push(trans);
			GLUtility.lineVAO.Program["transform_mat"].SetValue(lineStack.Result * cam.StackResult);
			GLUtility.lineVAO.Draw();

			GLUtility.lineVAO.Program["color"].SetValue(new Vector3(1, .5f, .5f));
			lineStack.Clear();
			lineStack.Push(Matrix4F.CreateScaling(new Vector3(4, 4, 4) * s.scale));
			lineStack.Push((new Vector4((float)Math.PI / 2f, Vector3.Down) * s.Rotation).QMatrix);
			lineStack.Push(trans);
			GLUtility.lineVAO.Program["transform_mat"].SetValue(lineStack.Result * cam.StackResult);
			GLUtility.lineVAO.Draw();

			GLUtility.lineVAO.Program["color"].SetValue(new Vector3(1, .5f, .75f));
			lineStack.Clear();
			lineStack.Push(Matrix4F.CreateScaling(new Vector3(4, 4, 4) * s.scale * s.AngMomentum.Length));
			lineStack.Push(new Vector4((float) (Math.PI / 2f), s.AngMomentum.Normalize()).QMatrix);
			lineStack.Push(Matrix4F.CreateTranslation(s.position + new Vector3(0, 2, -2) * s.scale));
			GLUtility.lineVAO.Program["transform_mat"].SetValue(lineStack.Result * cam.StackResult);
			GLUtility.lineVAO.Draw();
		}

		#region Static

		public static void StaticDispose() {
			physCube.DisposeChildren = true;
			physCube.Dispose();
			physShader.DisposeChildren = true;
			physShader.Dispose();
			physTex.Dispose();
		}

		static PhysBox() {
			physTex = new Texture("BoxNumber.png");
			GLUtility.WriteGLError("Load PhysTex");
			physShader = new ShaderProgram(GLUtility.LoadShaderString("texVert"), GLUtility.LoadShaderString("texFrag"));

			physCube = new VAO(physShader, physVerts, physUV, physIndices);

		}

		public static ShaderProgram physShader;

		public static Texture physTex;

		static VBO<Vector3> physVerts = new VBO<Vector3>(new[] {
			                                                         // Back Face
			                                                         new Vector3(1, -1, -1), new Vector3(-1, -1, -1), new Vector3(-1, 1, -1), new Vector3(1, 1, -1),
			                                                         // Front Face
			                                                         new Vector3(-1, -1, 1), new Vector3(1, -1, 1), new Vector3(1, 1, 1), new Vector3(-1, 1, 1),

			                                                         // This is so that UVs work
			                                                         // Bottom Face
			                                                         new Vector3(-1, -1, -1), new Vector3(1, -1, -1), new Vector3(1, -1, 1), new Vector3(-1, -1, 1),
			                                                         // Top Face
			                                                         new Vector3(-1, 1, 1), new Vector3(1, 1, 1), new Vector3(1, 1, -1), new Vector3(-1, 1, -1),

			                                                         // Left Face
			                                                         new Vector3(-1, -1, -1), new Vector3(-1, -1, 1), new Vector3(-1, 1, 1), new Vector3(-1, 1, -1),
			                                                         // Right Face
			                                                         new Vector3(1, -1, 1), new Vector3(1, -1, -1), new Vector3(1, 1, -1), new Vector3(1, 1, 1)
		                                                         });

		static VBO<Vector2> physUV = new VBO<Vector2>(new[] {
			                                                      // Back: 6
			                                                      new Vector2(.25f, .25f), new Vector2(.5f, .25f), new Vector2(.5f, .5f), new Vector2(.25f, .5f),
			                                                      // Front: 1
			                                                      new Vector2(0, 0), new Vector2(.25f, 0), new Vector2(.25f, .25f), new Vector2(0, .25f),
			                                                      // Bottom: 2
			                                                      new Vector2(.25f, 0), new Vector2(.5f, 0), new Vector2(.5f, .25f), new Vector2(.25f, .25f),
			                                                      // Top: 5
			                                                      new Vector2(0, .25f), new Vector2(.25f, .25f), new Vector2(.25f, .5f), new Vector2(0, .5f),
			                                                      // Left: 4
			                                                      new Vector2(.75f, 0), new Vector2(1, 0), new Vector2(1, .25f), new Vector2(.75f, .25f),
			                                                      // Right: 3
			                                                      new Vector2(.5f, 0), new Vector2(.75f, 0), new Vector2(.75f, .25f), new Vector2(.5f, .25f)
		                                                      });

		static VBO<int> physIndices = new VBO<int>(new[] {
			                                                 // Back
			                                                 0,   1,  2,  2,  3,  0,
			                                                 // Front
			                                                 4,   5,  6,  6,  7,  4,
			                                                 // Bottom
			                                                 8,   9, 10, 10, 11,  8,
			                                                 // Top
			                                                 12, 13, 14, 14, 15, 12,
			                                                 // Left
			                                                 16, 17, 18, 18, 19, 16,
			                                                 // Right
			                                                 20, 21, 22, 22, 23, 20
		                                                 });

		public static VAO physCube;

		#endregion

	}
}
