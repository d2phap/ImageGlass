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
  SET_COMPARISON_MODE = 'SET_COMPARISON_MODE',
  SET_COMPARISON_IMAGES = 'SET_COMPARISON_IMAGES',
  SET_COMPARISON_SLIDER = 'SET_COMPARISON_SLIDER',
}

enum Web2FrontendMsgNames {
  ON_ZOOM_CHANGED = 'ON_ZOOM_CHANGED',
  ON_POINTER_DOWN = 'ON_POINTER_DOWN',
  ON_MOUSE_WHEEL = 'ON_MOUSE_WHEEL',
  ON_CONTENT_SIZE_CHANGED = 'ON_CONTENT_SIZE_CHANGED',
  ON_FILE_DROP = 'ON_FILE_DROP',
  ON_NAV_CLICK = 'ON_NAV_CLICK',
  ON_COMPARISON_SLIDER_CHANGED = 'ON_COMPARISON_SLIDER_CHANGED',
  ON_COMPARISON_PANE_DROP = 'ON_COMPARISON_PANE_DROP',
}

const _transitionDuration = 300;
const _dragThreshold = 5;
let _boxEl: HapplaBoxHTMLElement = undefined;
let _zoomMode: ZoomMode = ZoomMode.AutoZoom;
let _isPointerDown = false;
let _navHitTest: 'left' | 'right' | '' = '';
let _comparisonMode = false;
let _comparisonSliderPos = 0.5;
let _comparisonSliderHandleY = 0.5;
let _isDraggingSlider = false;
let _isPanning = false;
let _panStartX = 0;
let _panStartY = 0;
let _panStartPanX = 0;
let _panStartPanY = 0;
let _comparisonZoom = 1;
let _comparisonPanX = 0;
let _comparisonPanY = 0;
let _comparisonImageWidth = 0;
let _comparisonImageHeight = 0;
let _dropHighlightPane: 'left' | 'right' | null = null;

let _dropToReplaceMainImageText = 'Drop to replace main image';
let _dropToSetComparisonImageText = 'Drop to set comparison image';

export default class HapplaBoxViewer {

  static initialize() {
    defineHapplaBoxHTMLElement();
    _boxEl = document.querySelector('happla-box').shadowRoot.host as HapplaBoxHTMLElement;

    _boxEl.initialize({
      zoomFactor: 1,
      onAfterZoomChanged: HapplaBoxViewer.onAfterZoomChanged,
      onMouseWheel: HapplaBoxViewer.onMouseWheel,
      onContentSizeChanged: HapplaBoxViewer.onContentSizeChanged,
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
    on(Web2BackendMsgNames.SET_COMPARISON_MODE, HapplaBoxViewer.onWeb2SetComparisonMode);
    on(Web2BackendMsgNames.SET_COMPARISON_IMAGES, HapplaBoxViewer.onWeb2SetComparisonImages);
    on(Web2BackendMsgNames.SET_COMPARISON_SLIDER, HapplaBoxViewer.onWeb2SetComparisonSlider);

    HapplaBoxViewer.initComparisonEvents();
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
      AltKey: e.altKey,
      CtrlKey: e.ctrlKey,
      ShiftKey: e.shiftKey,
    } as IMouseEventArgs, true);
  }

  private static onContentSizeChanged(rect: DOMRect) {
    post(Web2FrontendMsgNames.ON_CONTENT_SIZE_CHANGED, {
      Dpi: _boxEl.options.scaleRatio,
      X: rect.x,
      Y: rect.y,
      Width: rect.width,
      Height: rect.height,
    }, true);
  }

  private static onAfterZoomChanged(e: IZoomEventArgs) {
    post(Web2FrontendMsgNames.ON_ZOOM_CHANGED, {
      ZoomFactor: e.zoomFactor,
      IsManualZoom: e.isManualZoom,
      IsZoomModeChanged: e.isZoomModeChanged,
      X: e.x,
      Y: e.y,
    }, true);
  }

  private static async onWeb2LoadContentRequested(
    eventName: Web2BackendMsgNames,
    e: ILoadContentRequestedEventArgs,
  ) {
    _zoomMode = e.ZoomMode;

    if (eventName === Web2BackendMsgNames.SET_IMAGE) {
      await _boxEl.loadImage(e.Url, _zoomMode, e.ZoomFactor, e.DirPath);
    }
    else if (eventName === Web2BackendMsgNames.SET_HTML) {
      await _boxEl.loadHtml(e.Html, _zoomMode, e.ZoomFactor, e.DirPath);
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


  // #region Comparison mode methods

  private static initComparisonEvents() {
    const layerEl = query('#layerComparison');
    const sliderEl = query('.comparison-slider', layerEl);

    // Slider drag events
    sliderEl.addEventListener('pointerdown', HapplaBoxViewer.onComparisonSliderPointerDown);
    document.addEventListener('pointermove', HapplaBoxViewer.onComparisonSliderPointerMove);
    document.addEventListener('pointerup', HapplaBoxViewer.onComparisonSliderPointerUp);

    // Zoom with mouse wheel
    layerEl.addEventListener('wheel', HapplaBoxViewer.onComparisonWheel, { passive: false });

    // Pan with mouse drag on images
    layerEl.addEventListener('pointerdown', HapplaBoxViewer.onComparisonPointerDown);

    layerEl.addEventListener('dragenter', HapplaBoxViewer.onComparisonDragEnter);
    layerEl.addEventListener('dragover', HapplaBoxViewer.onComparisonDragOver);
    layerEl.addEventListener('dragleave', HapplaBoxViewer.onComparisonDragLeave);
    layerEl.addEventListener('drop', HapplaBoxViewer.onComparisonDrop);

    // Update slider position on window resize (image-based positioning depends on container size)
    window.addEventListener('resize', () => {
      if (_comparisonMode) {
        HapplaBoxViewer.setComparisonSliderPosition(_comparisonSliderPos);
      }
    });

    document.addEventListener('keydown', HapplaBoxViewer.onComparisonKeyDown);
  }

  private static onComparisonKeyDown(e: KeyboardEvent) {
    if (!_comparisonMode) return;

    // Backspace: Reset slider to center (both horizontal and vertical)
    if (e.key === 'Backspace') {
      e.preventDefault();
      HapplaBoxViewer.setComparisonSliderPosition(0.5, 0.5);
      post(Web2FrontendMsgNames.ON_COMPARISON_SLIDER_CHANGED, {
        SliderPosition: 0.5,
      }, true);
    }

    // Backtick (`): Reset zoom and pan
    if (e.key === '`') {
      e.preventDefault();
      _comparisonZoom = 1;
      _comparisonPanX = 0;
      _comparisonPanY = 0;
      HapplaBoxViewer.updateComparisonTransform();
    }
  }

  private static onComparisonSliderPointerDown(e: PointerEvent) {
    e.preventDefault();
    e.stopPropagation();
    _isDraggingSlider = true;
    (e.target as HTMLElement).setPointerCapture(e.pointerId);
  }

  private static onComparisonSliderPointerMove(e: PointerEvent) {
    if (!_isDraggingSlider) return;
    e.preventDefault();

    const layerEl = query('#layerComparison');
    const rect = layerEl.getBoundingClientRect();
    const screenX = e.clientX - rect.left;
    const screenY = e.clientY - rect.top;

    // Convert screen X to image-space position (0-1)
    const newPosX = HapplaBoxViewer.screenXToSliderPosition(screenX);

    // Convert screen Y to handle position (0-1)
    const newPosY = HapplaBoxViewer.screenYToHandlePosition(screenY, rect.height);

    HapplaBoxViewer.setComparisonSliderPosition(newPosX, newPosY);

    // Notify C# of slider change
    post(Web2FrontendMsgNames.ON_COMPARISON_SLIDER_CHANGED, {
      SliderPosition: newPosX,
    }, true);
  }

  /**
   * Converts screen Y coordinate to handle position (0-1).
   */
  private static screenYToHandlePosition(screenY: number, containerHeight: number): number {
    const HANDLE_PADDING = 20;
    const usableHeight = containerHeight - 2 * HANDLE_PADDING;
    if (usableHeight <= 0) return 0.5;

    const pos = (screenY - HANDLE_PADDING) / usableHeight;
    return Math.max(0, Math.min(1, pos));
  }

  private static onComparisonSliderPointerUp(e: PointerEvent) {
    if (_isDraggingSlider) {
      _isDraggingSlider = false;
      (e.target as HTMLElement).releasePointerCapture?.(e.pointerId);
    }
  }

  private static onComparisonWheel(e: WheelEvent) {
    e.preventDefault();

    // If modifier key is pressed, forward to C# for handling (e.g., Alt+scroll = browse images)
    if (e.altKey || e.ctrlKey || e.shiftKey) {
      HapplaBoxViewer.onMouseWheel(e);
      return;
    }

    const delta = e.deltaY > 0 ? 0.9 : 1.1;
    _comparisonZoom = Math.max(0.1, Math.min(10, _comparisonZoom * delta));

    // Re-constrain pan after zoom change (zooming out may invalidate current pan)
    const constrained = HapplaBoxViewer.constrainComparisonPan(_comparisonPanX, _comparisonPanY);
    _comparisonPanX = constrained.panX;
    _comparisonPanY = constrained.panY;

    HapplaBoxViewer.updateComparisonTransform();
  }

  private static onComparisonPointerDown(e: PointerEvent) {
    // Only handle if not on slider
    if ((e.target as HTMLElement).closest('.comparison-slider')) return;

    // Store pan start state
    _isPanning = true;
    _panStartX = e.clientX;
    _panStartY = e.clientY;
    _panStartPanX = _comparisonPanX;
    _panStartPanY = _comparisonPanY;

    // Add listeners for this pan session
    document.addEventListener('pointermove', HapplaBoxViewer.onComparisonPanMove);
    document.addEventListener('pointerup', HapplaBoxViewer.onComparisonPanUp);
    document.addEventListener('pointercancel', HapplaBoxViewer.onComparisonPanUp);
  }

  private static onComparisonPanMove(e: PointerEvent) {
    if (!_isPanning) return;

    const dx = e.clientX - _panStartX;
    const dy = e.clientY - _panStartY;
    const hasDragged = Math.abs(dx) > _dragThreshold || Math.abs(dy) > _dragThreshold;

    if (hasDragged) {
      const newPanX = _panStartPanX + dx;
      const newPanY = _panStartPanY + dy;

      // Apply constrained panning
      const constrained = HapplaBoxViewer.constrainComparisonPan(newPanX, newPanY);
      _comparisonPanX = constrained.panX;
      _comparisonPanY = constrained.panY;
      HapplaBoxViewer.updateComparisonTransform();
    }
  }

  /**
   * Constrains pan values based on zoom level and image/container dimensions.
   * Only allows panning when the scaled image is larger than the container.
   */
  private static constrainComparisonPan(panX: number, panY: number): { panX: number, panY: number } {
    const layerEl = query('#layerComparison');
    const containerRect = layerEl.getBoundingClientRect();

    const scaledWidth = _comparisonImageWidth * _comparisonZoom;
    const scaledHeight = _comparisonImageHeight * _comparisonZoom;

    let constrainedPanX = panX;
    let constrainedPanY = panY;

    // Only allow horizontal panning if image is wider than container
    if (scaledWidth <= containerRect.width) {
      constrainedPanX = 0;
    } else {
      // Clamp so image edges don't go past container center
      const maxPanX = (scaledWidth - containerRect.width) / 2;
      constrainedPanX = Math.max(-maxPanX, Math.min(maxPanX, panX));
    }

    // Only allow vertical panning if image is taller than container
    if (scaledHeight <= containerRect.height) {
      constrainedPanY = 0;
    } else {
      // Clamp so image edges don't go past container center
      const maxPanY = (scaledHeight - containerRect.height) / 2;
      constrainedPanY = Math.max(-maxPanY, Math.min(maxPanY, panY));
    }

    return { panX: constrainedPanX, panY: constrainedPanY };
  }

  private static onComparisonPanUp(e: PointerEvent) {
    if (!_isPanning) return;
    _isPanning = false;

    // Remove listeners
    document.removeEventListener('pointermove', HapplaBoxViewer.onComparisonPanMove);
    document.removeEventListener('pointerup', HapplaBoxViewer.onComparisonPanUp);
    document.removeEventListener('pointercancel', HapplaBoxViewer.onComparisonPanUp);

    // Check if this was a click (not a drag)
    const dx = e.clientX - _panStartX;
    const dy = e.clientY - _panStartY;
    const wasDrag = Math.abs(dx) > _dragThreshold || Math.abs(dy) > _dragThreshold;

    // If it was a click (not a drag), move the slider to that position
    if (!wasDrag) {
      const layerEl = query('#layerComparison');
      const rect = layerEl.getBoundingClientRect();
      const clickX = e.clientX - rect.left;
      const newPos = HapplaBoxViewer.screenXToSliderPosition(clickX);
      HapplaBoxViewer.setComparisonSliderPosition(newPos);

      post(Web2FrontendMsgNames.ON_COMPARISON_SLIDER_CHANGED, {
        SliderPosition: newPos,
      }, true);
    }
  }

  private static onComparisonDragEnter(e: DragEvent) {
    e.preventDefault();
    e.stopPropagation();

    // Only show highlight if dragging files
    if (!e.dataTransfer?.types?.includes('Files')) return;

    const pane = HapplaBoxViewer.getDropTargetPane(e);
    HapplaBoxViewer.setDropHighlight(pane);
  }

  private static onComparisonDragOver(e: DragEvent) {
    e.preventDefault();
    e.stopPropagation();
    e.dataTransfer.dropEffect = 'copy';

    // Update highlight based on current position
    const pane = HapplaBoxViewer.getDropTargetPane(e);
    if (_dropHighlightPane !== pane) {
      HapplaBoxViewer.setDropHighlight(pane);
    }
  }

  private static onComparisonDragLeave(e: DragEvent) {
    e.preventDefault();
    e.stopPropagation();

    const layerEl = query('#layerComparison');
    const relatedTarget = e.relatedTarget as Node | null;

    // Check if we're leaving to outside the layer
    if (!relatedTarget || !layerEl.contains(relatedTarget)) {
      HapplaBoxViewer.clearDropHighlight();
    }
  }

  private static onComparisonDrop(e: DragEvent) {
    e.preventDefault();
    e.stopPropagation();

    // Clear highlight first
    HapplaBoxViewer.clearDropHighlight();

    const pane = HapplaBoxViewer.getDropTargetPane(e);

    if (e.dataTransfer?.files?.length > 0) {
      const message = {
        name: Web2FrontendMsgNames.ON_COMPARISON_PANE_DROP,
        data: JSON.stringify({ Pane: pane }),
      };

      console.info('🔵 Calling webview.postMessageWithAdditionalObjects(): ',
        Web2FrontendMsgNames.ON_COMPARISON_PANE_DROP, pane, e.dataTransfer.files);

      // @ts-ignore
      window.chrome.webview?.postMessageWithAdditionalObjects(message, e.dataTransfer.files);
    }
  }

  /**
   * Gets the target pane ('left' or 'right') for a drag event based on cursor position.
   */
  private static getDropTargetPane(e: DragEvent): 'left' | 'right' {
    const layerEl = query('#layerComparison');
    const rect = layerEl.getBoundingClientRect();
    const x = e.clientX - rect.left;
    const sliderX = HapplaBoxViewer.sliderPositionToScreenX(_comparisonSliderPos);
    return x < sliderX ? 'left' : 'right';
  }

  /**
   * Sets the drop highlight on the specified pane.
   */
  private static setDropHighlight(pane: 'left' | 'right') {
    _dropHighlightPane = pane;

    const layerEl = query('#layerComparison');
    const leftOverlay = query('.comparison-drop-overlay-left', layerEl);
    const rightOverlay = query('.comparison-drop-overlay-right', layerEl);
    const leftText = query('.comparison-drop-text', leftOverlay);
    const rightText = query('.comparison-drop-text', rightOverlay);

    // Update text content
    leftText.textContent = _dropToReplaceMainImageText;
    rightText.textContent = _dropToSetComparisonImageText;

    // Show/hide overlays
    leftOverlay.classList.toggle('is--visible', pane === 'left');
    rightOverlay.classList.toggle('is--visible', pane === 'right');

    // Update overlay positions based on slider
    HapplaBoxViewer.updateDropOverlayPositions();
  }

  /**
   * Clears all drop highlights.
   */
  private static clearDropHighlight() {
    _dropHighlightPane = null;

    const layerEl = query('#layerComparison');
    const leftOverlay = query('.comparison-drop-overlay-left', layerEl);
    const rightOverlay = query('.comparison-drop-overlay-right', layerEl);

    leftOverlay.classList.remove('is--visible');
    rightOverlay.classList.remove('is--visible');
  }

  /**
   * Updates drop overlay positions based on slider position.
   */
  private static updateDropOverlayPositions() {
    const layerEl = query('#layerComparison');
    const leftOverlay = query('.comparison-drop-overlay-left', layerEl);
    const rightOverlay = query('.comparison-drop-overlay-right', layerEl);
    const sliderX = HapplaBoxViewer.sliderPositionToScreenX(_comparisonSliderPos);
    const bounds = HapplaBoxViewer.getComparisonImageBounds();

    // Left overlay: from left edge to slider
    leftOverlay.style.left = '0';
    leftOverlay.style.width = `${sliderX}px`;

    // Right overlay: from slider to right edge
    rightOverlay.style.left = `${sliderX}px`;
    rightOverlay.style.width = `${bounds.containerWidth - sliderX}px`;
  }

  /**
   * Gets the image bounds in screen coordinates (accounting for zoom and pan).
   */
  private static getComparisonImageBounds() {
    const layerEl = query('#layerComparison');
    const containerRect = layerEl.getBoundingClientRect();

    const scaledWidth = _comparisonImageWidth * _comparisonZoom;
    const scaledHeight = _comparisonImageHeight * _comparisonZoom;

    // Image is centered in container, then offset by pan
    const imageX = (containerRect.width - scaledWidth) / 2 + _comparisonPanX;
    const imageY = (containerRect.height - scaledHeight) / 2 + _comparisonPanY;

    return {
      x: imageX,
      y: imageY,
      width: scaledWidth,
      height: scaledHeight,
      containerWidth: containerRect.width,
      containerHeight: containerRect.height,
    };
  }

  /**
   * Converts screen X coordinate to slider position (0-1 in image space).
   */
  private static screenXToSliderPosition(screenX: number): number {
    const bounds = HapplaBoxViewer.getComparisonImageBounds();
    if (bounds.width > 0) {
      const pos = (screenX - bounds.x) / bounds.width;
      return Math.max(0, Math.min(1, pos));
    }
    return 0.5;
  }

  /**
   * Converts slider position (0-1 in image space) to screen X coordinate.
   */
  private static sliderPositionToScreenX(pos: number): number {
    const bounds = HapplaBoxViewer.getComparisonImageBounds();
    if (bounds.width > 0) {
      return bounds.x + bounds.width * pos;
    }
    return bounds.containerWidth / 2;
  }

  private static setComparisonSliderPosition(pos: number, handleY?: number) {
    _comparisonSliderPos = pos;
    if (handleY !== undefined) {
      _comparisonSliderHandleY = handleY;
    }

    const layerEl = query('#layerComparison');
    const sliderEl = query('.comparison-slider', layerEl);
    const sliderHandleEl = query('.comparison-slider-handle', layerEl);
    const leftImageEl = query('.comparison-image-left', layerEl);
    const rightImageEl = query('.comparison-image-right', layerEl);

    const bounds = HapplaBoxViewer.getComparisonImageBounds();

    // Calculate slider position in screen coordinates (image-based)
    let sliderScreenX = HapplaBoxViewer.sliderPositionToScreenX(pos);

    // Out of bounds protection: clamp slider to stay within visible viewport
    // with padding so the handle is always grabbable
    const SLIDER_PADDING = 20;
    const minX = SLIDER_PADDING;
    const maxX = bounds.containerWidth - SLIDER_PADDING;
    const clampedSliderX = Math.max(minX, Math.min(maxX, sliderScreenX));

    sliderEl.style.left = `${clampedSliderX}px`;

    // Update handle vertical position
    const HANDLE_PADDING = 20;
    const usableHeight = bounds.containerHeight - 2 * HANDLE_PADDING;
    const handleScreenY = HANDLE_PADDING + usableHeight * _comparisonSliderHandleY;
    const clampedHandleY = Math.max(HANDLE_PADDING, Math.min(bounds.containerHeight - HANDLE_PADDING, handleScreenY));
    sliderHandleEl.style.top = `${clampedHandleY}px`;

    const clipLeft = sliderScreenX;
    const clipRight = bounds.containerWidth - sliderScreenX;
    // inset(top right bottom left)
    leftImageEl.style.clipPath = `inset(0 ${clipRight}px 0 0)`;
    rightImageEl.style.clipPath = `inset(0 0 0 ${clipLeft}px)`;
  }

  private static updateComparisonTransform() {
    const layerEl = query('#layerComparison');
    const leftEl = layerEl.querySelector('#compareImageLeft') as HTMLElement | SVGSVGElement;
    const rightEl = layerEl.querySelector('#compareImageRight') as HTMLElement | SVGSVGElement;

    const transform = `translate(${_comparisonPanX}px, ${_comparisonPanY}px) scale(${_comparisonZoom})`;
    if (leftEl) leftEl.style.transform = transform;
    if (rightEl) rightEl.style.transform = transform;

    // Update slider position since image bounds changed (image-based positioning)
    HapplaBoxViewer.setComparisonSliderPosition(_comparisonSliderPos);
  }

  private static onWeb2SetComparisonMode(_: Web2BackendMsgNames, e: {
    Enabled: boolean,
    AccentColor: number[],
    SliderHandleUrl: string,
  }) {
    _comparisonMode = e.Enabled;

    const layerEl = query('#layerComparison');
    layerEl.hidden = !e.Enabled;

    // Hide normal viewer when in comparison mode
    if (_boxEl) {
      _boxEl.hidden = e.Enabled;
      _boxEl.style.display = e.Enabled ? 'none' : '';
    }

    if (e.Enabled) {
      // Reset state
      _comparisonZoom = 1;
      _comparisonPanX = 0;
      _comparisonPanY = 0;
      _comparisonSliderPos = 0.5;
      _comparisonSliderHandleY = 0.5;
      HapplaBoxViewer.setComparisonSliderPosition(0.5, 0.5);
      HapplaBoxViewer.updateComparisonTransform();

      layerEl.focus();

      // Set accent color
      if (e.AccentColor) {
        const [r, g, b] = e.AccentColor;
        layerEl.style.setProperty('--accent-color', `rgb(${r}, ${g}, ${b})`);
      }

      // Set slider handle icon from theme (using background-image for security)
      const handleEl = query('.comparison-slider-handle', layerEl);
      if (e.SliderHandleUrl) {
        // Use CSS background-image instead of innerHTML for security
        handleEl.style.backgroundImage = `url("${e.SliderHandleUrl}")`;
        handleEl.style.backgroundSize = 'contain';
        handleEl.style.backgroundPosition = 'center';
        handleEl.style.backgroundRepeat = 'no-repeat';
        handleEl.style.backgroundColor = 'transparent';
        // Hide the default SVG content
        const svgEl = handleEl.querySelector('svg');
        if (svgEl) svgEl.style.display = 'none';
      } else {
        // Show default SVG arrows with accent color background
        handleEl.style.backgroundImage = '';
        handleEl.style.backgroundColor = '';
        const svgEl = handleEl.querySelector('svg');
        if (svgEl) svgEl.style.display = '';
      }
    }
  }

  /**
   * Gets dimensions from an SVG element (from attributes or viewBox).
   */
  private static getSvgDimensions(svg: SVGSVGElement): { width: number, height: number } {
    let width = parseFloat(svg.getAttribute('width') || '0');
    let height = parseFloat(svg.getAttribute('height') || '0');

    if (width === 0 || height === 0) {
      const viewBox = svg.getAttribute('viewBox');
      if (viewBox) {
        const parts = viewBox.split(/[\s,]+/);
        if (parts.length >= 4) {
          width = parseFloat(parts[2]) || 0;
          height = parseFloat(parts[3]) || 0;
        }
      }
    }

    return { width, height };
  }

  /**
   * Updates comparison image dimensions for image-based slider positioning.
   */
  private static updateComparisonImageDimensions() {
    const layerEl = query('#layerComparison');
    const leftEl = layerEl.querySelector('#compareImageLeft') as HTMLImageElement | SVGSVGElement;

    if (!leftEl) return;

    if (leftEl instanceof SVGSVGElement) {
      const dims = HapplaBoxViewer.getSvgDimensions(leftEl);
      _comparisonImageWidth = dims.width;
      _comparisonImageHeight = dims.height;
    } else if (leftEl instanceof HTMLImageElement) {
      _comparisonImageWidth = leftEl.naturalWidth || leftEl.width;
      _comparisonImageHeight = leftEl.naturalHeight || leftEl.height;
    }

    // Update slider position with new dimensions
    HapplaBoxViewer.setComparisonSliderPosition(_comparisonSliderPos);
  }

  private static onWeb2SetComparisonImages(_: Web2BackendMsgNames, e: {
    LeftImageUrl: string,
    LeftImageHtml: string,
    RightImageUrl: string,
    RightImageHtml: string,
  }) {
    const layerEl = query('#layerComparison');
    const leftContainer = query('.comparison-image-left', layerEl);
    const rightContainer = query('.comparison-image-right', layerEl);

    const configureSvg = (svg: SVGSVGElement, id: string) => {
      svg.id = id;
      svg.style.maxWidth = 'none';
      svg.style.maxHeight = 'none';
      svg.style.overflow = 'visible';

      // Ensure SVG has proper dimensions for scaling
      // If no width/height, use viewBox or set defaults
      if (!svg.hasAttribute('width') && !svg.hasAttribute('height')) {
        const viewBox = svg.getAttribute('viewBox');
        if (viewBox) {
          const parts = viewBox.split(/[\s,]+/);
          if (parts.length >= 4) {
            svg.setAttribute('width', parts[2]);
            svg.setAttribute('height', parts[3]);
          }
        }
      }
    };

    const configureImg = (img: HTMLImageElement, id: string) => {
      img.id = id;
      img.style.maxWidth = 'none';
      img.style.maxHeight = 'none';
      img.onload = () => HapplaBoxViewer.updateComparisonImageDimensions();
      // If already loaded (cached), update immediately
      if (img.complete && img.naturalWidth > 0) {
        HapplaBoxViewer.updateComparisonImageDimensions();
      }
    };

    // Handle left image (could be URL or HTML for SVG/raster)
    if (e.LeftImageHtml) {
      // HTML content - could be SVG or base64 img tag (trusted local content)
      leftContainer.innerHTML = e.LeftImageHtml;
      const svg = leftContainer.querySelector('svg') as SVGSVGElement;
      const img = leftContainer.querySelector('img') as HTMLImageElement;
      if (svg) {
        configureSvg(svg, 'compareImageLeft');
        HapplaBoxViewer.updateComparisonImageDimensions();
      } else if (img) {
        configureImg(img, 'compareImageLeft');
      }
    } else if (e.LeftImageUrl) {
      leftContainer.innerHTML = `<img id="compareImageLeft" src="${e.LeftImageUrl}" alt="Image A" />`;
      const img = leftContainer.querySelector('img') as HTMLImageElement;
      if (img) {
        configureImg(img, 'compareImageLeft');
      }
    }

    // Handle right image
    if (e.RightImageHtml) {
      rightContainer.innerHTML = e.RightImageHtml;
      const svg = rightContainer.querySelector('svg') as SVGSVGElement;
      const img = rightContainer.querySelector('img') as HTMLImageElement;
      if (svg) {
        configureSvg(svg, 'compareImageRight');
      } else if (img) {
        configureImg(img, 'compareImageRight');
      }
    } else if (e.RightImageUrl) {
      rightContainer.innerHTML = `<img id="compareImageRight" src="${e.RightImageUrl}" alt="Image B" />`;
      const img = rightContainer.querySelector('img') as HTMLImageElement;
      if (img) {
        configureImg(img, 'compareImageRight');
      }
    }

    // Reset transform when images change to avoid "stuck in previous cutout" issue
    _comparisonZoom = 1;
    _comparisonPanX = 0;
    _comparisonPanY = 0;

    HapplaBoxViewer.updateComparisonTransform();
  }

  private static onWeb2SetComparisonSlider(_: Web2BackendMsgNames, e: {
    Position: number,
  }) {
    HapplaBoxViewer.setComparisonSliderPosition(e.Position);
  }

  // #endregion
}
