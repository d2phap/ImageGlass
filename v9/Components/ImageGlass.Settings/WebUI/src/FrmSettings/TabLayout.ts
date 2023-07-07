import { getChangedSettingsFromTab } from '@/helpers';

export default class TabLayout {
  /**
   * Loads settings for tab Layout.
   */
  static loadSettings() {
  }


  /**
   * Adds events for tab Layout.
   */
  static addEvents() {
    queryAll('[tab="layout"] .app-layout button[draggable="true"]').forEach(btnEl => {
      btnEl.removeEventListener('dragstart', TabLayout.handleLayoutItemDragStart, false);
      btnEl.addEventListener('dragstart', TabLayout.handleLayoutItemDragStart, false);
    });

    queryAll('[tab="layout"] .app-layout .region-drop').forEach(el => {
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


  /**
   * Save settings as JSON object.
   */
  static exportSettings() {
    return getChangedSettingsFromTab('layout');
  }


  private static handleLayoutItemDragStart(e: DragEvent) {
    const btnEl = e.target as HTMLButtonElement;
    const fromControlName = btnEl.getAttribute('data-control');
    const fromPosition = btnEl.closest('[data-position]').getAttribute('data-position');
    const fromOrder = btnEl.closest('[data-order]').getAttribute('data-order');

    const data = JSON.stringify({ fromControlName, fromPosition, fromOrder });
    e.dataTransfer.setData('application/json', data);
    e.dataTransfer.effectAllowed = 'move';

    // don't allow to drop toolbar to left/right position
    if (fromControlName.toLowerCase().includes('toolbar')) {
      query('[data-position="Left"] .region-drop').classList.add('nodrop');
      query('[data-position="Right"] .region-drop').classList.add('nodrop');
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

    query('[data-position="Left"] .region-drop').classList.remove('nodrop');
    query('[data-position="Right"] .region-drop').classList.remove('nodrop');

    // re-enable child el to receive drag events
    queryAll('.app-layout button[draggable="true"]').forEach(el => {
      el.style.pointerEvents = '';
    });
  }


  private static handleLayoutItemDrop(e: DragEvent, toDropEl: HTMLElement) {
    e.preventDefault();
    toDropEl.classList.remove('drag--enter');

    const toBtnEl = toDropEl.querySelector('button[draggable="true"]') as (HTMLButtonElement | null);
    const toControlName = toBtnEl?.getAttribute('data-control') || '';

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


    const fromDropEl = query(`.app-layout [data-position="${fromPosition}"] .region-drop[data-order="${fromOrder}"]`) as HTMLElement;
    const fromBtnEl = fromDropEl.querySelector('button[draggable="true"]') as HTMLButtonElement;

    // if the drop region is already occupied
    if (toBtnEl) {
      // swap the elements
      fromDropEl?.appendChild(toBtnEl);

      query<HTMLSelectElement>(`select[name="_Layout.${toControlName}.Position"]`).value = fromPosition;
      query<HTMLInputElement>(`input[name="_Layout.${toControlName}.Order"]`).value = fromOrder;
    }

    // drop the btn to the drop target
    toDropEl.appendChild(fromBtnEl);

    query<HTMLSelectElement>(`select[name="_Layout.${fromControlName}.Position"]`).value = position;
    query<HTMLInputElement>(`input[name="_Layout.${fromControlName}.Order"]`).value = order;
  }
}
