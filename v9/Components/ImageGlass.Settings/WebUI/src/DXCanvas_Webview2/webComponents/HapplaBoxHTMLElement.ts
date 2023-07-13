import merge from 'lodash.merge';
import { HapplaBox } from './happlajs/HapplaBox';
import { IHapplaBoxOptions, ZoomMode } from './happlajs/HapplaBoxTypes';
import { taggedTemplate } from '@/helpers';

const styles = `
  :host * {
    -webkit-user-drag: none;
    user-select: none;
  }

  .happlabox-container {
    margin: auto;
    max-width: 100vw;
    max-height: 100vh;
    width: 100vw;
    height: 100vh;
  }
  .happlabox-container:focus,
  .happlabox-container:focus-visible {
    outline: none;
  }

  :host([checkerboard=true i]) .happlabox-container {
    background-size: 1.5rem 1.5rem;
    background-image: conic-gradient(
      rgb(255 255 255 / 0.1) 25%,
      rgb(0 0 0 / 0.1) 0 50%,
      rgb(255 255 255 / 0.1) 0 75%,
      rgb(0 0 0 / 0.1) 0
    );
  }

  .happlabox-wrapper {
    width: 100%;
    height: 100%;
    opacity: 0;
    transition: all 100ms ease;
  }

  .happlabox-content {
    display: inline-flex;
  }
`;


const imgTemplate = taggedTemplate<{
  src: string,
  alt?: string,
}>`<img src="${'src'}" alt="${'alt'}" />`;


export class HapplaBoxHTMLElement extends HTMLElement {
  #box: HapplaBox;

  #containerEl: HTMLDivElement;
  #wrapperEl: HTMLDivElement;
  #contentEl: HTMLDivElement;

  #options: IHapplaBoxOptions = {};

  constructor() {
    super();

    // private methods
    this.createTemplate = this.createTemplate.bind(this);

    // private events
    this.onAuxClicked = this.onAuxClicked.bind(this);

    // initialize template
    this.createTemplate();
  }

  get options() {
    return this.#options;
  }

  private disconnectedCallback() {
    this.#box.disable();
  }

  private onAuxClicked(e: PointerEvent) {
    e.stopPropagation();

    // right click
    if (e.button === 2) {
      // todo
    }
  }

  private createTemplate() {
    // initialize component
    this.attachShadow({ mode: 'open' });

    const css = new CSSStyleSheet();
    css.replaceSync(styles);
    this.shadowRoot.adoptedStyleSheets = [css];


    // content
    const contentEl = document.createElement('div');
    contentEl.classList.add('happlabox-content');

    // wrapper
    const wrapperEl = document.createElement('div');
    wrapperEl.classList.add('happlabox-wrapper');
    wrapperEl.appendChild(contentEl);

    // container
    const containerEl = document.createElement('div');
    containerEl.tabIndex = 0;
    containerEl.classList.add('happlabox-container');
    containerEl.appendChild(wrapperEl);

    // disable browser default context menu
    containerEl.addEventListener('contextmenu', e => e.preventDefault(), true);
    containerEl.addEventListener('auxclick', this.onAuxClicked, true);


    this.#contentEl = contentEl;
    this.#wrapperEl = wrapperEl;
    this.#containerEl = containerEl;

    this.shadowRoot.appendChild(this.#containerEl);
  }


  public initialize(options: IHapplaBoxOptions = {}) {
    this.#options = merge(this.#options, options);
    this.#box = new HapplaBox(this.#containerEl, this.#contentEl, this.#options);

    this.#box.enable();
  }

  public async loadImage(url: string, zoomMode: ZoomMode = ZoomMode.AutoZoom) {
    const html = imgTemplate({ src: url });
    await this.loadHtml(html, zoomMode);
  }

  public async loadHtml(html: string, zoomMode: ZoomMode = ZoomMode.AutoZoom) {
    this.#wrapperEl.style.opacity = '0';

    await this.#box.loadHtmlContent(html);

    // fixed sixe of SVG
    const svgEls = Array.from(this.#contentEl.querySelectorAll('svg:not([width]), svg:not([height])'));
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

    await this.setZoomMode(zoomMode);

    this.#wrapperEl.style.opacity = '1';
  }

  public async setZoomMode(mode: ZoomMode = ZoomMode.AutoZoom, duration?: number) {
    const fullW = this.#contentEl.scrollWidth / this.#box.scaleRatio;
    const fullH = this.#contentEl.scrollHeight / this.#box.scaleRatio;
    const horizontalPadding = this.#box.padding.left + this.#box.padding.right;
    const verticalPadding = this.#box.padding.top + this.#box.padding.bottom;
    const widthScale = (this.#containerEl.clientWidth - horizontalPadding) / fullW;
    const heightScale = (this.#containerEl.clientHeight - verticalPadding) / fullH;
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
      zoomFactor = this.#box.zoomFactor;
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

    this.#box.zoomTo(zoomFactor, duration);
  }

  public focus() {
    this.#containerEl.focus({ preventScroll: true });
  }
}


/**
 * Creates and registers HapplaBoxHTMLElement to DOM.
 */
export const defineHapplaBoxHTMLElement = () => window.customElements.define(
  'happla-box',
  HapplaBoxHTMLElement,
);
