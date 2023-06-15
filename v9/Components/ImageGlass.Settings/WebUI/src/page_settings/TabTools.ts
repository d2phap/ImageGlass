import { ITool } from '@/@types/settings_types';
import { getChangedSettingsFromTab } from '@/helpers';
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
  }


  /**
   * Save settings as JSON object.
   */
  static exportSettings() {
    return getChangedSettingsFromTab('tools');
  }


  private static loadToolList(list?: ITool[]) {
    if (Array.isArray(list) && list.length > 0) {
      _pageSettings.toolList = list;
    }
    const toolList = _pageSettings.toolList || [];

    const tbodyEl = query<HTMLTableElement>('#Table_ToolList > tbody');
    let i = 0;
    let tbodyHtml = '';

    for (const item of toolList) {
      let args = '<i lang-text="_._Empty"></i>';
      if (item.Arguments) {
        args = `<code>${item.Arguments}</code>`;
      }

      const trHtml = `
        <tr>
          <td>${i + 1}</td>
          <td class="cell-sticky text-nowrap">${item.ToolId}</td>
          <td class="text-nowrap">${item.ToolName}</td>
          <td lang-text="_.${item.IsIntegrated ? '_Yes' : '_No'}"></td>
          <td>
            <kbd>Ctrl+S</kbd>
          </td>
          <td class="text-nowrap">
            <code>${item.Executable}</code>
          </td>
          <td class="text-nowrap">${args}</td>
          <td class="cell-sticky-right text-nowrap" width="1" style="border-left: 0;">
            <button type="button" class="px-1" lang-title="_._Edit"
              data-tool-id="${item.ToolId}" data-action="edit">✏️</button>
            <button type="button" class="px-1 ms-1" lang-title="_._Delete"
              data-tool-id="${item.ToolId}" data-action="delete">❌</button>
          </td>
        </tr>
      `;

      tbodyHtml += trHtml;
      i++;
    }

    tbodyEl.innerHTML = tbodyHtml;
    Language.load();
  }
}
