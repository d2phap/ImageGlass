import Language from '@/FrmSettings/Language';


/**
 * Pauses the thread for a period
 * @param time Duration to pause in millisecond
 * @param data Data to return after resuming
 * @returns a promise
 */
export const pause = <T>(time: number, data?: T): Promise<T> => new Promise((resolve) => {
  setTimeout(() => resolve(data as T), time);
});


/**
 * Gets the settings that are changed by user (`_pageSettings.config`) from the input tab.
 */
export const getChangedSettingsFromTab = (tab: string) => {
  const result: Record<string, any> = {};
  const inputEls = queryAll<HTMLInputElement>(`[tab="${tab}"] input[name]`);
  const selectEls = queryAll<HTMLSelectElement>(`[tab="${tab}"] select[name]`);
  const allEls = [...inputEls, ...selectEls];

  for (const el of allEls) {
    const configName = el.name;
    let configValue: boolean | number | string = '';
    if (el.name.startsWith('_')) continue;
    if (!el.checkValidity()) continue;


    // bool value
    if (el.type === 'checkbox') {
      configValue = (el as HTMLInputElement).checked;
    }
    // number value
    else if (el.type === 'number') {
      configValue = +el.value;
    }
    // string value
    else {
      configValue = el.value;
    }

    if (configValue !== _pageSettings.config[configName]) {
      result[configName] = configValue;
    }
  }

  return result;
};


/**
 * Escapes HTML characters.
 */
export const escapeHtml = (html: string) => {
  return html
    .replace(/&/g, '&amp;') // &
    .replace(/</g, '&lt;') // <
    .replace(/>/g, '&gt;') // >
    .replace(/"/g, '&quot;'); // "
};


/**
 * Creates tagged template function. Example:
 * ```ts
 * const imgTemplate = taggedTemplate`<img src="${'src'}" alt="${'alt'}" />`;
 * const html = imgTemplate({
 *    src: 'https://imageglass.org/photo.jpg',
 *    alt: 'Photo from imageglass.org',
 * });
 * ```
 * which is compiled as:
 * ```html
 * <img src="https://imageglass.org/photo.jpg" alt="Photo from imageglass.org" />
 * ```
 */
export const taggedTemplate = <T = Record<string, any>>(strings: TemplateStringsArray, ...keys: string[]) => {
  return (data: T) => {
    const dict: Record<string, any> = data || {};
    const result = [strings[0]];

    keys.forEach((key, i) => {
      const value = dict[key];
      result.push(value, strings[i + 1]);
    });

    return result.join('');
  };
};


/**
 * Opens modal dialog and return value.
 * @param selector Dialog selector.
 * @param data The data to pass to the dialog.
 */
export const openModalDialog = async (
  selector: string,
  purpose: 'create' | 'edit',
  data: Record<string, any> = {},
  onOpen?: (el: HTMLDialogElement) => any,
  onSubmit?: (e: SubmitEvent) => any) => {
  let isClosed = false;
  const dialogEl = query<HTMLDialogElement>(selector);
  dialogEl.classList.remove('dialog--create', 'dialog--edit');
  dialogEl.classList.add(`dialog--${purpose}`);

  // add shaking effect when clicking outside of dialog
  dialogEl.addEventListener('click', async (e) => {
    const el = e.target as HTMLDialogElement;
    const rect = el.getBoundingClientRect();

    // click on backdrop
    if (rect.left > e.clientX
      || rect.right < e.clientX
      || rect.top > e.clientY
      || rect.bottom < e.clientY) {
      // shake the modal for 500ms
      el.classList.add('ani-shaking');
      await pause(500);
      el.classList.remove('ani-shaking');
    }
  }, false);

  // on close
  dialogEl.addEventListener('close', () => isClosed = true, false);

  // on submit
  query<HTMLFormElement>(`${selector} > form`).addEventListener('submit', async (e) => {
    if (onSubmit) await Promise.resolve(onSubmit(e));
  });

  console.log(data);
  Object.keys(data).forEach(key => {
    const inputEl = query<HTMLInputElement>(`${selector} [name="_${key}"]`);
    if (inputEl) inputEl.value = data[key];
  });

  // on open
  if (onOpen) await Promise.resolve(onOpen(dialogEl));

  // open modal dialog
  dialogEl.showModal();

  while (!isClosed) {
    await pause(100);
  }

  return dialogEl;
};


/**
 * Open file picker.
 */
export const openFilePicker = async (options?: {
  multiple?: boolean,
  filter?: string,
}) => {
  const filePaths = await postAsync<string[]>('OpenFilePicker', options || {}, true);

  return filePaths;
};


/**
 * Open hotkey picker.
 */
export const openHotkeyPicker = async (): Promise<string | null> => {
  const hotkey = await postAsync<string>('OpenHotkeyPicker');

  return hotkey;
};


/**
 * Renders hotkey list
 * @param ulSelector CSS selector of the list element
 * @param hotkeys Hotkey list to render
 */
export const renderHotkeyList = async (
  ulSelector: string,
  hotkeys: string[],
  onChange?: (action: 'delete' | 'add') => any,
) => {
  const ulEl = query(ulSelector);
  let ulHtml = '';

  // load list of hotkeys
  for (const key of hotkeys) {
    ulHtml += `
    <li class="hotkey-item">
      <kbd>${key}</kbd>
      <button type="button" class="btn--icon" lang-title="_._Delete" data-action="delete">
        ${_pageSettings.icons.Delete}
      </button>
    </li>`;
  }

  // load 'Add hotkey' button
  ulHtml += `<li>
    <button type="button" lang-title="_._AddHotkey" data-action="add">[Add hotkey…]</button>
  </li>`;

  ulEl.innerHTML = ulHtml;
  Language.load();

  // add event listerner for 'Delete' hotkey
  queryAll<HTMLButtonElement>(`${ulSelector} button[data-action]`).forEach(el => {
    el.addEventListener('click', async () => {
      const action = el.getAttribute('data-action');
      const newHotkeys = queryAll(`${ulSelector} .hotkey-item > kbd`)
        .map(kbdEl => kbdEl.innerText);

      if (action === 'delete') {
        const hotkeyItemEl = el.closest('.hotkey-item');
        hotkeyItemEl?.remove();
        if (onChange) await Promise.resolve(onChange(action));
      }
      else if (action === 'add') {
        const hotkey = await openHotkeyPicker();
        if (!hotkey) return;


        renderHotkeyList(ulSelector, [...newHotkeys, hotkey], onChange);
        if (onChange) await Promise.resolve(onChange(action));

        // set focus to the 'Add hotkey' button
        query<HTMLButtonElement>(`${ulSelector} button[data-action="add"]`)?.focus();
      }
    }, false);
  });
};
