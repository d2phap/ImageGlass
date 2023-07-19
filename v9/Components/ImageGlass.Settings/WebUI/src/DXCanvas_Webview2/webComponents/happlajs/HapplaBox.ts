import merge from 'lodash.merge';
import { IHapplaBoxOptions, InterpolationMode, PanDirection, ZoomMode } from './HapplaBoxTypes';
import { pause } from '@/helpers';
import { DOMPadding } from './DOMPadding';

export class HapplaBox {
  private boxEl: HTMLElement;
  private boxContentEl: HTMLElement;
  private domMatrix: DOMMatrix;
  private isPointerDown = false;

  #contentDOMObserver: MutationObserver;
  #resizeObserver: ResizeObserver;

  #isContentElDOMChanged = false;
  #pointerLocation: { x?: number, y?: number } = {};

  private panningAnimationFrame: number;
  private zoomingAnimationFrame: number;
  private isPanning = false;
  private isZooming = false;
  // private arrowLeftDown = false;
  // private arrowRightDown = false;
  // private arrowUpDown = false;
  // private arrowDownDown = false;

  #options: IHapplaBoxOptions = {};
  #defaultOptions: IHapplaBoxOptions = {
    imageRendering: InterpolationMode.Auto,

    allowZoom: true,
    minZoom: 0.01,
    maxZoom: 100,
    zoomFactor: 1,
    panOffset: new DOMPoint(0, 0),

    allowPan: true,
    scaleRatio: window.devicePixelRatio,
    padding: new DOMPadding(),

    onBeforeContentReady() {},
    onContentReady() {},
    onResizing() {},
    onMouseWheel() {},

    onBeforeZoomChanged() {},
    onAfterZoomChanged() {},
    onAfterTransformed() {},

    onPanning() {},
    onAfterPanned() {},
  };


  /**
   * Initializes HapplaBox element.
   * @param boxEl Box element
   * @param boxContentEl Content element
   * @param options Options
   */
  constructor(boxEl: HTMLElement, boxContentEl: HTMLElement, options?: IHapplaBoxOptions) {
    this.boxEl = boxEl;
    this.boxContentEl = boxContentEl;
    this.#options = merge({}, this.#defaultOptions, options);

    // correct zoomFactor after calculating scaleRatio
    this.#options.zoomFactor /= this.#options.scaleRatio;

    this.domMatrix = new DOMMatrix()
      .scaleSelf(this.dpi(this.zoomFactor))
      .translateSelf(this.#options.panOffset.x, this.#options.panOffset.y);

    this.zoomByDelta = this.zoomByDelta.bind(this);
    this.panToDistance = this.panToDistance.bind(this);
    this.startPanningAnimation = this.startPanningAnimation.bind(this);
    this.stopPanningAnimation = this.stopPanningAnimation.bind(this);
    this.dpi = this.dpi.bind(this);
    this.updateImageRendering = this.updateImageRendering.bind(this);

    this.enable = this.enable.bind(this);
    this.disable = this.disable.bind(this);
    this.zoomToPoint = this.zoomToPoint.bind(this);
    this.zoomToCenter = this.zoomToCenter.bind(this);
    this.panTo = this.panTo.bind(this);
    this.applyTransform = this.applyTransform.bind(this);

    this.onContentElDOMChanged = this.onContentElDOMChanged.bind(this);
    this.onResizing = this.onResizing.bind(this);
    this.onMouseWheel = this.onMouseWheel.bind(this);
    this.onPointerEnter = this.onPointerEnter.bind(this);
    this.onPointerLeave = this.onPointerLeave.bind(this);
    this.onPointerDown = this.onPointerDown.bind(this);
    this.onPointerUp = this.onPointerUp.bind(this);
    this.onPointerMove = this.onPointerMove.bind(this);
    // this.onKeyDown = this.onKeyDown.bind(this);
    // this.onKeyUp = this.onKeyUp.bind(this);

    // create content DOM observer
    this.#contentDOMObserver = new MutationObserver(this.onContentElDOMChanged);
    this.#resizeObserver = new ResizeObserver(this.onResizing);

    this.disable();

    this.boxContentEl.style.transformOrigin = 'top left';
    this.boxEl.style.touchAction = 'none';
    this.boxEl.style.overflow = 'hidden';

    // emit event onBeforeContentReady
    this.#options.onBeforeContentReady();
  }

  // #region Getters & Setters
  get options() {
    return this.#options;
  }

  get pointerLocation(): { x?: number, y?: number } {
    return this.#pointerLocation || {};
  }

  get imageRendering() {
    return this.#options.imageRendering;
  }

  set imageRendering(value: InterpolationMode) {
    this.#options.imageRendering = value;

    this.updateImageRendering();
  }

  get scaleRatio() {
    return this.#options.scaleRatio;
  }

  set scaleRatio(value: number) {
    this.#options.scaleRatio = value;
  }

  get padding() {
    return this.#options.padding;
  }

  set padding(value: DOMPadding) {
    this.#options.padding = value;
  }

  /**
   * Gets zoom factor after computing device ratio (DPI)
   */
  get zoomFactor() {
    return this.#options.zoomFactor * this.#options.scaleRatio;
  }
  // #endregion


  // #region Private functions
  private onContentElDOMChanged(mutations: MutationRecord[]) {
    let isContentElDOMChanged = false;

    mutations.forEach(m => {
      if (m.type === 'childList') {
        isContentElDOMChanged = true;
      }
    });

    this.#isContentElDOMChanged = isContentElDOMChanged;
  }

  private onResizing() {
    this.#options.onResizing();
  }

  private onMouseWheel(e: WheelEvent) {
    // ignore horizontal scroll events
    if (e.deltaY === 0) return;

    // const direction = e.deltaY < 0 ? 'up' : 'down';
    // const normalizedDeltaY = 1 + Math.abs(e.deltaY) / 1000; // speed
    // const delta = direction === 'up' ? normalizedDeltaY : 1 / normalizedDeltaY;
    // this.zoomByDelta(delta, e.pageX, e.pageY);

    this.#options.onMouseWheel(e);
  }

  private onPointerEnter(e: PointerEvent) {
    this.#pointerLocation = { x: e.pageX, y: e.pageY };
  }

  private onPointerLeave(e: PointerEvent) {
    this.#pointerLocation = {};
    this.onPointerUp(e);
  }

  private onPointerDown(e: PointerEvent) {
    // ignore right clicks
    if (e.button !== 0) {
      return;
    }

    this.boxEl.setPointerCapture(e.pointerId);
    this.isPointerDown = true;

    // We get the pointer position on click so we can get the value once the user starts to drag
    this.#options.panOffset.x = e.clientX;
    this.#options.panOffset.y = e.clientY;
  }

  private async onPointerMove(e: PointerEvent) {
    this.#pointerLocation = { x: e.pageX, y: e.pageY };

    // Only run this function if the pointer is down
    if (!this.isPointerDown) return;

    await this.panToDistance(
      e.clientX - this.#options.panOffset.x,
      e.clientY - this.#options.panOffset.y,
    );

    this.#options.panOffset.x = e.clientX;
    this.#options.panOffset.y = e.clientY;
  }

  private onPointerUp(e: PointerEvent) {
    if (!this.isPointerDown) {
      return;
    }

    this.boxEl.releasePointerCapture(e.pointerId);
    this.isPointerDown = false;

    this.#options.panOffset.x += e.clientX - this.#options.panOffset.x;
    this.#options.panOffset.y += e.clientY - this.#options.panOffset.y;

    this.#options.onAfterPanned(this.domMatrix.e, this.domMatrix.f);
  }

  // private onKeyDown(event: KeyboardEvent) {
  //   switch (event.key) {
  //     case 'ArrowLeft':
  //       this.arrowLeftDown = true;
  //       if (!this.isMoving) {
  //         this.isMoving = true;
  //         this.startMoving();
  //       }
  //       break;
  //     case 'ArrowUp':
  //       this.arrowUpDown = true;
  //       if (!this.isMoving) {
  //         this.isMoving = true;
  //         this.startMoving();
  //       }
  //       break;
  //     case 'ArrowRight':
  //       this.arrowRightDown = true;
  //       if (!this.isMoving) {
  //         this.isMoving = true;
  //         this.startMoving();
  //       }
  //       break;
  //     case 'ArrowDown':
  //       this.arrowDownDown = true;
  //       if (!this.isMoving) {
  //         this.isMoving = true;
  //         this.startMoving();
  //       }
  //       break;
  //     default:
  //       break;
  //   }
  // }

  // private onKeyUp(event: KeyboardEvent) {
  //   switch (event.key) {
  //     case 'ArrowLeft':
  //       this.arrowLeftDown = false;
  //       break;
  //     case 'ArrowUp':
  //       this.arrowUpDown = false;
  //       break;
  //     case 'ArrowRight':
  //       this.arrowRightDown = false;
  //       break;
  //     case 'ArrowDown':
  //       this.arrowDownDown = false;
  //       break;
  //     default:
  //       break;
  //   }

  //   if ([
  //     this.arrowLeftDown,
  //     this.arrowUpDown,
  //     this.arrowRightDown,
  //     this.arrowDownDown,
  //   ].every((keyDown) => !keyDown)) {
  //     this.stopMoving();
  //   }
  // }

  private dpi(value: number) {
    return value / this.#options.scaleRatio;
  }

  private updateImageRendering() {
    switch (this.imageRendering) {
      case InterpolationMode.Auto:
        if (this.zoomFactor <= 1.0) {
          this.boxContentEl.style.imageRendering = InterpolationMode.CrispEdges;
        }
        else {
          this.boxContentEl.style.imageRendering = InterpolationMode.Pixelated;
        }
        break;

      default:
        this.boxContentEl.style.imageRendering = this.imageRendering;
        break;
    }
  }

  private getCenterPoint(zoomFactor: number) {
    const fullW = this.boxContentEl.scrollWidth * zoomFactor;
    const fullH = this.boxContentEl.scrollHeight * zoomFactor;
    const scaledPadding = this.padding.multiply(1 / this.scaleRatio);

    // center point
    const x = (this.boxEl.offsetWidth - fullW) / 2 + scaledPadding.left / 2 - scaledPadding.right / 2;
    const y = (this.boxEl.offsetHeight - fullH) / 2 + scaledPadding.top / 2 - scaledPadding.bottom / 2;

    return new DOMPoint(x, y);
  }

  private async animatePanning(direction: PanDirection, panSpeed = 20) {
    const speed = this.dpi(panSpeed || 20);
    let x = 0;
    let y = 0;

    if (direction === 'left') {
      x = speed;
    }
    else if (direction === 'right') {
      x = -speed;
    }

    if (direction === 'up') {
      y = speed;
    }
    else if (direction === 'down') {
      y = -speed;
    }

    await this.panToDistance(x, y);

    this.panningAnimationFrame = requestAnimationFrame(() => this.animatePanning(direction, panSpeed));
  }

  private async animateZooming(isZoomOut: boolean, zoomSpeed = 20) {
    const zoomDelta = isZoomOut ? -20 : 20;
    const speed = zoomDelta / (501 - zoomSpeed);
    let newZoomFactor = this.#options.zoomFactor;

    // zoom out
    if (isZoomOut) {
      newZoomFactor = this.#options.zoomFactor / (1 - speed);
    }
    // zoom in
    else {
      newZoomFactor = this.#options.zoomFactor * (1 + speed);
    }

    const newDelta = newZoomFactor / this.#options.zoomFactor;
    const x = this.pointerLocation.x ?? -1;
    const y = this.pointerLocation.y ?? -1;

    await this.zoomByDelta(newDelta, x, y, true);

    this.zoomingAnimationFrame = requestAnimationFrame(() => this.animateZooming(isZoomOut, zoomSpeed));
  }
  // #endregion


  // #region Public functions
  public async loadHtmlContent(html: string) {
    this.#isContentElDOMChanged = false;
    this.boxContentEl.innerHTML = html;

    while (!this.#isContentElDOMChanged) {
      await pause(10);
    }

    const list = this.boxContentEl.querySelectorAll('img');
    const imgs = Array.from(list);

    while (imgs.some((i) => !i.complete)) {
      await pause(10);
    }

    // emit event onContentReady
    this.#options.onContentReady();
  }

  public async startPanningAnimation(direction: PanDirection, panSpeed = 20) {
    if (this.isPanning) return;

    this.isPanning = true;
    this.animatePanning(direction, panSpeed);
  }

  public stopPanningAnimation() {
    cancelAnimationFrame(this.panningAnimationFrame);
    this.isPanning = false;
  }

  public async startZoomingAnimation(isZoomOut: boolean, zoomSpeed = 20) {
    if (this.isZooming) return;

    this.isZooming = true;
    this.animateZooming(isZoomOut, zoomSpeed);
  }

  public stopZoomingAnimation() {
    cancelAnimationFrame(this.zoomingAnimationFrame);
    this.isZooming = false;
  }

  public panToDistance(dx = 0, dy = 0, duration?: number) {
    const x = this.domMatrix.e + dx;
    const y = this.domMatrix.f + dy;

    return this.panTo(x, y, duration);
  }

  public async panTo(x: number, y: number, duration?: number) {
    const boxBounds = this.boxEl.getBoundingClientRect();
    const contentBounds = this.boxContentEl.getBoundingClientRect();
    let newX = x;
    let newY = y;

    const scaledPadding = this.padding.multiply(1 / this.scaleRatio);

    // left bound
    if (newX > scaledPadding.left) newX = scaledPadding.left;

    // right bound
    if (newX + contentBounds.width < boxBounds.right - scaledPadding.right) newX = this.domMatrix.e;

    // top bound
    if (newY > scaledPadding.top) newY = scaledPadding.top;

    // bottom bound
    if (newY + contentBounds.height < boxBounds.bottom - scaledPadding.bottom) newY = this.domMatrix.f;

    this.domMatrix.e = newX;
    this.domMatrix.f = newY;

    this.#options.onPanning(newX, newY);
    await this.applyTransform(duration);
  }

  public async zoomByDelta(
    delta: number, // zoom in: delta > 1, zoom out: delta < 1
    pageX?: number,
    pageY?: number,
    isManualZoom = false,
    duration: number = 0,
  ) {
    if (!this.#options.allowZoom) return;

    const oldZoom = this.#options.zoomFactor * this.scaleRatio;
    const newZoom = oldZoom * delta;

    const x = (pageX ?? this.boxEl.offsetLeft) - this.boxEl.offsetLeft;
    const y = (pageY ?? this.boxEl.offsetTop) - this.boxEl.offsetTop;

    return this.zoomToPoint(newZoom, {
      x, y,
      duration,
      isManualZoom,
      useDelta: true,
      isZoomModeChanged: false,
    });
  }

  public async setZoomMode(mode: ZoomMode = ZoomMode.AutoZoom, zoomLockFactor = -1, duration = 0) {
    const fullW = this.boxContentEl.scrollWidth / this.scaleRatio;
    const fullH = this.boxContentEl.scrollHeight / this.scaleRatio;
    if (fullW === 0 || fullH === 0) return;

    const scaledPadding = this.padding.multiply(1 / this.scaleRatio);
    const widthScale = (this.boxEl.clientWidth - scaledPadding.horizontal) / fullW;
    const heightScale = (this.boxEl.clientHeight - scaledPadding.vertical) / fullH;
    let zoomFactor = 1;

    if (mode === ZoomMode.ScaleToWidth) {
      zoomFactor = widthScale;
    }
    else if (mode === ZoomMode.ScaleToHeight) {
      zoomFactor = heightScale;
    }
    else if (mode === ZoomMode.ScaleToFit) {
      zoomFactor = Math.min(widthScale, heightScale);
    }
    else if (mode === ZoomMode.ScaleToFill) {
      zoomFactor = Math.max(widthScale, heightScale);
    }
    else if (mode === ZoomMode.LockZoom) {
      zoomFactor = zoomLockFactor > 0 ? zoomLockFactor : this.zoomFactor;
    }
    // AutoZoom
    else {
      // viewport size >= content size
      if (widthScale >= 1 && heightScale >= 1) {
        zoomFactor = 1; // show original size
      }
      else {
        zoomFactor = Math.min(widthScale, heightScale);
      }
    }

    this.zoomToCenter(zoomFactor, {
      isManualZoom: false,
      duration,
      isZoomModeChanged: true,
    });
  }

  public async zoomToCenter(factor: number, options: {
    isManualZoom?: boolean,
    duration?: number,
    isZoomModeChanged?: boolean,
  } = {}) {
    // center point
    const { x, y } = this.getCenterPoint(factor);

    // change zoom factor
    this.zoomToPoint(factor, {
      x, y,
      duration: options.duration,
      isManualZoom: options.isManualZoom,
      isZoomModeChanged: options.isZoomModeChanged,
    });
  }

  public async zoomToPoint(factor: number, options: {
    x?: number,
    y?: number,
    duration?: number,
    useDelta?: boolean,
    isManualZoom?: boolean,
    isZoomModeChanged?: boolean,
  } = {}) {
    let newZoomFactor = this.dpi(factor);
    const oldZoomFactor = this.#options.zoomFactor;

    // when useDelta = false, we must set an init location for the matrix
    const setInitLocation = !options.useDelta ?? true;

    // restrict the zoom factor
    newZoomFactor = Math.min(
      Math.max(this.#options.minZoom, newZoomFactor),
      this.#options.maxZoom,
    );

    // raise event onBeforeZoomChanged
    this.#options.onBeforeZoomChanged({
      zoomFactor: this.zoomFactor,
      x: this.domMatrix.e,
      y: this.domMatrix.f,
      isManualZoom: options.isManualZoom || false,
      isZoomModeChanged: options.isZoomModeChanged || false,
    });


    // move the content to center the box
    const boxBounds = this.boxEl.getBoundingClientRect();
    const contentBounds = this.boxContentEl.getBoundingClientRect();
    let x = options.x;
    let y = options.y;
    const isInsideBox = contentBounds.left >= boxBounds.left
      || contentBounds.top >= boxBounds.top
      || contentBounds.right <= boxBounds.right
      || contentBounds.bottom <= boxBounds.bottom;

    if (isInsideBox) {
      // center point
      const center = this.getCenterPoint(newZoomFactor);
      x = center.x;
      y = center.y;
    }


    // use delta to transform the matrix
    const delta = newZoomFactor / oldZoomFactor;
    this.#options.zoomFactor = newZoomFactor;

    if (setInitLocation || isInsideBox) {
      this.domMatrix.e = x;
      this.domMatrix.f = y;
    }

    // apply scale and translate value using zoom delta value
    this.domMatrix = new DOMMatrix()
      .translateSelf(x, y)
      .scaleSelf(delta)
      .translateSelf(-x, -y)
      .multiplySelf(this.domMatrix);

    // raise event onAfterZoomChanged
    this.#options.onAfterZoomChanged({
      zoomFactor: this.zoomFactor,
      x: this.domMatrix.e,
      y: this.domMatrix.f,
      isManualZoom: options.isManualZoom || false,
      isZoomModeChanged: options.isZoomModeChanged || false,
    });

    this.updateImageRendering();
    await this.applyTransform(options.duration);
  }

  public async applyTransform(duration = 0) {
    await new Promise((resolve) => {
      this.boxContentEl.style.transform = `${this.domMatrix.toString()}`;

      // apply animation
      if (duration > 0) {
        const transition = `transform ${duration}ms ease, opacity ${duration}ms ease`;
        this.boxContentEl.style.transition = transition;

        setTimeout(resolve, duration);
      }
      else {
        this.boxContentEl.style.transition = '';
        resolve(undefined);
      }
    });

    // raise event
    this.#options.onAfterTransformed(this.domMatrix);
  }

  public enable() {
    this.applyTransform();

    this.#resizeObserver.observe(this.boxEl);
    this.#contentDOMObserver.observe(this.boxContentEl, {
      attributes: false,
      childList: true,
    });

    this.boxEl.addEventListener('wheel', this.onMouseWheel, { passive: true });

    this.boxEl.addEventListener('pointerenter', this.onPointerEnter);
    this.boxEl.addEventListener('pointerleave', this.onPointerLeave);
    this.boxEl.addEventListener('pointerdown', this.onPointerDown);
    this.boxEl.addEventListener('pointerup', this.onPointerUp);
    this.boxEl.addEventListener('pointermove', this.onPointerMove);

    // this.boxEl.addEventListener('keydown', this.onKeyDown);
    // this.boxEl.addEventListener('keyup', this.onKeyUp);
  }

  public disable() {
    this.#resizeObserver.disconnect();
    this.#contentDOMObserver.disconnect();

    this.boxEl.removeEventListener('mousewheel', this.onMouseWheel);

    this.boxEl.removeEventListener('pointerenter', this.onPointerEnter);
    this.boxEl.removeEventListener('pointerleave', this.onPointerLeave);
    this.boxEl.removeEventListener('pointerdown', this.onPointerDown);
    this.boxEl.removeEventListener('pointerup', this.onPointerUp);
    this.boxEl.removeEventListener('pointermove', this.onPointerMove);

    // this.boxEl.removeEventListener('keydown', this.onKeyDown);
    // this.boxEl.removeEventListener('keyup', this.onKeyUp);
  }
  // #endregion
}


export default { HapplaBox };
