export default class TabAppearance {
    /**
     * Loads settings for tab Appearance.
     */
    static loadSettings(): void;
    /**
     * Loads theme list check status
     */
    static loadThemeListStatus(): void;
    /**
     * Adds events for tab Appearance.
     */
    static addEvents(): void;
    /**
     * Save settings as JSON object.
     */
    static exportSettings(): Record<string, any>;
    /**
     * Updates `_pageSettings.config.BackgroundColor` value and load UI.
     */
    static loadBackgroundColorConfig(hexColor: string): void;
    /**
     * Loads all themes into the list.
     */
    private static loadThemeList;
    /**
     * Resets the background color to the current theme's background color.
     */
    private static resetBackgroundColor;
    /**
     * Reset slideshow background color to black
     */
    private static resetSlideshowBackgroundColor;
    /**
     * Handles when `BackgroundColor` is changed.
     */
    private static handleBackgroundColorChanged;
    /**
     * Handles when `SlideshowBackgroundColor` is changed.
     */
    private static handleSlideshowBackgroundColorChanged;
}
