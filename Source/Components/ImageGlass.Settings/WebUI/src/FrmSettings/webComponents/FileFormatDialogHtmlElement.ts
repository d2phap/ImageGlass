import { openModalDialogEl } from '@/helpers';


export class FileFormatDialogHtmlElement extends HTMLDialogElement {
  constructor() {
    super();

    // private methods
    this.openCreate = this.openCreate.bind(this);
    this.getDialogData = this.getDialogData.bind(this);
  }


  private connectedCallback() {
    this.innerHTML = `
      <form method="dialog">
        <header class="dialog-header">
          <span class="create-only" lang-text="FrmSettings._AddNewFileExtension">[Add new file extension]</span>
        </header>
        <div class="dialog-body" style="width: 22rem;">
          <div class="mb-3">
            <div class="mb-1" lang-text="_._FileExtension">[File extension]</div>
            <input class="w-100" name="_Extension" required spellcheck="false"
              pattern="^\\.[\\w]+"
              placeholder='.jxl'
              style="font-family: var(--fontCode);" />
          </div>
        </div>
        <footer class="dialog-footer">
          <button type="submit" lang-text="_._Add">[Add]</button>
          <button type="button" data-dialog-action="close" lang-text="_._Cancel">[Cancel]</button>
        </footer>
      </form>`;
  }


  /**
   * Opens file format dialog for create.
   */
  public openCreate() {
    return openModalDialogEl(this, 'create', { Extension: '' }, null);
  }


  /**
   * Gets data from the tool dialog.
   */
  public getDialogData() {
    // get data
    const ext = query<HTMLTextAreaElement>('[name="_Extension"]').value || '';

    return ext;
  }
}


/**
 * Creates and registers FileFormatDialogHtmlElement to DOM.
 */
export const defineFileFormatDialogHtmlElement = () => window.customElements.define(
  'file-format-dialog',
  FileFormatDialogHtmlElement,
  { extends: 'dialog' },
);
