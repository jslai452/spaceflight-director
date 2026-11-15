shader_type sky;

uniform samplerCube sky_cubemap : source_color, filter_linear, hint_default_black;

void sky() {
    // Samples the cubemap using the camera's view direction (EYEDIR)
    COLOR = texture(sky_cubemap, EYEDIR).rgb;
}