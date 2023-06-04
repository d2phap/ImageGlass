import { getChangedSettingsFromTab } from '@/helpers';

export default class TabAppearance {
  /**
   * Loads settings for tab Appearance.
   */
  static loadSettings() {
    TabAppearance.loadThemeList();

    if (_pageSettings.themeList.length > 0) {
      query<HTMLInputElement>(`[name="DarkTheme"][value="${_pageSettings.config.DarkTheme}"]`).checked = true;
      query<HTMLInputElement>(`[name="LightTheme"][value="${_pageSettings.config.LightTheme}"]`).checked = true;
    }
  }


  /**
   * Adds events for tab Appearance.
   */
  static addEvents() {
    query('#Lnk_ResetBackgroundColor').addEventListener('click', TabAppearance.resetBackgroundColor, false);
    query('#Lnk_ResetSlideshowBackgroundColor').addEventListener('click', TabAppearance.resetSlideshowBackgroundColor, false);
  }


  /**
   * Save settings as JSON object.
   */
  static exportSettings() {
    const settings = getChangedSettingsFromTab('appearance');

    // DarkTheme
    settings.DarkTheme = query<HTMLInputElement>('[name="DarkTheme"]:checked').value;
    if (settings.DarkTheme === _pageSettings.config.DarkTheme) {
      delete settings.DarkTheme;
    }

    // LightTheme
    settings.LightTheme = query<HTMLInputElement>('[name="LightTheme"]:checked').value;
    if (settings.LightTheme === _pageSettings.config.LightTheme) {
      delete settings.LightTheme;
    }

    return settings;
  }


  /**
   * Loads all themes into the list.
   */
  private static loadThemeList() {
    const themeList = _pageSettings.themeList || [];

    const ulEl = query<HTMLTableElement>('#List_ThemeList');
    let ulHtml = '';

    for (const th of themeList) {
      const liHtml = `
        <li>
          <div class="theme-item">
            <div class="theme-preview">
              <img src="${th.PreviewImage}" alt="${th.Info.Name}" onerror="this.hidden = true;" />
            </div>
            <div class="theme-info">
              <div class="theme-heading">
                <div class="theme-title">
                  <span class="theme-name">${th.Info.Name}</span>
                  <span class="theme-version">${th.Info.Version}</span>
                  <span class="theme-mode ${th.IsDarkMode ? 'theme-dark' : 'theme-light'}">
                    ${th.IsDarkMode ? '🌙' : '☀️'}
                  </span>
                </div>
                <div class="theme-actions">
                  <label>
                    <input type="radio" name="DarkTheme" value="${th.FolderName}" />
                    <span>
                      <span>🌙</span>
                      <span data-lang="FrmSettings.Tab.Appearance._DarkTheme">[Dark]</span> 
                    </span>
                  </label>
                  <label>
                    <input type="radio" name="LightTheme" value="${th.FolderName}" />
                    <span>
                      <span>☀️</span>
                      <span data-lang="FrmSettings.Tab.Appearance._LightTheme">[Light]</span>
                    </span>
                  </label>
                </div>
              </div>
              <div class="theme-description" title="${th.Info.Description}">${th.Info.Description}</div>
              <div class="theme-location" title="${th.FolderPath}">${th.FolderPath}</div>
              <div class="theme-author">
                <span class="me-4">
                  <span data-lang="FrmSettings.Tab.Appearance._Author">[Author]</span>:
                  ${th.Info.Author || '?'}
                </span>
                <span class="me-4">
                  <span data-lang="_._Website">[Website]</span>:
                  ${th.Info.Website || '?'}
                </span>
                <span>
                  <span data-lang="_._Email">[Email]</span>:
                  ${th.Info.Email || '?'}
                </span>
              </div>
            </div>
          </div>
        </li>`;

      ulHtml += liHtml;
    }

    ulEl.innerHTML = ulHtml;
  }


  /**
   * Resets the background color to the current theme's background color.
   */
  private static resetBackgroundColor() {
    const isDarkMode = document.documentElement.getAttribute('color-mode') !== 'light';
    const currentThemeName = isDarkMode ? _pageSettings.config.DarkTheme : _pageSettings.config.LightTheme;
    const theme = _pageSettings.themeList.find(i => i.FolderName === currentThemeName);
    if (!theme) return;

    const colorHex = theme.BgColor || '#00000000';

    // remove alpha
    query<HTMLInputElement>('[name="BackgroundColor"]').value = colorHex.substring(0, colorHex.length - 2);
  }


  /**
   * Reset slideshow background color to black
   */
  private static resetSlideshowBackgroundColor() {
    query<HTMLInputElement>('[name="SlideshowBackgroundColor"]').value = '#000000';
  }
}
