import { IPageSettings } from './settings_types';
import { query as QueryFn, queryAll as QueryAllFn } from '../helpers';

declare global {
  interface Window {
    _pageSettings: IPageSettings,

    query: typeof QueryFn,
    queryAll: typeof QueryAllFn,
  }

  var _pageSettings: IPageSettings;
  var query: typeof QueryFn;
  var queryAll: typeof QueryAllFn;
}

