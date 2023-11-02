import { IEditApp } from '@/@types/FrmSettings';
import Language from '@/common/Language';
import { escapeHtml, getChangedSettingsFromTab, pause } from '@/helpers';
import { EditAppDialogHtmlElement } from './webComponents/EditAppDialogHtmlElement';


export default class TabEdit {
  static _areEditAppsChanged = false;
  static #editAppDialog = query<EditAppDialogHtmlElement>('[is="edit-app-dialog"]');

  /**
   * Loads settings for tab Edit.
   */
  static loadSettings() {
    TabEdit.loadEditApps();
  }


  /**
   * Adds events for tab Edit.
   */
  static addEvents() {
    query('#Btn_AddEditApp').addEventListener('click', () => TabEdit.openEditAppDialog(), false);
  }


  /**
   * Save settings as JSON object.
   */
  static exportSettings() {
    const settings = getChangedSettingsFromTab('edit');
    
    if (TabEdit._areEditAppsChanged) {
      // get new EditApps settings
      settings.EditApps = TabEdit.getEditAppsFromDom();
    }
    else {
      delete settings.EditApps;
    }

    return settings;
  }


  // Loads edit app list but do not update `_pageSettings.config.EditApps`
  private static loadEditApps(apps?: Record<string, IEditApp>, extKeyHighlight = '') {
    const editApps = apps || _pageSettings.config.EditApps || {};
    const extensions = Object.keys(editApps).sort();

    const tbodyEl = query<HTMLTableElement>('#Table_EditApps > tbody');
    let tbodyHtml = '';

    for (const extKey of extensions) {
      const item = editApps[extKey];
      if (!item) continue;

      let args = '<i lang-text="_._Empty"></i>';
      if (item.Argument) {
        args = `<code>${escapeHtml(item.Argument)}</code>`;
      }

      const trHtml = `
        <tr data-extkey="${escapeHtml(extKey)}">
          <td class="cell-counter">
            <code hidden name="_AppName">${escapeHtml(item.AppName)}</code>
            <code hidden name="_Executable">${escapeHtml(item.Executable)}</code>
            <code hidden name="_Argument">${escapeHtml(item.Argument)}</code>
          </td>
          <td class="cell-sticky text-nowrap">
            <code>${escapeHtml(extKey)}</code>
          </td>
          <td class="text-nowrap">${item.AppName}</td>
          <td class="text-nowrap">
            <code>${escapeHtml(item.Executable)}</code>
          </td>
          <td class="text-nowrap" style="--cell-border-right-color: transparent;">${args}</td>
          <td class="cell-sticky-right text-nowrap" width="1" style="border-left: 0;">
            <button type="button" class="btn--icon px-1" lang-title="_._Edit" data-action="edit">
              ${_pageSettings.icons.Edit}
            </button>
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

    queryAll<HTMLButtonElement>('#Table_EditApps button[data-action]').forEach(el => {
      el.addEventListener('click', async () => {
        const action = el.getAttribute('data-action');
        const trEl = el.closest('tr');
        const extKey = trEl.getAttribute('data-extkey');

        if (action === 'delete') {
          trEl.remove();
          TabEdit._areEditAppsChanged = true;
        }
        else if (action === 'edit') {
          await TabEdit.openEditAppDialog(extKey);
          el.focus();
        }
      }, false);
    });


    // scroll to & highlight the extension row
    if (extKeyHighlight) {
      const highlightedTrEl = query(`#Table_EditApps tr[data-extkey="${extKeyHighlight}"]`);
      if (!highlightedTrEl) return;

      highlightedTrEl.scrollIntoView({ block: 'center', behavior: 'smooth' });
      highlightedTrEl.classList.add('row--highlight');
      pause(2000).then(() => {
        if (highlightedTrEl) highlightedTrEl.classList.remove('row--highlight');
      });
    }
  }


  // Gets edit app list from DOM
  private static getEditAppsFromDom() {
    const trEls = queryAll<HTMLTableRowElement>('#Table_EditApps > tbody > tr');
    const editApps: Record<string, IEditApp> = {};

    trEls.forEach(trEl => {
      const extKey = trEl.getAttribute('data-extkey') || '';
      const appName = query('[name="_AppName"]', trEl).innerText || '';
      const appExecutable = query('[name="_Executable"]', trEl).innerText || '';
      const appArgument = query('[name="_Argument"]', trEl).innerText || '';

      editApps[extKey] = {
        AppName: appName,
        Executable: appExecutable,
        Argument: appArgument,
      };
    });

    return editApps;
  }


  private static async openEditAppDialog(extKey?: string) {
    let isSubmitted = false;

    if (extKey) {
      isSubmitted = await TabEdit.#editAppDialog.openEdit(extKey);
    }
    else {
      isSubmitted = await TabEdit.#editAppDialog.openCreate();
    }

    if (isSubmitted) {
      TabEdit._areEditAppsChanged = true;

      const data = TabEdit.#editAppDialog.getDialogData();
      TabEdit.setEditAppToList(data.app, data.extKey, extKey);
    }
  }


  /**
   * Sets the edit app to the list.
   * @param oldExtKey If not found, the app will be inserted into the list.
   */
  private static setEditAppToList(app: IEditApp, newExtKey: string, oldExtKey?: string) {
    if (!app.AppName || !app.Executable) return;
    const editApps = TabEdit.getEditAppsFromDom();

    // edit
    if (oldExtKey) {
      delete editApps[oldExtKey];
    }

    editApps[newExtKey] = app;
    TabEdit.loadEditApps(editApps, newExtKey);
  }
}
