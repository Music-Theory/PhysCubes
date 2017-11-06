namespace ReturnToGL.Physics {
	using Walker.Data.Geometry.Generic.Space;

	public interface ISpringAttachment {



		Vector3<float> CenterOfMass { get; }
		Vector3<float> AttachPoint { get; }

	}
}
