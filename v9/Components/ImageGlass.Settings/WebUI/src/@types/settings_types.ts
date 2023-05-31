
export type IPageSettings = Record<string, any> & {
  config: Record<string, any>,
  lang: Record<string, string>,
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
  setActiveTab?: (tabName: string) => void,
  loadLanguage?: () => void,
  loadSettings?: () => void,
};
