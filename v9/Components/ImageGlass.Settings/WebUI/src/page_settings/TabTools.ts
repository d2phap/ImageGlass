import { ITool } from '@/@types/settings_types';
import { escapeHtml, getChangedSettingsFromTab, openModalDialog } from '@/helpers';
import Language from './Language';

export default class TabTools {
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
    query('#Btn_AddTool').addEventListener('click', async () => {
      const dialog = await openModalDialog('#Dialog_AddOrEditTool');
      console.log(dialog);
    }, false);
  }


  /**
   * Save settings as JSON object.
   */
  static exportSettings() {
    const settings = getChangedSettingsFromTab('tools');
    const originalToolListJson = JSON.stringify(_pageSettings.toolList || []);

    const trEls = queryAll<HTMLTableRowElement>('#Table_ToolList > tbody > tr');
    settings.Tools = trEls.map(el => {
      const toolId = el.getAttribute('data-tool-id');
      const toolName = el.getAttribute('data-tool-name');
      const toolIntegrated = el.getAttribute('data-tool-integrated') === 'true';
      const toolHotkey = el.getAttribute('data-tool-hotkey');
      const toolExecutable = el.getAttribute('data-tool-executable');
      const toolArguments = el.getAttribute('data-tool-arguments');

      return {
        ToolId: toolId,
        ToolName: toolName,
        IsIntegrated: toolIntegrated,
        Executable: toolExecutable,
        Arguments: toolArguments,
      } as ITool;
    });
    const newToolListJson = JSON.stringify(settings.Tools);
    if (newToolListJson === originalToolListJson) {
      delete settings.Tools;
    }

    return settings;
  }


  /**
   * Loads tool list.
   */
  private static loadToolList(list?: ITool[]) {
    if (Array.isArray(list) && list.length > 0) {
      _pageSettings.toolList = list;
    }
    const toolList = _pageSettings.toolList || [];

    const tbodyEl = query<HTMLTableElement>('#Table_ToolList > tbody');
    let tbodyHtml = '';
    const btnDeleteHtml = `
      <button type="button" class="px-1 ms-1" lang-title="_._Delete" data-action="delete">❌</button>
    `;

    for (const item of toolList) {
      let args = '<i lang-text="_._Empty"></i>';
      if (item.Arguments) {
        args = `<code>${escapeHtml(item.Arguments)}</code>`;
      }

      const trHtml = `
        <tr data-tool-id="${item.ToolId}"
          data-tool-name="${item.ToolName}"
          data-tool-integrated="${item.IsIntegrated}"
          data-tool-executable="${item.Executable}"
          data-tool-arguments="${item.Arguments}">
          <td class="cell-counter"></td>
          <td class="cell-sticky text-nowrap">${item.ToolId}</td>
          <td class="text-nowrap">${item.ToolName}</td>
          <td lang-text="_.${item.IsIntegrated ? '_Yes' : '_No'}"></td>
          <td>
            <kbd>Ctrl+S</kbd>
          </td>
          <td class="text-nowrap">
            <code>${escapeHtml(item.Executable)}</code>
          </td>
          <td class="text-nowrap" style="--cell-border-right-color: transparent;">${args}</td>
          <td class="cell-sticky-right text-nowrap" width="1" style="border-left: 0;">
            <button type="button" class="px-1" lang-title="_._Edit" data-action="edit">✏️</button>
            ${item.ToolId !== 'Tool_ExifGlass' ? btnDeleteHtml : ''}
          </td>
        </tr>
      `;

      tbodyHtml += trHtml;
    }

    tbodyEl.innerHTML = tbodyHtml;
    Language.load();

    queryAll<HTMLButtonElement>('#Table_ToolList button[data-action]').forEach(el => {
      el.addEventListener('click', async (e) => {
        const action = (e.target as HTMLInputElement).getAttribute('data-action');
        const trEl = (e.target as HTMLInputElement).closest('tr');
        const toolId = trEl.getAttribute('data-tool-id');

        if (action === 'delete') {
          trEl.remove();
        }
        else if (action === 'edit') {
          const newToolList = await postAsync<ITool[]>('Tool_Edit', toolId);
          TabTools.loadToolList(newToolList);
        }
      }, false);
    });
  }

}
