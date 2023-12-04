import { getChangedSettingsFromTab } from '@/helpers';

export default class TabImage {
  /**
   * Loads settings for tab Image.
   */
  static loadSettings() {
    const colorProfile = (_pageSettings.config.ColorProfile as string || '');
    if (colorProfile.includes('.')) {
      query<HTMLSelectElement>('[name="ColorProfile"]').value = 'Custom';
      query('#Lnk_CustomColorProfile').innerText = colorProfile;
    }

    TabImage.handleColorProfileChanged();
    TabImage.handleUseEmbeddedThumbnailOptionsChanged();
  }


  /**
   * Add events for tab Image.
   */
  static addEvents() {
    query('#Btn_BrowseColorProfile').addEventListener('click', async () => {
      const profileFilePath = await postAsync<string>('Btn_BrowseColorProfile');
      query('#Lnk_CustomColorProfile').innerText = profileFilePath;
    }, false);
  
    query('#Lnk_CustomColorProfile').addEventListener('click', () => {
      const profileFilePath = query('#Lnk_CustomColorProfile').innerText.trim();
      post('Lnk_CustomColorProfile', profileFilePath);
    }, false);
  
    query('[name="ColorProfile"]').addEventListener('change', TabImage.handleColorProfileChanged, false);

    query('[name="UseEmbeddedThumbnailRawFormats"]').addEventListener('input', TabImage.handleUseEmbeddedThumbnailOptionsChanged, false);
    query('[name="UseEmbeddedThumbnailOtherFormats"]').addEventListener('input', TabImage.handleUseEmbeddedThumbnailOptionsChanged, false);
  }


  /**
   * Save settings as JSON object.
   */
  static exportSettings() {
    const settings = getChangedSettingsFromTab('image');

    // ImageBoosterCacheCount
    settings.ImageBoosterCacheCount = +(settings.ImageBoosterCacheCount || 0);
    if (settings.ImageBoosterCacheCount === _pageSettings.config.ImageBoosterCacheCount) {
      delete settings.ImageBoosterCacheCount;
    }


    // ColorProfile
    const originalColorProfile = _pageSettings.config.ColorProfile;
    if (settings.ColorProfile === 'Custom') {
      settings.ColorProfile = query('#Lnk_CustomColorProfile').innerText.trim();
    }
    if (!settings.ColorProfile || settings.ColorProfile === originalColorProfile) {
      delete settings.ColorProfile;
    }

    return settings;
  }


  /**
   * Handles when color profile option is changed.
   */
  static handleColorProfileChanged() {
    const selectEl = query<HTMLSelectElement>('[name="ColorProfile"]');
    const useCustomProfile = selectEl.value === 'Custom';
  
    query('#Btn_BrowseColorProfile').hidden = !useCustomProfile;
    query('#Section_CustomColorProfile').hidden = !useCustomProfile;
    query('#Section_CurrentMonitorProfile').hidden = selectEl.value !== 'CurrentMonitorProfile';
  }


  /**
   * Handle when the embedded thumbnail options are changed.
   */
  static handleUseEmbeddedThumbnailOptionsChanged() {
    const enableForRaw = query<HTMLInputElement>('[name="UseEmbeddedThumbnailRawFormats"]').checked;
    const enableForOthers = query<HTMLInputElement>('[name="UseEmbeddedThumbnailOtherFormats"]').checked;
    const showSizeSection = enableForRaw || enableForOthers;
  
    query('#Section_EmbeddedThumbnailSize').hidden = !showSizeSection;
  }
}
