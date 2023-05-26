import { setActiveTab } from './page_settings/sidebar';

// navigation bar event
const navItems = Array.from(document.querySelectorAll('input[name="nav"]'));
for (let i = 0; i < navItems.length; i++) {
  const itemEl = navItems[i] as HTMLInputElement;

  itemEl.addEventListener('change', (e) => {
    const activeTabName = (e.target as HTMLInputElement).value;
    setActiveTab(activeTabName);
  }, false);
}

setActiveTab('image');


// export to global
// @ts-ignore
window.setActiveTab = setActiveTab;
