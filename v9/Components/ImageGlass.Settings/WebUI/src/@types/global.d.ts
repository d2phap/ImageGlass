import { IPageSettings } from './FrmSettings';
import { Webview } from '@/helpers/webview';
import Language from '@/common/Language';

import {
  query as QueryFn,
  queryAll as QueryAllFn,
  on as OnFn,
  post as PostFn,
  postAsync as PostAsyncFn,
} from '../helpers/globalHelpers';


export type IPage = Record<string, any> & {
  lang: Record<string, string>,

  loadLanguage?: typeof Language.load,
};

declare global {
  interface Window {
    _webview: Webview;
    _pageSettings: IPageSettings,

    query: typeof QueryFn,
    queryAll: typeof QueryAllFn,

    on: typeof OnFn,
    post: typeof PostFn,
    postAsync: typeof PostAsyncFn,
  }

  var _webview: Webview;
  var _page: IPage;
  var _pageSettings: IPageSettings;

  var query: typeof QueryFn;
  var queryAll: typeof QueryAllFn;

  var on: typeof OnFn;
  var post: typeof PostFn;
  var postAsync: typeof PostAsyncFn;
}

