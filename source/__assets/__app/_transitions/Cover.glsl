// the new photo slides in over the old one, which fades so it does not linger around a smaller photo
vec4 transition(vec2 uv) {
  vec4 newColor = getToColor(uv + vec2((1.0 - progress) * navigationDirection, 0.0));
  return newColor + getFromColor(uv) * (1.0 - newColor.a) * (1.0 - progress);
}
