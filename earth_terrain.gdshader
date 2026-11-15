shader_type spatial;
render_mode blend_mix, depth_draw_opaque, cull_back, diffuse_lambert, specular_schlick_ggx;

uniform sampler2D heightmap_texture : source_color, filter_linear_mipmap;
uniform float planet_radius = 5000.0;
uniform float max_height = 400.0;

// Pass local-space vertex position from vertex() to fragment()
varying vec3 vertex_pos;

void vertex() {
	// VERTEX here is in object/local space (centered at the planet core)
	vertex_pos = VERTEX;
}

// Converts a 3D direction vector on a sphere to 2D equirectangular UV coordinates
vec2 dir_to_uv(vec3 dir) {
	float u = 0.5 + (atan(dir.z, dir.x) / (2.0 * 3.1415926535));
	float v = 0.5 - (asin(clamp(dir.y, -1.0, 1.0)) / 3.1415926535);
	return vec2(u, v);
}

void fragment() {
	// Use the true object-space position passed from the vertex function
	vec3 normal_dir = normalize(vertex_pos);
	vec2 uv = dir_to_uv(normal_dir);
	
	// Sample the heightmap directly at pixel precision
	float height_val = texture(heightmap_texture, uv).r;
	
	// Define proper height thresholds (0.0 = lowest, 1.0 = highest)
	float sea_level = 0.2;    // Anything below 20% height is water
	float snow_level = 0.8;   // Anything above 80% height is mountain snow
	
	vec3 final_albedo = vec3(0.18, 0.38, 0.12); // Default Grass (Land)
	float final_roughness = 0.95;
	float final_metallic = 0.1;
	
	if (height_val < sea_level) {
		// Oceans
		final_albedo = vec3(0.04, 0.12, 0.35);
		final_roughness = 0.08;
	}
	else if (height_val > snow_level) {
		// Mountain peaks / Snow
		final_albedo = vec3(0.85, 0.85, 0.85);
		final_roughness = 0.6;
	}
	else {
		// Mid-elevation grass / rock
		final_albedo = vec3(0.38, 0.35, 0.32);
		final_roughness = 0.8;
	}
	
	ALBEDO = final_albedo;
	ROUGHNESS = final_roughness;
	METALLIC = final_metallic;
}