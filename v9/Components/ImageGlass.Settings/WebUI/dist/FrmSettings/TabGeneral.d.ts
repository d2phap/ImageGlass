export default class TabGeneral {
    private static get isOriginalAutoUpdateEnabled();
    /**
     * Loads settings for tab General.
     */
    static loadSettings(): void;
    /**
     * Adds events for tab General.
     */
    static addEvents(): void;
    /**
     * Save settings as JSON object.
     */
    static exportSettings(): Record<string, any>;
}
