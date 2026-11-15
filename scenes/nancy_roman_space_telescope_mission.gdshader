shader_type spatial;

// 1. The Main Planet Map
uniform sampler2D earth_albedo_png : source_color, filter_linear_mipmap;

// 2. Color Pickers (Pick these directly from your image in the Inspector!)
uniform vec3 target_ocean_color : source_color = vec3(0.0, 0.2, 0.6);
uniform vec3 target_grass_color : source_color = vec3(0.1, 0.5, 0.1);
uniform vec3 target_snow_color  : source_color = vec3(0.9, 0.9, 0.9);
uniform vec3 target_sand_color  : source_color = vec3(0.7, 0.6, 0.4);

// 3. Tolerance Sliders (Controls how strict the color matching is)
uniform float match_sensitivity : hint_range(0.1, 5.0) = 2.0;

// 4. Detail Micro-Textures
uniform sampler2D grass_texture  : source_color, filter_linear_mipmap_anisotropic;
uniform sampler2D ocean_texture  : source_color, filter_linear_mipmap_anisotropic;
uniform sampler2D snow_texture   : source_color, filter_linear_mipmap_anisotropic;
uniform sampler2D sand_texture   : source_color, filter_linear_mipmap_anisotropic;
// Expanded range so you can type or slide much larger numbers
uniform float texture_scale : hint_range(1.0, 10000.0) = 1000.0;
uniform float detail_blend : hint_range(0.0, 1.0) = 0.6;

void fragment() {
    vec3 base = texture(earth_albedo_png, UV).rgb;
    
    // Sample micro-textures
    vec2 scaled_uv = UV * texture_scale;
    vec3 grass = texture(grass_texture, scaled_uv).rgb;
    vec3 ocean = texture(ocean_texture, scaled_uv).rgb;
    vec3 snow  = texture(snow_texture, scaled_uv).rgb;
    vec3 sand  = texture(sand_texture, scaled_uv).rgb;
    
    // --- COLOR DISTANCE MATCHING ENGINE ---
    // Calculate how close the current pixel color is to our targets (0.0 = exact match)
    float dist_ocean = distance(base, target_ocean_color);
    float dist_grass = distance(base, target_grass_color);
    float dist_snow  = distance(base, target_snow_color);
    float dist_sand  = distance(base, target_sand_color);
    
    // Convert distances to weights (invert so smaller distance = higher weight)
    float w_ocean = exp(-dist_ocean * match_sensitivity);
    float w_grass = exp(-dist_grass * match_sensitivity);
    float w_snow  = exp(-dist_snow  * match_sensitivity);
    float w_sand  = exp(-dist_sand  * match_sensitivity);
    
    // Normalize weights so they always add up to 1.0 (guarantees something always renders!)
    float total_weight = w_ocean + w_grass + w_snow + w_sand;
    w_ocean /= total_weight;
    w_grass /= total_weight;
    w_snow  /= total_weight;
    w_sand  /= total_weight;
    
    // --- BLEND BASED ON WEIGHTS ---
    vec3 micro_blend = (ocean * w_ocean) + (grass * w_grass) + (snow * w_snow) + (sand * w_sand);
    
    // Final output mix
    ALBEDO = mix(base, base * micro_blend * 1.5, detail_blend);
    
    // Dynamically adjust shininess based on how "ocean-like" the pixel is
    ROUGHNESS = mix(0.7, 0.1, w_ocean); 
}
