import { getChangedSettingsFromTab } from '@/helpers';

export default class TabToolbar {
  /**
   * Loads settings for tab Toolbar.
   */
  static loadSettings() {
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
