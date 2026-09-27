// navigationDirection 1 pushes the old photo out to the left, -1 to the right
vec4 transition(vec2 uv) {
  float offset = progress * navigationDirection;
  vec4 oldColor = getFromColor(uv + vec2(offset, 0.0));
  vec4 newColor = getToColor(uv + vec2(offset - navigationDirection, 0.0));
  return newColor + oldColor * (1.0 - newColor.a);
}
