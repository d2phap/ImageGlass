import { IToolbarButton } from '@/@types/FrmSettings';
import Language from '@/common/Language';
import { arrayMoveMutable, pause } from '@/helpers';

export class ToolbarEditorHtmlElement extends HTMLElement {
  #items: IToolbarButton[];
  #listEl: HTMLUListElement;
  #dragIndex = -1;
  #hasChanges = false;

  constructor() {
    super();

    // private methods
    this.reloadItems = this.reloadItems.bind(this);
    this.addEvents = this.addEvents.bind(this);
    this.onBtnAddToolbarButtonClicked = this.onBtnAddToolbarButtonClicked.bind(this);
    this.onToolbarActionButtonClicked = this.onToolbarActionButtonClicked.bind(this);

    this.onBtnToolbarDragStart = this.onBtnToolbarDragStart.bind(this);
    this.onToolbarItemDragEnter = this.onToolbarItemDragEnter.bind(this);
    this.onToolbarItemDragOver = this.onToolbarItemDragOver.bind(this);
    this.onToolbarItemDragLeave = this.onToolbarItemDragLeave.bind(this);
    this.onToolbarItemDragEnd = this.onToolbarItemDragEnd.bind(this);
    this.onToolbarItemDrop = this.onToolbarItemDrop.bind(this);

    this.moveToolbarButton = this.moveToolbarButton.bind(this);
    this.deleteToolbarButton = this.deleteToolbarButton.bind(this);
  }

  get hasChanges() {
    return this.#hasChanges;
  }

  get items() {
    return this.#items;
  }

  set items(value: IToolbarButton[]) {
    this.#items = value;
    this.reloadItems();
    this.#hasChanges = false;
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


  private reloadItems(focusButtonIndex = -1) {
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
              <button type="button" class="btn btn--icon" lang-title="_._MoveUp" data-action="move_up">
                ${_pageSettings.icons.ArrowUp}
              </button>
              <button type="button" class="btn btn--icon" lang-title="_._MoveDown" data-action="move_down">
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

    // render toolbar buttons list
    this.#listEl.innerHTML = html;

    // load language
    Language.loadForEl(this.#listEl);

    // set focus to the item
    if (focusButtonIndex >= 0) {
      const movedItem = query(`.toolbar-item[data-index="${focusButtonIndex}"]`, this.#listEl);

      if (movedItem) {
        movedItem.classList.add('drag--drop');
        pause(1000).then(() => {
          if (movedItem) movedItem.classList.remove('drag--drop');
        });

        query('.btn-toolbar', movedItem).focus();
      }
    }


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

    queryAll('.toolbar-list [data-action]', this).forEach(el => {
      el.addEventListener('click', this.onToolbarActionButtonClicked, false);
    });
  }


  private addEvents() {
    query('#BtnAddToolbarButton', this).addEventListener('click', this.onBtnAddToolbarButtonClicked, false);
  }

  private onBtnAddToolbarButtonClicked() {
    //
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
    dropEL.classList.remove('drag--enter', 'position--after');
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
    toDropEl.classList.remove('drag--enter', 'position--after');
    if (this.#dragIndex === -1) return;

    const fromIndex = this.#dragIndex;
    const toIndex = +toDropEl.getAttribute('data-index');
    this.#dragIndex = -1;

    // move item
    this.moveToolbarButton(fromIndex, toIndex);
  }


  private onToolbarActionButtonClicked(e: Event) {
    e.stopPropagation();
    e.preventDefault();

    const el = e.target as HTMLButtonElement;
    const action = el.getAttribute('data-action').toLocaleLowerCase();
    const toolbarIndex = +el.closest('.toolbar-item').getAttribute('data-index');

    if (action === 'move_up') {
      this.moveToolbarButton(toolbarIndex, toolbarIndex - 1);
    }
    else if (action === 'move_down') {
      this.moveToolbarButton(toolbarIndex, toolbarIndex + 1);
    }
    else if (action === 'edit') {
      //
    }
    else if (action === 'delete') {
      this.deleteToolbarButton(toolbarIndex);
    }
  }

  private moveToolbarButton(fromIndex: number, toIndex: number) {
    if (toIndex < 0 ) toIndex = 0;
    else if (toIndex > this.#items.length - 1) toIndex = this.#items.length - 1;
    if (fromIndex === toIndex) return;

    // move button
    arrayMoveMutable(this.#items, fromIndex, toIndex);
    this.#hasChanges = true;

    // reload buttons list
    this.reloadItems(toIndex);

    // set focus to the action button
    const action = fromIndex < toIndex ? 'move_down' : 'move_up';
    query(`.toolbar-item[data-index="${toIndex}"] [data-action="${action}"]`)?.focus();
  }

  private deleteToolbarButton(toolbarIndex: number) {
    // remove button
    this.#items.splice(toolbarIndex, 1);
    this.#hasChanges = true;

    // reload buttons list
    this.reloadItems(toolbarIndex - 1);
  }
}


/**
 * Creates and registers ToolbarEditorHtmlElement to DOM.
 */
export const defineToolbarEditorHtmlElement = () => window.customElements.define(
  'toolbar-editor',
  ToolbarEditorHtmlElement,
);
