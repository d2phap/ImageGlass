import { ILanguage } from './@types/FrmSettings';
import './main';

const _stepCount = queryAll('.page-content .step').length;
let _currentStep = 1;

let _langList: ILanguage[] = [];
let _currentLang = '';

const goToStep = (stepNumber: number) => {
  _currentStep = Math.max(1, Math.min(stepNumber, _stepCount));
  const isLastStep = _currentStep >= _stepCount;

  queryAll('.page-content .step').forEach(el => {
    el.hidden = el.getAttribute('step') !== `${_currentStep}`;
  });

  query('#LblStepInfo').innerText = `${_currentStep}/${_stepCount}`;
  query('#BtnNext').innerText = _page.lang[isLastStep ? '_._Save' : '_._Next'] || '[Next]';
  query('#BtnBack').hidden = _currentStep <= 1;
};


/**
 * Loads language list to select box.
 */
const loadLanguageList = () => {
  const selectEl = query<HTMLSelectElement>('#Cmb_LanguageList');

  // clear current list
  while (selectEl.options.length) selectEl.remove(0);

  _langList.forEach((lang: ILanguage) => {
    let displayText = `${lang.Metadata.LocalName} (${lang.Metadata.EnglishName})`;
    if (!lang.FilePath || lang.FilePath.length === 0) {
      displayText = lang.Metadata.EnglishName;
    }

    const optionEl = new Option(displayText, lang.FilePath);
    selectEl.add(optionEl);
  });

  selectEl.value = _currentLang;
  // TabLanguage.handleLanguageChanged();
};


// footer actions
query('#LnkSkip').addEventListener('click', (e) => post((e.target as HTMLElement).id), false);
query('#BtnBack').addEventListener('click', () => {
  _currentStep--;
  goToStep(_currentStep);
}, false);
query('#BtnNext').addEventListener('click', (e) => {
  const btnEl = e.target as HTMLButtonElement;

  if (_currentStep >= _stepCount) {
    post(btnEl.id);
  }
  else {
    _currentStep++;
    goToStep(_currentStep);
  }
}, false);


// load initial data
window._page.loadData = (data: Record<string, any> = {}) => {
  console.log(data);
  _currentLang = data.currentLangName;
  _langList = data.langList || [];
  loadLanguageList();

  query('#Img_AppLogo').setAttribute('src', data.appLogo || '');
  query('#Img_AppLogo').toggleAttribute('hidden', false);
};


// setting profile options
queryAll<HTMLInputElement>('[name="_SettingProfile"]').forEach(el => {
  el.addEventListener('change', () => {
    const profileName = query<HTMLInputElement>('[name="_SettingProfile"]:checked').value;
    const isProfessional = profileName === 'professional';

    query<HTMLInputElement>('[name="UseEmbeddedThumbnailRawFormats"]').checked = !isProfessional;
    query<HTMLInputElement>('[name="ColorProfile"]').checked = isProfessional;
  });
});
