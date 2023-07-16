shader_type canvas_item;

uniform vec2 sdf_box = vec2(0.);

uniform float msdf_pixel_range: hint_range(1, 250, 0.25) = 8;
uniform float msdf_outline_size: hint_range(0.1, 10, 0.1) = 0.1;

uniform float progress: hint_range(0., 1.) = 0.;

uniform vec3 foreground_color: source_color;
uniform vec3 foreground_grad_color: source_color;

uniform float flames_power: hint_range(0., 1.) = 0.;

uniform sampler2D noise_tex: repeat_enable;

float msdf_median(float r, float g, float b, float a) {
	return min(max(min(r, g), min(max(r, g), b)), a);
}

float msdf_sample(sampler2D tex2D, vec2 base_uv, float pixel_range, float outline) {
	vec4 tex = texture(tex2D, base_uv);
	
	tex.rgb = mix(vec3(1.0 + 0.055) * pow(tex.rgb, vec3(1.0 / 2.4)) - vec3(0.055), vec3(12.92) * tex.rgb, lessThan(tex.rgb, vec3(0.0031308)));
	vec2 msdf_size = vec2(textureSize(tex2D, 0));
	vec2 dest_size = vec2(1.0) / fwidth(base_uv);
	
	float px_size = max(0.5 * dot(msdf_size, dest_size), 1.0);
	float d = msdf_median(tex.r, tex.g, tex.b, tex.a) - 0.5;
	if (outline > 0.0) {
		float cr = clamp(outline, 0.0, pixel_range / 2.0) / pixel_range;
		return clamp((d + cr) * px_size, 0.0, 1.0);
	} else {
		return clamp(d * px_size + 0.5, 0.0, 1.0);
	}
}

float aat(in float d, float thickness) {
	const float Smoothness = 10.;
	
	float a = smoothstep(thickness * 0.1 - Smoothness * 0.1, thickness * 0.1 + Smoothness * 0.1, d);
	
	float pwidth = length(vec2(dFdx(a), dFdy(a))); 
	a = clamp((0.5 - a) / pwidth, 0., 1.);

	return a;
}

varying vec2 quad_uv;

void vertex() {
	//VERTEX.x += -VERTEX.y * 0.25;
	
	switch (VERTEX_ID) {
		case 0:
			quad_uv = vec2(sdf_box.x, sdf_box.y);
			break;
		case 1:
			quad_uv = vec2(sdf_box.x, 1. - sdf_box.y);
			break;
		case 2:
			quad_uv = vec2(1. - sdf_box.x, 1. - sdf_box.y);
			break;
		case 3:
			quad_uv = vec2(1. - sdf_box.x, sdf_box.y);
			break;
	}
}

void fragment() {
	vec2 base_uv = UV;
	vec4 original_col = COLOR;
	
	// Place fragment code here.
	{
		COLOR = texture(TEXTURE, base_uv);
		
		/*COLOR.rgb = mix(vec3(1.0 + 0.055) * pow(COLOR.rgb, vec3(1.0 / 2.4)) - vec3(0.055), vec3(12.92) * COLOR.rgb, lessThan(COLOR.rgb, vec3(0.0031308)));
		vec2 msdf_size = vec2(textureSize(TEXTURE, 0));
		vec2 dest_size = vec2(1.0) / fwidth(base_uv);
		
		float px_size = max(0.5 * dot(msdf_size, dest_size), 1.0);
		float d = msdf_median(COLOR.r, COLOR.g, COLOR.b, COLOR.a) - 0.5;
		if (msdf_outline_size > 0.0) {
			float cr = clamp(msdf_outline_size, 0.0, msdf_pixel_range / 2.0) / msdf_pixel_range;
			COLOR.a = original_col.a * clamp((d + cr) * px_size, 0.0, 1.0);
		} else {
			COLOR.a = clamp(d * px_size + 0.5, 0.0, 1.0);
		}
			
		COLOR.rgb = vec3(1.0);*/
		vec3 bg_col = vec3(0.18);
				
		// Checkerboard background
		{
			//vec2 checkerboard_uv = FRAGCOORD.xy * 3.5;
			vec2 checkerboard_uv = quad_uv * 150.;
			checkerboard_uv.x += checkerboard_uv.y * 0.1;
			checkerboard_uv += TIME * 8.0;

			
			vec2 p = checkerboard_uv / 10.;
			
			vec2 ddx = dFdx(checkerboard_uv * 0.1);
			vec2 ddy = dFdy(checkerboard_uv * 0.1);
			vec2 w = max(abs(ddx), abs(ddy)) + 0.001;
			vec2 s = 2.0 * (abs(fract((p - 0.5 * w) / 2.0) - 0.5) - abs(fract((p + 0.5 * w) / 2.0) - 0.5)) / w;
			float t = 0.5 - 0.5 * s.x * s.y;
			float pattern_p = t;
			bg_col = mix(bg_col, vec3(0.25), pattern_p);
		}
		
		vec4 fg_col = vec4(foreground_color, 0.8);
		if (progress > 0.999) {
			fg_col.a = 1.;
			fg_col = mix(fg_col, vec4(foreground_grad_color, 1.), quad_uv.y);
		}
		
		vec4 base_fg_col = fg_col;
		float fg_edge_power = 0.25;
		
		// Edge effect
		{
			float edge = aat(COLOR.a, 8.5);
			edge = 1. - smoothstep(0.55, 0.625, COLOR.a);
			
			fg_col.rgb = mix(fg_col.rgb, mix(fg_col.rgb, vec3(1.0), fg_edge_power), vec3(edge));
			//fg_col.rgb += vec3(1.0) * aat(COLOR.a, 7.5 * fg_edge_power);
			fg_col.a = max(fg_col.a, edge * 0.9);
		}
		
		// Gauge
		{
			fg_col.a = fg_col.a * step(1. - progress, quad_uv.y);
		}
		
		// Gradient effect
		{			
			float p = clamp((quad_uv.y - (1. - progress)) * 40., 0., 1.);
			p = 1. - p;
			if (p > 0.) {
				p *= 2.;
				p += (p * p) * 0.25;
				p = clamp(p * p, 0., 1.);
			}
			
			float dist = distance(quad_uv.y, 1. - progress);
			//p = clamp(smoothstep(0.95, 1.0, 1. - dist), 0., 1.);
			
			fg_col.rgb = mix(fg_col.rgb, vec3(1.), p * 0.6);
			if (fg_col.a > 0.1)
				fg_col.a = clamp(fg_col.a + p * 0.6, 0., 1.);
			
			float bar = smoothstep(0.95, 1.05, 1. - dist);
			bar += (bar * bar) * 2.0;
			//bar *= bar;
			if (fg_col.a < 0.1)
				fg_col.a = clamp(fg_col.a + bar, 0., 1.);
		}
		
		vec3 final_col;
		final_col = mix(bg_col.rgb, fg_col.rgb, fg_col.a);
		
		COLOR.rgb = clamp(final_col, vec3(0.), vec3(1.));
		COLOR.a = original_col.a * msdf_sample(TEXTURE, base_uv, msdf_pixel_range, msdf_outline_size);
		
		float __time = floor(TIME * 16.) * 0.05;
		#define TIME __time
		
		if (flames_power > 0.001) {
			float fc = smoothstep(0.1, 0.5, texture(TEXTURE, base_uv).a);
			fc *= clamp(((quad_uv.y * quad_uv.y) + 0.1) * 4.0, 0., 1.);
			
			float noise = texture(noise_tex, (base_uv * 0.5) + (TIME * 0.05)).r;
			float noise2 = texture(noise_tex, (base_uv * 2.0) + vec2(TIME * 0.5, 0.)).r;
			float noise3 = texture(noise_tex, (base_uv * 0.75) + vec2(0., TIME * 0.2)).r;
			
			float prev_fc = fc;
			fc *= max(fc * fc * noise, max(fc, noise) * noise2);
			fc *= (1. + (prev_fc * prev_fc * prev_fc * prev_fc));
			fc = max(noise3 * 10. * fc, fc);
			
			float curr_fc = fc;
			fc = max(fc * 0.5, (1. - aat(fc, 2.)) * 0.4);
			fc = clamp(fc, 0., 1.);
			
			/*if (prev_fc < 0.9)
				prev_fc = 0.;
			else
				prev_fc = 1.;*/
				
			/*if (curr_fc < 0.9)
				curr_fc = 0.;
			else
				curr_fc = 1.;*/
			
			if (fc < 0.5)
				fc = 0.5 * fc;
			else
				fc = 1.;
			
			COLOR.rgb = mix(COLOR.rgb, vec3(foreground_grad_color), (1. - COLOR.a) * flames_power);
			COLOR.rgb = mix(COLOR.rgb, vec3(foreground_color), ((1. - COLOR.a) * curr_fc * ((1. - fc))) * flames_power);
			COLOR.a = max(COLOR.a, fc * 0.4 * (flames_power * flames_power));
			COLOR.a = max(
				COLOR.a,
				max(curr_fc * 0.5, quad_uv.y * quad_uv.y * quad_uv.y) * 0.6 * flames_power * (prev_fc * (1. - quad_uv.y))
			);
			
			float clamped_fc = clamp(fc, 0., 1.);
			COLOR.rgb = mix(
				COLOR.rgb,
				vec3(foreground_grad_color * vec3(0.5, 2.0, 10.)), 
				max(curr_fc * 0.5, quad_uv.y * quad_uv.y * quad_uv.y) * 0.6 * flames_power
			);
			COLOR.rgb = mix(
				COLOR.rgb,
				vec3(foreground_color), 
				clamped_fc * flames_power * clamp((1. - quad_uv.y) * 2., 0., 1.)
			);
		}
	}
}
