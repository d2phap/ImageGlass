import { ITool } from '@/@types/FrmSettings';
import {
  escapeHtml,
  getChangedSettingsFromTab,
  openFilePicker,
  openModalDialog,
  renderHotkeyList,
} from '@/helpers';
import Language from '../common/Language';


export default class TabTools {
  private static HOTKEY_SEPARATOR = '#';

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
      const defaultTool = {
        ToolId: '',
        ToolName: '',
        Executable: '',
        Arguments: _pageSettings.FILE_MACRO,
        Hotkeys: [],
        IsIntegrated: false,
      } as ITool;

      await openModalDialog('#Dialog_AddOrEditTool', 'create', defaultTool, async () => {
        TabTools.addEventsForToolDialog();
        TabTools.updateToolCommandPreview();

        await renderHotkeyList('#Dialog_AddOrEditTool .hotkey-list', defaultTool.Hotkeys);
      });

      const tool = TabTools.getToolDialogFormData();
      TabTools.setToolItemToList(tool.ToolId, tool);
    }, false);
  }


  /**
   * Save settings as JSON object.
   */
  static exportSettings() {
    const settings = getChangedSettingsFromTab('tools');
    const originalToolListJson = JSON.stringify(_pageSettings.toolList || []);

    settings.Tools = TabTools.getToolListFromDom();
    const newToolListJson = JSON.stringify(settings.Tools);

    if (newToolListJson === originalToolListJson) {
      delete settings.Tools;
    }

    return settings;
  }


  /**
   * Loads tool list but do not update `_pageSettings.toolList`.
   */
  private static loadToolList(list?: ITool[]) {
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
      if (item.Arguments) {
        args = `<code>${escapeHtml(item.Arguments)}</code>`;
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
        <tr data-tool-id="${item.ToolId}"
          data-tool-name="${item.ToolName}"
          data-tool-integrated="${item.IsIntegrated}"
          data-tool-executable="${item.Executable}"
          data-tool-arguments="${item.Arguments}"
          data-tool-hotkeys="${(item.Hotkeys || []).join(TabTools.HOTKEY_SEPARATOR)}">
          <td class="cell-counter"></td>
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
    Language.load();

    queryAll<HTMLButtonElement>('#Table_ToolList button[data-action]').forEach(el => {
      el.addEventListener('click', async () => {
        const action = el.getAttribute('data-action');
        const trEl = el.closest('tr');
        const toolId = trEl.getAttribute('data-tool-id');

        if (action === 'delete') {
          trEl.remove();
        }
        else if (action === 'edit') {
          await TabTools.editTool(toolId);
          el.focus();
        }
      }, false);
    });
  }


  /**
   * Gets tool list from DOM.
   */
  private static getToolListFromDom() {
    const trEls = queryAll<HTMLTableRowElement>('#Table_ToolList > tbody > tr');
    const toolList = trEls.map(el => {
      const toolId = el.getAttribute('data-tool-id') ?? '';
      const toolName = el.getAttribute('data-tool-name') ?? '';
      const toolIntegrated = el.getAttribute('data-tool-integrated') === 'true';
      const toolExecutable = el.getAttribute('data-tool-executable') ?? '';
      const toolArguments = el.getAttribute('data-tool-arguments') ?? '';

      const hotkeysStr = el.getAttribute('data-tool-hotkeys') ?? '';
      const toolHotkeys = hotkeysStr.split(TabTools.HOTKEY_SEPARATOR).filter(Boolean);

      return {
        ToolId: toolId,
        ToolName: toolName,
        IsIntegrated: toolIntegrated,
        Executable: toolExecutable,
        Arguments: toolArguments,
        Hotkeys: toolHotkeys,
      } as ITool;
    });

    return toolList;
  }


  /**
   * Open modal dialog to edit the tool.
   * @param toolId Tool id
   */
  private static async editTool(toolId: string) {
    const trEl = query<HTMLTableRowElement>(`#Table_ToolList tr[data-tool-id="${toolId}"]`);

    const hotkeysStr = trEl.getAttribute('data-tool-hotkeys') || '';
    const toolHotkeys = hotkeysStr.split(TabTools.HOTKEY_SEPARATOR).filter(Boolean);

    let tool: ITool = {
      ToolId: toolId,
      ToolName: trEl.getAttribute('data-tool-name') || '',
      Executable: trEl.getAttribute('data-tool-executable') || '',
      Arguments: trEl.getAttribute('data-tool-arguments') || '',
      IsIntegrated: trEl.getAttribute('data-tool-integrated') === 'true',
      Hotkeys: toolHotkeys,
    };

    // open dialog
    await openModalDialog('#Dialog_AddOrEditTool', 'edit', tool, async () => {
      query<HTMLInputElement>('[name="_IsIntegrated"]').checked = tool.IsIntegrated ?? false;
      TabTools.addEventsForToolDialog();
      TabTools.updateToolCommandPreview();

      await renderHotkeyList('#Dialog_AddOrEditTool .hotkey-list', tool.Hotkeys);
    });

    tool = TabTools.getToolDialogFormData();
    TabTools.setToolItemToList(toolId, tool);
  }


  /**
   * Gets tool data from the modal dialog.
   */
  private static getToolDialogFormData() {
    // get data
    const tool: ITool = {
      ToolId: query<HTMLInputElement>('#Dialog_AddOrEditTool [name="_ToolId"]').value.trim(),
      ToolName: query<HTMLInputElement>('#Dialog_AddOrEditTool [name="_ToolName"]').value.trim(),
      Executable: query<HTMLInputElement>('#Dialog_AddOrEditTool [name="_Executable"]').value.trim(),
      Arguments: query<HTMLInputElement>('#Dialog_AddOrEditTool [name="_Arguments"]').value.trim(),
      Hotkeys: queryAll('#Dialog_AddOrEditTool .hotkey-list > .hotkey-item > kbd').map(el => el.innerText),
      IsIntegrated: query<HTMLInputElement>('#Dialog_AddOrEditTool [name="_IsIntegrated"]').checked,
    };

    return tool;
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

    TabTools.loadToolList(toolList);
  }


  private static addEventsForToolDialog() {
    query('[name="_Executable"]').removeEventListener('input', TabTools.updateToolCommandPreview, false);
    query('[name="_Executable"]').addEventListener('input', TabTools.updateToolCommandPreview, false);

    query('[name="_Arguments"]').removeEventListener('input', TabTools.updateToolCommandPreview, false);
    query('[name="_Arguments"]').addEventListener('input', TabTools.updateToolCommandPreview, false);

    query('#btnBrowseTool').removeEventListener('click', TabTools.handleBtnBrowseToolClickEvent, false);
    query('#btnBrowseTool').addEventListener('click', TabTools.handleBtnBrowseToolClickEvent, false);
  }


  private static updateToolCommandPreview() {
    let executable = query<HTMLInputElement>('[name="_Executable"]').value || '';
    executable = executable.trim();

    let args = query<HTMLInputElement>('[name="_Arguments"]').value || '';
    args = args.trim().replaceAll('<file>', '"C:\\fake dir\\photo.jpg"');

    query('#Tool_CommandPreview').innerText = [executable, args].filter(Boolean).join(' ');
  }


  private static async handleBtnBrowseToolClickEvent() {
    const filePaths = await openFilePicker() ?? [];
    if (!filePaths.length) return;

    query<HTMLInputElement>('[name="_Executable"]').value = `"${filePaths[0]}"`;
    TabTools.updateToolCommandPreview();
  }

}
