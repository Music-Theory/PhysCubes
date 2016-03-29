#version 130

out vec2 uv;

uniform mat4 transform_mat;

attribute vec3 in_position;
attribute vec2 in_uv;

void main(void) {
	uv = in_uv;
	gl_Position = transform_mat * vec4(in_position, 1);
};