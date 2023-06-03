import { loadLanguage } from '@/page_settings/Language';
import { loadSettings } from '@/page_settings/Settings2';
import { setSidebarActiveMenu } from '@/page_settings/Sidebar2';
import TabLanguage from '@/page_settings/TabLanguage2';

export type ILanguage = {
  FileName: string,
  Metadata: Record<string, any> & {
    Code: string,
    EnglishName: string,
    LocalName: string,
    Author: string,
    MinVersion: string,
  },
};

export type IPageSettings = Record<string, any> & {
  config: Record<string, any>,
  lang: Record<string, string>,
  langList: ILanguage[],
  startUpDir: string,
  configDir: string,
  userConfigFilePath: string,
  enums: Record<string, string[]> & {
    ImageOrderBy: string[],
    ImageOrderType: string[],
    ColorProfileOption: string[],
    AfterEditAppAction: string[],
    ImageInterpolation: string[],
    MouseWheelAction: string[],
    MouseWheelEvent: string[],
    MouseClickEvent: string[],
    BackdropStyle: string[],
    ToolbarItemModelType: string[],
  },

  // global functions
  setSidebarActiveMenu?: typeof setSidebarActiveMenu,
  loadLanguage?: typeof loadLanguage,
  loadSettings?: typeof loadSettings,
  loadLanguageList?: typeof TabLanguage.loadLanguageList,
};
