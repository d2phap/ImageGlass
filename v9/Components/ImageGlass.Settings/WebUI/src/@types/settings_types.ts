
export type IPageSettings = Record<string, any> & {
  config: Record<string, any>;
  lang: Record<string, string>;

  // global functions
  setActiveTab?: (tabName: string) => void;
  loadLanguage?: () => void;
};
