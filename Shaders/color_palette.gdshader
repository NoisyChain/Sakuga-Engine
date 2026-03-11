shader_type spatial;

render_mode blend_mix, cull_disabled, unshaded, specular_disabled;
uniform sampler2D albedo_texture : source_color, filter_nearest;

uniform int num_colors : hint_range(1, 100, 1) = 1;
uniform float precision : hint_range(0.0, 1.0, 0.01) = 0.1;

uniform sampler2D palette_in : source_color, filter_nearest;
uniform sampler2D palette_out : source_color, filter_nearest;

vec4 swap_color(vec4 color) {
    float inc = 1.0 / float(num_colors);
    for (float i = 0.0; i < 1.0; i += inc) {
        vec2 uv = vec2(i, 1.0);
        vec4 color_in = texture(palette_in, uv);
        if (distance(color, color_in) <= precision) {
            return texture(palette_out, uv);
        }
    }
    //return color;
}

void fragment() {
	vec4 albedo_tex = texture(albedo_texture,UV);
	//ALBEDO = albedo_tex.rgb;
	ALBEDO = swap_color(albedo_tex).rgb;
	ALPHA = albedo_tex.a;
}