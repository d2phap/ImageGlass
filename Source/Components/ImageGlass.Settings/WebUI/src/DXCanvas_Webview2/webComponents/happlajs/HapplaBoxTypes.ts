import { DOMPadding } from './DOMPadding';

export type IMouseEventArgs = {
  Dpi: number;
  Button: number;
  X: number;
  Y: number;
  Delta: number;
  NavigationButton?: 'left' | 'right' | '';
};

export type IZoomEventArgs = {
  zoomFactor: number,
  x: number,
  y: number,
  isManualZoom: boolean,
  isZoomModeChanged: boolean,
};

export type ZoomEventFunction = (e: IZoomEventArgs) => void;
export type TransformEventFunction = (matrix: DOMMatrix) => void;
export type PanEventFunction = (x: number, y: number) => void;
export type PanDirection = 'left' | 'right' | 'up' | 'down';

export enum InterpolationMode {
  Pixelated = 'pixelated',
  Auto = 'auto',
  CrispEdges = 'auto', // 'crisp-edges',
}

export enum ZoomMode {
  AutoZoom = 'AutoZoom',
  LockZoom = 'LockZoom',
  ScaleToWidth = 'ScaleToWidth',
  ScaleToHeight = 'ScaleToHeight',
  ScaleToFit = 'ScaleToFit',
  ScaleToFill = 'ScaleToFill',
}

export type ILoadContentRequestedEventArgs = {
  ZoomMode: ZoomMode,
  ZoomFactor: number,
  Html?: string,
  Url?: string,
};


export interface IHapplaBoxOptions {
  allowZoom?: boolean;
  zoomFactor?: number;
  minZoom?: number;
  maxZoom?: number;

  allowPan?: boolean;
  panOffset?: DOMPoint;

  imageRendering?: InterpolationMode;
  scaleRatio?: number;
  padding?: DOMPadding,

  onBeforeContentReady?: () => void;
  onContentReady?: () => void;
  onResizing?: () => void;
  onMouseWheel?: (e: WheelEvent) => void;

  onBeforeZoomChanged?: ZoomEventFunction;
  onAfterZoomChanged?: ZoomEventFunction;
  onAfterTransformed?: TransformEventFunction;

  onPanning?: PanEventFunction;
  onAfterPanned?: PanEventFunction;
}
