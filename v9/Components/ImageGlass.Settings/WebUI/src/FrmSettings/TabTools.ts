import { ITool } from '@/@types/FrmSettings';
import {
  escapeHtml,
  getChangedSettingsFromTab,
  pause,
} from '@/helpers';
import Language from '../common/Language';
import { ToolDialogHtmlElement } from './webComponents/ToolDialogHtmlElement';


export default class TabTools {
  static HOTKEY_SEPARATOR = '#';
  static _areToolsChanged = false;
  static #toolDialog = query<ToolDialogHtmlElement>('[is="tool-dialog"]');

  /**
   * Loads settings for tab Tools.
   */
  static loadSettings() {
    TabTools.loadToolList();
  }


  /**
   * Adds events for tab Tools.
   */
  static addEvents() {
    query('#Btn_AddTool').addEventListener('click', () => TabTools.openToolDialog(), false);
  }


  /**
   * Save settings as JSON object.
   */
  static exportSettings() {
    const settings = getChangedSettingsFromTab('tools');
    
    if (TabTools._areToolsChanged) {
      // get new tool settings
      settings.Tools = TabTools.getToolListFromDom();
    }
    else {
      delete settings.Tools;
    }

    return settings;
  }


  /**
   * Loads tool list but do not update `_pageSettings.toolList`.
   */
  private static loadToolList(list?: ITool[], toolIdHighlight = '') {
    const toolList: ITool[] = list ?? _pageSettings.toolList ?? [];

    const tbodyEl = query<HTMLTableElement>('#Table_ToolList > tbody');
    let tbodyHtml = '';
    const btnDeleteHtml = `
      <button type="button" class="btn--icon px-1 ms-1" lang-title="_._Delete" data-action="delete">
        ${_pageSettings.icons.Delete}
      </button>
    `;

    for (const item of toolList) {
      let args = '<i lang-text="_._Empty"></i>';
      if (item.Argument) {
        args = `<code>${escapeHtml(item.Argument)}</code>`;
      }

      const hotkeysHtml = (item.Hotkeys || [])
        .map((key, index) => {
          const margin = index === 0 ? '' : 'ms-1';
          return `<kbd class="${margin}">${key}</kbd>`;
        }).join('');

      const chkIntegratedHtml = `
        <label class="ig-checkbox">
          <input type="checkbox" disabled ${item.IsIntegrated === true ? 'checked' : ''} />
          <div></div>
        </label>
      `;

      const trHtml = `
        <tr data-toolId="${item.ToolId}">
          <td class="cell-counter">
            <code hidden name="_ToolName">${escapeHtml(item.ToolName)}</code>
            <code hidden name="_IsIntegrated">${item.IsIntegrated}</code>
            <code hidden name="_Executable">${escapeHtml(item.Executable)}</code>
            <code hidden name="_Argument">${escapeHtml(item.Argument)}</code>
            <code hidden name="_Hotkeys">${(item.Hotkeys || []).join(TabTools.HOTKEY_SEPARATOR)}</code>
          </td>
          <td class="cell-sticky text-nowrap">${item.ToolId}</td>
          <td class="text-nowrap">${item.ToolName}</td>
          <td class="text-center">${chkIntegratedHtml}</td>
          <td class="text-nowrap">${hotkeysHtml}</td>
          <td class="text-nowrap">
            <code>${escapeHtml(item.Executable)}</code>
          </td>
          <td class="text-nowrap" style="--cell-border-right-color: transparent;">${args}</td>
          <td class="cell-sticky-right text-nowrap" width="1" style="border-left: 0;">
            <button type="button" class="btn--icon px-1" lang-title="_._Edit" data-action="edit">
              ${_pageSettings.icons.Edit}
            </button>
            ${item.ToolId !== 'Tool_ExifGlass' ? btnDeleteHtml : ''}
          </td>
        </tr>
      `;

      tbodyHtml += trHtml;
    }

    tbodyEl.innerHTML = tbodyHtml;
    Language.loadForEl(tbodyEl);

    queryAll<HTMLButtonElement>('#Table_ToolList button[data-action]').forEach(el => {
      el.addEventListener('click', async () => {
        const action = el.getAttribute('data-action');
        const trEl = el.closest('tr');
        const toolId = trEl.getAttribute('data-toolId');

        if (action === 'delete') {
          trEl.remove();
          TabTools._areToolsChanged = true;
        }
        else if (action === 'edit') {
          await TabTools.openToolDialog(toolId);
          el.focus();
        }
      }, false);
    });


    // scroll to & highlight the extension row
    if (toolIdHighlight) {
      const highlightedTrEl = query(`#Table_ToolList tr[data-toolId="${toolIdHighlight}"]`);
      if (!highlightedTrEl) return;

      highlightedTrEl.scrollIntoView({ block: 'center', behavior: 'smooth' });
      highlightedTrEl.classList.add('row--highlight');
      pause(2000).then(() => {
        if (highlightedTrEl) highlightedTrEl.classList.remove('row--highlight');
      });
    }
  }


  /**
   * Gets tool list from DOM.
   */
  private static getToolListFromDom() {
    const trEls = queryAll<HTMLTableRowElement>('#Table_ToolList > tbody > tr');
    const toolList = trEls.map(trEl => {
      const toolId = trEl.getAttribute('data-toolId') || '';
      const toolName = query('[name="_ToolName"]', trEl).innerText || '';
      const toolIntegrated = (query('[name="_IsIntegrated"]', trEl).innerText) === 'true';
      const toolExecutable = query('[name="_Executable"]', trEl).innerText || '';
      const toolArgument = query('[name="_Argument"]', trEl).innerText || '';

      const hotkeysStr = query('[name="_Hotkeys"]', trEl).innerText || '';
      const toolHotkeys = hotkeysStr.split(TabTools.HOTKEY_SEPARATOR).filter(Boolean);

      return {
        ToolId: toolId,
        ToolName: toolName,
        IsIntegrated: toolIntegrated,
        Executable: toolExecutable,
        Argument: toolArgument,
        Hotkeys: toolHotkeys,
      } as ITool;
    });

    return toolList;
  }


  /**
   * Open tool dialog for create or edit.
   */
  private static async openToolDialog(toolId?: string) {
    let isSubmitted = false;

    if (toolId) {
      isSubmitted = await TabTools.#toolDialog.openEdit(toolId);
    }
    else {
      isSubmitted = await TabTools.#toolDialog.openCreate();
    }

    if (isSubmitted) {
      TabTools._areToolsChanged = true;

      const tool = TabTools.#toolDialog.getDialogData();
      TabTools.setToolItemToList(tool.ToolId, tool);
    }
  }


  /**
   * Sets the tool item to the list.
   * @param oldToolId If not found, the tool will be inserted into the list.
   */
  private static setToolItemToList(oldToolId: string, tool: ITool) {
    if (!tool.ToolId || !tool.ToolName || !tool.Executable) return;
    const toolList = TabTools.getToolListFromDom();
    const toolIndex = toolList.findIndex(i => i.ToolId === oldToolId);

    // edit
    if (toolIndex !== -1) {
      toolList[toolIndex] = tool;
    }
    // create
    else {
      toolList.push(tool);
    }

    TabTools.loadToolList(toolList, tool.ToolId);
  }
}
