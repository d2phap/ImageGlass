import { Webview } from './helpers/webview';
import { query, queryAll, on, post, postAsync } from './helpers/globalHelpers';

import Sidebar from './page_settings/Sidebar';
import Language from './page_settings/Language';
import Settings from './page_settings/Settings';

import TabGeneral from './page_settings/TabGeneral';
import TabImage from './page_settings/TabImage';
import TabSlideshow from './page_settings/TabSlideshow';
import TabMouseKeyboard from './page_settings/TabMouseKeyboard';
import TabLanguage from './page_settings/TabLanguage';
import TabEdit from './page_settings/TabEdit';
import TabViewer from './page_settings/TabViewer';
import TabToolbar from './page_settings/TabToolbar';
import TabGallery from './page_settings/TabGallery';
import TabAppearance from './page_settings/TabAppearance';
import TabFileAssocs from './page_settings/TabFileAssocs';
import TabTools from './page_settings/TabTools';


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
    initTab: '',
    config: {},
    lang: {},
    langList: [],
    themeList: [],
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
    defaultThemeDir: '',
  };
}
_pageSettings.setSidebarActiveMenu = Sidebar.setActiveMenu;
_pageSettings.loadLanguage = Language.load;
_pageSettings.loadSettings = Settings.load;
_pageSettings.loadLanguageList = TabLanguage.loadLanguageList;


// sidebar
Sidebar.addEvents();

// load settings
Settings.load();
Language.load();

// add event listeners
Settings.addEventsForFooter();
TabGeneral.addEvents();
TabImage.addEvents();
TabSlideshow.addEvents();
TabEdit.addEvents();
TabViewer.addEvents();
TabToolbar.addEvents();
TabGallery.addEvents();
TabLanguage.addEvents();
TabMouseKeyboard.addEvents();
TabFileAssocs.addEvents();
TabTools.addEvents();
TabLanguage.addEvents();
TabAppearance.addEvents();

// load the last open tab
Sidebar.setActiveMenu(_pageSettings.initTab);
