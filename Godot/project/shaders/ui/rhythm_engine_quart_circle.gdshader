shader_type canvas_item;

uniform float gauge_progress: hint_range(0., 1.) = 0.5;
uniform float gauge_progress_factor: hint_range(0., 1.) = 1.0;
uniform float gauge_offset: hint_range(0., 1.) = 0.0;

uniform float edge_power: hint_range(0., 1.) = 1.0;
uniform float circle_thickness: hint_range(0., 1.) = 0.2;

float aat(in float d, float thickness) {
	const float Smoothness = 10.;
	
	float a = smoothstep(thickness * 0.1 - Smoothness * 0.1, thickness * 0.1 + Smoothness * 0.1, d);
	
	float pwidth = length(vec2(dFdx(a), dFdy(a))); 
	a = clamp((0.5 - a) / pwidth, 0., 1.);

	return a;
}

void fragment() {	
	//vec2 R = 1. / SCREEN_PIXEL_SIZE;
	vec2 R = vec2(1.);
	vec2 uv = UV.xy;
    uv = (uv+uv - R) / R.y;
	
	float pi = 3.141592,
           d = length(uv), 
           a = atan(uv.y + gauge_offset, uv.x) / pi;
		
	float uv_p = (a + 1.) * 0.5;
    
	float progress = gauge_progress * gauge_progress_factor;
	progress -= gauge_offset * 0.1;
	
	float edge_p = (uv_p - (progress - 0.1)) * 8.;
	if (edge_p > 0.) {
		edge_p += (edge_p * edge_p) * 0.1;
		edge_p *= edge_p * edge_p;
	}
	edge_p *= edge_power;
	
	vec3 col = clamp(COLOR.rgb + vec3(clamp(edge_p * 0.8, 0., 1.)), vec3(0.), vec3(1.));
		
	float opacity = mix(0., 1., 
               aat(abs(d-.65)-circle_thickness, 0.001)
			*  aat(progress > 0.9995 ? uv_p * 0.01 : uv_p * 0.1, progress)
           );
		
	COLOR = vec4(col.rgb, opacity * COLOR.a);
}
