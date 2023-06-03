import { Webview } from './webview';
import { query, queryAll, on, post, postAsync } from './helpers';

import TabGeneral from './page_settings/TabGeneral2';
import TabImage from './page_settings/TabImage2';
import TabSlideshow from './page_settings/TabSlideshow2';
import TabMouseKeyboard from './page_settings/TabMouseKeyboard2';
import TabLanguage from './page_settings/TabLanguage2';
import TabEdit from './page_settings/TabEdit2';
import Settings from './page_settings/Settings2';
import Language from './page_settings/Language';
import Sidebar from './page_settings/Sidebar2';


// initialize webview event listeners
window._webview = new Webview();
_webview.startListening();


// export to global
window.query = query;
window.queryAll = queryAll;
window.on = on;
window.post = post;
window.postAsync = postAsync;

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
_pageSettings.setSidebarActiveMenu = Sidebar.setActiveMenu;
_pageSettings.loadLanguage = Language.load;
_pageSettings.loadSettings = Settings.load;
_pageSettings.loadLanguageList = TabLanguage.loadLanguageList;


// sidebar
Sidebar.addEvents();
Sidebar.setActiveMenu('image');

// load settings
Settings.load();
Language.load();

// add event listeners
Settings.addEventsForFooter();
TabGeneral.addEvents();
TabImage.addEvents();
TabSlideshow.addEvents();
TabEdit.addEvents();
TabMouseKeyboard.addEvents();
TabLanguage.addEvents();
