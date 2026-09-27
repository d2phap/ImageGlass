// a soft edge sweeps across, revealing the new photo from the side it comes from
uniform float smoothness; // = 0.08

vec4 transition(vec2 uv) {
  float x = navigationDirection < 0.0 ? 1.0 - uv.x : uv.x;
  float t = progress * (1.0 + smoothness);
  float amount = smoothstep(1.0 - t, 1.0 - t + smoothness, x);
  return mix(getFromColor(uv), getToColor(uv), amount);
}
