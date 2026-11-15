shader_type spatial;
render_mode blend_mix, depth_draw_never, cull_disabled, unshaded;

uniform vec4 atmosphere_color : source_color = vec4(0.3, 0.6, 1.0, 1.0);
uniform float fresnel_power = 3.0;
uniform float intensity = 1.5;

void fragment() {
    // Calculate standard rim lighting (fresnel)
    float fresnel = dot(VIEW, NORMAL);
    fresnel = clamp(1.0 - fresnel, 0.0, 1.0);
    fresnel = pow(fresnel, fresnel_power) * intensity;
    
    // When inside the atmosphere, make it gently visible all around instead of totally invisible
    float inner_glow = 0.15;
    
    ALBEDO = atmosphere_color.rgb;
    ALPHA = clamp(fresnel + inner_glow, 0.0, 1.0);
}