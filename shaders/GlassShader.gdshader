shader_type canvas_item;

// Godot 4.x 新语法：显式声明屏幕纹理
uniform sampler2D screen_texture : hint_screen_texture, repeat_disable, filter_nearest;
uniform float blur_radius : hint_range(0.0, 8.0) = 4.0;
uniform float brightness : hint_range(0.0, 2.0) = 1.1;
uniform vec4 tint : source_color = vec4(0.9, 0.95, 1.0, 0.15);
uniform float opacity : hint_range(0.0, 1.0) = 0.0;

vec4 blur(sampler2D tex, vec2 uv, vec2 pixel_size, float radius) {
    vec4 color = vec4(0.0);
    float total = 0.0;
    for(float x = -radius; x <= radius; x++) {
        for(float y = -radius; y <= radius; y++) {
            vec2 offset = vec2(x, y) * pixel_size;
            color += texture(tex, uv + offset);
            total += 1.0;
        }
    }
    return color / total;
}

void fragment() {
    // 使用 uniform 名称而非 SCREEN_TEXTURE 内置变量
    vec4 c = blur(screen_texture, SCREEN_UV, SCREEN_PIXEL_SIZE, blur_radius);
    c.rgb *= brightness;
    c.rgb = mix(c.rgb, tint.rgb, tint.a);
    vec4 original = texture(screen_texture, SCREEN_UV);
    COLOR = mix(original, c, opacity);
}