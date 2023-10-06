import { getChangedSettingsFromTab } from '@/helpers';

export default class TabGeneral {
  private static get isOriginalAutoUpdateEnabled() {
    return _pageSettings.config.AutoUpdate !== '0';
  }

  /**
   * Loads settings for tab General.
   */
  static loadSettings() {
    query('#Lnk_StartupDir').title = _pageSettings.startUpDir || '(unknown)';
    query('#Lnk_ConfigDir').title = _pageSettings.configDir || '(unknown)';
    query('#Lnk_UserConfigFile').title = _pageSettings.userConfigFilePath || '(unknown)';

    // AutoUpdate is a string
    query<HTMLInputElement>('[name="AutoUpdate"]').checked = TabGeneral.isOriginalAutoUpdateEnabled;

    // ImageInfoTags is a string array
    const imgTags = _pageSettings.config.ImageInfoTags as number[] || [];
    query<HTMLTextAreaElement>('[name="ImageInfoTags"]').value = imgTags.join('; ');
  }


  /**
   * Adds events for tab General.
   */
  static addEvents() {
    query('#Lnk_StartupDir').addEventListener('click', () => post('Lnk_StartupDir', _pageSettings.startUpDir), false);
    query('#Lnk_ConfigDir').addEventListener('click', () => post('Lnk_ConfigDir', _pageSettings.configDir), false);
    query('#Lnk_UserConfigFile').addEventListener('click', () => post('Lnk_UserConfigFile', _pageSettings.userConfigFilePath), false);

    query('[name="ImageInfoTags"]').addEventListener('blur', TabGeneral.onImageInfoTagsBlur, false);
    query('#LnkResetImageInfoTags').addEventListener('click', TabGeneral.resetImageInfoTags, false);
  }


  /**
   * Save settings as JSON object.
   */
  static exportSettings() {
    const settings = getChangedSettingsFromTab('general');

    // convert AutoUpdate back to string
    const isNewAutoUpdateEnabled = settings.AutoUpdate === true;
    if (isNewAutoUpdateEnabled !== TabGeneral.isOriginalAutoUpdateEnabled) {
      settings.AutoUpdate = settings.AutoUpdate
        ? new Date().toISOString()
        : '0';
    }
    else {
      delete settings.AutoUpdate;
    }


    // ImageInfoTags
    settings.ImageInfoTags = TabGeneral.getImageInfoTags();

    if (query<HTMLTextAreaElement>('[name="ImageInfoTags"]').checkValidity()) {
      const originalTagsString = _pageSettings.config.ImageInfoTags?.toString();
      const newTagsString = settings.ImageInfoTags?.toString();

      if (newTagsString === originalTagsString) {
        delete settings.ImageInfoTags;
      }
    }
    else {
      delete settings.ImageInfoTags;
    }

    return settings;
  }


  // reset image info tags to default
  private static resetImageInfoTags() {
    const el = query<HTMLTextAreaElement>('[name="ImageInfoTags"]');
    const defaultTags = _pageSettings.defaultImageInfoTags as string[] || [];

    el.value = defaultTags.join('; ');
  }


  // Handle when the ImageInfoTags box is blur.
  private static onImageInfoTagsBlur() {
    const el = query<HTMLTextAreaElement>('[name="ImageInfoTags"]');
    if (!el.checkValidity()) return;

    el.value = TabGeneral.getImageInfoTags().join('; ');
  }

  // gets image info tags
  private static getImageInfoTags() {
    const el = query<HTMLTextAreaElement>('[name="ImageInfoTags"]');
    const tags = el.value.split(';').map(i => i.trim()).filter(Boolean);

    return tags;
  }
}
