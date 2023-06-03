import { getChangedSettingsFromTab } from '@/helpers';

export default class TabEdit {
  /**
   * Loads settings for tab Edit.
   */
  static loadSettings() {
  }


  /**
   * Adds events for tab Edit.
   */
  static addEvents() {
  }


  /**
   * Save settings as JSON object.
   */
  static exportSettings() {
    return getChangedSettingsFromTab('edit');
  }
}
