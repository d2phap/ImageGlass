
/**
 * Set the active tab of Settings page.
 */
export const setActiveTab = (tabPageName: string) => {
  // hide all tabs
  const allTabPages = document.querySelectorAll('.tab-page');
  allTabPages.forEach(el => el.classList.remove('active'));

  // show the selected tab
  const tabPageEl = document.querySelector(`.tab-page[tab="${tabPageName}"]`);
  tabPageEl?.classList.add('active');

  // select the active nav item
  const allNavItems = document.querySelectorAll('input[type="radio"]');
  allNavItems.forEach((item: HTMLInputElement) => item.checked = false);
  const navItem = document.querySelector(`input[type="radio"][value="${tabPageName}"]`) as HTMLInputElement;
  if (navItem) navItem.checked = true;
};

