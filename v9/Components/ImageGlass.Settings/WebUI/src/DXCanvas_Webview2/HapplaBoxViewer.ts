import { HapplaBoxHTMLElement, defineHapplaBoxHTMLElement } from './webComponents/HapplaBoxHTMLElement';


enum Web2BackendMsgNames {
  SET_HTML = 'SET_HTML',
  SET_IMAGE = 'SET_IMAGE',
}

enum Web2FrontendMsgNames {
  ZOOM_CHANGED = 'ZOOM_CHANGED',
}

let _boxEl: HapplaBoxHTMLElement = undefined;
let _isManualZoom = false;

export default class HapplaBoxViewer {
  static initialize() {
    defineHapplaBoxHTMLElement();
    _boxEl = document.querySelector('happla-box').shadowRoot.host as HapplaBoxHTMLElement;

    _boxEl.initialize({
      zoomFactor: 1,
      onAfterZoomChanged: HapplaBoxViewer.onAfterZoomChanged,
      onResizing: HapplaBoxViewer.onResizing,
    });

    _boxEl.addEventListener('dragenter', (e) => {
      e.stopPropagation();
      e.preventDefault();
    });

    _boxEl.addEventListener('dragover', (e) => {
      e.stopPropagation();
      e.preventDefault();
      e.dataTransfer.dropEffect = 'copy';
    });

    _boxEl.addEventListener('drop', (e) => {
      e.stopPropagation();
      e.preventDefault();
      console.info(e.dataTransfer.files);
    });

    _boxEl.focus();

    // listen to Web2 Backend message
    on(Web2BackendMsgNames.SET_IMAGE, HapplaBoxViewer.onWeb2LoadContentRequested);
    on(Web2BackendMsgNames.SET_HTML, HapplaBoxViewer.onWeb2LoadContentRequested);
  }

  private static onResizing() {
    if (!_isManualZoom) {
      _boxEl.setZoomMode();
    }
  }

  private static onAfterZoomChanged(e: {
    zoomFactor: number,
    x: number,
    y: number,
    isManualZoom: boolean,
    isZoomModeChanged: boolean,
  }) {
    _isManualZoom = e.isManualZoom;

    post(Web2FrontendMsgNames.ZOOM_CHANGED, {
      zoomFactor: e.zoomFactor,
      isManualZoom: e.isManualZoom,
      isZoomModeChanged: e.isZoomModeChanged,
    }, true);
  }

  private static async onWeb2LoadContentRequested(eventName: Web2BackendMsgNames, data: string) {
    if (eventName === Web2BackendMsgNames.SET_IMAGE) {
      await _boxEl.loadImage(data);
    }
    else if (eventName === Web2BackendMsgNames.SET_HTML) {
      await _boxEl.loadHtml(data);
    }
  }
}
