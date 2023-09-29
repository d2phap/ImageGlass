import { getChangedSettingsFromTab } from '@/helpers';
import { ToolbarEditorHtmlElement } from './webComponents/ToolbarEditorHtmlElement';

export default class TabToolbar {
  static #toolbarEditor = query<ToolbarEditorHtmlElement>('#ToolbarEditor');

  /**
   * Loads settings for tab Toolbar.
   */
  static loadSettings() {
    TabToolbar.#toolbarEditor.initialize();
  }


  /**
   * Adds events for tab Toolbar.
   */
  static addEvents() {
    query('#Btn_AddCustomToolbarButton').addEventListener('click', TabToolbar.onBtnAddCustomToolbarButtonClick, false);
    query('#Btn_ResetToolbarButtons').addEventListener('click', TabToolbar.onBtnResetToolbarButtonsClick, false);
  }


  /**
   * Save settings as JSON object.
   */
  static exportSettings() {
    const settings = getChangedSettingsFromTab('toolbar');

    if (TabToolbar.#toolbarEditor.hasChanges) {
      settings.ToolbarButtons = TabToolbar.#toolbarEditor.currentButtons;
    }
    else {
      delete settings.ToolbarButtons;
    }

    return settings;
  }


  private static onBtnAddCustomToolbarButtonClick() {
    //
  }

  private static async onBtnResetToolbarButtonsClick() {
    const defaultToolbarIds = await postAsync<string[]>('Btn_ResetToolbarButtons');
    TabToolbar.#toolbarEditor.loadItemsByIds(defaultToolbarIds);
  }
}
