import { getChangedSettingsFromTab } from '@/helpers';

export default class TabFileAssocs {
  /**
   * Loads settings for tab FileAssocs.
   */
  static loadSettings() {
  }


  /**
   * Adds events for tab FileAssocs.
   */
  static addEvents() {
  }


  /**
   * Save settings as JSON object.
   */
  static exportSettings() {
    return getChangedSettingsFromTab('file_assocs');
  }
}
