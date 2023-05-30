
export type IPageSettings = Record<string, any> & {
  config: Record<string, any>;
  lang: Record<string, string>;
  startUpDir: string;
  configDir: string;
  userConfigFilePath: string;

  // global functions
  setActiveTab?: (tabName: string) => void;
  loadLanguage?: () => void;
  loadSettings?: () => void;
};
