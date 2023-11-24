import { getChangedSettingsFromTab } from '@/helpers';

export default class TabMouse {
  /**
   * Loads settings for tab Mouse.
   */
  static loadSettings() {
    query<HTMLSelectElement>('#Cmb_MouseWheel_Scroll').value = _pageSettings.config.MouseWheelActions?.Scroll || 'DoNothing';
    query<HTMLSelectElement>('#Cmb_MouseWheel_CtrlAndScroll').value = _pageSettings.config.MouseWheelActions?.CtrlAndScroll || 'DoNothing';
    query<HTMLSelectElement>('#Cmb_MouseWheel_ShiftAndScroll').value = _pageSettings.config.MouseWheelActions?.ShiftAndScroll || 'DoNothing';
    query<HTMLSelectElement>('#Cmb_MouseWheel_AltAndScroll').value = _pageSettings.config.MouseWheelActions?.AltAndScroll || 'DoNothing';
  }


  /**
   * Adds events for tab Mouse.
   */
  static addEvents() {
    query('#Btn_ResetMouseWheelAction').addEventListener('click', TabMouse.resetDefaultMouseWheelActions, false);
  }


  /**
   * Save settings as JSON object.
   */
  static exportSettings() {
    const settings = getChangedSettingsFromTab('mouse');

    // MouseWheelActions
    const newWheelScrollValue = query<HTMLSelectElement>('#Cmb_MouseWheel_Scroll').value;
    const newWheelCtrlAndScrollValue = query<HTMLSelectElement>('#Cmb_MouseWheel_CtrlAndScroll').value;
    const newWheelShiftAndScrollValue = query<HTMLSelectElement>('#Cmb_MouseWheel_ShiftAndScroll').value;
    const newWheelAltAndScrollValue = query<HTMLSelectElement>('#Cmb_MouseWheel_AltAndScroll').value;

    const mouseWheelActions: Record<string, string> = {};
    if (newWheelScrollValue !== _pageSettings.config.MouseWheelActions?.Scroll) {
      mouseWheelActions.Scroll = newWheelScrollValue;
    }
    if (newWheelCtrlAndScrollValue !== _pageSettings.config.MouseWheelActions?.CtrlAndScroll) {
      mouseWheelActions.CtrlAndScroll = newWheelCtrlAndScrollValue;
    }
    if (newWheelShiftAndScrollValue !== _pageSettings.config.MouseWheelActions?.ShiftAndScroll) {
      mouseWheelActions.ShiftAndScroll = newWheelShiftAndScrollValue;
    }
    if (newWheelAltAndScrollValue !== _pageSettings.config.MouseWheelActions?.AltAndScroll) {
      mouseWheelActions.AltAndScroll = newWheelAltAndScrollValue;
    }

    if (Object.keys(mouseWheelActions).length > 0) {
      settings.MouseWheelActions = mouseWheelActions;
    }
    else {
      delete settings.MouseWheelActions;
    }

    return settings;
  }


  /**
   * Resets the mouse wheel actions to the default settings.
   */
  private static resetDefaultMouseWheelActions() {
    query<HTMLSelectElement>('#Cmb_MouseWheel_Scroll').value = 'Zoom';
    query<HTMLSelectElement>('#Cmb_MouseWheel_CtrlAndScroll').value = 'PanVertically';
    query<HTMLSelectElement>('#Cmb_MouseWheel_ShiftAndScroll').value = 'PanHorizontally';
    query<HTMLSelectElement>('#Cmb_MouseWheel_AltAndScroll').value = 'BrowseImages';
  }
}
