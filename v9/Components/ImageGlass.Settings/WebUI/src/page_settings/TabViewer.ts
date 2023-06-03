import { getChangedSettingsFromTab } from '@/helpers';

export default class TabViewer {
  /**
   * Loads settings for tab Viewer.
   */
  static loadSettings() {
    const zoomLevels = _pageSettings.config.ZoomLevels as number[] || [];
    query<HTMLTextAreaElement>('[name="ZoomLevels"]').value = zoomLevels.join('; ');
  }


  /**
   * Adds events for tab Viewer.
   */
  static addEvents() {
    query('[name="ZoomLevels"]').addEventListener('input', TabViewer.handleZoomLevelsChanged, false);
    query('[name="ZoomLevels"]').addEventListener('blur', TabViewer.handleZoomLevelsBlur, false);
  }


  /**
   * Save settings as JSON object.
   */
  static exportSettings() {
    const settings = getChangedSettingsFromTab('viewer');

    // ZoomLevels
    settings.ZoomLevels = TabViewer.getZoomLevels();

    if (query<HTMLTextAreaElement>('[name="ZoomLevels"]').checkValidity()) {
      const originalLevelsString = _pageSettings.config.ZoomLevels.toString();
      const newLevelsString = settings.ZoomLevels.toString();

      if (newLevelsString === originalLevelsString) {
        delete settings.ZoomLevels;
      }
    }
    else {
      delete settings.ZoomLevels;
    }

    return settings;
  }


  /**
   * Handle when ZoomLevels is changed.
   */
  private static handleZoomLevelsChanged() {
    const el = query<HTMLTextAreaElement>('[name="ZoomLevels"]');
    const levels = TabViewer.getZoomLevels();

    // validate
    if (levels.some(i => !Number.isFinite(i))) {
      el.setCustomValidity('Value contains invalid characters. Only number, semi-colon are allowed.');
    }
    else {
      el.setCustomValidity('');
    }
  }


  /**
   * Handle when the ZoomLevels box is blur.
   */
  private static handleZoomLevelsBlur() {
    const el = query<HTMLTextAreaElement>('[name="ZoomLevels"]');
    if (!el.checkValidity()) return;

    el.value = TabViewer.getZoomLevels().join('; ');
  }


  /**
   * Gets zoom levels
   */
  private static getZoomLevels() {
    const el = query<HTMLTextAreaElement>('[name="ZoomLevels"]');
    const levels = el.value.split(';').map(i => i.trim()).filter(Boolean)
      .map(i => parseFloat(i));

    return levels;
  }
}
