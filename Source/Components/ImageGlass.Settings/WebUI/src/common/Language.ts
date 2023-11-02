
export default class Language {
  /**
   * Loads language for all elements.
   */
  static load() {
    for (const langKey in _page.lang) {
      if (!Object.prototype.hasOwnProperty.call(_page.lang, langKey)) {
        continue;
      }
  
      const langValue = _page.lang[langKey];
  
      queryAll(`[lang-text="${langKey}"]`, null, true).forEach(el => {
        el.innerText = langValue;
      });

      queryAll(`[lang-title="${langKey}"]`, null, true).forEach(el => {
        el.title = langValue;
      });

      queryAll(`[lang-html="${langKey}"]`, null, true).forEach(el => {
        let html = langValue;

        for (let i = 0; i < el.childElementCount; i++) {
          html = html.replaceAll(`{${i}}`, el.children.item(i).outerHTML);
        }

        el.innerHTML = html;
      });
    }
  }


  /**
   * Loads language for the given element.
   */
  static loadForEl(parentEl: HTMLElement) {
    // lang text
    const langTextEls = queryAll('[lang-text]', parentEl);
    [parentEl, ...langTextEls].forEach(el => {
      const langKey = el.getAttribute('lang-text') || '';
      const langValue = _page.lang[langKey] || '';
      if (langValue) {
        el.innerText = langValue;
      }
    });

    // lang title
    const langTitleEls = queryAll('[lang-title]', parentEl);
    [parentEl, ...langTitleEls].forEach(el => {
      const langKey = el.getAttribute('lang-title') || '';
      const langValue = _page.lang[langKey] || '';
      if (langValue) {
        el.title = langValue;
      }
    });

    // lang html
    const langHtmlEls = queryAll('[lang-html]', parentEl);
    [parentEl, ...langHtmlEls].forEach(el => {
      const langKey = el.getAttribute('lang-html') || '';
      const langValue = _page.lang[langKey] || '';
      if (langValue) {
        let html = langValue;

        for (let i = 0; i < el.childElementCount; i++) {
          html = html.replaceAll(`{${i}}`, el.children.item(i).outerHTML);
        }

        el.innerHTML = html;
      }
    });
  }


  /**
   * Loads language for the given element.
   */
  static loadFor(selector: string) {
    const parentEl = query(selector);
    Language.loadForEl(parentEl);
  }
}
