
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
