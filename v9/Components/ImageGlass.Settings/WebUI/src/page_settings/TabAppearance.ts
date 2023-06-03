import { getChangedSettingsFromTab } from '@/helpers';

export default class TabAppearance {
  /**
   * Loads settings for tab Appearance.
   */
  static loadSettings() {
  }


  /**
   * Adds events for tab Appearance.
   */
  static addEvents() {
  }


  /**
   * Save settings as JSON object.
   */
  static exportSettings() {
    return getChangedSettingsFromTab('appearance');
  }
}
