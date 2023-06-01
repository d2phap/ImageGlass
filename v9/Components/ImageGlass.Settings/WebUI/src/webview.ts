
export type WebviewEventHandlerFn = (name: string, data?: any) => void;

export class Webview {
  public eventHandlers: Record<string, WebviewEventHandlerFn> = {};

  addEvent(name: string, handler: WebviewEventHandlerFn) {
    this.eventHandlers[name] = handler;
  }

  removeEvent(name: string) {
    delete this.eventHandlers[name];
  }

  startListening() {
    // @ts-ignore
    window.chrome.webview?.addEventListener('message', ({ data }) => {
      const eName = data?.Name ?? '';
      const eData = data?.Data ?? '';
      const handler = this.eventHandlers[eName];
      const hasHandler = !!handler;

      console.info(`Received event '${eName}' (handler=${hasHandler}) with data:`, eData);

      if (!hasHandler) return;
      handler(eName, eData);
    });
  }
}
