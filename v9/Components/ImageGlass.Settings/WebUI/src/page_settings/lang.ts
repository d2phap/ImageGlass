
/**
 * Loads language.
 */
export const loadLanguage = () => {
  for (const langKey in window._pageSettings.lang) {
    if (Object.prototype.hasOwnProperty.call(window._pageSettings.lang, langKey)) {
      const langValue = window._pageSettings.lang[langKey];

      const els = Array.from<HTMLElement>(document.querySelectorAll(`[data-lang="${langKey}"]`));
      for (const el of els) {
        el.innerText = langValue;
      }
    }
  }
};
