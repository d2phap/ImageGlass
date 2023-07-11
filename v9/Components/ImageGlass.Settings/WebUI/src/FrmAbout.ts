import './main';


const onButtonClicked = (e: Event) => {
  e.preventDefault();
  e.stopPropagation();
  const btnEl = e.target as HTMLButtonElement;

  post(btnEl.id);
};

query('#BtnImageGlassStore').addEventListener('click', onButtonClicked, false);
query('#BtnCheckForUpdate').addEventListener('click', onButtonClicked, false);
query('#BtnDonate').addEventListener('click', onButtonClicked, false);
query('#BtnClose').addEventListener('click', onButtonClicked, false);

query('#BtnCheckForUpdate').focus();
