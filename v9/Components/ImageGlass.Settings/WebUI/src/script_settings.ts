import { query, queryAll } from './helpers';
import { addSidebarClickEvents, setSidebarActiveMenu } from './page_settings/sidebar';
import { loadLanguage } from './page_settings/lang';
import { addEventsForTabSlideshow, loadSettings } from './page_settings/settings';

// export to global
window.query = query;
window.queryAll = queryAll;
if (!window._pageSettings) {
  window._pageSettings = {
    config: {},
    lang: {},
    langList: [],
    enums: {
      ImageOrderBy: [],
      ImageOrderType: [],
      ColorProfileOption: [],
      AfterEditAppAction: [],
      ImageInterpolation: [],
      MouseWheelAction: [],
      MouseWheelEvent: [],
      MouseClickEvent: [],
      BackdropStyle: [],
      ToolbarItemModelType: [],
    },
    startUpDir: '',
    configDir: '',
    userConfigFilePath: '',
  };
}
_pageSettings.setActiveTab = setSidebarActiveMenu;
_pageSettings.loadLanguage = loadLanguage;
_pageSettings.loadSettings = loadSettings;


// sidebar
addSidebarClickEvents();
setSidebarActiveMenu('image');

// load settings
loadSettings();
loadLanguage();

// add event listeners
addEventsForTabSlideshow();
