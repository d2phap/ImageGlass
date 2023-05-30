
/**
 * Gets the first matched element with the query selector.
 */
export const query = (selector: string): HTMLElement | null => {
  try {
    return document.querySelector(selector) as HTMLElement;
  }
  catch {}

  return null;
};


/**
 * Gets all matched elements with the query selector.
 */
export const queryAll = (selector: string) => {
  try {
    return Array.from(document.querySelectorAll(selector)) as HTMLElement[];
  }
  catch {}

  return [];
};
