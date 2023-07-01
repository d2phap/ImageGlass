/**
 * Pauses the thread for a period
 * @param time Duration to pause in millisecond
 * @param data Data to return after resuming
 * @returns a promise
 */
export declare const pause: <T>(time: number, data?: T) => Promise<T>;
/**
 * Gets the settings that are changed by user (`_pageSettings.config`) from the input tab.
 */
export declare const getChangedSettingsFromTab: (tab: string) => Record<string, any>;
/**
 * Escapes HTML characters.
 */
export declare const escapeHtml: (html: string) => string;
/**
 * Opens modal dialog and return value.
 * @param selector Dialog selector.
 * @param data The data to pass to the dialog.
 */
export declare const openModalDialog: (selector: string, purpose: 'create' | 'edit', data?: Record<string, any>, onOpen?: (el: HTMLDialogElement) => any, onSubmit?: (e: SubmitEvent) => any) => Promise<HTMLDialogElement>;
/**
 * Open file picker.
 */
export declare const openFilePicker: (options?: {
    multiple?: boolean;
    filter?: string;
}) => Promise<string[]>;
/**
 * Open hotkey picker.
 */
export declare const openHotkeyPicker: () => Promise<string | null>;
/**
 * Renders hotkey list
 * @param ulSelector CSS selector of the list element
 * @param hotkeys Hotkey list to render
 */
export declare const renderHotkeyList: (ulSelector: string, hotkeys: string[], onChange?: (action: 'delete' | 'add') => any) => Promise<void>;
