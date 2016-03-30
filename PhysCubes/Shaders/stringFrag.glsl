uniform sampler2D texture;
uniform vec4 color;

in vec2 uv;

out vec4 fragment;

void main(void) {
	vec4 tex = texture2D(texture, uv);
	fragment = tex * color;
};