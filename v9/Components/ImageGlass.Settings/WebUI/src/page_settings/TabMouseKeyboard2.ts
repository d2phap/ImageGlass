
export default class TabMouseKeyboard {
  /**
   * Loads settings for tab Mouse & Keyboard.
   */
  static loadSettings() {
    query<HTMLSelectElement>('#Cmb_MouseWheel_Scroll').value = _pageSettings.config.MouseWheelActions?.Scroll || 'DoNothing';
    query<HTMLSelectElement>('#Cmb_MouseWheel_CtrlAndScroll').value = _pageSettings.config.MouseWheelActions?.CtrlAndScroll || 'DoNothing';
    query<HTMLSelectElement>('#Cmb_MouseWheel_ShiftAndScroll').value = _pageSettings.config.MouseWheelActions?.ShiftAndScroll || 'DoNothing';
    query<HTMLSelectElement>('#Cmb_MouseWheel_AltAndScroll').value = _pageSettings.config.MouseWheelActions?.AltAndScroll || 'DoNothing';
  }


  /**
   * Adds events for tab Mouse & Keyboard.
   */
  static addEvents() {
    query('#Btn_ResetMouseWheelAction').addEventListener('click', TabMouseKeyboard.resetDefaultMouseWheelActions, false);
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
