
/**
 * Gets the settings that are changed by user (`_pageSettings.config`) from the input tab.
 */
export const getChangedSettingsFromTab = (tab: string) => {
  const result: Record<string, any> = {};
  const inputEls = queryAll<HTMLInputElement>(`[tab="${tab}"] input[name]`);
  const selectEls = queryAll<HTMLSelectElement>(`[tab="${tab}"] select[name]`);
  const allEls = [...inputEls, ...selectEls];

  for (const el of allEls) {
    const configName = el.name;
    let configValue: boolean | number | string = '';
    if (!el.checkValidity()) continue;


    // bool value
    if (el.type === 'checkbox') {
      configValue = (el as HTMLInputElement).checked;
    }
    // number value
    else if (el.type === 'number') {
      configValue = +el.value;
    }
    // string value
    else {
      configValue = el.value;
    }

    if (configValue !== _pageSettings.config[configName]) {
      result[configName] = configValue;
    }
  }

  return result;
};
