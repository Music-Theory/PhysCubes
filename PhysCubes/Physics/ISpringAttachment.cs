namespace ReturnToGL.Physics {
	using System.Numerics;

	public interface ISpringAttachment {



		Vector3 CenterOfMass { get; }
		Vector3 AttachPoint { get; }

	}
}
