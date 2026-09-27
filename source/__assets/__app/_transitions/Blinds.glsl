// horizontal slats, each opening from its top edge
uniform float count; // = 10.0

vec4 transition(vec2 uv) {
  float slat = fract((1.0 - uv.y) * count);
  return slat < progress ? getToColor(uv) : getFromColor(uv);
}
