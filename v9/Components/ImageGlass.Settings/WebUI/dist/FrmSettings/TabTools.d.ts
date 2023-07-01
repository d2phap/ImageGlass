export default class TabTools {
    private static HOTKEY_SEPARATOR;
    /**
     * Loads settings for tab Tools.
     */
    static loadSettings(): void;
    /**
     * Adds events for tab Tools.
     */
    static addEvents(): void;
    /**
     * Save settings as JSON object.
     */
    static exportSettings(): Record<string, any>;
    /**
     * Loads tool list but do not update `_pageSettings.toolList`.
     */
    private static loadToolList;
    /**
     * Gets tool list from DOM.
     */
    private static getToolListFromDom;
    /**
     * Open modal dialog to edit the tool.
     * @param toolId Tool id
     */
    private static editTool;
    /**
     * Gets tool data from the modal dialog.
     */
    private static getToolDialogFormData;
    /**
     * Sets the tool item to the list.
     * @param oldToolId If not found, the tool will be inserted into the list.
     */
    private static setToolItemToList;
    private static addEventsForToolDialog;
    private static updateToolCommandPreview;
    private static handleBtnBrowseToolClickEvent;
}
