export default class TabLayout {
    private static get defaultLayout();
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
    private static loadLayoutConfigs;
    private static loadLayoutMapDOM;
    private static convertRawLayoutToLayoutObject;
    private static getLayoutSettingObjectFromDOM;
    private static handleLayoutInputsChanged;
    private static handleLayoutItemDragStart;
    private static handleLayoutItemDragOver;
    private static handleLayoutItemDragEnter;
    private static handleLayoutItemDragLeave;
    private static handleLayoutItemDragEnd;
    private static handleLayoutItemDrop;
}
