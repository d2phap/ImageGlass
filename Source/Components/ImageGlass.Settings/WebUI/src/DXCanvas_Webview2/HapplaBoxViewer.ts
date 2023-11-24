import { HapplaBoxHTMLElement, defineHapplaBoxHTMLElement } from './webComponents/HapplaBoxHTMLElement';
import {
  IMouseEventArgs,
  ILoadContentRequestedEventArgs,
  IZoomEventArgs,
  ZoomMode,
  PanDirection,
} from './webComponents/happlajs/HapplaBoxTypes';


enum Web2BackendMsgNames {
  SET_HTML = 'SET_HTML',
  SET_IMAGE = 'SET_IMAGE',
  SET_ZOOM_MODE = 'SET_ZOOM_MODE',
  SET_ZOOM_FACTOR = 'SET_ZOOM_FACTOR',
  START_PANNING_ANIMATION = 'START_PANNING_ANIMATION',
  START_ZOOMING_ANIMATION = 'START_ZOOMING_ANIMATION',
  STOP_ANIMATIONS = 'STOP_ANIMATIONS',
  SET_MESSAGE = 'SET_MESSAGE',
  SET_NAVIGATION = 'SET_NAVIGATION',
}

enum Web2FrontendMsgNames {
  ON_ZOOM_CHANGED = 'ON_ZOOM_CHANGED',
  ON_POINTER_DOWN = 'ON_POINTER_DOWN',
  ON_MOUSE_WHEEL = 'ON_MOUSE_WHEEL',
  ON_FILE_DROP = 'ON_FILE_DROP',
  ON_NAV_CLICK = 'ON_NAV_CLICK',
}

const _transitionDuration = 300;
let _boxEl: HapplaBoxHTMLElement = undefined;
let _zoomMode: ZoomMode = ZoomMode.AutoZoom;
let _isPointerDown = false;
let _navHitTest: 'left' | 'right' | '' = '';

export default class HapplaBoxViewer {

  static initialize() {
    defineHapplaBoxHTMLElement();
    _boxEl = document.querySelector('happla-box').shadowRoot.host as HapplaBoxHTMLElement;

    _boxEl.initialize({
      zoomFactor: 1,
      onAfterZoomChanged: HapplaBoxViewer.onAfterZoomChanged,
      onMouseWheel: HapplaBoxViewer.onMouseWheel,
    });

    _boxEl.addEventListener('dragenter', HapplaBoxViewer.onFileDragEntered);
    _boxEl.addEventListener('dragover', HapplaBoxViewer.onFileDragOver);
    _boxEl.addEventListener('drop', HapplaBoxViewer.onFileDropped);

    _boxEl.addEventListener('pointerleave', HapplaBoxViewer.onPointerLeave);
    _boxEl.addEventListener('pointerup', HapplaBoxViewer.onPointerUp);
    _boxEl.addEventListener('pointerdown', HapplaBoxViewer.onPointerDown);
    _boxEl.addEventListener('pointermove', HapplaBoxViewer.onPointerMove);
    _boxEl.focus();

    // listen to Web2 Backend message
    on(Web2BackendMsgNames.SET_IMAGE, HapplaBoxViewer.onWeb2LoadContentRequested);
    on(Web2BackendMsgNames.SET_HTML, HapplaBoxViewer.onWeb2LoadContentRequested);
    on(Web2BackendMsgNames.SET_ZOOM_MODE, HapplaBoxViewer.onWeb2ZoomModeChanged);
    on(Web2BackendMsgNames.SET_ZOOM_FACTOR, HapplaBoxViewer.onWeb2ZoomFactorChanged);
    on(Web2BackendMsgNames.START_PANNING_ANIMATION, HapplaBoxViewer.onWeb2StartPanAnimationRequested);
    on(Web2BackendMsgNames.START_ZOOMING_ANIMATION, HapplaBoxViewer.onWeb2StartZoomAnimationRequested);
    on(Web2BackendMsgNames.STOP_ANIMATIONS, HapplaBoxViewer.onWeb2StopAnimationsRequested);
    on(Web2BackendMsgNames.SET_MESSAGE, HapplaBoxViewer.onWeb2SetMessage);
    on(Web2BackendMsgNames.SET_NAVIGATION, HapplaBoxViewer.onWeb2SetNavigation);
  }

  private static onFileDragEntered(e: DragEvent) {
    e.stopPropagation();
    e.preventDefault();
  }

  private static onFileDragOver(e: DragEvent) {
    e.stopPropagation();
    e.preventDefault();
    e.dataTransfer.dropEffect = 'link';
  }

  private static onFileDropped(e: DragEvent) {
    e.stopPropagation();
    e.preventDefault();

    post(Web2FrontendMsgNames.ON_FILE_DROP, e.dataTransfer.files, false);
  }


  private static onPointerLeave(e: PointerEvent) {
    e.preventDefault();

    const navLeftEl = query('#layerNavigation .nav-left');
    const navRightEl = query('#layerNavigation .nav-right');
    navLeftEl.classList.remove('is--visible');
    navRightEl.classList.remove('is--visible');
  }

  private static onPointerDown(e: PointerEvent) {
    e.preventDefault();
    _isPointerDown = true;

    const navLeftEl = query('#layerNavigation .nav-left');
    const navRightEl = query('#layerNavigation .nav-right');
    _navHitTest = HapplaBoxViewer.hitTestNav(e);

    navLeftEl.classList.toggle('is--visible', _navHitTest === 'left');
    navRightEl.classList.toggle('is--visible', _navHitTest === 'right');
    navLeftEl.classList.toggle('is--active', _navHitTest === 'left');
    navRightEl.classList.toggle('is--active', _navHitTest === 'right');

    post(Web2FrontendMsgNames.ON_POINTER_DOWN, {
      Dpi: _boxEl.options.scaleRatio,
      Button: e.button,
      X: e.pageX,
      Y: e.pageY,
      Delta: 0,
    } as IMouseEventArgs, true);
  }

  private static onPointerUp(e: PointerEvent) {
    e.preventDefault();

    const navLeftEl = query('#layerNavigation .nav-left');
    const navRightEl = query('#layerNavigation .nav-right');
    navLeftEl.classList.toggle('is--active', false);
    navRightEl.classList.toggle('is--active', false);

    const nav = HapplaBoxViewer.hitTestNav(e);
    if (_isPointerDown && _navHitTest !== '' && _navHitTest === nav) {
      post(Web2FrontendMsgNames.ON_NAV_CLICK, {
        NavigationButton: nav,
        Button: e.button,
        X: e.pageX,
        Y: e.pageY,
        Delta: 0,
      } as IMouseEventArgs, true);
    }

    _isPointerDown = false;
    _navHitTest = '';
  }

  private static onPointerMove(e: PointerEvent) {
    e.preventDefault();

    const navLeftEl = query('#layerNavigation .nav-left');
    const navRightEl = query('#layerNavigation .nav-right');
    const nav = HapplaBoxViewer.hitTestNav(e);

    if (!_isPointerDown) {
      navLeftEl.classList.toggle('is--visible', nav === 'left');
      navRightEl.classList.toggle('is--visible', nav === 'right');
    }
    else {
      navLeftEl.classList.toggle('is--active', _navHitTest === 'left');
      navRightEl.classList.toggle('is--active', _navHitTest === 'right');
    }
  }

  private static onMouseWheel(e: WheelEvent) {
    post(Web2FrontendMsgNames.ON_MOUSE_WHEEL, {
      Dpi: _boxEl.options.scaleRatio,
      Button: e.button,
      Delta: e.deltaY,
      X: e.pageX,
      Y: e.pageY,
    } as IMouseEventArgs, true);
  }

  private static onAfterZoomChanged(e: IZoomEventArgs) {
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

  private static async onWeb2ZoomModeChanged(_: Web2BackendMsgNames, e: {
    ZoomMode: ZoomMode,
    IsManualZoom: boolean,
  }) {
    _zoomMode = e.ZoomMode;
    await _boxEl.setZoomMode(e.ZoomMode, -1, _transitionDuration);
  }

  private static async onWeb2ZoomFactorChanged(_: Web2BackendMsgNames, e: {
    ZoomFactor: number,
    IsManualZoom: boolean,
    ZoomDelta: number,
  }) {
    await _boxEl.setZoomFactor(e.ZoomFactor, e.IsManualZoom, e.ZoomDelta, _transitionDuration);
  }

  private static async onWeb2StartPanAnimationRequested(_: Web2BackendMsgNames, e: {
    PanSpeed: number,
    Direction: PanDirection,
  }) {
    await _boxEl.startPanningAnimation(e.Direction, e.PanSpeed);
  }

  private static async onWeb2StartZoomAnimationRequested(_: Web2BackendMsgNames, e: {
    IsZoomOut: boolean,
    ZoomSpeed: number,
  }) {
    await _boxEl.startZoomingAnimation(e.IsZoomOut, e.ZoomSpeed);
  }

  private static async onWeb2StopAnimationsRequested() {
    _boxEl.stopAnimations();
  }

  private static onWeb2SetMessage(_: Web2BackendMsgNames, e: {
    Heading: string,
    Text: string,
  }) {
    const msgLayerEl = query('#layerMessage');
    const headingEl = query('.message-heading', msgLayerEl);
    const textEl = query('.message-text', msgLayerEl);

    // update text
    headingEl.innerText = e.Heading;
    textEl.innerText = e.Text;

    const isHeadingHidden = !e.Heading;
    const isTextHidden = !e.Text;
    const isLayerHidden = isHeadingHidden && isTextHidden;

    // update visibility
    msgLayerEl.hidden = isLayerHidden;
    headingEl.hidden = isHeadingHidden;
    textEl.hidden = isTextHidden;
  }

  private static onWeb2SetNavigation(_: Web2BackendMsgNames, e: {
    Visible: boolean,
    LeftImageUrl: string,
    RightImageUrl: string,
    NavButtonColor: number[],
  }) {
    const navLayerEl = query('#layerNavigation');
    const navLeftImgEl = query<HTMLImageElement>('.nav-left img', navLayerEl);
    const navRightImgEl = query<HTMLImageElement>('.nav-right img', navLayerEl);

    const [r, g, b] = e.NavButtonColor;
    const rgbaColor = [r, g, b].join(' ');

    navLayerEl.hidden = !e.Visible;
    navLayerEl.style.setProperty('--nav-button-color', rgbaColor);
    navLeftImgEl.src = e.LeftImageUrl;
    navRightImgEl.src = e.RightImageUrl;
  }

  private static hitTestNav(e: PointerEvent) {
    if (e.button > 0) return ''; // only check if left-mouse clicked or no button clicked

    const boxBounds = _boxEl.getBoundingClientRect();
    const layerNavEl = query('#layerNavigation');
    const navLeftEl = query('#layerNavigation .nav-left');
    const navRightEl = query('#layerNavigation .nav-right');

    if (boxBounds.width < navLeftEl.clientWidth + navRightEl.clientWidth
      || layerNavEl.hidden) return '';


    // check right nav
    // right clickable region
    const rectRight = new DOMRect(
      navRightEl.offsetLeft,
      0,
      boxBounds.width - navRightEl.offsetLeft,
      boxBounds.height,
    );
    const isRightNav = rectRight.x <= e.x && e.x < rectRight.right
      && rectRight.y <= e.y && e.y < rectRight.bottom;
    if (isRightNav) return 'right';


    // check left nav
    // left clickable region
    const rectLeft = new DOMRect(
      0,
      0,
      navLeftEl.offsetLeft + navLeftEl.clientWidth,
      boxBounds.height,
    );
    const isLeftNav = rectLeft.x <= e.x && e.x < rectLeft.right
      && rectLeft.y <= e.y && e.y < rectLeft.bottom;
    if (isLeftNav) return 'left';


    return '';
  }
}
