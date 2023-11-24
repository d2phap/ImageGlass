import Language from '@/common/Language';
import { escapeHtml, getChangedSettingsFromTab, pause } from '@/helpers';
import { FileFormatDialogHtmlElement } from './webComponents/FileFormatDialogHtmlElement';

export default class TabFileAssocs {
  static _areFileFormatsChanged = false;
  static #fileFormatDialog = query<FileFormatDialogHtmlElement>('[is="file-format-dialog"]');

  /**
   * Loads settings for tab FileAssocs.
   */
  static loadSettings() {
    TabFileAssocs.loadFileFormatList();
  }


  /**
   * Adds events for tab FileAssocs.
   */
  static addEvents() {
    query('#Btn_OpenExtIconFolder').addEventListener('click', TabFileAssocs.onBtn_OpenExtIconFolderClicked, false);

    query('#Btn_MakeDefaultViewer').addEventListener('click', TabFileAssocs.onBtn_MakeDefaultViewerClicked, false);
    query('#Btn_RemoveDefaultViewer').addEventListener('click', TabFileAssocs.onBtn_RemoveDefaultViewerClicked, false);
    query('#Lnk_OpenDefaultAppsSetting').addEventListener('click', TabFileAssocs.onLnk_OpenDefaultAppsSettingClicked, false);

    query('#Btn_AddFileFormat').addEventListener('click', () => TabFileAssocs.openFileFormatDialog(), false);
    query('#Btn_ResetFileFormats').addEventListener('click', TabFileAssocs.onBtn_ResetFileFormatsClicked, false);
  }


  /**
   * Save settings as JSON object.
   */
  static exportSettings() {
    const settings = getChangedSettingsFromTab('file_assocs');

    if (TabFileAssocs._areFileFormatsChanged) {
      // get new formats settings
      settings.FileFormats = TabFileAssocs.getFileFormatListFromDom().join(';');
    }
    else {
      delete settings.FileFormats;
    }

    return settings;
  }

  private static onBtn_OpenExtIconFolderClicked() {
    post('Btn_OpenExtIconFolder');
  }

  private static onBtn_MakeDefaultViewerClicked() {
    post('Btn_MakeDefaultViewer');
  }

  private static onBtn_RemoveDefaultViewerClicked() {
    post('Btn_RemoveDefaultViewer');
  }

  private static onLnk_OpenDefaultAppsSettingClicked() {
    post('Lnk_OpenDefaultAppsSetting');
  }

  private static async onBtn_ResetFileFormatsClicked() {
    const extStr = await postAsync<string>('Btn_ResetFileFormats') || '';
    const exts = extStr.split(';').sort();

    TabFileAssocs._areFileFormatsChanged = true;
    TabFileAssocs.loadFileFormatList(exts);
  }


  // Loads formats list but do not update `_pageSettings.config.FileFormats`.
  private static loadFileFormatList(extensions?: string[], extHighlight = '') {
    let exts = extensions
      || (_pageSettings.config.FileFormats as string || '').split(';').filter(Boolean)
      || [];
    exts = exts.sort();

    const tbodyEl = query<HTMLTableElement>('#Table_FileFormats > tbody');
    let tbodyHtml = '';

    for (const ext of exts) {
      const trHtml = `
        <tr data-ext="${ext}">
          <td class="cell-counter">
          </td>
          <td class="text-nowrap" style="--cell-border-right-color: transparent;">
            <code>${escapeHtml(ext)}</code>
          </td>
          <td class="cell-sticky-right text-nowrap" width="1" style="border-left: 0;">
            <button type="button" class="btn--icon px-1 ms-1" lang-title="_._Delete" data-action="delete">
              ${_pageSettings.icons.Delete}
            </button>
          </td>
        </tr>
      `;

      tbodyHtml += trHtml;
    }

    tbodyEl.innerHTML = tbodyHtml;
    Language.loadForEl(tbodyEl);

    query('#Lbl_TotalSupportedFormats').innerText = exts.length.toString();
    queryAll<HTMLButtonElement>('#Table_FileFormats button[data-action]').forEach(el => {
      el.addEventListener('click', async () => {
        const action = el.getAttribute('data-action');
        const trEl = el.closest('tr');

        if (action === 'delete') {
          trEl.remove();
          query('#Lbl_TotalSupportedFormats').innerText = queryAll('#Table_FileFormats > tbody > tr').length.toString();
          TabFileAssocs._areFileFormatsChanged = true;
        }
      }, false);
    });

    // scroll to & highlight the extension row
    if (extHighlight) {
      const highlightedTrEl = query(`#Table_FileFormats tr[data-ext="${extHighlight}"]`);
      if (!highlightedTrEl) return;

      highlightedTrEl.scrollIntoView({ block: 'center', behavior: 'smooth' });
      highlightedTrEl.classList.add('row--highlight');
      pause(2000).then(() => {
        if (highlightedTrEl) highlightedTrEl.classList.remove('row--highlight');
      });
    }
  }


  // Open file format dialog for create.
  private static async openFileFormatDialog() {
    const isSubmitted = await TabFileAssocs.#fileFormatDialog.openCreate();

    if (isSubmitted) {
      TabFileAssocs._areFileFormatsChanged = true;

      const ext = TabFileAssocs.#fileFormatDialog.getDialogData();
      TabFileAssocs.addNewFileFormat(ext);
    }
  }


  private static addNewFileFormat(ext: string) {
    if (!ext) return;
    const exts = TabFileAssocs.getFileFormatListFromDom();
    const extIndex = exts.indexOf(ext);

    // edit
    if (extIndex !== -1) {
      exts[extIndex] = ext;
    }
    // create
    else {
      exts.push(ext);
    }

    TabFileAssocs.loadFileFormatList(exts, ext);
  }


  private static getFileFormatListFromDom() {
    const trEls = queryAll<HTMLTableRowElement>('#Table_FileFormats > tbody > tr');
    const exts = trEls.map(trEl => trEl.getAttribute('data-ext') || '')
      .filter(Boolean);

    return exts;
  }
}
