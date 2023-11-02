
export class DOMPadding {
  left = 0;
  right = 0;
  top = 0;
  bottom = 0;

  constructor(left = 0, top = 0, right = 0, bottom = 0) {
    this.left = left;
    this.top = top;
    this.right = right;
    this.bottom = bottom;

    this.multiply = this.multiply.bind(this);
  }

  get horizontal() {
    return this.left + this.right;
  }

  get vertical() {
    return this.top + this.bottom;
  }

  multiply(value: number) {
    if (value < 0) return this;

    return new DOMPadding(
      this.left * value,
      this.top * value,
      this.right * value,
      this.bottom * value,
    );
  }
}
