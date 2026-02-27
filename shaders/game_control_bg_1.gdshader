shader_type canvas_item;

uniform float vignette_intensity : hint_range(0.0, 1.0) = 0.6;
uniform float blur_samples : hint_range(1.0, 32.0) = 8.0;
uniform float blur_radius : hint_range(0.0, 0.1) = 0.02;

vec4 radial_blur(sampler2D tex, vec2 uv, float intensity) {
    vec4 color = vec4(0.0);
    float total = 0.0;
    vec2 center = vec2(0.5);
    vec2 dir = normalize(uv - center);
    float dist = distance(uv, center);
    
    // 边缘处增加模糊
    float blur_amount = dist * intensity;
    
    for(float i = 0.0; i < blur_samples; i++) {
        float t = i / blur_samples;
        vec2 offset = dir * blur_amount * t;
        color += texture(tex, uv + offset);
        total += 1.0;
    }
    
    return color / total;
}

void fragment() {
    vec4 blurred = radial_blur(TEXTURE, UV, blur_radius);
    
    // 晕影计算
    float dist = distance(UV, vec2(0.5));
    float vignette = smoothstep(0.4, 0.8, dist) * vignette_intensity;
    
    COLOR = mix(blurred, vec4(0.0), vignette);
}