import merge from 'lodash.merge';
import { IHapplaBoxOptions, InterpolationMode, IPadding } from './HapplaBoxTypes';
import { pause } from '@/helpers';


export class HapplaBox {
  private contentEl: HTMLElement;
  private boxEl: HTMLElement;
  private domMatrix: DOMMatrix;
  private isPointerDown = false;

  #isContentElDOMChanged = false;
  #contentDOMObserver: MutationObserver;
  #resizeObserver: ResizeObserver;

  private animationFrame: number;
  private isMoving = false;
  private arrowLeftDown = false;
  private arrowRightDown = false;
  private arrowUpDown = false;
  private arrowDownDown = false;

  private options: IHapplaBoxOptions = {};
  private defaultOptions: IHapplaBoxOptions = {
    imageRendering: InterpolationMode.Auto,

    allowZoom: true,
    minZoom: 0.01,
    maxZoom: 100,
    zoomFactor: 1,
    panOffset: { x: 0, y: 0 },

    allowPan: true,
    scaleRatio: window.devicePixelRatio,
    padding: {
      left: 0, right: 0,
      top: 0, bottom: 0,
    },

    onBeforeContentReady() {},
    onContentReady() {},
    onResizing() {},

    onBeforeZoomChanged() {},
    onAfterZoomChanged() {},
    onAfterTransformed() {},

    onPanning() {},
    onAfterPanned() {},
  };


  /**
   * Initializes HapplaBox element.
   * @param boxEl Box element
   * @param contentEl Content element
   * @param options Options
   */
  constructor(boxEl: HTMLElement, contentEl: HTMLElement, options?: IHapplaBoxOptions) {
    this.boxEl = boxEl;
    this.contentEl = contentEl;
    this.options = merge({}, this.defaultOptions, options);

    // correct zoomFactor after calculating scaleRatio
    this.options.zoomFactor /= this.options.scaleRatio;

    this.domMatrix = new DOMMatrix()
      .scaleSelf(this.zoomFactor)
      .translateSelf(this.options.panOffset.x, this.options.panOffset.y);

    this.zoomDistance = this.zoomDistance.bind(this);
    this.moveDistance = this.moveDistance.bind(this);
    this.startMoving = this.startMoving.bind(this);
    this.stopMoving = this.stopMoving.bind(this);
    this.dpi = this.dpi.bind(this);
    this.updateImageRendering = this.updateImageRendering.bind(this);

    this.enable = this.enable.bind(this);
    this.disable = this.disable.bind(this);
    this.zoomToPoint = this.zoomToPoint.bind(this);
    this.zoomTo = this.zoomTo.bind(this);
    this.panTo = this.panTo.bind(this);
    this.applyTransform = this.applyTransform.bind(this);

    this.onContentElDOMChanged = this.onContentElDOMChanged.bind(this);
    this.onResizing = this.onResizing.bind(this);
    this.onMouseWheel = this.onMouseWheel.bind(this);
    this.onPointerDown = this.onPointerDown.bind(this);
    this.onPointerUp = this.onPointerUp.bind(this);
    this.onPointerMove = this.onPointerMove.bind(this);
    // this.onKeyDown = this.onKeyDown.bind(this);
    // this.onKeyUp = this.onKeyUp.bind(this);

    // create content DOM observer
    this.#contentDOMObserver = new MutationObserver(this.onContentElDOMChanged);
    this.#resizeObserver = new ResizeObserver(this.onResizing);

    this.disable();

    this.contentEl.style.transformOrigin = 'top left';
    this.boxEl.style.touchAction = 'none';
    this.boxEl.style.overflow = 'hidden';

    // emit event onBeforeContentReady
    this.options.onBeforeContentReady();
  }

  // #region Getters & Setters
  get imageRendering() {
    return this.options.imageRendering;
  }

  set imageRendering(value: InterpolationMode) {
    this.options.imageRendering = value;

    this.updateImageRendering();
  }

  get scaleRatio() {
    return this.options.scaleRatio;
  }

  set scaleRatio(value: number) {
    this.options.scaleRatio = value;
  }

  get padding() {
    return this.options.padding;
  }

  set padding(value: IPadding) {
    this.options.padding = value;
  }

  /**
   * Gets zoom factor after computing device ratio (DPI)
   */
  get zoomFactor() {
    return this.options.zoomFactor * this.options.scaleRatio;
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
    this.options.onResizing();
  }

  private onMouseWheel(e: WheelEvent) {
    // ignore horizontal scroll events
    if (e.deltaY === 0) {
      return;
    }

    const direction = e.deltaY < 0 ? 'up' : 'down';
    const normalizedDeltaY = 1 + Math.abs(e.deltaY) / 1000; // speed
    const delta = direction === 'up' ? normalizedDeltaY : 1 / normalizedDeltaY;

    this.zoomDistance(delta, e.clientX, e.clientY);
  }

  private onPointerDown(e: PointerEvent) {
    // ignore right clicks
    if (e.button !== 0) {
      return;
    }

    this.boxEl.setPointerCapture(e.pointerId);
    this.isPointerDown = true;

    // We get the pointer position on click so we can get the value once the user starts to drag
    this.options.panOffset.x = e.clientX;
    this.options.panOffset.y = e.clientY;
  }

  private onPointerMove(event: PointerEvent) {
    // Only run this function if the pointer is down
    if (!this.isPointerDown) {
      return;
    }

    this.moveDistance(
      event.clientX - this.options.panOffset.x,
      event.clientY - this.options.panOffset.y,
    );

    this.options.panOffset.x = event.clientX;
    this.options.panOffset.y = event.clientY;

    this.options.onPanning(this.domMatrix.e, this.domMatrix.f);
  }

  private onPointerUp(e: PointerEvent) {
    if (!this.isPointerDown) {
      return;
    }

    this.boxEl.releasePointerCapture(e.pointerId);
    this.isPointerDown = false;

    this.options.panOffset.x += e.clientX - this.options.panOffset.x;
    this.options.panOffset.y += e.clientY - this.options.panOffset.y;

    this.options.onAfterPanned(this.domMatrix.e, this.domMatrix.f);
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
    return value / this.options.scaleRatio;
  }

  private updateImageRendering() {
    switch (this.imageRendering) {
      case InterpolationMode.Auto:
        if (this.zoomFactor <= 1.0) {
          this.contentEl.style.imageRendering = InterpolationMode.CrispEdges;
        }
        else {
          this.contentEl.style.imageRendering = InterpolationMode.Pixelated;
        }
        break;

      default:
        this.contentEl.style.imageRendering = this.imageRendering;
        break;
    }
  }

  private moveDistance(x = 0, y = 0) {
    // Update the transform coordinates with the distance from origin and current position
    this.domMatrix.e += x;
    this.domMatrix.f += y;

    this.options.onPanning(this.domMatrix.e, this.domMatrix.f);

    this.applyTransform();
  }

  private zoomDistance(delta: number, dx?: number, dy?: number, duration?: number) {
    if (!this.options.allowZoom) {
      return;
    }

    // update the current zoom factor
    this.options.zoomFactor = this.domMatrix.a;

    const oldZoom = this.options.zoomFactor;
    const newZoom = oldZoom * delta;

    // raise event onBeforeZoomChanged
    this.options.onBeforeZoomChanged(this.zoomFactor, this.domMatrix.e, this.domMatrix.f);

    // restrict the zoom factor
    this.options.zoomFactor = Math.min(
      Math.max(this.options.minZoom, newZoom),
      this.options.maxZoom,
    );

    const newX = (dx ?? this.boxEl.offsetLeft) - this.boxEl.offsetLeft;
    const newY = (dy ?? this.boxEl.offsetTop) - this.boxEl.offsetTop;
    let newDelta = delta;

    // check zoom -> maxZoom
    if (newZoom * this.options.scaleRatio > this.options.maxZoom) {
      newDelta = this.dpi(this.options.maxZoom) / oldZoom;
      this.options.zoomFactor = this.dpi(this.options.maxZoom);
    }

    // check zoom -> minZoom
    else if (newZoom * this.options.scaleRatio < this.options.minZoom) {
      newDelta = this.dpi(this.options.minZoom) / oldZoom;
      this.options.zoomFactor = this.dpi(this.options.minZoom);
    }

    this.domMatrix = new DOMMatrix()
      .translateSelf(newX, newY)
      .scaleSelf(newDelta)
      .translateSelf(-newX, -newY)
      .multiplySelf(this.domMatrix);

    // raise event onAfterZoomChanged
    this.options.onAfterZoomChanged(this.zoomFactor, this.domMatrix.e, this.domMatrix.f);


    this.updateImageRendering();
    this.applyTransform(duration);
  }

  private startMoving() {
    const speed = 20;
    let x = 0;
    let y = 0;

    if (this.arrowLeftDown && !this.arrowRightDown) {
      x = speed;
    }
    else if (!this.arrowLeftDown && this.arrowRightDown) {
      x = -speed;
    }

    if (this.arrowUpDown && !this.arrowDownDown) {
      y = speed;
    }
    else if (!this.arrowUpDown && this.arrowDownDown) {
      y = -speed;
    }

    this.moveDistance(x, y);
    this.animationFrame = requestAnimationFrame(this.startMoving);
  }

  private stopMoving() {
    cancelAnimationFrame(this.animationFrame);
    this.isMoving = false;
  }
  // #endregion


  // #region Public functions
  public async loadHtmlContent(html: string) {
    await this.zoomTo(0.01);

    this.#isContentElDOMChanged = false;
    this.contentEl.innerHTML = html;

    while (!this.#isContentElDOMChanged) {
      await pause(10);
    }

    const list = this.contentEl.querySelectorAll('img');
    const imgs = Array.from(list);

    while (imgs.some((i) => !i.complete)) {
      await pause(10);
    }

    // emit event onContentReady
    this.options.onContentReady();
  }

  public async panTo(x: number, y: number, duration?: number) {
    this.domMatrix.e = x;
    this.domMatrix.f = y;

    await this.applyTransform(duration);
  }

  public async zoomToPoint(factor: number, options: { x?: number, y?: number, duration?: number }) {
    // restrict the zoom factor
    this.options.zoomFactor = Math.min(
      Math.max(this.options.minZoom, this.dpi(factor)),
      this.options.maxZoom,
    );

    // raise event onBeforeZoomChanged
    this.options.onBeforeZoomChanged(this.zoomFactor, this.domMatrix.e, this.domMatrix.f);

    // apply scale and translate value
    this.domMatrix.a = this.options.zoomFactor;
    this.domMatrix.d = this.options.zoomFactor;
    this.domMatrix.e = (options.x || 0) + this.options.padding.left;
    this.domMatrix.f = (options.y || 0) + this.options.padding.top;

    // raise event onAfterZoomChanged
    this.options.onAfterZoomChanged(this.zoomFactor, this.domMatrix.e, this.domMatrix.f);

    this.updateImageRendering();
    await this.applyTransform(options.duration);
  }

  public async zoomTo(factor: number, duration?: number) {
    const fullW = this.contentEl.scrollWidth / this.scaleRatio;
    const fullH = this.contentEl.scrollHeight / this.scaleRatio;
    const horizontalPadding = this.padding.left + this.padding.right;
    const verticalPadding = this.padding.top + this.padding.bottom;

    let x = (this.boxEl.offsetWidth - horizontalPadding - (fullW * factor)) / 2;
    let y = (this.boxEl.offsetHeight - verticalPadding - (fullH * factor)) / 2;

    // // move to center point
    // await this.panTo(-fullW, -fullH);

    // change zoom factor
    this.zoomToPoint(factor, { x, y, duration });
  }

  public async applyTransform(duration = 0) {
    await new Promise((resolve) => {
      this.contentEl.style.transform = `${this.domMatrix.toString()}`;

      // apply animation
      if (duration > 0) {
        const transition = `transform ${duration}ms ease, opacity ${duration}ms ease`;
        this.contentEl.style.transition = transition;

        setTimeout(resolve, duration);
      }
      else {
        this.contentEl.style.transition = '';
        resolve(undefined);
      }
    });

    // raise event
    this.options.onAfterTransformed(this.domMatrix);
  }

  public enable() {
    this.applyTransform();

    this.#resizeObserver.observe(this.boxEl);
    this.#contentDOMObserver.observe(this.contentEl, {
      attributes: false,
      childList: true,
    });

    this.boxEl.addEventListener('wheel', this.onMouseWheel, { passive: true });

    this.boxEl.addEventListener('pointerdown', this.onPointerDown);
    this.boxEl.addEventListener('pointerup', this.onPointerUp);
    this.boxEl.addEventListener('pointerleave', this.onPointerUp);
    this.boxEl.addEventListener('pointermove', this.onPointerMove);

    // this.boxEl.addEventListener('keydown', this.onKeyDown);
    // this.boxEl.addEventListener('keyup', this.onKeyUp);
  }

  public disable() {
    this.#resizeObserver.disconnect();
    this.#contentDOMObserver.disconnect();

    this.boxEl.removeEventListener('mousewheel', this.onMouseWheel);

    this.boxEl.removeEventListener('pointerdown', this.onPointerDown);
    this.boxEl.removeEventListener('pointerup', this.onPointerUp);
    this.boxEl.removeEventListener('pointerleave', this.onPointerUp);
    this.boxEl.removeEventListener('pointermove', this.onPointerMove);

    // this.boxEl.removeEventListener('keydown', this.onKeyDown);
    // this.boxEl.removeEventListener('keyup', this.onKeyUp);
  }
  // #endregion
}


export default { HapplaBox };
