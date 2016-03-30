using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using OpenGL;

namespace ReturnToGL.Physics {
	public interface ISpringAttachment {



		Vector3 CenterOfMass { get; }
		Vector3 AttachPoint { get; }

	}
}
