# ReturnToGL
Practice code for OpenGL.

CONTROLS

WSAD: Forward/Backward/Left/Right

Numpad 8,2,4,6: Rotate Up/Down/Left/Right

Click: Fire Cube

Space: Return Cubes To Individual Origins

R: Return Camera To Its Origin

T: Switch Textures

Escape: Delete Spawned Cubes



DEPENDENCIES

OpenGL4CSharp https://github.com/giawa/opengl4csharp

There's a quaternion bug in OpenGL4CSharp that breaks normalization. To fix it:

	Change this line in the method "public static Quaternion operator /(Quaternion q, float scalar):"

	return new Quaternion(q.x * invScalar, q.y + invScalar, q.z * invScalar, q.w * invScalar);

	To this:

	return new Quaternion(q.x * invScalar, q.y * invScalar, q.z * invScalar, q.w * invScalar);

SFML.Net http://www.sfml-dev.org/download/sfml.net/
