#version 130

uniform mat4 transform_mat;

attribute vec3 in_position;

void main(void) {
	gl_Position = transform_mat * vec4(in_position, 1);
};