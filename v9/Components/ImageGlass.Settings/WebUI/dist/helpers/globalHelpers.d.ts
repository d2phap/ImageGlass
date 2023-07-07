import { WebviewEventHandlerFn } from './webview';
/**
 * Gets the first matched element with the query selector.
 */
export declare const query: <T = HTMLElement>(selector: string, hideWarning?: boolean) => T;
/**
 * Gets all matched elements with the query selector.
 */
export declare const queryAll: <T = HTMLElement>(selector: string, hideWarning?: boolean) => T[];
/**
 * Add event listerner from backend.
 * @param name Event name
 * @param handler Function to handle the event
 */
export declare const on: (name: string, handler: WebviewEventHandlerFn) => void;
/**
 * Send an event to backend.
 * @param name Event name
 * @param data Data to send to backend, it will be converted to JSON string.
 */
export declare const post: (name: string, data?: any, autoConvertToJson?: boolean) => void;
/**
 * Send an event to backend and wait for the returned data.
 * @param name Event name
 * @param data Data to send to backend, it will be converted to JSON string.
 */
export declare const postAsync: <T = unknown>(name: string, data?: any, autoConvertToJson?: boolean) => Promise<T>;
