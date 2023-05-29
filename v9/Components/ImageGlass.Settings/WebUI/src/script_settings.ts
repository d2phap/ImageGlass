import { setActiveTab } from './page_settings/sidebar';
import { loadLanguage } from './page_settings/lang';
import { loadSettings } from './page_settings/settings';

// export to global
if (!window._pageSettings) {
  window._pageSettings = {
    config: {},
    lang: {},
  };
}
window._pageSettings.setActiveTab = setActiveTab;
window._pageSettings.loadLanguage = loadLanguage;
window._pageSettings.loadSettings = loadSettings;


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
