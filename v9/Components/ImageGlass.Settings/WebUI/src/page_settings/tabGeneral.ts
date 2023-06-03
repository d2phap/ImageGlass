
export default class TabGeneral {
  /**
   * Loads settings for tab General.
   */
  static loadSettings() {
    query('#Lnk_StartupDir').innerText = _pageSettings.startUpDir || '(unknown)';
    query('#Lnk_ConfigDir').innerText = _pageSettings.configDir || '(unknown)';
    query('#Lnk_UserConfigFile').innerText = _pageSettings.userConfigFilePath || '(unknown)';
  }


  /**
   * Adds events for tab General.
   */
  static addEvents() {
    query('#Lnk_StartupDir').addEventListener('click', () => post('Lnk_StartupDir', _pageSettings.startUpDir), false);
    query('#Lnk_ConfigDir').addEventListener('click', () => post('Lnk_ConfigDir', _pageSettings.configDir), false);
    query('#Lnk_UserConfigFile').addEventListener('click', () => post('Lnk_UserConfigFile', _pageSettings.userConfigFilePath), false);
  }
}
