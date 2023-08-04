import { ILanguage } from './@types/FrmSettings';
import './main';

const _stepCount = queryAll('.page-content .step').length;
let _currentStep = 1;

let _langList: ILanguage[] = [];
let _currentLang = '';
let _loadLanguageTimer: any = null;

// Navigates to step
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


// Loads language list to select box.
const loadLanguageList = () => {
  const selectEl = query<HTMLSelectElement>('#Cmb_LanguageList');

  // clear current list
  while (selectEl.options.length) selectEl.remove(0);

  _langList.forEach((lang: ILanguage) => {
    let displayText = `${lang.Metadata.LocalName} (${lang.Metadata.EnglishName})`;
    if (!lang.FileName || lang.FileName.length === 0) {
      displayText = lang.Metadata.EnglishName;
    }

    const optionEl = new Option(displayText, lang.FileName);
    selectEl.add(optionEl);
  });

  selectEl.value = _currentLang;
};


// Loads language
const requestToUpdateLanguage = async (langName: string) => {
  _currentLang = langName;
  await postAsync('LOAD_LANGUAGE', _currentLang);
};


// delays loading language
const delayRequestToUpdateLanguage = (langName: string) => {
  clearTimeout(_loadLanguageTimer);

  return new Promise((resolve) => {
    _loadLanguageTimer = setTimeout(async () => {
      clearTimeout(_loadLanguageTimer);

      await requestToUpdateLanguage(langName);
      resolve(undefined);
    }, 100);
  });
};


// footer actions
query('#LnkSkip').addEventListener('click', () => post('SKIP_AND_LAUNCH'), false);
query('#BtnBack').addEventListener('click', () => {
  _currentStep--;
  goToStep(_currentStep);
}, false);
query('#BtnNext').addEventListener('click', () => {
  if (_currentStep >= _stepCount) {
    post('APPLY_SETTINGS');
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


// language option
query<HTMLSelectElement>('#Cmb_LanguageList').addEventListener('change', (e) => {
  delayRequestToUpdateLanguage((e.target as HTMLInputElement).value);
}, false);
