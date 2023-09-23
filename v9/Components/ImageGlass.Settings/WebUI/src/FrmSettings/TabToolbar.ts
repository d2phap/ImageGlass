import { getChangedSettingsFromTab } from '@/helpers';
import { ToolbarEditorHtmlElement } from './webComponents/ToolbarEditorHtmlElement';

export default class TabToolbar {
  static #toolbarEditor = query<ToolbarEditorHtmlElement>('toolbar-editor');

  /**
   * Loads settings for tab Toolbar.
   */
  static loadSettings() {
    this.#toolbarEditor.items = _pageSettings.config.ToolbarButtons || [];
  }


  /**
   * Adds events for tab Toolbar.
   */
  static addEvents() {
  }


  /**
   * Save settings as JSON object.
   */
  static exportSettings() {
    return getChangedSettingsFromTab('toolbar');
  }
}
