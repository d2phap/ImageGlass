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

query('#BtnUpdate').focus();


window._page.loadData = (data: Record<string, any> = {}) => {
  console.info('🔵 Loading data', data);

  query('#Lbl_CurrentVersion').innerText = data.CurrentVersion || '';
  query('#LbL_LatestVersion').innerText = data.LatestVersion || '';
  query('#Lbl_PublishedDate').innerText = data.PublishedDate || '';
  query('#Lnk_ReleaseLink').innerText = data.ReleaseTitle || '';
  query('#Lnk_ReleaseLink').setAttribute('href', data.ReleaseLink || '');
  query('#Section_ReleaseDetails').innerHTML = data.ReleaseDetails || '';
};

