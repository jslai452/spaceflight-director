shader_type spatial;
render_mode blend_mix, depth_draw_opaque, cull_back, diffuse_lambert, specular_schlick_ggx;

uniform sampler2D planet_map : source_color, filter_linear_mipmap;

// Define the "target" colors you used on your map
uniform vec3 target_water = vec3(0.0, 0.4, 0.8);
uniform vec3 target_grass = vec3(0.2, 0.6, 0.2);
uniform vec3 target_rock  = vec3(0.5, 0.4, 0.3);
uniform vec3 target_snow  = vec3(0.9, 0.9, 0.9);

// Control how strict the color matching is
uniform float color_tolerance : hint_range(1.0, 20.0) = 8.0;

// Output biome colors
uniform vec3 color_water_out : source_color = vec3(0.04, 0.12, 0.35);
uniform vec3 color_grass_out : source_color = vec3(0.18, 0.38, 0.12);
uniform vec3 color_rock_out  : source_color = vec3(0.35, 0.32, 0.28);
uniform vec3 color_snow_out  : source_color = vec3(0.90, 0.90, 0.95);

varying vec3 vertex_pos;

void vertex() {
	vertex_pos = VERTEX;
}

vec2 dir_to_uv(vec3 dir) {
	// Prevent divide-by-zero if dir is zero length
	float len = length(dir);
	if (len < 0.0001) {
		return vec2(0.5, 0.5);
	}
	vec3 n_dir = dir / len;
	
	float u = 0.5 + (atan(n_dir.z, n_dir.x) / (2.0 * 3.1415926535));
	float v = 0.5 - (asin(clamp(n_dir.y, -1.0, 1.0)) / 3.1415926535);
	return vec2(u, v);
}

void fragment() {
	// dir_to_uv handles safe normalization now
	vec2 uv = dir_to_uv(vertex_pos);
	vec3 sample_col = texture(planet_map, uv).rgb;

	// Calculate similarity weights based on RGB distance
	float dist_water = distance(sample_col, target_water);
	float dist_grass = distance(sample_col, target_grass);
	float dist_rock  = distance(sample_col, target_rock);
	float dist_snow  = distance(sample_col, target_snow);

	// Convert distance to weights using exponential falloff
	float w_water = exp(-dist_water * color_tolerance);
	float w_grass = exp(-dist_grass * color_tolerance);
	float w_rock  = exp(-dist_rock * color_tolerance);
	float w_snow  = exp(-dist_snow * color_tolerance);

	float total_weight = w_water + w_grass + w_rock + w_snow;

	if (total_weight > 0.0) {
		w_water /= total_weight;
		w_grass /= total_weight;
		w_rock  /= total_weight;
		w_snow  /= total_weight;
	} else {
		w_grass = 1.0;
		w_water = 0.0;
		w_rock  = 0.0;
		w_snow  = 0.0;
	}

	// Blend the output colors
	vec3 final_albedo = (color_water_out * w_water) + (color_grass_out * w_grass) + (color_rock_out * w_rock) + (color_snow_out * w_snow);
	float final_roughness = mix(0.85, 0.05, w_water);
	float final_metallic = w_water * 0.1;

	ALBEDO = final_albedo;
	ROUGHNESS = final_roughness;
	METALLIC = final_metallic;
}
