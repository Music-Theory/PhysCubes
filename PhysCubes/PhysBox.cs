using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenGL;
using PhysCubes.Utility;
using static PhysCubes.Utility.Utility;
using static PhysCubes.Physics;

namespace PhysCubes {

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
			scale = Vector3.UnitScale;

			linMomentum = Vector3.Zero;
			angMomentum = Vector3.Zero;

			linearVel = Vector3.Zero;
			angularVel = Vector3.Zero;
			spin = Quaternion.Zero;

			this.mass = 1;
			inertiaTensor = mass * scale.x * scale.x * 1 / 6;
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
			get { return linMomentum; }
			set {
				linMomentum = value;
				updated = false;
			}
		}

		public Vector3 AngMomentum {
			get { return angMomentum; }
			set {
				angMomentum = value;
				updated = false;
			}
		}

		public float Mass {
			get { return mass; }
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

		public Quaternion Spin {
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
			spin = new Quaternion(new Vector4(angularVel, 0)) * .5f * rotation;
			updated = true;
		}

	}

	public class PhysBox {

		public PhysState initState;
		public PhysState prevState;
		public PhysState currState;

		public PhysState InterState {
			get { return Physics.Interpolate(prevState, currState, currAlpha); }
		}

		public AxisAlignedBoundingBox bBox;

		bool interStackUpdated = false;
		MatrixStack interStack = new MatrixStack();

		public Texture texture = physTex;

		public Matrix4 InterStackRes {
			get {
				if (!interStackUpdated) {
					PhysState s = InterState;
					interStack.Clear();
					interStack.Push(Matrix4.CreateScaling(s.scale));
					interStack.Push(s.Rotation.Matrix4);
					interStack.Push(Matrix4.CreateTranslation(s.position));
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

		public PhysBox(Vector3 pos) : this(pos, Quaternion.Identity, Vector3.UnitScale, Vector3.Zero, Vector3.Zero, true) { }
		public PhysBox(Vector3 pos, Vector3 sca) : this(pos, Quaternion.Identity, sca, Vector3.Zero, Vector3.Zero, true) { }
		public PhysBox(Vector3 pos, Vector3 sca, Vector3 vel) : this(pos, Quaternion.Identity, sca, vel, Vector3.Zero, true) { }
		public PhysBox(Vector3 pos, bool live) : this(pos, Quaternion.Identity, Vector3.UnitScale, Vector3.Zero, Vector3.Zero, live) { }
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

		public void Refresh() {
			interStackUpdated = false;
			UpdateBoundingBox();
		}

		void UpdateBoundingBox() {
			//bBox.Transform(StackRes);
			bBox.Translate(currState.position - bBox.Center);
		}

		public void Accelerate(Vector3 lin) {
			//float x = Math.Max(-MAX_VEL, Math.Min(MAX_VEL, lin.x + currState.linearVel.x)),
			//	y = Math.Max(-MAX_VEL, Math.Min(MAX_VEL, lin.y + currState.linearVel.y)),
			//	z = Math.Max(-MAX_VEL, Math.Min(MAX_VEL, lin.z + currState.linearVel.z));
			//currState.linearVel = new Vector3(x, y, z);
		}

		void ApplyVelocity() {
			//currState.position += currState.linearVel;
			//Refresh();
			//List<PhysBox> collisions = Physics.GetCollisions(this);
			//if (collisions.Count > 0) {
			//	currState.position.y = collisions[0].currState.position.y + collisions[0].currState.scale.y + currState.scale.y;
			//	currState.linearVel.y = -currState.linearVel.y * ELASTICITY;
			//}
		}

		public void Update() {
			Accelerate(GRAVITY);
			ApplyVelocity();
			if (Physics.CheckForDead(this)) { currState.live = false; }
		}

		public void Draw(Camera cam, Texture tex = null) {
			physCube.Program.Use();
			physCube.Program["transform_mat"].SetValue(InterStackRes * cam.StackResult);
			Gl.BindTexture(tex ?? texture);
			physCube.Draw();
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
			WriteGLError("Load PhysTex");
			physShader = new ShaderProgram(LoadShaderString("texVert"), LoadShaderString("texFrag"));
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
				new Vector2(.25, .25), new Vector2(.5, .25), new Vector2(.5, .5), new Vector2(.25, .5),
				// Front: 1
				new Vector2(0, 0), new Vector2(.25, 0), new Vector2(.25, .25), new Vector2(0, .25),
				// Bottom: 2
				new Vector2(.25, 0), new Vector2(.5, 0), new Vector2(.5, .25), new Vector2(.25, .25),
				// Top: 5
				new Vector2(0, .25), new Vector2(.25, .25), new Vector2(.25, .5), new Vector2(0, .5),
				// Left: 4
				new Vector2(.75, 0), new Vector2(1, 0), new Vector2(1, .25), new Vector2(.75, .25),
				// Right: 3
				new Vector2(.5, 0), new Vector2(.75, 0), new Vector2(.75, .25), new Vector2(.5, .25),
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
