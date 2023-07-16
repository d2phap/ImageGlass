import { HapplaBoxHTMLElement, defineHapplaBoxHTMLElement } from './webComponents/HapplaBoxHTMLElement';
import { IMouseEventArgs, ILoadContentRequestedEventArgs, IZoomEventArgs, ZoomMode } from './webComponents/happlajs/HapplaBoxTypes';


enum Web2BackendMsgNames {
  SET_HTML = 'SET_HTML',
  SET_IMAGE = 'SET_IMAGE',
  SET_ZOOM_MODE = 'SET_ZOOM_MODE',
  SET_ZOOM_FACTOR = 'SET_ZOOM_FACTOR',
}

enum Web2FrontendMsgNames {
  ON_ZOOM_CHANGED = 'ON_ZOOM_CHANGED',
  ON_POINTER_DOWN = 'ON_POINTER_DOWN',
  ON_MOUSE_WHEEL = 'ON_MOUSE_WHEEL',
}

const _transitionDuration = 300;
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
      onMouseWheel: HapplaBoxViewer.onMouseWheel,
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

    _boxEl.addEventListener('pointerdown', HapplaBoxViewer.onPointerDown);

    _boxEl.focus();

    // listen to Web2 Backend message
    on(Web2BackendMsgNames.SET_IMAGE, HapplaBoxViewer.onWeb2LoadContentRequested);
    on(Web2BackendMsgNames.SET_HTML, HapplaBoxViewer.onWeb2LoadContentRequested);
    on(Web2BackendMsgNames.SET_ZOOM_MODE, HapplaBoxViewer.onWeb2ZoomModeChanged);
    on(Web2BackendMsgNames.SET_ZOOM_FACTOR, HapplaBoxViewer.onWeb2ZoomFactorChanged);
  }

  private static onResizing() {
    if (!_isManualZoom) {
      _boxEl.setZoomMode(_zoomMode);
    }
  }

  private static onPointerDown(e: PointerEvent) {
    e.preventDefault();
    post(Web2FrontendMsgNames.ON_POINTER_DOWN, {
      Dpi: _boxEl.options.scaleRatio,
      Button: e.button,
      X: e.pageX,
      Y: e.pageY,
      Delta: 0,
    } as IMouseEventArgs, true);
  }

  private static onMouseWheel(e: WheelEvent) {
    e.preventDefault();
    post(Web2FrontendMsgNames.ON_MOUSE_WHEEL, {
      Dpi: _boxEl.options.scaleRatio,
      Button: e.button,
      Delta: e.deltaY,
      X: e.pageX,
      Y: e.pageY,
    } as IMouseEventArgs, true);
  }

  private static onAfterZoomChanged(e: IZoomEventArgs) {
    _isManualZoom = e.isManualZoom;

    post(Web2FrontendMsgNames.ON_ZOOM_CHANGED, {
      ZoomFactor: e.zoomFactor,
      IsManualZoom: e.isManualZoom,
      IsZoomModeChanged: e.isZoomModeChanged,
    }, true);
  }

  private static async onWeb2LoadContentRequested(
    eventName: Web2BackendMsgNames,
    e: ILoadContentRequestedEventArgs,
  ) {
    _zoomMode = e.ZoomMode;

    if (eventName === Web2BackendMsgNames.SET_IMAGE) {
      await _boxEl.loadImage(e.Url, _zoomMode, e.ZoomFactor);
    }
    else if (eventName === Web2BackendMsgNames.SET_HTML) {
      await _boxEl.loadHtml(e.Html, _zoomMode, e.ZoomFactor);
    }
  }

  private static async onWeb2ZoomModeChanged(_: Web2BackendMsgNames, data: {
    ZoomMode: ZoomMode,
    IsManualZoom: boolean,
  }) {
    _zoomMode = data.ZoomMode;
    await _boxEl.setZoomMode(data.ZoomMode, -1, _transitionDuration);
  }

  private static async onWeb2ZoomFactorChanged(_: Web2BackendMsgNames, data: {
    ZoomFactor: number,
    IsManualZoom: boolean,
    ZoomDelta: number,
  }) {
    await _boxEl.setZoomFactor(data.ZoomFactor, data.IsManualZoom, data.ZoomDelta, _transitionDuration);
  }
}
