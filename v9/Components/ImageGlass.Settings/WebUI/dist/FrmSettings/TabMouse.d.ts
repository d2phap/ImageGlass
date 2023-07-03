export default class TabMouse {
    /**
     * Loads settings for tab Mouse.
     */
    static loadSettings(): void;
    /**
     * Adds events for tab Mouse.
     */
    static addEvents(): void;
    /**
     * Save settings as JSON object.
     */
    static exportSettings(): Record<string, any>;
    /**
     * Resets the mouse wheel actions to the default settings.
     */
    private static resetDefaultMouseWheelActions;
}
