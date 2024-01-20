import { getChangedSettingsFromTab } from '@/helpers';
import { ToolbarEditorHtmlElement } from './webComponents/ToolbarEditorHtmlElement';
import { ToolbarButtonEditDialogHtmlElement } from './webComponents/ToolbarButtonEditDialogHtmlElement';
import { IToolbarButton } from '@/@types/FrmSettings';

export default class TabToolbar {
  static #toolbarEditor = query<ToolbarEditorHtmlElement>('#ToolbarEditor');
  static #toolbarBtnDialog = query<ToolbarButtonEditDialogHtmlElement>('[is="edit-toolbar-dialog"]');

  /**
   * Gets current selected theme.
   */
  static get currentTheme() {
    return _pageSettings.themeList.find(i => i.FolderName === _page.theme);
  }


  /**
   * Loads settings for tab Toolbar.
   */
  static loadSettings() {
    TabToolbar.#toolbarEditor.initialize(TabToolbar.onEditToolbarButton);
  }


  /**
   * Adds events for tab Toolbar.
   */
  static addEvents() {
    query('#Btn_AddCustomToolbarButton').addEventListener('click', TabToolbar.onBtnAddCustomToolbarButtonClick, false);
    query('#Btn_ResetToolbarButtons').addEventListener('click', TabToolbar.onBtnResetToolbarButtonsClick, false);
  }


  /**
   * Save settings as JSON object.
   */
  static exportSettings() {
    const settings = getChangedSettingsFromTab('toolbar');

    if (TabToolbar.#toolbarEditor.hasChanges) {
      settings.ToolbarButtons = TabToolbar.#toolbarEditor.currentButtons;
    }
    else {
      delete settings.ToolbarButtons;
    }

    return settings;
  }


  private static async onBtnResetToolbarButtonsClick() {
    const defaultToolbarIds = await postAsync<string[]>('Btn_ResetToolbarButtons');
    TabToolbar.#toolbarEditor.loadItemsByIds(defaultToolbarIds);
  }

  private static async onBtnAddCustomToolbarButtonClick() {
    const isSubmitted = await TabToolbar.#toolbarBtnDialog.openCreate();
    if (!isSubmitted) return;

    const data = TabToolbar.#toolbarBtnDialog.getDialogData();
    const btn = JSON.parse(data.ButtonJson) as IToolbarButton;
    const themeBtnIconUrl = TabToolbar.currentTheme.ToolbarIcons[btn.Image];

    // image is theme icon
    if (themeBtnIconUrl) {
      btn.ImageUrl = themeBtnIconUrl;
    }
    else {
      // image is an external file
      const imgUrl = new URL(`file:///${btn.Image}`);
      btn.ImageUrl = imgUrl.toString();
    }

    TabToolbar.#toolbarEditor.insertItems(btn, 0);
  }

  private static async onEditToolbarButton(toolbarBtn: IToolbarButton) {
    const isSubmitted = await TabToolbar.#toolbarBtnDialog.openEdit(toolbarBtn);
    if (!isSubmitted) return null;

    const data = TabToolbar.#toolbarBtnDialog.getDialogData();
    const btn = JSON.parse(data.ButtonJson) as IToolbarButton;
    const themeBtnIconUrl = TabToolbar.currentTheme.ToolbarIcons[btn.Image];

    // image is theme icon
    if (themeBtnIconUrl) {
      btn.ImageUrl = themeBtnIconUrl;
    }
    else {
      // image is an external file
      const imgUrl = new URL(`file:///${btn.Image}`);
      btn.ImageUrl = imgUrl.toString();
    }

    return btn;
  }
}
