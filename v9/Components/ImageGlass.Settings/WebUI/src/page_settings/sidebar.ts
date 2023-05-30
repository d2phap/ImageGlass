
/**
 * Set the active tab of Settings page.
 */
export const setActiveTab = (tabPageName: string) => {
  // hide all tabs
  const allTabPages = queryAll('.tab-page');
  allTabPages.forEach(el => el.classList.remove('active'));

  // show the selected tab
  const tabPageEl = query(`.tab-page[tab="${tabPageName}"]`);
  tabPageEl?.classList.add('active');

  // select the active nav item
  const allNavItems = queryAll('input[type="radio"]');
  allNavItems.forEach((item: HTMLInputElement) => item.checked = false);
  const navItem = query(`input[type="radio"][value="${tabPageName}"]`) as HTMLInputElement;
  if (navItem) navItem.checked = true;
};

