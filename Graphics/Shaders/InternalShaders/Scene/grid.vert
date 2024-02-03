#version 430 core

layout (location = 0) in vec3 aPosition;

layout (std140, binding = 0) uniform ProjView
{
	mat4 projection;
	mat4 view;
	vec3 viewPos;
};

uniform mat4 model;

void main()
{
	gl_Position = vec4(aPosition, 1.0) * model * view * projection;
}