export default class TabKeyboard {
    /**
     * Loads settings for tab Keyboard.
     */
    static loadSettings(): void;
    /**
     * Adds events for tab Keyboard.
     */
    static addEvents(): void;
    /**
     * Save settings as JSON object.
     */
    static exportSettings(): Record<string, any>;
}
