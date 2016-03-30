#version 130

uniform vec2 translation;
uniform vec2 scale;

out vec2 uv;

attribute vec3 in_position;
attribute vec2 in_uv;

void main(void) {
	gl_Position = vec4(in_position * vec3(scale, 0) + vec3(translation, 0), 1);
	uv = in_uv;
};