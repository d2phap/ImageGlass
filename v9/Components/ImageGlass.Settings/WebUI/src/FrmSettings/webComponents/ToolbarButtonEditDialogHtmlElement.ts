import { IToolbarButton } from '@/@types/FrmSettings';
import { openModalDialogEl } from '@/helpers';

export class ToolbarButtonEditDialogHtmlElement extends HTMLDialogElement {
  constructor() {
    super();

    // private methods
    this.openCreate = this.openCreate.bind(this);
    this.getDialogData = this.getDialogData.bind(this);
    this.addDialogEvents = this.addDialogEvents.bind(this);
  }


  private connectedCallback() {
    this.innerHTML = `
      <form method="dialog">
        <header class="dialog-header">
          <span class="create-only" lang-text="FrmSettings.Tab.Toolbar._AddNewButton">[Add a custom toolbar button]</span>
          <span class="edit-only" lang-text="FrmSettings.Tab.Toolbar._EditButton">[Edit toolbar button]</span>
        </header>
        <div class="dialog-body" style="width: 33rem;">
          <div class="mb-3">
            <div class="mb-1" lang-text="FrmSettings.Tab.Toolbar._ButtonJson">[Button JSON]</div>
            <textarea class="w-100" name="_ButtonJson" required rows="15" spellcheck="false" style="font-family: var(--fontCode);"></textarea>
          </div>
        </div>
        <footer class="dialog-footer">
          <button type="submit" lang-text="_._OK">[OK]</button>
          <button type="button" data-dialog-action="close" lang-text="_._Cancel">[Cancel]</button>
        </footer>
      </form>`;
  }


  /**
   * Opens tool dialog for create.
   */
  public async openCreate() {
    const defaultBtn = {
      Id: '',
      Type: 'Button',
      Alignment: 'Left',
      CheckableConfigBinding: '',
      DisplayStyle: 'Image',
      Image: '',
      Text: '',
      OnClick: {
        Executable: '',
        Arguments: [],
      },
    } as IToolbarButton;
    const json = JSON.stringify(defaultBtn, null, 2);

    const isSubmitted = await openModalDialogEl(this, 'create', { ButtonJson: json }, async () => {
      this.addDialogEvents();
    });

    return isSubmitted;
  }
  

  /**
   * Gets data from the tool dialog.
   */
  public getDialogData() {
    // get data
    const btn: IToolbarButton = {
      Id: '',
      Type: 'Button',
      Alignment: 'Left',
      CheckableConfigBinding: '',
      DisplayStyle: 'Image',
      Image: '',
      Text: '',
      OnClick: {
        Executable: '',
        Arguments: [],
      },
    } as IToolbarButton;

    return btn;
  }


  private addDialogEvents() {
    query('[data-dialog-action="close"]', this).removeEventListener('click', () => this.close(), false);
    query('[data-dialog-action="close"]', this).addEventListener('click', () => this.close(), false);
  }
}


/**
 * Creates and registers ToolbarButtonEditDialogHtmlElement to DOM.
 */
export const defineToolbarButtonEditDialogHtmlElement = () => window.customElements.define(
  'edit-toolbar-dialog',
  ToolbarButtonEditDialogHtmlElement,
  { extends: 'dialog' },
);
