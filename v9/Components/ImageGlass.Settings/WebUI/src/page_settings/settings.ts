
/**
 * Loads settings.
 */
export const loadSettings = () => {
  for (const configKey in window._pageSettings.config) {
    if (!Object.prototype.hasOwnProperty.call(window._pageSettings.config, configKey)) {
      continue;
    }

    const configValue = window._pageSettings.config[configKey];

    // only auto load the settings if the value type is supported
    const canAutoSet = typeof configValue === 'string'
      || typeof configValue === 'number'
      || typeof configValue === 'boolean';
    if (!canAutoSet) continue;


    // find the html element
    const el = document.querySelector(`[name="${configKey}"]`);
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
};
