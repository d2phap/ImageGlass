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


const getHotkeys = (e: KeyboardEvent) => {
  let key = e.key.toLowerCase();

  const ctrl = e.ctrlKey
    ? 'ctrl'
    : (key === 'control' ? 'ctrl' : '');
  const shift = e.shiftKey
    ? 'shift'
    : (key === 'shift' ? 'shift' : '');
  const alt = e.altKey
    ? 'alt'
    : (key === 'alt' ? 'alt' : '');


  const keyMaps: Record<string, string> = {
    control: '',
    shift: '',
    alt: '',
    arrowleft: 'left',
    arrowright: 'right',
    arrowup: 'up',
    arrowdown: 'down',
    backspace: 'back',
    ' ': 'space',
  };

  if (keyMaps[key] !== undefined) {
    key = keyMaps[key];
  }

  const hotkeys = [ctrl, shift, alt, key].filter(Boolean).join('+');

  return hotkeys;
};

// handle keydown event
window.onkeydown = (e: KeyboardEvent) => {
  const hotkeys = getHotkeys(e);

  // preserve ESCAPE key for closing HTML5 dialog
  if (hotkeys === 'escape' && document.querySelector('dialog[open]')) {
    return;
  }

  if (!hotkeys) return;
  post('KEYDOWN', hotkeys);
};


// handle keyup event
window.onkeyup = (e: KeyboardEvent) => {
  const hotkeys = getHotkeys(e);

  if (!hotkeys) return;
  post('KEYUP', hotkeys);
};
