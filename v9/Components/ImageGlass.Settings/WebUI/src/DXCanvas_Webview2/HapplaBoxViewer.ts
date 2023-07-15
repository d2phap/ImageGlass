import { HapplaBoxHTMLElement, defineHapplaBoxHTMLElement } from './webComponents/HapplaBoxHTMLElement';
import { ZoomMode } from './webComponents/happlajs/HapplaBoxTypes';


enum Web2BackendMsgNames {
  SET_HTML = 'SET_HTML',
  SET_IMAGE = 'SET_IMAGE',
  SET_ZOOM_MODE = 'SET_ZOOM_MODE',
}

enum Web2FrontendMsgNames {
  ON_ZOOM_CHANGED = 'ON_ZOOM_CHANGED',
  ON_POINTER_DOWN = 'ON_POINTER_DOWN',
}

let _boxEl: HapplaBoxHTMLElement = undefined;
let _zoomMode: ZoomMode = ZoomMode.AutoZoom;
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

    _boxEl.addEventListener('pointerdown', (e) => {
      e.preventDefault();
      post(Web2FrontendMsgNames.ON_POINTER_DOWN, {
        button: e.button,
        x: e.pageX,
        y: e.pageY,
      }, true);
    });

    _boxEl.focus();

    // listen to Web2 Backend message
    on(Web2BackendMsgNames.SET_IMAGE, HapplaBoxViewer.onWeb2LoadContentRequested);
    on(Web2BackendMsgNames.SET_HTML, HapplaBoxViewer.onWeb2LoadContentRequested);
    on(Web2BackendMsgNames.SET_ZOOM_MODE, HapplaBoxViewer.onWeb2ZoomModeChanged);
  }

  private static onResizing() {
    if (!_isManualZoom) {
      _boxEl.setZoomMode(_zoomMode);
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

    post(Web2FrontendMsgNames.ON_ZOOM_CHANGED, {
      zoomFactor: e.zoomFactor,
      isManualZoom: e.isManualZoom,
      isZoomModeChanged: e.isZoomModeChanged,
    }, true);
  }

  private static async onWeb2LoadContentRequested(eventName: Web2BackendMsgNames, data: {
    ZoomMode: ZoomMode,
    Html?: string,
    Url?: string,
  }) {
    _zoomMode = data.ZoomMode;

    if (eventName === Web2BackendMsgNames.SET_IMAGE) {
      await _boxEl.loadImage(data.Url, _zoomMode);
    }
    else if (eventName === Web2BackendMsgNames.SET_HTML) {
      await _boxEl.loadHtml(data.Html, _zoomMode);
    }
  }

  private static async onWeb2ZoomModeChanged(_: Web2BackendMsgNames, data: {
    ZoomMode: ZoomMode,
    IsManualZoom: boolean,
  }) {
    _zoomMode = data.ZoomMode;
    await _boxEl.setZoomMode(data.ZoomMode, 300);
  }
}
