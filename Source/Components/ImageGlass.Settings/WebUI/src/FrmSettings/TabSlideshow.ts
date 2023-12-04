import { getChangedSettingsFromTab } from '@/helpers';

export default class TabSlideshow {
  /**
   * Loads settings for tab Slideshow.
   */
  static loadSettings() {
    TabSlideshow.handleUseRandomIntervalForSlideshowChanged();
    TabSlideshow.handleSlideshowIntervalsChanged();
    TabSlideshow.handleSlideshowBackgroundColorChanged();
  }


  /**
   * Adds events for tab Slideshow.
   */
  static addEvents() {
    query('[name="UseRandomIntervalForSlideshow"]').addEventListener('input', TabSlideshow.handleUseRandomIntervalForSlideshowChanged, false);
    query('[name="SlideshowInterval"]').addEventListener('input', TabSlideshow.handleSlideshowIntervalsChanged, false);
    query('[name="SlideshowIntervalTo"]').addEventListener('input', TabSlideshow.handleSlideshowIntervalsChanged, false);
    query('#Lnk_ResetSlideshowBackgroundColor').addEventListener('click', TabSlideshow.resetSlideshowBackgroundColor, false);
    query('#Btn_SlideshowBackgroundColor').addEventListener('click', TabSlideshow.onBtn_SlideshowBackgroundColor, false);
  }


  /**
   * Save settings as JSON object.
   */
  static exportSettings() {
    return getChangedSettingsFromTab('slideshow');
  }


  /**
   * Handle when slideshow intervals are changed.
   */
  private static handleSlideshowIntervalsChanged() {
    const fromEl = query<HTMLInputElement>('[name="SlideshowInterval"]');
    const toEl = query<HTMLInputElement>('[name="SlideshowIntervalTo"]');
    const useRandomInterval = query<HTMLInputElement>('[name="UseRandomIntervalForSlideshow"]').checked;

    if (useRandomInterval) {
      fromEl.max = toEl.value;
      toEl.min = fromEl.value;
    }
    else {
      fromEl.max = '';
    }    

    const intervalFrom = +fromEl.value || 5;
    const intervalTo = +toEl.value || 5;
    const intervalFromText = TabSlideshow.toTimeString(intervalFrom);
    const intervalToText = TabSlideshow.toTimeString(intervalTo);

    if (useRandomInterval) {
      query('#Lbl_SlideshowInterval').innerText = `${intervalFromText} - ${intervalToText}`;
    }
    else {
      query('#Lbl_SlideshowInterval').innerText = intervalFromText;
    }
  }


  /**
   * handle when `UseRandomIntervalForSlideshow` is changed.
   */
  private static handleUseRandomIntervalForSlideshowChanged() {
    const fromEl = query<HTMLInputElement>('[name="SlideshowInterval"]');
    const toEl = query<HTMLInputElement>('[name="SlideshowIntervalTo"]');
    const useRandomInterval = query<HTMLInputElement>('[name="UseRandomIntervalForSlideshow"]').checked;
  
    query('#Lbl_SlideshowIntervalFrom').hidden = !useRandomInterval;
    query('#Section_SlideshowIntervalTo').hidden = !useRandomInterval;

    const intervalFrom = +fromEl.value || 5;
    const intervalTo = +toEl.value || 5;
    if (useRandomInterval && intervalFrom > intervalTo) {
      toEl.min = intervalFrom.toString();
      toEl.value = intervalFrom.toString();
    }
  }


  // Formats total seconds to time format: `mm:ss.fff`.
  private static toTimeString(totalSeconds: number) {
    const dt = new Date(totalSeconds * 1000);
    let minutes = dt.getUTCMinutes().toString();
    let seconds = dt.getUTCSeconds().toString();
    const msSeconds = dt.getUTCMilliseconds().toString();

    if (minutes.length < 2) minutes = `0${minutes}`;
    if (seconds.length < 2) seconds = `0${seconds}`;

    return `${minutes}:${seconds}.${msSeconds}`;
  }


  // Reset slideshow background color to black
  private static resetSlideshowBackgroundColor() {
    query<HTMLInputElement>('[name="SlideshowBackgroundColor"]').value = '#000000';
    TabSlideshow.handleSlideshowBackgroundColorChanged();
  }


  // Handles when `SlideshowBackgroundColor` is changed.
  private static handleSlideshowBackgroundColorChanged() {
    const colorHex = query<HTMLInputElement>('[name="SlideshowBackgroundColor"]').value;
    if (!colorHex) return;

    query<HTMLInputElement>('#Btn_SlideshowBackgroundColor > .color-display').style.setProperty('--color-picker-value', colorHex);
    query('#Lbl_SlideshowBackgroundColorValue').innerText = colorHex;
  }


  private static async onBtn_SlideshowBackgroundColor() {
    const colorEL = query<HTMLInputElement>('[name="SlideshowBackgroundColor"]');
    const colorValue = await postAsync<string>('Btn_SlideshowBackgroundColor', colorEL.value);

    if (colorValue) {
      colorEL.value = colorValue;
      TabSlideshow.handleSlideshowBackgroundColorChanged();
    }
  }
}
