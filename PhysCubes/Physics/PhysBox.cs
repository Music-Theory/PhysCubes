using System;
using OpenGL;
using PhysCubes;
using PhysCubes.Utility;

namespace ReturnToGL.Physics {
	using Walker.Data.Geometry.Generic.Plane;
	using Walker.Data.Geometry.Speed.Rotation;
	using Walker.Data.Geometry.Speed.Space;

	public struct PhysState {

		public const float MIN_MASS = 0.0001f;

		Vector4F rotation;
		public Vector3F position;
		public Vector3F scale;

		Vector3F linMomentum;
		Vector3F angMomentum;

		Vector3F linearVel;
		Vector3F angularVel;
		Vector4F spin;

		float mass;
		float inertiaTensor;

		public bool live;
		bool updated;

		public PhysState(float mass = 1) {
			rotation = Vector4F.QIdentity;
			position = Vector3F.Zero;
			scale = Vector3F.UnitScale;

			linMomentum = Vector3F.Zero;
			angMomentum = Vector3F.Zero;

			linearVel = Vector3F.Zero;
			angularVel = Vector3F.Zero;
			spin = Vector4F.Zero;

			this.mass = 1;
			inertiaTensor = mass * scale.x * scale.x * 1 / 6;
			live = true;

			updated = false;
		}

		public Vector4F Rotation {
			get {
				Update();
				return rotation;
			}
			set {
				rotation = value;
				updated = false;
			}
		}

		public Vector3F LinMomentum {
			get => linMomentum;
			set {
				linMomentum = value;
				updated = false;
			}
		}

		public Vector3F AngMomentum {
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

		public Vector3F LinearVel {
			get {
				Update();
				return linearVel;
			}
		}

		public Vector3F AngularVel {
			get {
				Update();
				return angularVel;
			}
		}

		public Vector4F Spin {
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
			inertiaTensor = mass * scale.x * scale.x * 1 / 6;
			angularVel = angMomentum / inertiaTensor;
			rotation /= rotation.Length;
			spin = new Vector4F(angularVel, 0) * .5f * rotation;
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

		public Matrix4F InterStackRes {
			get {
				if (!interStackUpdated) {
					PhysState s = InterState;
					interStack.Clear();
					interStack.Push(Matrix4F.CreateScaling(s.scale));
					interStack.Push(s.Rotation.QMatrix);
					interStack.Push(Matrix4F.CreateTranslation(s.position));
					interStackUpdated = true;
				}
				return interStack.Result;
			}
		}

		#region Constructor

		public PhysBox(Vector3F pos, Vector4F rot, Vector3F sca, Vector3F linMom, Vector3F angMom, bool live) {
			initState = new PhysState() {
				position = pos,
				Rotation = rot,
				scale = sca,
				LinMomentum = linMom,
				live = live
			};
			prevState = initState;
			currState = initState;

			bBox = new AxisAlignedBoundingBox(new Vector3F(-1, -1, -1), new Vector3F(1, 1, 1));

			bBox.Transform(InterStackRes);

			UpdateBoundingBox();
		}

		public PhysBox(Vector3F pos) : this(pos, Vector4F.Identity, Vector4F.UnitScale, Vector3F.Zero, Vector3F.Zero, true) { }
		public PhysBox(Vector3F pos, Vector3F sca) : this(pos, Vector4F.Identity, sca, Vector3F.Zero, Vector3F.Zero, true) { }
		public PhysBox(Vector3F pos, Vector3F sca, Vector3F vel) : this(pos, Vector4F.Identity, sca, vel, Vector3F.Zero, true) { }
		public PhysBox(Vector3F pos, bool live) : this(pos, Vector4F.Identity, Vector3F.UnitScale, Vector3F.Zero, Vector3F.Zero, live) { }
		public PhysBox(Vector3F pos, Vector3F sca, bool live) : this(pos, Vector4F.Identity, sca, Vector3F.Zero, Vector3F.Zero, live) { }

		public PhysBox(PhysState init) {
			initState = init;
			prevState = init;
			currState = init;

			bBox = new AxisAlignedBoundingBox(new Vector3F(-1, -1, -1), new Vector3F(1, 1, 1));
			bBox.Transform(InterStackRes);
			UpdateBoundingBox();
		}

		#endregion

		public void RefreshInit() { initState = currState; }

		public void ApplyForce(Vector3F force, Vector3F pos) {
			currState.LinMomentum += force;
			currState.AngMomentum += force.Cross(pos - currState.position);
		}

		public Vector3F GetVelOfPoint(Vector3F point) {
			return currState.LinearVel + currState.AngularVel.Cross(point - currState.position);
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
			Matrix4F trans = Matrix4F.CreateTranslation(s.position);

			GLUtility.lineVAO.Program["color"].SetValue(new Vector3F(.5f, .5f, 1));
			lineStack.Push(Matrix4F.CreateScaling(new Vector3F(4, 4, 4) * s.scale));
			lineStack.Push(s.Rotation.QMatrix);
			lineStack.Push(trans);
			GLUtility.lineVAO.Program["transform_mat"].SetValue(lineStack.Result * cam.StackResult);
			GLUtility.lineVAO.Draw();

			GLUtility.lineVAO.Program["color"].SetValue(new Vector3F(.5f, 1, .5f));
			lineStack.Clear();
			lineStack.Push(Matrix4F.CreateScaling(new Vector3F(4, 4, 4) * s.scale));
			lineStack.Push((new Vector4F((float) Math.PI / 2f, Vector3F.Right) * s.Rotation).QMatrix);
			lineStack.Push(trans);
			GLUtility.lineVAO.Program["transform_mat"].SetValue(lineStack.Result * cam.StackResult);
			GLUtility.lineVAO.Draw();

			GLUtility.lineVAO.Program["color"].SetValue(new Vector3F(1, .5f, .5f));
			lineStack.Clear();
			lineStack.Push(Matrix4F.CreateScaling(new Vector3F(4, 4, 4) * s.scale));
			lineStack.Push((new Vector4F((float)Math.PI / 2f, Vector3F.Down) * s.Rotation).QMatrix);
			lineStack.Push(trans);
			GLUtility.lineVAO.Program["transform_mat"].SetValue(lineStack.Result * cam.StackResult);
			GLUtility.lineVAO.Draw();

			GLUtility.lineVAO.Program["color"].SetValue(new Vector3F(1, .5f, .75f));
			lineStack.Clear();
			lineStack.Push(Matrix4F.CreateScaling(new Vector3F(4, 4, 4) * s.scale * s.AngMomentum.Length));
			lineStack.Push((Quaternion.FromAngleAxis((float) Math.PI / 2f, s.AngMomentum.Normalize())).Matrix4);
			lineStack.Push(Matrix4F.CreateTranslation(s.position + new Vector3F(0, 2, -2) * s.scale));
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

		static VBO<Vector3F> physVerts = new VBO<Vector3F>(new[] {
				// Back Face
				new Vector3F(1, -1, -1), new Vector3F(-1, -1, -1), new Vector3F(-1, 1, -1), new Vector3F(1, 1, -1),
				// Front Face
				new Vector3F(-1, -1, 1), new Vector3F(1, -1, 1), new Vector3F(1, 1, 1), new Vector3F(-1, 1, 1),

				// This is so that UVs work
				// Bottom Face
				new Vector3F(-1, -1, -1), new Vector3F(1, -1, -1), new Vector3F(1, -1, 1), new Vector3F(-1, -1, 1),
				// Top Face
				new Vector3F(-1, 1, 1), new Vector3F(1, 1, 1), new Vector3F(1, 1, -1), new Vector3F(-1, 1, -1),

				// Left Face
				new Vector3F(-1, -1, -1), new Vector3F(-1, -1, 1), new Vector3F(-1, 1, 1), new Vector3F(-1, 1, -1),
				// Right Face
				new Vector3F(1, -1, 1), new Vector3F(1, -1, -1), new Vector3F(1, 1, -1), new Vector3F(1, 1, 1)
			});
		static VBO<Vector2F> physUV = new VBO<Vector2F>(new[] {
				// Back: 6
				new Vector2FF(.25, .25), new Vector2F(.5, .25), new Vector2F(.5, .5), new Vector2F(.25, .5),
				// Front: 1
				new Vector2F(0, 0), new Vector2F(.25, 0), new Vector2F(.25, .25), new Vector2F(0, .25),
				// Bottom: 2
				new Vector2F(.25, 0), new Vector2F(.5, 0), new Vector2F(.5, .25), new Vector2F(.25, .25),
				// Top: 5
				new Vector2F(0, .25), new Vector2F(.25, .25), new Vector2F(.25, .5), new Vector2F(0, .5),
				// Left: 4
				new Vector2F(.75, 0), new Vector2F(1, 0), new Vector2F(1, .25), new Vector2F(.75, .25),
				// Right: 3
				new Vector2F(.5, 0), new Vector2F(.75, 0), new Vector2F(.75, .25), new Vector2F(.5, .25),
			});
		static VBO<int> physIndices = new VBO<int>(new[] {
				// Back
				0, 1, 2, 2, 3, 0,
				// Front
				4, 5, 6, 6, 7, 4,
				// Bottom
				8, 9, 10, 10, 11, 8,
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
