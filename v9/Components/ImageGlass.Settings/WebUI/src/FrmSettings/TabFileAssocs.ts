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
    query('#Btn_OpenExtIconFolder').addEventListener('click', TabFileAssocs.onBtn_OpenExtIconFolderClicked, false);

    query('#Btn_MakeDefaultViewer').addEventListener('click', TabFileAssocs.onBtn_MakeDefaultViewerClicked, false);
    query('#Btn_RemoveDefaultViewer').addEventListener('click', TabFileAssocs.onBtn_RemoveDefaultViewerClicked, false);
    query('#Lnk_OpenDefaultAppsSetting').addEventListener('click', TabFileAssocs.onLnk_OpenDefaultAppsSettingClicked, false);

    query('#Btn_AddFileFormat').addEventListener('click', TabFileAssocs.onBtn_AddFileFormatClicked, false);
    query('#Btn_ResetFileFormats').addEventListener('click', TabFileAssocs.onBtn_ResetFileFormatsClicked, false);
  }


  /**
   * Save settings as JSON object.
   */
  static exportSettings() {
    return getChangedSettingsFromTab('file_assocs');
  }


  private static onBtn_OpenExtIconFolderClicked() {
    post('Btn_OpenExtIconFolder');
  }

  private static onBtn_MakeDefaultViewerClicked() {
    post('Btn_MakeDefaultViewer');
  }

  private static onBtn_RemoveDefaultViewerClicked() {
    post('Btn_RemoveDefaultViewer');
  }

  private static onLnk_OpenDefaultAppsSettingClicked() {
    post('Lnk_OpenDefaultAppsSetting');
  }

  private static onBtn_AddFileFormatClicked() {
    //
  }

  private static onBtn_ResetFileFormatsClicked() {
    //
  }
}
