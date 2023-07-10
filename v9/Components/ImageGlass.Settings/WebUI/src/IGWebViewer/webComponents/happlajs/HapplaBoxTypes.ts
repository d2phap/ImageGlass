
export type IRect = {
  left: number,
  right: number,
  top: number,
  bottom: number,
};
export type IPadding = {
  left?: number,
  right?: number,
  top?: number,
  bottom?: number,
};

export type ZoomEventFunction = (zoomFactor: number, x: number, y: number) => void;
export type TransformEventFunction = (matrix: DOMMatrix) => void;
export type PanEventFunction = (x: number, y: number) => void;

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

export interface IHapplaBoxOptions {
  allowZoom?: boolean;
  zoomFactor?: number;
  minZoom?: number;
  maxZoom?: number;

  allowPan?: boolean;
  panOffset?: { x: number, y: number };

  imageRendering?: InterpolationMode;
  scaleRatio?: number;
  padding?: IPadding,

  onBeforeContentReady?: () => void;
  onContentReady?: () => void;

  onBeforeZoomChanged?: ZoomEventFunction;
  onAfterZoomChanged?: ZoomEventFunction;
  onAfterTransformed?: TransformEventFunction;

  onPanning?: PanEventFunction;
  onAfterPanned?: PanEventFunction;
}

export interface ViewportOptions extends IHapplaBoxOptions {
  onResizing?: () => void;
}
