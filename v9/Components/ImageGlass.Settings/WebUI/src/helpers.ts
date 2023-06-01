
/**
 * Gets the first matched element with the query selector.
 */
export const query = <T = HTMLElement>(selector: string): T | null => {
  try {
    return document.querySelector(selector) as T;
  }
  catch {}

  return null;
};


/**
 * Gets all matched elements with the query selector.
 */
export const queryAll = <T = HTMLElement>(selector: string) => {
  try {
    return Array.from(document.querySelectorAll(selector)) as T[];
  }
  catch {}

  return [];
};


/**
 * Send an event to backend.
 * @param name Event name
 * @param data Data to send to backend
 */
export const post = (name: string, data?: any) => {
  // @ts-ignore
  window.chrome.webview?.postMessage({ name, data });
};


/**
 * Add event listerner from backend.
 * @param name Event name
 * @param handler Function to handle the event
 */
export const on = (name: string, handler: (name: string, data?: any) => void) => {
  // @ts-ignore
  window.chrome.webview?.addEventListener(name, ({ data }) => handler(data.name, data.data));
};

