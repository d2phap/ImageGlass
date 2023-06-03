import { getChangedSettingsFromTab } from '@/helpers';

export default class TabGeneral {
  private static get isOriginalAutoUpdateEnabled() {
    return _pageSettings.config.AutoUpdate !== '0';
  }

  /**
   * Loads settings for tab General.
   */
  static loadSettings() {
    query('#Lnk_StartupDir').innerText = _pageSettings.startUpDir || '(unknown)';
    query('#Lnk_ConfigDir').innerText = _pageSettings.configDir || '(unknown)';
    query('#Lnk_UserConfigFile').innerText = _pageSettings.userConfigFilePath || '(unknown)';

    // AutoUpdate is a string
    query<HTMLInputElement>('[name="AutoUpdate"]').checked = TabGeneral.isOriginalAutoUpdateEnabled;
  }


  /**
   * Adds events for tab General.
   */
  static addEvents() {
    query('#Lnk_StartupDir').addEventListener('click', () => post('Lnk_StartupDir', _pageSettings.startUpDir), false);
    query('#Lnk_ConfigDir').addEventListener('click', () => post('Lnk_ConfigDir', _pageSettings.configDir), false);
    query('#Lnk_UserConfigFile').addEventListener('click', () => post('Lnk_UserConfigFile', _pageSettings.userConfigFilePath), false);
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

    return settings;
  }
}
