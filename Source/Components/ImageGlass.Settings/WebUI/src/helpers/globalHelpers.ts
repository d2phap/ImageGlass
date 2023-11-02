import { pause } from '.';
import { WebviewEventHandlerFn } from './webview';


/**
 * Gets the first matched element with the query selector.
 */
export const query = <T = HTMLElement>(
  selector: string,
  parentEl: HTMLElement = null,
  hideWarning = false,
): T | null => {
  try {
    const fromEl = parentEl ?? document;
    const el = fromEl.querySelector(selector) as T;

    if (!el && !hideWarning) {
      console.warn(`🟠 query() returns NULL with selector '${selector}'`);
    }

    return el;
  }
  catch {}

  return null;
};


/**
 * Gets all matched elements with the query selector.
 */
export const queryAll = <T = HTMLElement>(
  selector: string,
  parentEl: HTMLElement = null,
  hideWarning = false,
) => {
  try {
    const fromEl = parentEl ?? document;
    const els = Array.from(fromEl.querySelectorAll(selector)) as T[];

    if (els.length === 0 && !hideWarning) {
      console.info(`🔵 queryAll() returns ZERO elements with selector '${selector}'`);
    }

    return els;
  }
  catch {}

  return [];
};


/**
 * Add event listerner from backend.
 * @param name Event name
 * @param handler Function to handle the event
 */
export const on = (name: string, handler: WebviewEventHandlerFn) => {
  _webview.addEvent(name, handler);
};


/**
 * Send an event to backend.
 * @param name Event name.
 * @param data Data to send to backend, it will be converted to JSON string.
 * @param convertToJson Converts {@link data} to JSON. Default value is `false`.
 */
export const post = (name: string, data?: any, convertToJson = false) => {
  if (data instanceof FileList && !convertToJson) {
    console.info('🔵 Calling webview.postMessageWithAdditionalObjects(): ', name, data);

    // @ts-ignore
    window.chrome.webview?.postMessageWithAdditionalObjects({ name, data: null }, data);
    return;
  }

  const msgData = convertToJson ? JSON.stringify(data) : data;
  console.info('🔵 Calling webview.postMessage(): ', name, msgData);

  // @ts-ignore
  window.chrome.webview?.postMessage({ name, data: msgData });
};


/**
 * Send an event to backend and wait for the returned data.
 * @param name Event name
 * @param data Data to send to backend, it will be converted to JSON string.
 * @param convertToJson Converts {@link data} to JSON. Default value is `false`.
 */
export const postAsync = async <T = unknown>(name: string, data?: any, convertToJson = false) => {
  let hasResult = false;
  let result: T = null;

  on(name, (eventName, eventData) => {
    if (eventName !== name) return;

    hasResult = true;
    result = eventData;
    _webview.removeEvent(name);
  });

  post(name, data, convertToJson);

  // wait for the returned data
  while (!hasResult) {
    await pause(100);
  }

  return result;
};
