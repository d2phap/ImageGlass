import { getChangedSettingsFromTab } from '@/helpers';

export default class TabViewer {
  /**
   * Loads settings for tab Viewer.
   */
  static loadSettings() {
    const zoomLevels = _pageSettings.config.ZoomLevels as number[] || [];
    query<HTMLTextAreaElement>('[name="ZoomLevels"]').value = zoomLevels.join('; ');
    query<HTMLInputElement>('[name="_UseSmoothZooming"]').checked = zoomLevels.length === 0;
    TabViewer.onUseSmoothZoomingChanged();
  }


  /**
   * Adds events for tab Viewer.
   */
  static addEvents() {
    query('[name="_UseSmoothZooming"]').addEventListener('input', TabViewer.onUseSmoothZoomingChanged, false);
    query('[name="ZoomLevels"]').addEventListener('input', TabViewer.handleZoomLevelsChanged, false);
    query('[name="ZoomLevels"]').addEventListener('blur', TabViewer.handleZoomLevelsBlur, false);

    query('#LnkLoadDefaultZoomLevels').addEventListener('click', TabViewer.onLoadDefaultZoomLevelsClicked, false);
  }


  /**
   * Save settings as JSON object.
   */
  static exportSettings() {
    const settings = getChangedSettingsFromTab('viewer');

    // ZoomLevels
    settings.ZoomLevels = TabViewer.getZoomLevels();

    if (query<HTMLTextAreaElement>('[name="ZoomLevels"]').checkValidity()) {
      const originalLevelsString = _pageSettings.config.ZoomLevels?.toString();
      const newLevelsString = settings.ZoomLevels?.toString();

      if (newLevelsString === originalLevelsString) {
        delete settings.ZoomLevels;
      }
    }
    else {
      delete settings.ZoomLevels;
    }

    return settings;
  }


  private static onUseSmoothZoomingChanged() {
    const isDisabled = query<HTMLInputElement>('[name="_UseSmoothZooming"]').checked;

    query('[name="ZoomLevels"]').toggleAttribute('disabled', isDisabled);
    query('#LnkLoadDefaultZoomLevels').toggleAttribute('disabled', isDisabled);
    query('#LnkLoadDefaultZoomLevels').setAttribute('tabindex', isDisabled ? '-1' : '0');
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
    const isEnabled = query<HTMLInputElement>('[name="_UseSmoothZooming"]').checked;
    if (isEnabled) return [];

    const el = query<HTMLTextAreaElement>('[name="ZoomLevels"]');
    const levels = el.value.split(';').map(i => i.trim()).filter(Boolean)
      .map(i => parseFloat(i));

    return levels;
  }


  private static onLoadDefaultZoomLevelsClicked() {
    const defaultLevels = '5; 10; 15; 20; 30; 40; 50; 60; 70; 80; 90; 100; 125; 150; 175; 200; 250; 300; 350; 400; 500; 600; 700; 800; 1000; 1200; 1500; 1800; 2100; 2500; 3000; 3500; 4500; 6000; 8000; 10000';

    query<HTMLTextAreaElement>('[name="ZoomLevels"]').value = defaultLevels;
  }
}
