import { Webview } from './helpers/webview';
import { query, queryAll, on, post, postAsync } from './helpers/globalHelpers';
import HapplaBoxViewer from './IGWebViewer/HapplaBoxViewer';


// initialize webview event listeners
window._webview = new Webview();
_webview.startListening();


// export to global
window.query = query;
window.queryAll = queryAll;
window.on = on;
window.post = post;
window.postAsync = postAsync;


// load the viewer
HapplaBoxViewer.load();
