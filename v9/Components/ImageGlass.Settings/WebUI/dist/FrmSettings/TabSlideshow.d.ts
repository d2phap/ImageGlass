export default class TabSlideshow {
    /**
     * Loads settings for tab Slideshow.
     */
    static loadSettings(): void;
    /**
     * Adds events for tab Slideshow.
     */
    static addEvents(): void;
    /**
     * Save settings as JSON object.
     */
    static exportSettings(): Record<string, any>;
    /**
     * Handle when slideshow intervals are changed.
     */
    private static handleSlideshowIntervalsChanged;
    /**
     * handle when `UseRandomIntervalForSlideshow` is changed.
     */
    private static handleUseRandomIntervalForSlideshowChanged;
    /**
     * Formats total seconds to time format: `mm:ss.fff`.
     */
    private static toTimeString;
}
