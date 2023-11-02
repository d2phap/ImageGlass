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

export type ITool = {
  ToolId: string,
  ToolName: string,
  Executable: string,
  Argument: string,
  IsIntegrated?: boolean,
  Hotkeys?: string[],
};

export type ITheme = {
  ConfigFilePath: string,
  FolderName: string,
  FolderPath: string,
  BgColor: string,
  IsDarkMode: boolean,
  PreviewImage: string,
  Info: Record<string, any> & {
    Name: string,
    Version: string,
    Author: string,
    Email: string,
    Website: string,
    Description: string,
  },
  ToolbarIcons: Record<string, string>,
};

export type SingleAction = {
  Executable: string,
  Arguments: Array<string|number|boolean>,
  NextAction?: SingleAction,
};

export type IToolbarButton = {
  Type: 'Button' | 'Separator',
  Id: string,
  Text: string,
  DisplayStyle: 'Image' | 'ImageAndText',
  CheckableConfigBinding: string,
  Alignment: 'Left' | 'Right',
  Image: string,
  ImageUrl: string,
  OnClick: SingleAction,
};

export type IEditApp = {
  AppName: string,
  Executable: string,
  Argument: string,
}

export type IPageSettings = Record<string, any> & {
  config: Record<string, any>,
  langList: ILanguage[],
  toolList: ITool[],
  themeList: ITheme[],
  startUpDir: string,
  configDir: string,
  userConfigFilePath: string,
  defaultThemeDir: string,
  defaultImageInfoTags: string[],
  builtInToolbarButtons: IToolbarButton[],
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
    ImageInfoUpdateTypes: string[],
  },
  icons: Record<string, string> & {
    Delete: string,
    Edit: string,
    ArrowUp: string,
    ArrowDown: string,
    ArrowLeft: string,
    ArrowRight: string,
    ArrowExchange: string,
    Sun: string,
    Moon: string,
  },
  FILE_MACRO: string;
};
