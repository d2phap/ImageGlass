
/**
 * Loads language.
 */
export const loadLanguage = () => {
  for (const langKey in _pageSettings.lang) {
    if (!Object.prototype.hasOwnProperty.call(_pageSettings.lang, langKey)) {
      continue;
    }

    const langValue = _pageSettings.lang[langKey];

    const els = queryAll(`[data-lang="${langKey}"]`);
    for (const el of els) {
      el.innerText = langValue;
    }
  }
};
