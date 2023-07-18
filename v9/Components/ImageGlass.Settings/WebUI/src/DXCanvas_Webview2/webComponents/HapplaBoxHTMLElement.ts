
import { HapplaBox } from './happlajs/HapplaBox';
import { IHapplaBoxOptions, PanDirection, ZoomMode } from './happlajs/HapplaBoxTypes';
import { taggedTemplate } from '@/helpers';

const styles = `
  :host * {
    -webkit-user-drag: none;
    user-select: none;
  }

  .hp-box {
    margin: auto;
    max-width: 100vw;
    max-height: 100vh;
    width: 100vw;
    height: 100vh;
  }
  .hp-box:focus,
  .hp-box:focus-visible {
    outline: none;
  }

  :host([checkerboard=true i]) .hp-box {
    background-size: 1.5rem 1.5rem;
    background-image: conic-gradient(
      rgb(255 255 255 / 0.1) 25%,
      rgb(0 0 0 / 0.1) 0 50%,
      rgb(255 255 255 / 0.1) 0 75%,
      rgb(0 0 0 / 0.1) 0
    );
  }

  .hp-box-wrapper {
    width: 100%;
    height: 100%;
    opacity: 0;
    transition: all 100ms ease;
  }

  .hp-box-content {
    display: inline-flex;
  }
`;


const imgTemplate = taggedTemplate<{
  src: string,
  alt?: string,
}>`<img src="${'src'}" alt="${'alt'}" />`;


export class HapplaBoxHTMLElement extends HTMLElement {
  #box: HapplaBox;

  #boxEl: HTMLDivElement;
  #wrapperEl: HTMLDivElement;
  #boxContentEl: HTMLDivElement;

  constructor() {
    super();

    // private methods
    this.createTemplate = this.createTemplate.bind(this);

    // initialize template
    this.createTemplate();
  }

  get options() {
    return this.#box.options;
  }

  private disconnectedCallback() {
    this.#box.disable();
  }


  private createTemplate() {
    // initialize component
    this.attachShadow({ mode: 'open' });

    const css = new CSSStyleSheet();
    css.replaceSync(styles);
    this.shadowRoot.adoptedStyleSheets = [css];


    // content
    const boxContentEl = document.createElement('div');
    boxContentEl.classList.add('hp-box-content');

    // wrapper
    const wrapperEl = document.createElement('div');
    wrapperEl.classList.add('hp-box-wrapper');
    wrapperEl.appendChild(boxContentEl);

    // container
    const boxEl = document.createElement('div');
    boxEl.tabIndex = 0;
    boxEl.classList.add('hp-box');
    boxEl.appendChild(wrapperEl);

    this.#boxContentEl = boxContentEl;
    this.#wrapperEl = wrapperEl;
    this.#boxEl = boxEl;

    this.shadowRoot.appendChild(this.#boxEl);
  }


  public initialize(options: IHapplaBoxOptions = {}) {
    this.#box = new HapplaBox(this.#boxEl, this.#boxContentEl, options);

    this.#box.enable();
  }

  public async loadImage(url: string, zoomMode: ZoomMode = ZoomMode.AutoZoom, zoomLockFactor = -1) {
    const html = imgTemplate({ src: url });
    await this.loadHtml(html, zoomMode, zoomLockFactor);
  }

  public async loadHtml(html: string, zoomMode: ZoomMode = ZoomMode.AutoZoom, zoomLockFactor = -1) {
    this.#wrapperEl.style.transition = 'none';
    this.#boxContentEl.style.transform = 'scale(0.01)';
    this.#wrapperEl.style.opacity = '0';

    await this.#box.loadHtmlContent(html);

    // fixed sixe of SVG
    const svgEls = Array.from(this.#boxContentEl.querySelectorAll('svg:not([width]), svg:not([height])'));
    svgEls.forEach((svgEl: SVGMarkerElement) => {
      const { width, height } = svgEl.viewBox.baseVal;

      if (width === 0 || height === 0) {
        svgEl.style.width = '100vw';
        svgEl.style.height = '100vw';
      }
      else {
        svgEl.setAttribute('width', width.toString());
        svgEl.setAttribute('height', height.toString());
      }
    });

    await this.setZoomMode(zoomMode, zoomLockFactor);

    this.#wrapperEl.style.transition = '';
    this.#wrapperEl.style.opacity = '1';
  }

  public setZoomMode(mode: ZoomMode = ZoomMode.AutoZoom, zoomLockFactor = -1, duration = 0) {
    return this.#box.setZoomMode(mode, zoomLockFactor, duration);
  }

  public setZoomFactor(zoomFactor: number, isManualZoom: boolean, zoomDelta = 1, duration = 0) {
    const x = this.#box.pointerLocation.x ?? -1;
    const y = this.#box.pointerLocation.y ?? -1;

    if (zoomDelta !== 1 && x >= 0 && y >= 0) {
      return this.#box.zoomByDelta(zoomDelta, x, y, isManualZoom, duration / 3);
    }

    return this.#box.zoomToCenter(zoomFactor, { isManualZoom, duration });
  }

  public startPanningAnimation(direction: PanDirection, speed: number) {
    return this.#box.startPanningAnimation(direction, speed);
  }

  public stopPanningAnimation() {
    this.#box.stopPanningAnimation();
  }

  public focus() {
    this.#boxEl.focus({ preventScroll: true });
  }
}


/**
 * Creates and registers HapplaBoxHTMLElement to DOM.
 */
export const defineHapplaBoxHTMLElement = () => window.customElements.define(
  'happla-box',
  HapplaBoxHTMLElement,
);
