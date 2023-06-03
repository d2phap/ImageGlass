import { getChangedSettingsFromTab } from '@/helpers';

export default class TabLayout {
  /**
   * Loads settings for tab Layout.
   */
  static loadSettings() {
  }


  /**
   * Adds events for tab Layout.
   */
  static addEvents() {
  }


  /**
   * Save settings as JSON object.
   */
  static exportSettings() {
    return getChangedSettingsFromTab('layout');
  }
}
