// the old photo grows past the viewer while the new one settles in from slightly smaller
vec4 transition(vec2 uv) {
  vec2 center = vec2(0.5);
  vec4 oldColor = getFromColor(center + (uv - center) / (1.0 + progress * 0.5));
  vec4 newColor = getToColor(center + (uv - center) / (0.8 + progress * 0.2));
  return mix(oldColor, newColor, progress);
}
