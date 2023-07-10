import { HapplaBoxHTMLElement, defineHapplaBoxHTMLElement } from './webComponents/HapplaBoxHTMLElement';

let boxEl: HapplaBoxHTMLElement = undefined;

export default class HapplaBoxViewer {
  static initialize() {
    defineHapplaBoxHTMLElement();
    boxEl = document.querySelector('happla-box').shadowRoot.host as HapplaBoxHTMLElement;
    boxEl.setAttribute('checkerboard', 'true');

    boxEl.initialize({
      zoomFactor: 1,
      onAfterZoomChanged: HapplaBoxViewer.onAfterZoomChanged,
      onResizing: HapplaBoxViewer.onResizing,
    });

    boxEl.addEventListener('dragenter', (e) => {
      e.stopPropagation();
      e.preventDefault();
    });

    boxEl.addEventListener('dragover', (e) => {
      e.stopPropagation();
      e.preventDefault();
      e.dataTransfer.dropEffect = 'copy';
    });

    boxEl.addEventListener('drop', (e) => {
      e.stopPropagation();
      e.preventDefault();
      console.log(e.dataTransfer.files);
    });

    boxEl.focus();
  }


  private static onResizing() {
    console.log('onResizing');
    boxEl.setZoomMode();
  }


  private static onAfterZoomChanged(factor: number) {
    console.log('onAfterZoomChanged', factor);
  }
}
