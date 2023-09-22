import './main';

import Sidebar from './FrmSettings/Sidebar';
import Settings from './FrmSettings/Settings';
import TabAppearance from './FrmSettings/TabAppearance';
import { defineToolDialogHtmlElement } from './FrmSettings/webComponents/ToolDialogHtmlElement';
import { defineToolbarEditorHtmlElement } from './FrmSettings/webComponents/ToolbarEditorHtmlElement';


if (!window._pageSettings) {
  window._pageSettings = {
    config: {},
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
_page.loadSettings = Settings.load;
_page.setActiveMenu = Sidebar.setActiveMenu;
_page.loadBackgroundColorConfig = TabAppearance.loadBackgroundColorConfig;


// register web components
defineToolDialogHtmlElement();
defineToolbarEditorHtmlElement();


// sidebar
Sidebar.addEvents();
