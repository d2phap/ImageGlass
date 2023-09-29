import { getChangedSettingsFromTab } from '@/helpers';
import { ToolbarEditorHtmlElement } from './webComponents/ToolbarEditorHtmlElement';

export default class TabToolbar {
  static #toolbarEditor = query<ToolbarEditorHtmlElement>('#ToolbarEditor');

  /**
   * Loads settings for tab Toolbar.
   */
  static loadSettings() {
    this.#toolbarEditor.initialize();
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

    if (this.#toolbarEditor.hasChanges) {
      settings.ToolbarButtons = this.#toolbarEditor.currentButtons;
    }
    else {
      delete settings.ToolbarButtons;
    }

    return settings;
  }


  private static onBtnAddCustomToolbarButtonClick() {
    //
  }

  private static onBtnResetToolbarButtonsClick() {
    //
  }
}
