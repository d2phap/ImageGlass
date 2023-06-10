import { getChangedSettingsFromTab } from '@/helpers';

export default class TabAppearance {
  /**
   * Loads settings for tab Appearance.
   */
  static loadSettings() {
    TabAppearance.loadThemeList();
    TabAppearance.loadThemeListStatus();
    TabAppearance.handleBackgroundColorChanged();
    TabAppearance.handleSlideshowBackgroundColorChanged();
  }


  /**
   * Loads theme list check status
   */
  static loadThemeListStatus() {
    const darkTheme = query<HTMLInputElement>('[name="DarkTheme"]').value;
    const lightTheme = query<HTMLInputElement>('[name="LightTheme"]').value;

    const darkEl = query<HTMLInputElement>(`[name="_DarkThemeOptions"][value="${darkTheme}"]`);
    const lightEl = query<HTMLInputElement>(`[name="_LightThemeOptions"][value="${lightTheme}"]`);
    if (darkEl) darkEl.checked = true;
    if (lightEl) lightEl.checked = true;
  }


  /**
   * Adds events for tab Appearance.
   */
  static addEvents() {
    query('#Lnk_ResetBackgroundColor').addEventListener('click', TabAppearance.resetBackgroundColor, false);
    query('#Lnk_ResetSlideshowBackgroundColor').addEventListener('click', TabAppearance.resetSlideshowBackgroundColor, false);

    query('#Btn_BackgroundColor').addEventListener('click', async () => {
      const colorEL = query<HTMLInputElement>('[name="BackgroundColor"]');
      const colorValue = await postAsync<string>('Btn_BackgroundColor', colorEL.value);

      if (colorValue) {
        colorEL.value = colorValue;
        TabAppearance.handleBackgroundColorChanged();
      }
    }, false);
    query('#Btn_SlideshowBackgroundColor').addEventListener('click', async () => {
      const colorEL = query<HTMLInputElement>('[name="SlideshowBackgroundColor"]');
      const colorValue = await postAsync<string>('Btn_SlideshowBackgroundColor', colorEL.value);

      if (colorValue) {
        colorEL.value = colorValue;
        TabAppearance.handleSlideshowBackgroundColorChanged();
      }
    }, false);
  }


  /**
   * Save settings as JSON object.
   */
  static exportSettings() {
    const settings = getChangedSettingsFromTab('appearance');

    // DarkTheme
    settings.DarkTheme = query<HTMLInputElement>('[name="DarkTheme"]').value;
    if (settings.DarkTheme === _pageSettings.config.DarkTheme) {
      delete settings.DarkTheme;
    }

    // LightTheme
    settings.LightTheme = query<HTMLInputElement>('[name="LightTheme"]').value;
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
                    <input type="radio" name="_DarkThemeOptions" value="${th.FolderName}" />
                    <span>
                      <span>🌙</span>
                      <span data-lang="FrmSettings.Tab.Appearance._DarkTheme">[Dark]</span> 
                    </span>
                  </label>
                  <label>
                    <input type="radio" name="_LightThemeOptions" value="${th.FolderName}" />
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

    queryAll<HTMLInputElement>('[name="_DarkThemeOptions"]').forEach(el => {
      el.addEventListener('change', (e) => {
        const themeName = (e.target as HTMLInputElement).value;
        query<HTMLInputElement>('[name="DarkTheme"]').value = themeName;
      });
    });

    queryAll<HTMLInputElement>('[name="_LightThemeOptions"]').forEach(el => {
      el.addEventListener('change', (e) => {
        const themeName = (e.target as HTMLInputElement).value;
        query<HTMLInputElement>('[name="LightTheme"]').value = themeName;
      });
    });
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

    query<HTMLInputElement>('[name="BackgroundColor"]').value = colorHex;
    TabAppearance.handleBackgroundColorChanged();
  }


  /**
   * Reset slideshow background color to black
   */
  private static resetSlideshowBackgroundColor() {
    query<HTMLInputElement>('[name="SlideshowBackgroundColor"]').value = '#000000';
    TabAppearance.handleSlideshowBackgroundColorChanged();
  }


  /**
   * Handles when `BackgroundColor` is changed.
   */
  private static handleBackgroundColorChanged() {
    const colorHex = query<HTMLInputElement>('[name="BackgroundColor"]').value;
    if (!colorHex) return;

    query<HTMLInputElement>('#Btn_BackgroundColor > .color-display').style.setProperty('--color-picker-value', colorHex);
    query('#Lbl_BackgroundColorValue').innerText = colorHex;
  }


  /**
   * Handles when `SlideshowBackgroundColor` is changed.
   */
  private static handleSlideshowBackgroundColorChanged() {
    const colorHex = query<HTMLInputElement>('[name="SlideshowBackgroundColor"]').value;
    if (!colorHex) return;

    query<HTMLInputElement>('#Btn_SlideshowBackgroundColor > .color-display').style.setProperty('--color-picker-value', colorHex);
    query('#Lbl_SlideshowBackgroundColorValue').innerText = colorHex;
  }
}