using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenGL;
using SFML.System;

namespace PhysCubes {

	public struct PhysDeriv {

		public Vector3 velocity;
		public Vector3 force;
		public Quaternion spin;
		public Vector3 torque;

	}

	public static class Physics {

		public const float MAX_VEL = 999999999999f;
		public static readonly Vector3 GRAVITY = new Vector3(0, -.1635, 0);
		public const float ELASTICITY = .5f;

		public static readonly float MIN_LIVING_VEL = .0000000000001f;

		public static void MakeBox(Vector3 pos, Vector3 sca, Vector3 vel) {
			boxes.Add(new PhysBox(pos, sca, vel));
		}

		public static PhysBox MakeBox(PhysState init) {
			PhysBox box = new PhysBox(init);
			boxes.Add(box);
			return box;
		}

		public static PhysDeriv Evaluate(PhysState init, double t) {

			PhysDeriv output = new PhysDeriv {
				velocity = init.LinearVel,
				spin = init.Spin
			};
			GetForces(init, t, ref output);
			return output;
		}

		public static PhysDeriv Evaluate(PhysState init, double t, float dt, PhysDeriv derivative) {

			init.position += derivative.velocity * dt;
			init.LinMomentum += derivative.force * dt;
			init.Rotation += derivative.spin * dt;
			init.AngMomentum += derivative.torque * dt;

			PhysDeriv output = new PhysDeriv {
				velocity = init.LinearVel,
				spin = init.Spin
			};
			GetForces(init, t+dt, ref output);
			return output;
		}

		public static void GetForces(PhysState state, double t, ref PhysDeriv der) {
			//float x = Math.Max(-MAX_VEL, Math.Min(MAX_VEL, state.linearVel.x)),
			//	y = Math.Max(-MAX_VEL, Math.Min(MAX_VEL, GRAVITY.y + state.linearVel.y)),
			//	z = Math.Max(-MAX_VEL, Math.Min(MAX_VEL, state.linearVel.z));
			der.force = state.position.Normalize() * GRAVITY.y;
			//der.torque.x = (float) ( 1.0f * Math.Sin(t * 0.9f + 0.5f) );
			//der.torque.y = (float) ( 1.1f * Math.Sin(t * 0.5f + 0.4f) );
			//der.torque.z = (float) ( 1.2f * Math.Sin(t * 0.7f + 0.9f) );
			//der.torque -= 0.2f * state.AngularVel;
			//der.torque = state.position.Normalize() * GRAVITY.y * .01f;
			//der.torque = Vector3.Forward;
			//der.torque -= .2f * state.AngularVel;
		}

		// f = ma
		// o = momentum
		// o' = f

		public static void Integrate(ref PhysState phys, double t, float dt) {
			PhysDeriv a = Evaluate(phys, t),
				b = Evaluate(phys, t, dt*.5f, a),
				c = Evaluate(phys, t, dt*.5f, b),
				d = Evaluate(phys, t, dt, c);
			

			// dPos
			phys.position += 1f / 6f * dt * ( a.velocity + 2 * ( b.velocity + c.velocity ) + d.velocity );
			// dLinMomentum
			phys.LinMomentum += 1f / 6f * dt * ( a.force + 2 * ( b.force + c.force ) + d.force );
			// dRot
			phys.Rotation += 1f / 6f * dt * (a.spin + 2 * (b.spin + c.spin) + d.spin);
			// dAngMomentum
			phys.AngMomentum += 1f / 6f * dt * (a.torque + 2 * (b.torque + c.torque) + d.torque);


		}

		public static bool CheckForDead(PhysBox box) {
			//return Math.Abs(box.currState.LinearVel.x) < MIN_LIVING_VEL
			//	&& Math.Abs(box.currState.LinearVel.y) < MIN_LIVING_VEL
			//	&& Math.Abs(box.currState.LinearVel.z) < MIN_LIVING_VEL;
			return false;
		}

		public static List<PhysBox> boxes = new List<PhysBox>();

		public static void ResetBoxes() {
			foreach (PhysBox box in boxes) { box.currState = box.initState; }
		}

		static double currTime = 0;
		static float deltaTime = 1f / 60f;

		static float accumulator = 0.0f;

		static Clock clock = new Clock();

		public static float currAlpha = 1;

		public static void UpdateLiving() {

			float dTime = clock.ElapsedTime.AsSeconds();
			currTime += dTime;

			dTime = (float) Math.Max(.25, dTime);

			accumulator += dTime;

			while (accumulator >= deltaTime) {

				accumulator -= deltaTime;

				foreach (PhysBox box in boxes.Where(box => box.currState.live)) {
					box.prevState = box.currState;
					Integrate(ref box.currState, currTime, deltaTime);
					box.Refresh();
					currTime += deltaTime;
				}

			}

			clock.Restart();

		}

		public static PhysState Interpolate(PhysState prev, PhysState curr, float alpha) {
			curr.position = curr.position * alpha + prev.position * ( 1 - alpha );
			curr.LinMomentum = curr.LinMomentum * alpha + prev.LinMomentum * ( 1 - alpha );

			//curr.Rotation = Quaternion.Slerp(prev.Rotation, curr.Rotation, alpha);
			//curr.AngMomentum = curr.AngMomentum * alpha + prev.AngMomentum * (1 - alpha);

			return curr;
		}

		public static List<PhysBox> GetCollisions(PhysBox box) {
			return boxes.Where(t => t != box && box.bBox.Intersects(t.bBox)).ToList();
		}

	}
}
