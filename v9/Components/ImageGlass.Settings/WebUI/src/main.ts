import './styles/main.scss';

import { Webview } from './helpers/webview';
import { query, queryAll, on, post, postAsync } from './helpers/globalHelpers';
import Language from './common/Language';
import { pause } from './helpers';

// initialize webview event listeners
window._webview = new Webview();
_webview.startListening();


// export to global
window.query = query;
window.queryAll = queryAll;
window.on = on;
window.post = post;
window.postAsync = postAsync;

if (!window._page) {
  window._page = {
    lang: {},
  };
}
_page.loadLanguage = Language.load;


// enable transition after 1 second
pause(1000).then(() => {
  document.documentElement.style.setProperty('--transitionMs', '300ms');
});


// handle keydown event
window.onkeydown = (e: KeyboardEvent) => {
  const ctrl = e.ctrlKey ? 'ctrl' : '';
  const shift = e.shiftKey ? 'shift' : '';
  const alt = e.altKey ? 'alt' : '';

  let key = e.key.toLowerCase();
  const keyMaps: Record<string, string> = {
    control: '',
    shift: '',
    alt: '',
    arrowleft: 'left',
    arrowright: 'right',
    arrowup: 'up',
    arrowdown: 'down',
    backspace: 'back',
  };
  if (keyMaps[key] !== undefined) {
    key = keyMaps[key];
  }
  const keyCombo = [ctrl, shift, alt, key].filter(Boolean).join('+');

  // preserve ESCAPE key for closing HTML5 dialog
  if (keyCombo === 'escape' && document.querySelector('dialog[open]')) {
    return;
  }

  console.log('KEYDOWN', keyCombo);
  post('KEYDOWN', keyCombo);
};
