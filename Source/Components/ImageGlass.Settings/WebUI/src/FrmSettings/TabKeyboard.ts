import { getChangedSettingsFromTab } from '@/helpers';

export default class TabKeyboard {
  /**
   * Loads settings for tab Keyboard.
   */
  static loadSettings() {
  }


  /**
   * Adds events for tab Keyboard.
   */
  static addEvents() {
  }


  /**
   * Save settings as JSON object.
   */
  static exportSettings() {
    return getChangedSettingsFromTab('keyboard');
  }
}
