import { getChangedSettingsFromTab } from '@/helpers';

export default class TabAppearance {
  /**
   * Loads settings for tab Appearance.
   */
  static loadSettings() {
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
    return getChangedSettingsFromTab('appearance');
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
