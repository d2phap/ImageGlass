import { IEditApp } from '@/@types/FrmSettings';
import { openFilePicker, openModalDialogEl } from '@/helpers';


export class EditAppDialogHtmlElement extends HTMLDialogElement {
  constructor() {
    super();

    // private methods
    this.openCreate = this.openCreate.bind(this);
    this.openEdit = this.openEdit.bind(this);
    this.getDialogData = this.getDialogData.bind(this);
    this.addDialogEvents = this.addDialogEvents.bind(this);
    this.updateToolCommandPreview = this.updateToolCommandPreview.bind(this);
    this.handleBtnBrowseAppClickEvent = this.handleBtnBrowseAppClickEvent.bind(this);
  }


  private connectedCallback() {
    this.innerHTML = `
      <form method="dialog">
        <header class="dialog-header">
          <span class="create-only" lang-text="FrmSettings.EditAppDialog._AddApp">[Add a new app]</span>
          <span class="edit-only" lang-text="FrmSettings.EditAppDialog._EditApp">[Edit app]</span>
        </header>
        <div class="dialog-body" style="width: 33rem;">
          <div class="mb-3">
            <div class="mb-1" lang-text="_._FileExtension">[File extension]</div>
            <input type="text" name="_FileExtension" class="w-100" required spellcheck="false"
              pattern="^\\.([\\w;\\.\\w])+"
              placeholder=".jpg;.png;.svg" />
          </div>
          <div class="mb-3">
            <div class="mb-1" lang-text="FrmSettings._EditApps._AppName">[App name]</div>
            <input type="text" name="_AppName" class="w-100" required spellcheck="false" placeholder="MS Paint" />
          </div>
          <div class="mb-3">
            <div class="mb-1" lang-text="_._Executable">[Executable]</div>
            <div class="d-flex align-items-center">
              <input type="text" name="_Executable" class="me-2 w-100" required spellcheck="false"
                placeholder="mspaint.exe"
                style="width: calc(100vw - calc(var(--controlHeight) * 1px) - 0.5rem);" />
              <button id="BtnBrowseApp" type="button" class="px-1" lang-title="_._Browse">…</button>
            </div>
          </div>
          <div class="mb-3">
            <div class="mb-1" lang-text="_._Argument">[Argument]</div>
            <input type="text" name="_Argument" class="w-100" spellcheck="false" placeholder="<file>" value="<file>" />
          </div>
          <div class="mt-4">
            <div class="mb-1" lang-text="_._CommandPreview">[Command preview]</div>
            <pre class="command-preview text-code px-2 py-1"><code id="App_CommandPreview"></code></pre>
          </div>
        </div>
        <footer class="dialog-footer">
          <button type="submit" lang-text="_._OK">[OK]</button>
          <button type="button" data-dialog-action="close" lang-text="_._Cancel">[Cancel]</button>
        </footer>
      </form>`;
  }


  /**
   * Opens EditApp dialog for create.
   */
  public async openCreate() {
    const data = {
      FileExtension: '',
      AppName: '',
      Executable: '',
      Argument: _pageSettings.FILE_MACRO,
    } as IEditApp;

    const isSubmitted = await openModalDialogEl(this, 'create', data, async () => {
      this.addDialogEvents();
      this.updateToolCommandPreview();
    });

    return isSubmitted;
  }


  /**
   * Opens EditApp dialog for edit.
   * @param extKey Extension key
   */
  public async openEdit(extKey: string) {
    const trEl = query<HTMLTableRowElement>(`#Table_EditApps tr[data-extkey="${extKey}"]`);

    const data = {
      FileExtension: extKey,
      AppName: query('[name="_AppName"]', trEl).innerText || '',
      Executable: query('[name="_Executable"]', trEl).innerText || '',
      Argument: query('[name="_Argument"]', trEl).innerText || '',
    } as IEditApp;

    // open dialog
    const isSubmitted = await openModalDialogEl(this, 'edit', data, async () => {
      this.addDialogEvents();
      this.updateToolCommandPreview();
    });

    return isSubmitted;
  }


  /**
   * Gets data from the tool dialog.
   */
  public getDialogData() {
    // get data
    const extKey = query<HTMLInputElement>('[name="_FileExtension"]', this).value.trim();
    const app: IEditApp = {
      AppName: query<HTMLInputElement>('[name="_AppName"]', this).value.trim(),
      Executable: query<HTMLInputElement>('[name="_Executable"]', this).value.trim(),
      Argument: query<HTMLInputElement>('[name="_Argument"]', this).value.trim(),
    };

    return { extKey, app };
  }


  private addDialogEvents() {
    query('[name="_Executable"]', this).removeEventListener('input', this.updateToolCommandPreview, false);
    query('[name="_Executable"]', this).addEventListener('input', this.updateToolCommandPreview, false);

    query('[name="_Argument"]', this).removeEventListener('input', this.updateToolCommandPreview, false);
    query('[name="_Argument"]', this).addEventListener('input', this.updateToolCommandPreview, false);

    query('#BtnBrowseApp', this).removeEventListener('click', this.handleBtnBrowseAppClickEvent, false);
    query('#BtnBrowseApp', this).addEventListener('click', this.handleBtnBrowseAppClickEvent, false);
  }


  private updateToolCommandPreview() {
    let executable = query<HTMLInputElement>('[name="_Executable"]', this).value || '';
    executable = executable.trim();

    let args = query<HTMLInputElement>('[name="_Argument"]', this).value || '';
    args = args.trim().replaceAll('<file>', '"C:\\fake dir\\photo.jpg"');

    query('#App_CommandPreview', this).innerText = [executable, args].filter(Boolean).join(' ');
  }


  private async handleBtnBrowseAppClickEvent() {
    const filePaths = await openFilePicker() ?? [];
    if (!filePaths.length) return;

    query<HTMLInputElement>('[name="_Executable"]', this).value = filePaths[0];
    this.updateToolCommandPreview();
  }
}


/**
 * Creates and registers EditAppDialogHtmlElement to DOM.
 */
export const defineEditAppDialogHtmlElement = () => window.customElements.define(
  'edit-app-dialog',
  EditAppDialogHtmlElement,
  { extends: 'dialog' },
);
