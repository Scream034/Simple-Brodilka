shader_type spatial;
render_mode unshaded, cull_front;

uniform float sun_size : hint_range(0.0, 1.0) = 0.2;
uniform float sun_sharpness : hint_range(0.0, 10.0) = 5.0;
uniform vec4 sun_color = vec4(1.0, 1.0, 0.0, 1.0);
uniform vec3 CameraPosition;
uniform vec3 WorldPosition;

void fragment()
{
    // Вычисляем направление камеры
    vec3 view_dir = normalize(CameraPosition - WorldPosition);

    // Вычисляем направление на солнце
    vec3 sun_dir = normalize(vec3(1.0, 1.0, 1.0));

    // Вычисляем угол между направлением камеры и направлением на солнце
    float angle = dot(view_dir, sun_dir);

    // Если угол меньше 0, значит солнце находится за камерой и лучи не рисуем
    if (angle < 0.0)
    {
        discard;
    }

    // Вычисляем размер солнца на экране
    float sun_size_screen = sun_size / abs(angle);

    // Вычисляем остроту границы солнца
    float sharpness = pow(sun_sharpness, 2.0);

    // Вычисляем расстояние от текущей точки до границы солнца
    float distance = length(FRAGCOORD.xy - vec2(0.5, 0.5)) / sun_size_screen;

    // Вычисляем интенсивность лучей солнца
    float intensity = pow(1.0 - distance, sharpness);

    // Рисуем лучи солнца
    ALBEDO = (sun_color * intensity).rgb;
}