export default class Settings {
    /**
     * Loads settings.
     */
    static load(): void;
    /**
     * Adds events for the footer of setting.
     */
    static addEventsForFooter(): void;
    /**
     * Gets all settings as an object.
     */
    static getAllSettings(): Record<string, any>;
    /**
     * Updates the `_pageSettings.config`.
     */
    static updateInitSettings(newSettings: Record<string, any>): void;
    /**
     * Loads select box items.
     */
    private static loadSelectBoxEnums;
}
