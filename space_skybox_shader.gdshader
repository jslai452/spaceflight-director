shader_type sky;

uniform sampler2D front : source_color, filter_linear, repeat_disable, hint_default_black;
uniform sampler2D back : source_color, filter_linear, repeat_disable, hint_default_black;
uniform sampler2D left : source_color, filter_linear, repeat_disable, hint_default_black;
uniform sampler2D right : source_color, filter_linear, repeat_disable, hint_default_black;
uniform sampler2D top : source_color, filter_linear, repeat_disable, hint_default_black;
uniform sampler2D bottom : source_color, filter_linear, repeat_disable, hint_default_black;
void sky() {
	vec3 dir = EYEDIR;
	vec3 abs_dir = abs(dir);
	float max_axis = max(max(abs_dir.x, abs_dir.y), abs_dir.z);
	vec3 normalized_dir = dir / (max_axis + 0.0001);
	vec2 uv = vec2(0.0);
	vec3 color = vec3(0.0);
	
	if (abs_dir.x >= max_axis) {
		if (dir.x > 0.0) {
			// Right face
			uv = vec2(-normalized_dir.z, -normalized_dir.y) * 0.5 + 0.5;
			color = texture(right, uv).rgb;
		} else {
			// Left face
			uv = vec2(normalized_dir.z, -normalized_dir.y) * 0.5 + 0.5;
			color = texture(left, uv).rgb;
		}
	} else if (abs_dir.y >= max_axis) {
		if (dir.y > 0.0) {
			// Top face
			uv = vec2(normalized_dir.x, normalized_dir.z) * 0.5 + 0.5;
			color = texture(top, uv).rgb;
		} else {
			// Bottom face
			uv = vec2(normalized_dir.x, -normalized_dir.z) * 0.5 + 0.5;
			color = texture(bottom, uv).rgb;
		}
	} else {
		if (dir.z > 0.0) {
			// Back face
			uv = vec2(normalized_dir.x, -normalized_dir.y) * 0.5 + 0.5;
			color = texture(back, uv).rgb;
		} else {
			// Front face
			uv = vec2(-normalized_dir.x, -normalized_dir.y) * 0.5 + 0.5;
			color = texture(front, uv).rgb;
		}
	}
	
	COLOR = color;
}
