// the radius grows from the center until it reaches the corners
uniform float smoothness; // = 0.02

vec4 transition(vec2 uv) {
  vec2 halfSize = vec2(ratio, 1.0) * 0.5;
  float dist = length((uv - 0.5) * vec2(ratio, 1.0)) / length(halfSize);
  float t = progress * (1.0 + smoothness);
  float amount = 1.0 - smoothstep(t - smoothness, t, dist);
  return mix(getFromColor(uv), getToColor(uv), amount);
}
