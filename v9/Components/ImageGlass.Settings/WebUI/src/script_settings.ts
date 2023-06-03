import { Webview } from './webview';
import { query, queryAll, on, post, postAsync } from './helpers';
import { addSidebarClickEvents, setSidebarActiveMenu } from './page_settings/sidebar';
import { loadLanguage } from './page_settings/lang';
import {
  addEventsForTabLanguage,
  addEventsForTabMouseKeyboard,
  addEventsForTabSlideshow,
  loadLanguageList,
  loadSettings,
} from './page_settings/settings';
import TabGeneral from './page_settings/tabGeneral';
import TabImage from './page_settings/tabImage';


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
_pageSettings.setSidebarActiveMenu = setSidebarActiveMenu;
_pageSettings.loadLanguage = loadLanguage;
_pageSettings.loadSettings = loadSettings;
_pageSettings.loadLanguageList = loadLanguageList;


// sidebar
addSidebarClickEvents();
setSidebarActiveMenu('image');

// load settings
loadSettings();
loadLanguage();

// add event listeners
query('#BtnOK').addEventListener('click', () => post('BtnOK'), false);
query('#BtnCancel').addEventListener('click', () => post('BtnCancel'), false);
query('#BtnApply').addEventListener('click', () => post('BtnApply'), false);

TabGeneral.addEvents();
TabImage.addEvents();
addEventsForTabSlideshow();
addEventsForTabMouseKeyboard();
addEventsForTabLanguage();
