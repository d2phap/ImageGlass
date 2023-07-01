import { ILanguage } from '@/@types/FrmSettings';
export default class TabLanguage {
    /**
     * Loads settings for tab Language.
     */
    static loadSettings(): void;
    /**
     * Adds events for tab Language.
     */
    static addEvents(): void;
    private static onBtn_RefreshLanguageList;
    private static onLnk_InstallLanguage;
    /**
     * Save settings as JSON object.
     */
    static exportSettings(): Record<string, any>;
    /**
     * Handles when language is changed.
     */
    private static handleLanguageChanged;
    /**
     * Loads language list to select box.
     * @param list If defined, it overrides `_pageSettings.langList`.
     */
    static loadLanguageList(list?: ILanguage[]): void;
}
