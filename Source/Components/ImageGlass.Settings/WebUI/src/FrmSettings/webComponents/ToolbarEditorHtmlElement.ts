import { IToolbarButton } from '@/@types/FrmSettings';
import Language from '@/common/Language';
import { arrayMoveMutable, pause } from '@/helpers';

export type TEditButtonFn = (btn: IToolbarButton) => Promise<IToolbarButton | null>;

export class ToolbarEditorHtmlElement extends HTMLElement {
  #listAvailableEl: HTMLUListElement;

  #itemsCurrent: IToolbarButton[];
  #listCurrentEl: HTMLUListElement;

  #hasChanges = false;
  #dragData: { fromSource: string, fromIndex: number } = {
    fromSource: '',
    fromIndex: -1,
  };

  #editButtonFn: TEditButtonFn;

  constructor() {
    super();

    // methods
    this.initialize = this.initialize.bind(this);
    this.loadItems = this.loadItems.bind(this);
    this.loadItemsByIds = this.loadItemsByIds.bind(this);
    this.insertItems = this.insertItems.bind(this);
    this.reloadAvailableItems = this.reloadAvailableItems.bind(this);
    this.reloadCurrentItems = this.reloadCurrentItems.bind(this);
    this.isBuiltInButton = this.isBuiltInButton.bind(this);
    this.onToolbarActionButtonClicked = this.onToolbarActionButtonClicked.bind(this);

    this.onBtnToolbarDragStart = this.onBtnToolbarDragStart.bind(this);
    this.onToolbarItemDragEnter = this.onToolbarItemDragEnter.bind(this);
    this.onToolbarItemDragOver = this.onToolbarItemDragOver.bind(this);
    this.onToolbarItemDragLeave = this.onToolbarItemDragLeave.bind(this);
    this.onToolbarItemDragEnd = this.onToolbarItemDragEnd.bind(this);
    this.onToolbarItemDrop = this.onToolbarItemDrop.bind(this);

    this.moveToolbarButton = this.moveToolbarButton.bind(this);
    this.deleteToolbarButton = this.deleteToolbarButton.bind(this);
    this.insertButtonFromAvailableList = this.insertButtonFromAvailableList.bind(this);
    this.editToolbarButton = this.editToolbarButton.bind(this);
  }

  get hasChanges() {
    return this.#hasChanges;
  }

  get currentButtons() {
    return this.#itemsCurrent;
  }

  get builtInButtons() {
    return _pageSettings.builtInToolbarButtons || [];
  }

  get availableButtons() {
    const availableItems: IToolbarButton[] = [
      { Type: 'Separator' } as IToolbarButton,
    ];

    for (let icon = 0; icon < this.builtInButtons.length; icon++) {
      const btn = this.builtInButtons[icon];
      if (this.#itemsCurrent.some(i => i.Type === 'Button' && i.Id === btn.Id)) {
        continue;
      }

      availableItems.push(btn);
    }

    return availableItems.sort((a, b) => {
      if (a.Text < b.Text) return -1;
      if (a.Text > b.Text) return 1;
      return 0;
    });
  }

  private connectedCallback() {
    this.innerHTML = `
      <div class="section-available">
        <div class="mb-1" lang-text="FrmSettings.Toolbar._AvailableButtons">[Available buttons:]</div>
        <ul class="ig-list-vertical is--no-separator toolbar-list" data-source="available">
        </ul>
      </div>
      <div class="section-middle">[Icon]</div>
      <div class="section-current">
        <div class="mb-1" lang-text="FrmSettings.Toolbar._CurrentButtons">[Current buttons:]</div>
        <ul class="ig-list-vertical is--no-separator toolbar-list" data-source="current">
        </ul>
      </div>`;

    this.#listAvailableEl = query('.section-available .toolbar-list', this);
    this.#listCurrentEl = query('.section-current .toolbar-list', this);
  }


  /**
   * Initializes and loads data into the toolbar editor.
   */
  public initialize(onEditButton: TEditButtonFn) {
    this.#editButtonFn = onEditButton.bind(this);
    this.#hasChanges = false;
    this.loadItems();

    // load icon
    query('.section-middle', this).innerHTML = _pageSettings.icons.ArrowExchange;
  }

  private loadItems(items?: IToolbarButton[]) {
    this.#itemsCurrent = items || _pageSettings.config.ToolbarButtons || [];

    this.reloadAvailableItems();
    this.reloadCurrentItems();
  }

  public loadItemsByIds(btnIds: string[]) {
    if (!Array.isArray(btnIds) || btnIds.length === 0) return;

    const items: IToolbarButton[] = [];
    btnIds.forEach(id => {
      if (id === 'Separator') {
        items.push({ Type: 'Separator' } as IToolbarButton);
      }
      else {
        const btn = this.builtInButtons.find(i => i.Id === id);
        if (btn) items.push(btn);
      }
    });

    this.loadItems(items);
    this.#hasChanges = true;
  }

  public insertItems(btn: IToolbarButton, toIndex: number) {
    this.#itemsCurrent.splice(toIndex, 0, btn);
    this.#hasChanges = true;

    // reload buttons list
    this.reloadCurrentItems(toIndex);
  }

  private reloadAvailableItems() {
    let html = '';

    this.availableButtons.forEach((item, index) => {
      let imageHtml = '';
      let textLang = item.Text;

      if (item.Type === 'Separator') {
        textLang = '_._Separator';
        imageHtml = '<div class="button-icon"></div>';
      }
      else {
        if (!textLang) {
          textLang = `FrmMain.${item.OnClick.Executable}`;
        }

        imageHtml = `<img class="button-icon" src="${item.ImageUrl}" onerror="this.style.visibility = 'hidden';" />`;
      }


      html += `
        <li class="toolbar-item" data-index="${index}">
          <div class="btn btn-toolbar" draggable="true" tabindex="0" lang-title="${textLang}">
            ${imageHtml}
            <span class="button-text" lang-text="${textLang}"></span>

            <div class="button-actions">
              <button type="button" class="btn btn--icon" lang-title="_._Add" data-action="add">
                ${_pageSettings.icons.ArrowRight}
              </button>
            </div>
          </div>
        </li>`;
    });

    // render toolbar buttons list
    this.#listAvailableEl.innerHTML = html;

    // load language
    Language.loadForEl(this.#listAvailableEl);

    // add drag/drop events
    queryAll('.btn-toolbar[draggable="true"]', this.#listAvailableEl).forEach(btnEl => {
      btnEl.removeEventListener('dragstart', this.onBtnToolbarDragStart, false);
      btnEl.addEventListener('dragstart', this.onBtnToolbarDragStart, false);
    });

    queryAll('.toolbar-item', this.#listAvailableEl).forEach(el => {
      el.removeEventListener('dragend', this.onToolbarItemDragEnd, false);
      el.addEventListener('dragend', this.onToolbarItemDragEnd, false);
    });

    // add action events
    queryAll('[data-action]', this.#listAvailableEl).forEach(el => {
      el.addEventListener('click', this.onToolbarActionButtonClicked, false);
    });
  }

  private reloadCurrentItems(focusButtonIndex = -1) {
    let html = '';

    this.#itemsCurrent.forEach((item, index) => {
      let imageHtml = '';
      let textLang = item.Text;
      const disableEditing = this.isBuiltInButton(item) || item.Type === 'Separator';

      if (item.Type === 'Separator') {
        textLang = '_._Separator';
        imageHtml = '<div class="button-icon"></div>';
      }
      else {
        if (!textLang) {
          textLang = `FrmMain.${item.OnClick.Executable}`;
        }

        imageHtml = `<img class="button-icon" src="${item.ImageUrl}" onerror="this.style.visibility = 'hidden';" />`;
      }


      html += `
        <li class="toolbar-item" data-index="${index}" data-alignment="${item.Alignment}">
          <div class="btn btn-toolbar" draggable="true" tabindex="0" lang-title="${textLang}">
            ${imageHtml}
            <span class="button-text" lang-text="${textLang}">${textLang}</span>

            <div class="button-actions">
              <button type="button" class="btn btn--icon" lang-title="_._MoveUp" data-action="move_up">
                ${_pageSettings.icons.ArrowUp}
              </button>
              <button type="button" class="btn btn--icon" lang-title="_._MoveDown" data-action="move_down">
                ${_pageSettings.icons.ArrowDown}
              </button>

              <button type="button" class="btn btn--icon" lang-title="_._Edit" data-action="edit"
                ${disableEditing ? 'disabled' : ''}>
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
    this.#listCurrentEl.innerHTML = html;

    // load language
    Language.loadForEl(this.#listCurrentEl);

    // set focus to the item
    if (focusButtonIndex >= 0) {
      const movedItem = query(`.toolbar-item[data-index="${focusButtonIndex}"]`, this.#listCurrentEl);

      if (movedItem) {
        movedItem.classList.add('drag--drop');
        pause(1000).then(() => {
          if (movedItem) movedItem.classList.remove('drag--drop');
        });

        query('.btn-toolbar', movedItem).focus();
      }
    }

    // add drag/drop events
    queryAll('.btn-toolbar[draggable="true"]', this.#listCurrentEl).forEach(btnEl => {
      btnEl.removeEventListener('dragstart', this.onBtnToolbarDragStart, false);
      btnEl.addEventListener('dragstart', this.onBtnToolbarDragStart, false);
    });

    queryAll('.toolbar-item', this.#listCurrentEl).forEach(el => {
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

    // add action events
    queryAll('[data-action]', this.#listCurrentEl).forEach(el => {
      el.addEventListener('click', this.onToolbarActionButtonClicked, false);
    });
  }

  private isBuiltInButton(btn: IToolbarButton) {
    return this.builtInButtons.some(i => i.Id === btn.Id);
  }


  private onBtnToolbarDragStart(e: DragEvent) {
    const btnEl = e.target as HTMLButtonElement;
    const fromIndex = +btnEl.parentElement.getAttribute('data-index');
    const source = btnEl.closest('.toolbar-list').getAttribute('data-source');

    // set drag data
    this.#dragData = { fromSource: source, fromIndex };
    e.dataTransfer.effectAllowed = 'move';

    // set custom drag image
    e.dataTransfer.setDragImage(btnEl, -20, 0);

    // disable child el to receive drag events
    queryAll('.btn-toolbar[draggable="true"]', this.#listCurrentEl).forEach(el => {
      el.style.pointerEvents = 'none';
    });
    btnEl.style.pointerEvents = '';
    btnEl.style.opacity = '0.4';
  }

  private onToolbarItemDragEnter(e: DragEvent, dropEl: HTMLElement) {
    e.preventDefault();
    const { fromSource, fromIndex } = this.#dragData;
    const toIndex = +dropEl.getAttribute('data-index');
    const toSource = dropEl.closest('.toolbar-list').getAttribute('data-source');
    if (fromIndex === toIndex) return;

    const cssClass = ['drag--enter'];
    if (fromIndex < toIndex && fromSource === toSource) {
      cssClass.push('position--after');
    }

    dropEl.classList.add(...cssClass);
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
    queryAll('.btn-toolbar[draggable="true"]').forEach(el => {
      el.style.pointerEvents = '';
      el.style.opacity = '1';
    });
  }

  private onToolbarItemDrop(e: DragEvent, toDropEl: HTMLElement) {
    e.preventDefault();
    toDropEl.classList.remove('drag--enter', 'position--after');
    if (this.#dragData.fromIndex === -1) return;

    const { fromSource, fromIndex } = this.#dragData;
    const toIndex = +toDropEl.getAttribute('data-index');
    const toSource = toDropEl.closest('.toolbar-list').getAttribute('data-source');
    this.#dragData = { fromSource: '', fromIndex: -1 };

    // move item
    if (fromSource === toSource) {
      this.moveToolbarButton(fromIndex, toIndex);
    }
    // add item
    else {
      this.insertButtonFromAvailableList(fromIndex, toIndex);
    }
  }


  private onToolbarActionButtonClicked(e: Event) {
    e.stopPropagation();
    e.preventDefault();

    const el = e.target as HTMLButtonElement;
    const action = el.getAttribute('data-action').toLocaleLowerCase();
    const btnIndex = +el.closest('.toolbar-item').getAttribute('data-index');

    if (action === 'move_up') {
      this.moveToolbarButton(btnIndex, btnIndex - 1);
    }
    else if (action === 'move_down') {
      this.moveToolbarButton(btnIndex, btnIndex + 1);
    }
    else if (action === 'edit') {
      this.editToolbarButton(btnIndex);
    }
    else if (action === 'delete') {
      this.deleteToolbarButton(btnIndex);
    }
    // add a button from the available buttons list to the current list
    else if (action === 'add') {
      this.insertButtonFromAvailableList(btnIndex);
    }
  }

  private moveToolbarButton(fromIndex: number, toIndex: number) {
    if (toIndex < 0 ) toIndex = 0;
    else if (toIndex > this.#itemsCurrent.length - 1) toIndex = this.#itemsCurrent.length - 1;
    if (fromIndex === toIndex) return;

    // move button
    arrayMoveMutable(this.#itemsCurrent, fromIndex, toIndex);
    this.#hasChanges = true;

    // reload buttons list
    this.reloadCurrentItems(toIndex);

    // set focus to the action button
    const action = fromIndex < toIndex ? 'move_down' : 'move_up';
    query(`.toolbar-item[data-index="${toIndex}"] [data-action="${action}"]`)?.focus();
  }

  private deleteToolbarButton(btnIndex: number) {
    // remove button
    this.#itemsCurrent.splice(btnIndex, 1);
    this.#hasChanges = true;

    // reload buttons list
    this.reloadCurrentItems(btnIndex - 1);
    this.reloadAvailableItems();
  }

  private insertButtonFromAvailableList(availableBtnIndex: number, toIndex?: number) {
    const btn = this.availableButtons[availableBtnIndex];
    if (!btn) return;

    toIndex ??= this.#itemsCurrent.length;
    this.#itemsCurrent.splice(toIndex, 0, btn);
    this.#hasChanges = true;

    // reload buttons list
    this.reloadCurrentItems(toIndex);
    this.reloadAvailableItems();
  }

  private async editToolbarButton(btnIndex: number) {
    const btn = this.#itemsCurrent[btnIndex];
    if (!btn || !this.#editButtonFn) return;

    const newBtn = await Promise.resolve(this.#editButtonFn({
      ...btn,
      ImageUrl: undefined,
    }));
    if (!newBtn) return;

    this.#itemsCurrent[btnIndex] = newBtn;
    this.#hasChanges = true;
    this.reloadCurrentItems(btnIndex);
  }
}


/**
 * Creates and registers ToolbarEditorHtmlElement to DOM.
 */
export const defineToolbarEditorHtmlElement = () => window.customElements.define(
  'toolbar-editor',
  ToolbarEditorHtmlElement,
);
