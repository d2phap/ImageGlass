export type IPageSettings = Record<string, any> & {
    config: Record<string, any>;
    lang: Record<string, string>;
    startUpDir: string;
    configDir: string;
    userConfigFilePath: string;
    enums: {
        ImageOrderBy: string[];
        ImageOrderType: string[];
        ColorProfileOption: string[];
        AfterEditAppAction: string[];
        ImageInterpolation: string[];
        MouseWheelAction: string[];
        MouseWheelEvent: string[];
        MouseClickEvent: string[];
        BackdropStyle: string[];
        ToolbarItemModelType: string[];
    };
    setActiveTab?: (tabName: string) => void;
    loadLanguage?: () => void;
    loadSettings?: () => void;
};
