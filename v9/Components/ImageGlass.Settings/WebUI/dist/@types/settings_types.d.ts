export type IPageSettings = Record<string, any> & {
    config: Record<string, any>;
    lang: Record<string, string>;
    setActiveTab?: (tabName: string) => void;
    loadLanguage?: () => void;
};
