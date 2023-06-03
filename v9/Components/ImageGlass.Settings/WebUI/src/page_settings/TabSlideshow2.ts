
export default class TabSlideshow {
  /**
   * Loads settings for tab Slideshow.
   */
  static loadSettings() {
    TabSlideshow.handleUseRandomIntervalForSlideshowChanged();
    TabSlideshow.handleSlideshowIntervalsChanged();
  }


  /**
   * Adds events for tab Slideshow.
   */
  static addEvents() {
    query('[name="UseRandomIntervalForSlideshow"]').addEventListener('input', TabSlideshow.handleUseRandomIntervalForSlideshowChanged, false);
    query('[name="SlideshowInterval"]').addEventListener('input', TabSlideshow.handleSlideshowIntervalsChanged, false);
    query('[name="SlideshowIntervalTo"]').addEventListener('input', TabSlideshow.handleSlideshowIntervalsChanged, false);
  }


  /**
   * Handle when slideshow intervals are changed.
   */
  private static handleSlideshowIntervalsChanged() {
    const intervalFrom = +query<HTMLInputElement>('[name="SlideshowInterval"]').value || 5;
    const intervalTo = +query<HTMLInputElement>('[name="SlideshowIntervalTo"]').value || 5;
    const intervalFromText = TabSlideshow.toTimeString(intervalFrom);
    const intervalToText = TabSlideshow.toTimeString(intervalTo);

    const useRandomInterval = query<HTMLInputElement>('[name="UseRandomIntervalForSlideshow"]').checked;

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
    const useRandomInterval = query<HTMLInputElement>('[name="UseRandomIntervalForSlideshow"]').checked;
  
    query('#Lbl_SlideshowIntervalFrom').hidden = !useRandomInterval;
    query('#Section_SlideshowIntervalTo').hidden = !useRandomInterval;
  }


  /**
   * Formats total seconds to time format: `mm:ss.fff`.
   */
  private static toTimeString(totalSeconds: number) {
    const dt = new Date(totalSeconds * 1000);
    let minutes = dt.getUTCMinutes().toString();
    let seconds = dt.getUTCSeconds().toString();
    const msSeconds = dt.getUTCMilliseconds().toString();

    if (minutes.length < 2) minutes = `0${minutes}`;
    if (seconds.length < 2) seconds = `0${seconds}`;

    return `${minutes}:${seconds}.${msSeconds}`;
  }
}
