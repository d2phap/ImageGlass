import { IToolbarButton } from '@/@types/FrmSettings';

export class ToolbarEditorHtmlElement extends HTMLElement {
  #listEl: HTMLUListElement;

  constructor() {
    super();

    // public methods
    this.loadItems = this.loadItems.bind(this);

    // private methods
    this.addEvents = this.addEvents.bind(this);
    this.onAddBtnClicked = this.onAddBtnClicked.bind(this);
    this.onToolBarActionButtonClicked = this.onToolBarActionButtonClicked.bind(this);
  }


  private connectedCallback() {
    this.innerHTML = `
      <div class="toolbar-editor">
        <div class="toolbar-actions">
          <button id="BtnAddToolbarButton" type="button" lang-text="_._Add">
            [Add...]
          </button>
        </div>
        <ul class="ig-list-vertical is--no-separator toolbar-list">
        </ul>
      </div>`;

    this.#listEl = query('.toolbar-list', this);
    this.addEvents();
  }


  public loadItems(items: IToolbarButton[]) {
    let html = '';

    items.forEach(item => {
      let imageHtml = '';
      let textLang = item.Text;

      if (item.Type === 'Separator') {
        textLang = 'FrmSetting._Separator';
        imageHtml = '<div class="button-icon"></div>';
      }
      else {
        if (!textLang) {
          textLang = `FrmMain.${item.OnClick.Executable}`;
        }

        imageHtml = `<img class="button-icon" src="${item.Image}" onerror="this.style.visibility = 'hidden';" />`;
      }


      html += `
        <li data-id="${item.Id}" data-type="${item.Type}">
          <div tabindex="0" class="btn btn-toolbar" lang-title="${textLang}">
            ${imageHtml}
            <span class="button-text" lang-text="${textLang}"></span>

            <div class="button-actions">
              <button type="button" class="btn btn--icon" lang-title="_._MoveUp" data-action="moveUp">
                ${_pageSettings.icons.ArrowUp}
              </button>
              <button type="button" class="btn btn--icon" lang-title="_._MoveDown" data-action="moveDown">
                ${_pageSettings.icons.ArrowDown}
              </button>

              <button type="button" class="btn btn--icon" lang-title="_._Edit" data-action="edit">
                ${_pageSettings.icons.Edit}
              </button>
              <button type="button" class="btn btn--icon" lang-title="_._Delete" data-action="delete">
                ${_pageSettings.icons.Delete}
              </button>
            </div>
          </div>
        </li>`;
    });

    this.#listEl.innerHTML = html;
  }


  private addEvents() {
    query('#BtnAddToolbarButton', this).addEventListener('click', this.onAddBtnClicked, false);
    queryAll('.toolbar-list [data-action]', this).forEach(el => {
      el.addEventListener('click', this.onToolBarActionButtonClicked, false);
    });
  }

  private onAddBtnClicked() {
    //
  }

  private onToolBarActionButtonClicked(e: Event) {
    e.stopPropagation();
    e.preventDefault();

    const el = e.target as HTMLButtonElement;
    const action = el.getAttribute('data-action');

    console.log(action);
  }
}


/**
 * Creates and registers ToolbarEditorHtmlElement to DOM.
 */
export const defineToolbarEditorHtmlElement = () => window.customElements.define(
  'toolbar-editor',
  ToolbarEditorHtmlElement,
);
