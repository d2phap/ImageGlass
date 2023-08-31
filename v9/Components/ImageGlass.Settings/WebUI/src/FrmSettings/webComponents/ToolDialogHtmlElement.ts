import { ITool } from '@/@types/FrmSettings';
import { openFilePicker, openModalDialogEl, renderHotkeyListEl } from '@/helpers';

const HOTKEY_SEPARATOR = '#';
const styles = `
`;


export class ToolDialogHtmlElement extends HTMLElement {
  #dialogEl: HTMLDialogElement;

  constructor() {
    super();

    // private methods
    this.createTemplate = this.createTemplate.bind(this);

    // initialize template
    this.createTemplate();
  }


  private createTemplate() {
    // initialize component
    this.attachShadow({ mode: 'open' });

    const css = new CSSStyleSheet();
    css.replaceSync(styles);
    this.shadowRoot.adoptedStyleSheets = [css];

    this.#dialogEl = document.createElement('dialog');
    this.#dialogEl.innerHTML = `
      <form method="dialog">
        <header class="dialog-header">
          <span class="create-only" lang-text="FrmSettings.Tab.Tools._AddNewTool">[Add a new external tool]</span>
          <span class="edit-only" lang-text="FrmSettings.Tab.Tools._EditTool">[Edit external tool]</span>
        </header>
        <div class="dialog-body" style="width: 33rem;">
          <div class="mb-3">
            <div class="mb-1" lang-text="_._ID">[ID]</div>
            <input type="text" name="_ToolId" class="w-100" required spellcheck="false" />
          </div>
          <div class="mb-3">
            <div class="mb-1" lang-text="_._Name">[Name]</div>
            <input type="text" name="_ToolName" class="w-100" required spellcheck="false" />
          </div>
          <div class="mb-3">
            <div class="mb-1" lang-text="_._Executable">[Executable]</div>
            <div class="d-flex align-items-center">
              <input type="text" name="_Executable" class="me-2 w-100" required spellcheck="false"
                style="width: calc(100vw - calc(var(--controlHeight) * 1px) - 0.5rem);" />
              <button id="BtnBrowseTool" type="button" class="px-1" lang-title="_._Browse">…</button>
            </div>
          </div>
          <div class="mb-3">
            <div class="mb-1" lang-text="_._Arguments">[Arguments]</div>
            <input type="text" name="_Arguments" class="w-100" spellcheck="false" value="<file>" />
          </div>
          <div class="mb-3">
            <div class="mb-1" lang-text="_._Hotkeys">[Hotkeys]</div>
            <ul class="hotkey-list mb-1"></ul>
          </div>
          <div class="mb-3 mt-4">
            <label class="ig-checkbox">
              <input type="checkbox" name="_IsIntegrated" />
              <div lang-html="FrmSettings.Tab.Tools._IntegratedWith">
                [Integrated <a href="https://github.com/ImageGlass/ImageGlass.Tools" target="_blank">ImageGlass.Tools</a>]
              </div>
            </label>
          </div>
          <div class="mt-4">
            <div class="mb-1" lang-text="_._CommandPreview">[Command preview]</div>
            <pre class="command-preview text-code px-2 py-1"><code id="Tool_CommandPreview"></code></pre>
          </div>
        </div>
        <footer class="dialog-footer">
          <button type="submit" lang-text="_._OK">[OK]</button>
          <button id="BtnCancel" type="button" lang-text="_._Cancel">[Cancel]</button>
        </footer>
      </form>`;

    this.shadowRoot.appendChild(this.#dialogEl);
  }


  /**
   * Opens tool dialog for create.
   */
  public async showCreate() {
    const defaultTool = {
      ToolId: '',
      ToolName: '',
      Executable: '',
      Arguments: _pageSettings.FILE_MACRO,
      Hotkeys: [],
      IsIntegrated: false,
    } as ITool;

    const isSubmitted = await openModalDialogEl(this.#dialogEl, 'create', defaultTool, async () => {
      this.addDialogEvents();
      this.updateToolCommandPreview();

      const hotkeyListEl = query<HTMLUListElement>('.hotkey-list', this.#dialogEl);
      await renderHotkeyListEl(hotkeyListEl, defaultTool.Hotkeys);
    });

    return isSubmitted;
  }


  /**
   * Opens tool dialog for edit.
   * @param toolId Tool ID
   */
  public async showEdit(toolId: string) {
    const trEl = query<HTMLTableRowElement>(`#Table_ToolList tr[data-toolId="${toolId}"]`);

    const hotkeysStr = query('[name="_Hotkeys"]', trEl).innerText || '';
    const toolHotkeys = hotkeysStr.split(HOTKEY_SEPARATOR).filter(Boolean);

    const tool: ITool = {
      ToolId: toolId,
      ToolName: query('[name="_ToolName"]', trEl).innerText || '',
      Executable: query('[name="_Executable"]', trEl).innerText || '',
      Arguments: query('[name="_Arguments"]', trEl).innerText || '',
      IsIntegrated: query('[name="_IsIntegrated"]', trEl).innerText === 'true',
      Hotkeys: toolHotkeys,
    };

    // open dialog
    const isSubmitted = await openModalDialogEl(this.#dialogEl, 'edit', tool, async () => {
      query<HTMLInputElement>('[name="_IsIntegrated"]', this.#dialogEl).checked = tool.IsIntegrated ?? false;
      this.addDialogEvents();
      this.updateToolCommandPreview();

      const hotkeyListEl = query<HTMLUListElement>('.hotkey-list', this.#dialogEl);
      await renderHotkeyListEl(hotkeyListEl, tool.Hotkeys);
    });

    return isSubmitted;
  }


  /**
   * Gets data from the tool dialog.
   */
  public getDialogData() {
    // get data
    const tool: ITool = {
      ToolId: query<HTMLInputElement>('[name="_ToolId"]', this.#dialogEl).value.trim(),
      ToolName: query<HTMLInputElement>('[name="_ToolName"]', this.#dialogEl).value.trim(),
      Executable: query<HTMLInputElement>('[name="_Executable"]', this.#dialogEl).value.trim(),
      Arguments: query<HTMLInputElement>('[name="_Arguments"]', this.#dialogEl).value.trim(),
      Hotkeys: queryAll('.hotkey-list > .hotkey-item > kbd', this.#dialogEl).map(el => el.innerText),
      IsIntegrated: query<HTMLInputElement>('[name="_IsIntegrated"]', this.#dialogEl).checked,
    };

    return tool;
  }


  private addDialogEvents() {
    query('[name="_Executable"]', this.#dialogEl).removeEventListener('input', this.updateToolCommandPreview, false);
    query('[name="_Executable"]', this.#dialogEl).addEventListener('input', this.updateToolCommandPreview, false);

    query('[name="_Arguments"]', this.#dialogEl).removeEventListener('input', this.updateToolCommandPreview, false);
    query('[name="_Arguments"]', this.#dialogEl).addEventListener('input', this.updateToolCommandPreview, false);

    query('#BtnBrowseTool', this.#dialogEl).removeEventListener('click', this.handleBtnBrowseToolClickEvent, false);
    query('#BtnBrowseTool', this.#dialogEl).addEventListener('click', this.handleBtnBrowseToolClickEvent, false);

    query('#BtnCancel', this.#dialogEl).removeEventListener('click', () => this.#dialogEl.close(), false);
    query('#BtnCancel', this.#dialogEl).addEventListener('click', () => this.#dialogEl.close(), false);
  }


  private updateToolCommandPreview() {
    let executable = query<HTMLInputElement>('[name="_Executable"]', this.#dialogEl).value || '';
    executable = executable.trim();

    let args = query<HTMLInputElement>('[name="_Arguments"]', this.#dialogEl).value || '';
    args = args.trim().replaceAll('<file>', '"C:\\fake dir\\photo.jpg"');

    query('#Tool_CommandPreview', this.#dialogEl).innerText = [executable, args].filter(Boolean).join(' ');
  }


  private async handleBtnBrowseToolClickEvent() {
    const filePaths = await openFilePicker() ?? [];
    if (!filePaths.length) return;

    query<HTMLInputElement>('[name="_Executable"]', this.#dialogEl).value = `"${filePaths[0]}"`;
    this.updateToolCommandPreview();
  }
}


/**
 * Creates and registers ToolDialogHtmlElement to DOM.
 */
export const defineToolDialogHtmlElement = () => window.customElements.define(
  'tool-dialog',
  ToolDialogHtmlElement,
);