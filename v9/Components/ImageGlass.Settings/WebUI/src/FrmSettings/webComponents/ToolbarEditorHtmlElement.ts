import { IToolbarButton } from '@/@types/FrmSettings';
import Language from '@/common/Language';
import { arrayMoveMutable, pause } from '@/helpers';

export class ToolbarEditorHtmlElement extends HTMLElement {
  #items: IToolbarButton[];
  #listEl: HTMLUListElement;
  #dragIndex = -1;

  constructor() {
    super();

    // public methods
    this.loadItems = this.loadItems.bind(this);

    // private methods
    this.addEvents = this.addEvents.bind(this);
    this.onAddBtnClicked = this.onAddBtnClicked.bind(this);
    this.onToolBarActionButtonClicked = this.onToolBarActionButtonClicked.bind(this);

    this.onBtnToolbarDragStart = this.onBtnToolbarDragStart.bind(this);
    this.onToolbarItemDragEnter = this.onToolbarItemDragEnter.bind(this);
    this.onToolbarItemDragOver = this.onToolbarItemDragOver.bind(this);
    this.onToolbarItemDragLeave = this.onToolbarItemDragLeave.bind(this);
    this.onToolbarItemDragEnd = this.onToolbarItemDragEnd.bind(this);
    this.onToolbarItemDrop = this.onToolbarItemDrop.bind(this);
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
    this.#items = items;
    let html = '';

    this.#items.forEach((item, index) => {
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
        <li class="toolbar-item" data-index="${index}">
          <div class="btn btn-toolbar" tabindex="0" draggable="true" lang-title="${textLang}">
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


    // add drag/drop events
    queryAll('.btn-toolbar[draggable="true"]', this.#listEl).forEach(btnEl => {
      btnEl.removeEventListener('dragstart', this.onBtnToolbarDragStart, false);
      btnEl.addEventListener('dragstart', this.onBtnToolbarDragStart, false);
    });

    queryAll('.toolbar-item', this.#listEl).forEach(el => {
      el.removeEventListener('dragover', this.onToolbarItemDragOver, false);
      el.addEventListener('dragover', this.onToolbarItemDragOver, false);

      el.removeEventListener('dragenter', (e) => this.onToolbarItemDragEnter(e, el), false);
      el.addEventListener('dragenter', (e) => this.onToolbarItemDragEnter(e, el), false);

      el.removeEventListener('dragleave', (e) => this.onToolbarItemDragLeave(e, el), false);
      el.addEventListener('dragleave', (e) => this.onToolbarItemDragLeave(e, el), false);
      el.removeEventListener('dragend', this.onToolbarItemDragEnd, false);
      el.addEventListener('dragend', this.onToolbarItemDragEnd, false);

      el.removeEventListener('drop', (e) => this.onToolbarItemDrop(e, el), false);
      el.addEventListener('drop', (e) => this.onToolbarItemDrop(e, el), false);
    });
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


  private onBtnToolbarDragStart(e: DragEvent) {
    const btnEl = e.target as HTMLButtonElement;
    const fromIndex = +btnEl.parentElement.getAttribute('data-index');

    // set drag data
    this.#dragIndex = fromIndex;
    e.dataTransfer.effectAllowed = 'move';

    // set custom drag image
    e.dataTransfer.setDragImage(btnEl, -20, 0);

    // disable child el to receive drag events
    queryAll('.btn-toolbar[draggable="true"]', this.#listEl).forEach(el => {
      el.style.pointerEvents = 'none';
    });
    btnEl.style.pointerEvents = '';
  }

  private onToolbarItemDragEnter(e: DragEvent, dropEL: HTMLElement) {
    e.preventDefault();
    const fromIndex = this.#dragIndex;
    const toIndex = +dropEL.getAttribute('data-index');
    if (fromIndex === toIndex) return;

    const cssClass = ['drag--enter'];
    if (fromIndex < toIndex) {
      cssClass.push('position--after');
    }

    dropEL.classList.add(...cssClass);
  }

  private onToolbarItemDragOver(e: DragEvent) {
    e.preventDefault();
    e.dataTransfer.dropEffect = 'move';
  }

  private onToolbarItemDragLeave(e: DragEvent, dropEL: HTMLElement) {
    e.preventDefault();
    dropEL.classList.remove('drag--enter');
  }

  private onToolbarItemDragEnd(e: DragEvent) {
    e.preventDefault();

    // re-enable child el to receive drag events
    queryAll('.btn-toolbar[draggable="true"]', this.#listEl).forEach(el => {
      el.style.pointerEvents = '';
    });
  }

  private onToolbarItemDrop(e: DragEvent, toDropEl: HTMLElement) {
    e.preventDefault();
    toDropEl.classList.remove('drag--enter');
    if (this.#dragIndex === -1) return;

    const fromIndex = this.#dragIndex;
    const toIndex = +toDropEl.getAttribute('data-index');
    this.#dragIndex = -1;
    if (fromIndex === toIndex) return;

    // move item
    arrayMoveMutable(this.#items, fromIndex, toIndex);

    // reload buttons list
    this.loadItems(this.#items);
    Language.loadForEl(this.#listEl);

    // set focus to the moved item
    const droppedItem = query(`.toolbar-item[data-index="${toIndex}"]`);
    droppedItem.classList.add('drag--drop');
    pause(1000).then(() => droppedItem.classList.remove('drag--drop'));
    query('.btn-toolbar', droppedItem).focus();
  }
}


/**
 * Creates and registers ToolbarEditorHtmlElement to DOM.
 */
export const defineToolbarEditorHtmlElement = () => window.customElements.define(
  'toolbar-editor',
  ToolbarEditorHtmlElement,
);
