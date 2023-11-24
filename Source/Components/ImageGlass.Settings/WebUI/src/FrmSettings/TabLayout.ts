import { getChangedSettingsFromTab } from '@/helpers';
import Language from '../common/Language';

type LayoutControlName = 'Toolbar' | 'ToolbarContext' | 'Gallery';
type ILayoutObject = Record<LayoutControlName, {
  position: string,
  order: string,
}>;

export default class TabLayout {
  private static get defaultLayout(): ILayoutObject {
    return {
      Toolbar:        { position: 'Top',    order: '0' },
      ToolbarContext: { position: 'Bottom', order: '1' },
      Gallery:        { position: 'Bottom', order: '0' },
    };
  }


  /**
   * Loads settings for tab Layout.
   */
  static loadSettings() {
    TabLayout.loadLayoutConfigs();
  }


  /**
   * Adds events for tab Layout.
   */
  static addEvents() {
    query('[name="_Layout.Toolbar.Position"]').removeEventListener('change', TabLayout.handleLayoutInputsChanged, false);
    query('[name="_Layout.Toolbar.Position"]').addEventListener('change', TabLayout.handleLayoutInputsChanged, false);
    query('[name="_Layout.Toolbar.Order"]').removeEventListener('change', TabLayout.handleLayoutInputsChanged, false);
    query('[name="_Layout.Toolbar.Order"]').addEventListener('change', TabLayout.handleLayoutInputsChanged, false);

    query('[name="_Layout.ToolbarContext.Position"]').removeEventListener('change', TabLayout.handleLayoutInputsChanged, false);
    query('[name="_Layout.ToolbarContext.Position"]').addEventListener('change', TabLayout.handleLayoutInputsChanged, false);
    query('[name="_Layout.ToolbarContext.Order"]').removeEventListener('change', TabLayout.handleLayoutInputsChanged, false);
    query('[name="_Layout.ToolbarContext.Order"]').addEventListener('change', TabLayout.handleLayoutInputsChanged, false);

    query('[name="_Layout.Gallery.Position"]').removeEventListener('change', TabLayout.handleLayoutInputsChanged, false);
    query('[name="_Layout.Gallery.Position"]').addEventListener('change', TabLayout.handleLayoutInputsChanged, false);
    query('[name="_Layout.Gallery.Order"]').removeEventListener('change', TabLayout.handleLayoutInputsChanged, false);
    query('[name="_Layout.Gallery.Order"]').addEventListener('change', TabLayout.handleLayoutInputsChanged, false);
  }


  /**
   * Save settings as JSON object.
   */
  static exportSettings() {
    const settings = getChangedSettingsFromTab('layout');
    const oldLayout = TabLayout.convertRawLayoutToLayoutObject();

    // get layout settings
    let hasChanged = false;
    const layout: Record<string, string> = {};
    TabLayout.getLayoutSettingObjectFromDOM(item => {
      if (oldLayout[item.controlName].position !== item.position) hasChanged = true;
      if (oldLayout[item.controlName].order !== item.order) hasChanged = true;

      layout[item.controlName] = [item.position, item.order].join(';');
    });

    if (hasChanged) settings.Layout = layout;

    return settings;
  }


  private static loadLayoutConfigs() {
    const layout = TabLayout.convertRawLayoutToLayoutObject();

    Object.keys(layout).forEach((controlName: LayoutControlName) => {
      const { position, order } = layout[controlName];

      query<HTMLSelectElement>(`[name="_Layout.${controlName}.Position"]`).value = position;
      query<HTMLSelectElement>(`[name="_Layout.${controlName}.Order"]`).value = order;
    });

    TabLayout.loadLayoutMapDOM(layout);
  }


  private static loadLayoutMapDOM(layout?: ILayoutObject) {
    layout ||= TabLayout.getLayoutSettingObjectFromDOM();

    // clear all DOM buttons
    queryAll('.app-layout .region-drop[data-order]').forEach(el => el.innerHTML = '');

    for (const controlName in layout) {
      if (!Object.prototype.hasOwnProperty.call(layout, controlName)) continue;
      const item = layout[controlName as LayoutControlName];

      const regionEl = query(`.app-layout [data-position="${item.position}"i] .region-drop[data-order="${item.order}"]`);
      if (regionEl === null) continue;

      regionEl.innerHTML = `
        <button draggable="true" tabindex="-1" data-control="${controlName}">
          <span lang-text="FrmSettings.Layout._${controlName}">[${controlName}]</span>
        </button>`;
    }

    Language.loadFor('.app-layout');

    // add drag/drop events
    queryAll('.app-layout button[draggable="true"]').forEach(btnEl => {
      btnEl.removeEventListener('dragstart', TabLayout.handleLayoutItemDragStart, false);
      btnEl.addEventListener('dragstart', TabLayout.handleLayoutItemDragStart, false);
    });

    queryAll('.app-layout .region-drop').forEach(el => {
      el.removeEventListener('dragover', (e) => TabLayout.handleLayoutItemDragOver(e), false);
      el.addEventListener('dragover', (e) => TabLayout.handleLayoutItemDragOver(e), false);

      el.removeEventListener('dragenter', (e) => TabLayout.handleLayoutItemDragEnter(e, el), false);
      el.addEventListener('dragenter', (e) => TabLayout.handleLayoutItemDragEnter(e, el), false);

      el.removeEventListener('dragleave', (e) => TabLayout.handleLayoutItemDragLeave(e, el), false);
      el.addEventListener('dragleave', (e) => TabLayout.handleLayoutItemDragLeave(e, el), false);
      el.removeEventListener('dragend', (e) => TabLayout.handleLayoutItemDragEnd(e), false);
      el.addEventListener('dragend', (e) => TabLayout.handleLayoutItemDragEnd(e), false);

      el.removeEventListener('drop', (e) => TabLayout.handleLayoutItemDrop(e, el), false);
      el.addEventListener('drop', (e) => TabLayout.handleLayoutItemDrop(e, el), false);
    });
  }


  private static convertRawLayoutToLayoutObject(rawLayout?: Partial<Record<LayoutControlName, string>>): ILayoutObject {
    rawLayout ||= _pageSettings.config?.Layout || {};
    const layout = TabLayout.defaultLayout;

    for (const key in layout) {
      if (!Object.prototype.hasOwnProperty.call(layout, key)) continue;
      const controlName = key as LayoutControlName;

      const arr = rawLayout[controlName]?.split(';').filter(Boolean) || [];
      const position = arr[0] ?? layout[controlName].position;
      const order = arr[1] ?? layout[controlName].order;

      layout[controlName] = { position, order };
    }

    return layout as ILayoutObject;
  }


  private static getLayoutSettingObjectFromDOM(callbackFn?: (item: {
    controlName: LayoutControlName,
    position: string,
    order: string,
  }) => any): ILayoutObject {
    const layout: Partial<ILayoutObject> = {};

    ['Toolbar', 'ToolbarContext', 'Gallery'].forEach((controlName: LayoutControlName) => {
      const position = query<HTMLSelectElement>(`[name="_Layout.${controlName}.Position"]`).value || '0';
      const order = query<HTMLSelectElement>(`[name="_Layout.${controlName}.Order"]`).value || '0';
      if (callbackFn) callbackFn({ controlName, position, order });

      layout[controlName] = { position, order };
    });

    return layout as ILayoutObject;
  }


  private static handleLayoutInputsChanged(e: Event) {
    e.preventDefault();

    const layout = TabLayout.getLayoutSettingObjectFromDOM();
    TabLayout.loadLayoutMapDOM(layout);

    // hide gallery order if the position is either left / right
    const hideGalleryOrder = layout.Gallery.position === 'Left' || layout.Gallery.position === 'Right';
    query('#Section_LayoutGalleryOrder').toggleAttribute('hidden', hideGalleryOrder);
  }


  private static handleLayoutItemDragStart(e: DragEvent) {
    const btnEl = e.target as HTMLButtonElement;
    const fromControlName = btnEl.getAttribute('data-control');
    const fromPosition = btnEl.closest('[data-position]').getAttribute('data-position');
    const fromOrder = btnEl.closest('[data-order]').getAttribute('data-order');

    const data = JSON.stringify({ fromControlName, fromPosition, fromOrder });
    e.dataTransfer.setData('application/json', data);
    e.dataTransfer.effectAllowed = 'move';

    // set custom drag image
    const btnContentEl = btnEl.querySelector('*') as HTMLElement;
    e.dataTransfer.setDragImage(btnContentEl, -20, 0);

    // don't allow to drop toolbar to left/right position
    if (fromControlName.toLowerCase().includes('toolbar')) {
      query('[data-position="Left"i] .region-drop').classList.add('nodrop');
      query('[data-position="Right"i] .region-drop').classList.add('nodrop');
    }

    // disable child el to receive drag events
    queryAll('.app-layout button[draggable="true"]').forEach(el => {
      el.style.pointerEvents = 'none';
    });
    btnEl.style.pointerEvents = '';
  }


  private static handleLayoutItemDragOver(e: DragEvent) {
    e.preventDefault();
    e.dataTransfer.dropEffect = 'move';
  }


  private static handleLayoutItemDragEnter(e: DragEvent, dropEL: HTMLElement) {
    e.preventDefault();
    dropEL.classList.add('drag--enter');
  }

  
  private static handleLayoutItemDragLeave(e: DragEvent, dropEL: HTMLElement) {
    e.preventDefault();
    dropEL.classList.remove('drag--enter');
  }


  private static handleLayoutItemDragEnd(e: DragEvent) {
    e.preventDefault();

    query('[data-position="Left"i] .region-drop').classList.remove('nodrop');
    query('[data-position="Right"i] .region-drop').classList.remove('nodrop');

    // re-enable child el to receive drag events
    queryAll('.app-layout button[draggable="true"]').forEach(el => {
      el.style.pointerEvents = '';
    });
  }


  private static handleLayoutItemDrop(e: DragEvent, toDropEl: HTMLElement) {
    e.stopImmediatePropagation();
    e.preventDefault();
    toDropEl.classList.remove('drag--enter');

    const toBtnEl = toDropEl.querySelector('button[draggable="true"]') as (HTMLButtonElement | null);
    const toControlName = (toBtnEl?.getAttribute('data-control') || '') as LayoutControlName;

    // get the drop data
    const jsonData = e.dataTransfer.getData('application/json') || '{}';
    const { fromControlName, fromPosition, fromOrder } = JSON.parse(jsonData) || {};
    if (!fromControlName || !fromPosition || !fromOrder) return;

    // get the new drop data
    const position = toDropEl.closest('[data-position]').getAttribute('data-position');
    const order = toDropEl.getAttribute('data-order');


    // don't allow to drop toolbar to left/right position
    if ((fromControlName.toLowerCase().includes('toolbar') && (position === 'Left' || position === 'Right'))
      || toControlName.toLowerCase().includes('toolbar') && (fromPosition === 'Left' || fromPosition === 'Right')) {
      return;
    }


    const fromDropEl = query(`.app-layout [data-position="${fromPosition}"i] .region-drop[data-order="${fromOrder}"]`) as HTMLElement;
    const fromBtnEl = fromDropEl.querySelector('button[draggable="true"]') as HTMLButtonElement;

    // if the drop region is already occupied
    if (toBtnEl) {
      // swap the elements
      fromDropEl?.appendChild(toBtnEl);

      query<HTMLSelectElement>(`[name="_Layout.${toControlName}.Position"]`).value = fromPosition;
      query<HTMLSelectElement>(`[name="_Layout.${toControlName}.Order"]`).value = fromOrder;
    }

    // drop the btn to the drop target
    toDropEl.appendChild(fromBtnEl);

    query<HTMLSelectElement>(`[name="_Layout.${fromControlName}.Position"]`).value = position;
    query<HTMLSelectElement>(`[name="_Layout.${fromControlName}.Order"]`).value = order;

    // hide gallery order if the position is either left / right
    if (fromControlName === 'Gallery') {
      const hideGalleryOrder = position === 'Left' || position === 'Right';
      query('#Section_LayoutGalleryOrder').toggleAttribute('hidden', hideGalleryOrder);
    }
  }
}
