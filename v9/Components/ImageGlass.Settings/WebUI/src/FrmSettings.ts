import { Webview } from './helpers/webview';
import { query, queryAll, on, post, postAsync } from './helpers/globalHelpers';

import Sidebar from './FrmSettings/Sidebar';
import Language from './FrmSettings/Language';
import Settings from './FrmSettings/Settings';

import TabGeneral from './FrmSettings/TabGeneral';
import TabImage from './FrmSettings/TabImage';
import TabSlideshow from './FrmSettings/TabSlideshow';
import TabMouseKeyboard from './FrmSettings/TabMouseKeyboard';
import TabLanguage from './FrmSettings/TabLanguage';
import TabEdit from './FrmSettings/TabEdit';
import TabViewer from './FrmSettings/TabViewer';
import TabToolbar from './FrmSettings/TabToolbar';
import TabGallery from './FrmSettings/TabGallery';
import TabAppearance from './FrmSettings/TabAppearance';
import TabFileAssocs from './FrmSettings/TabFileAssocs';
import TabTools from './FrmSettings/TabTools';


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
    initTab: 'tools',
    config: {},
    lang: {},
    langList: [],
    toolList: [],
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
    icons: {
      Delete: '',
      Edit: '',
      Moon: '',
      Sun: '',
    },
    startUpDir: '',
    configDir: '',
    userConfigFilePath: '',
    defaultThemeDir: '',
    FILE_MACRO: '',
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
