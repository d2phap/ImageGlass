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
    //
  }

  private static onBtn_MakeDefaultViewerClicked() {
    //
  }

  private static onBtn_RemoveDefaultViewerClicked() {
    //
  }

  private static onLnk_OpenDefaultAppsSettingClicked() {
    //
  }

  private static onBtn_AddFileFormatClicked() {
    //
  }

  private static onBtn_ResetFileFormatsClicked() {
    //
  }
}
