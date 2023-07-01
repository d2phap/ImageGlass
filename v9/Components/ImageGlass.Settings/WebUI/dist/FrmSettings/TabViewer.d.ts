export default class TabViewer {
    /**
     * Loads settings for tab Viewer.
     */
    static loadSettings(): void;
    /**
     * Adds events for tab Viewer.
     */
    static addEvents(): void;
    /**
     * Save settings as JSON object.
     */
    static exportSettings(): Record<string, any>;
    /**
     * Handle when ZoomLevels is changed.
     */
    private static handleZoomLevelsChanged;
    /**
     * Handle when the ZoomLevels box is blur.
     */
    private static handleZoomLevelsBlur;
    /**
     * Gets zoom levels
     */
    private static getZoomLevels;
}
