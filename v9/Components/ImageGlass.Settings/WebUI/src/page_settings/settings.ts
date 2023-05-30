
/**
 * Loads settings.
 */
export const loadSettings = () => {
  // auto loads settings for String, Number, Boolean
  for (const configKey in _pageSettings.config) {
    if (!Object.prototype.hasOwnProperty.call(_pageSettings.config, configKey)) {
      continue;
    }

    const configValue = _pageSettings.config[configKey];

    // only auto load the settings if the value type is supported
    const canAutoSet = typeof configValue === 'string'
      || typeof configValue === 'number'
      || typeof configValue === 'boolean';
    if (!canAutoSet) continue;


    // find the html element
    const el = query(`[name="${configKey}"]`);
    if (!el) continue;


    // check the tag name and type
    const tagName = el.tagName.toLowerCase();
    if (tagName === 'select') {
      (el as HTMLSelectElement).value = configValue.toString();
    }
    else if (tagName === 'input') {
      const inputType = el.getAttribute('type').toLowerCase();
      const inputEl = el as HTMLInputElement;

      if (inputType === 'radio' || inputType === 'checkbox') {
        inputEl.checked = Boolean(configValue);
      }
      else if (inputType === 'color') {
        const colorHex = configValue.toString() || '#00000000';

        // remove alpha
        inputEl.value = colorHex.substring(0, colorHex.length - 2);
      }
      else {
        inputEl.value = configValue.toString();
      }
    }
  }


  // tab General
  query('#Lnk_StartupDir').innerText = _pageSettings.startUpDir || '(unknown)';
  query('#Lnk_ConfigDir').innerText = _pageSettings.configDir || '(unknown)';
  query('#Lnk_UserConfigFile').innerText = _pageSettings.userConfigFilePath || '(unknown)';

};
