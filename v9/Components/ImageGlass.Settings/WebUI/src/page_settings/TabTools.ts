import { getChangedSettingsFromTab } from '@/helpers';

export default class TabTools {
  /**
   * Loads settings for tab Tools.
   */
  static loadSettings() {
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
}
