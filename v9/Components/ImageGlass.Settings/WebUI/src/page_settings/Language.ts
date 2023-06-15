
export default class Language {
  /**
   * Loads language.
   */
  static load() {
    for (const langKey in _pageSettings.lang) {
      if (!Object.prototype.hasOwnProperty.call(_pageSettings.lang, langKey)) {
        continue;
      }
  
      const langValue = _pageSettings.lang[langKey];
  
      queryAll(`[lang-text="${langKey}"]`).forEach(el => {
        el.innerText = langValue;
      });

      queryAll(`[lang-title="${langKey}"]`).forEach(el => {
        el.title = langValue;
      });
    }
  }
}
