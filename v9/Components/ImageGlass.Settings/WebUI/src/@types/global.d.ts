import { IPageSettings } from './settings_types';
import {
  query as QueryFn,
  queryAll as QueryAllFn,
  on as OnFn,
  post as PostFn,
} from '../helpers';

declare global {
  interface Window {
    _pageSettings: IPageSettings,

    query: typeof QueryFn,
    queryAll: typeof QueryAllFn,

    on: typeof OnFn,
    post: typeof PostFn,
  }

  var _pageSettings: IPageSettings;
  var query: typeof QueryFn;
  var queryAll: typeof QueryAllFn;

  var on: typeof OnFn;
  var post: typeof PostFn;
}

