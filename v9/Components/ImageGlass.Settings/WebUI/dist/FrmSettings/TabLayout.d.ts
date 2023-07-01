export default class TabLayout {
    /**
     * Loads settings for tab Layout.
     */
    static loadSettings(): void;
    /**
     * Adds events for tab Layout.
     */
    static addEvents(): void;
    /**
     * Save settings as JSON object.
     */
    static exportSettings(): Record<string, any>;
}
