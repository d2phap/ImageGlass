import { getChangedSettingsFromTab } from '@/helpers';

export default class TabGallery {
  /**
   * Loads settings for tab Gallery.
   */
  static loadSettings() {
  }


  /**
   * Adds events for tab Gallery.
   */
  static addEvents() {
  }


  /**
   * Save settings as JSON object.
   */
  static exportSettings() {
    return getChangedSettingsFromTab('gallery');
  }
}
