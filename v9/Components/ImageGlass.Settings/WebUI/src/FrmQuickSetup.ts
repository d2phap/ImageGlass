import './main';


const onButtonClicked = (e: Event) => {
  e.preventDefault();
  e.stopPropagation();
  const btnEl = e.target as HTMLButtonElement;

  post(btnEl.id);
};

query('#BtnImageGlassStore').addEventListener('click', onButtonClicked, false);
query('#BtnUpdate').addEventListener('click', onButtonClicked, false);
query('#BtnClose').addEventListener('click', onButtonClicked, false);


window._page.loadData = (data: Record<string, any> = {}) => {
  // load data
};

