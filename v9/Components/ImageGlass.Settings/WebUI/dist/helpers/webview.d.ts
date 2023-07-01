export type WebviewEventHandlerFn = (name: string, data?: any) => void;
export declare class Webview {
    eventHandlers: Record<string, WebviewEventHandlerFn>;
    addEvent(name: string, handler: WebviewEventHandlerFn): void;
    removeEvent(name: string): void;
    startListening(): void;
}
