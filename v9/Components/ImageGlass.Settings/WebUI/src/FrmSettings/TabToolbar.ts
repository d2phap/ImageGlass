import { getChangedSettingsFromTab } from '@/helpers';
import { ToolbarEditorHtmlElement } from './webComponents/ToolbarEditorHtmlElement';
import { ToolbarButtonEditDialogHtmlElement } from './webComponents/ToolbarButtonEditDialogHtmlElement';

export default class TabToolbar {
  static #toolbarEditor = query<ToolbarEditorHtmlElement>('#ToolbarEditor');
  static #toolbarBtnDialog = query<ToolbarButtonEditDialogHtmlElement>('[is="edit-toolbar-dialog"]');

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


  private static async onBtnAddCustomToolbarButtonClick() {
    const isSubmitted = await TabToolbar.#toolbarBtnDialog.openCreate();

    if (isSubmitted) {
      const tool = TabToolbar.#toolbarBtnDialog.getDialogData();
    }
  }

  private static async onBtnResetToolbarButtonsClick() {
    const defaultToolbarIds = await postAsync<string[]>('Btn_ResetToolbarButtons');
    TabToolbar.#toolbarEditor.loadItemsByIds(defaultToolbarIds);
  }
}
