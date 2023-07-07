
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
  
      queryAll(`[lang-text="${langKey}"]`, true).forEach(el => {
        el.innerText = langValue;
      });

      queryAll(`[lang-title="${langKey}"]`, true).forEach(el => {
        el.title = langValue;
      });

      queryAll(`[lang-html="${langKey}"]`, true).forEach(el => {
        let html = langValue;

        for (let i = 0; i < el.childElementCount; i++) {
          html = html.replaceAll(`{${i}}`, el.children.item(i).outerHTML);
        }

        el.innerHTML = html;
      });
    }
  }
}
