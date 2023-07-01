export default class TabImage {
    /**
     * Loads settings for tab Image.
     */
    static loadSettings(): void;
    /**
     * Add events for tab Image.
     */
    static addEvents(): void;
    /**
     * Save settings as JSON object.
     */
    static exportSettings(): Record<string, any>;
    /**
     * Handles when color profile option is changed.
     */
    static handleColorProfileChanged(): void;
    /**
     * Handle when the embedded thumbnail options are changed.
     */
    static handleUseEmbeddedThumbnailOptionsChanged(): void;
}
