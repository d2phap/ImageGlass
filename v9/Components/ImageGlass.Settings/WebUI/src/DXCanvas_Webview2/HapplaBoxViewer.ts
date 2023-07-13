import { HapplaBoxHTMLElement, defineHapplaBoxHTMLElement } from './webComponents/HapplaBoxHTMLElement';


enum Web2BackendMsgNames {
  SET_HTML = 'SET_HTML',
  SET_IMAGE = 'SET_IMAGE',
}

enum Web2FrontendMsgNames {
  ZOOM_CHANGED = 'ZOOM_CHANGED',
}

let boxEl: HapplaBoxHTMLElement = undefined;

export default class HapplaBoxViewer {
  static initialize() {
    defineHapplaBoxHTMLElement();
    boxEl = document.querySelector('happla-box').shadowRoot.host as HapplaBoxHTMLElement;

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
      console.info(e.dataTransfer.files);
    });

    boxEl.focus();

    // listen to Web2 Backend message
    on(Web2BackendMsgNames.SET_IMAGE, HapplaBoxViewer.onWeb2LoadContentRequested);
    on(Web2BackendMsgNames.SET_HTML, HapplaBoxViewer.onWeb2LoadContentRequested);
  }

  private static onResizing() {
    boxEl.setZoomMode();
  }

  private static onAfterZoomChanged(factor: number) {
    post(Web2FrontendMsgNames.ZOOM_CHANGED, factor, true);
  }

  private static async onWeb2LoadContentRequested(eventName: Web2BackendMsgNames, data: string) {
    if (eventName === Web2BackendMsgNames.SET_IMAGE) {
      await boxEl.loadImage(data);
    }
    else if (eventName === Web2BackendMsgNames.SET_HTML) {
      await boxEl.loadHtml(data);
    }
  }
}
