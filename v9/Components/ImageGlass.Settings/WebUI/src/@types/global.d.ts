import { IPageSettings } from './settings_types';

declare global {
  interface Window {
    _pageSettings: IPageSettings,
  }
}

