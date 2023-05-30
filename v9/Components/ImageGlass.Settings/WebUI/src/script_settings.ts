import { query, queryAll } from './helpers';
import { setActiveTab } from './page_settings/sidebar';
import { loadLanguage } from './page_settings/lang';
import { loadSettings } from './page_settings/settings';

// export to global
window.query = query;
window.queryAll = queryAll;
if (!window._pageSettings) {
  window._pageSettings = {
    config: {},
    lang: {},
    startUpDir: '',
    configDir: '',
    userConfigFilePath: '',
  };
}
_pageSettings.setActiveTab = setActiveTab;
_pageSettings.loadLanguage = loadLanguage;
_pageSettings.loadSettings = loadSettings;


// navigation bar event
const navItems = Array.from(document.querySelectorAll('input[name="nav"]'));
for (let i = 0; i < navItems.length; i++) {
  const itemEl = navItems[i] as HTMLInputElement;

  itemEl.addEventListener('change', (e) => {
    const activeTabName = (e.target as HTMLInputElement).value;
    setActiveTab(activeTabName);
  }, false);
}

setActiveTab('image');
loadLanguage();
loadSettings();
