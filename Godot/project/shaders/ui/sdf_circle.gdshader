shader_type canvas_item;

uniform float radius: hint_range(0., 1.) = 1.;
uniform float start_radius: hint_range(0., 1.) = 0.;

float sdCircle(in vec2 p, in float r) {
	return length(p) - r;
}

float aat(in float d, float thickness) {
	const float Smoothness = 10.;
	
	float a = smoothstep(thickness * 0.1 - Smoothness * 0.1, thickness * 0.1 + Smoothness * 0.1, d);
	
	float pwidth = length(vec2(dFdx(a), dFdy(a))); 
	a = clamp((0.5 - a) / pwidth, 0., 1.);

	return a;
}

void fragment() {
	// Place fragment code here.
	COLOR.a = COLOR.a 
		* aat(sdCircle(UV * 2.0 - vec2(1.0), radius), 0.001)
		* (1. - aat(sdCircle(UV * 2.0 - vec2(1.0), start_radius), 0.001));
}
