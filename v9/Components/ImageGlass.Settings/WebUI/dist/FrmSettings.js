﻿!function(e,t){"object"==typeof exports&&"object"==typeof module?module.exports=t():"function"==typeof define&&define.amd?define("ig-ui",[],t):"object"==typeof exports?exports["ig-ui"]=t():e["ig-ui"]=t()}(this,(()=>(()=>{"use strict";var e={r:e=>{"undefined"!=typeof Symbol&&Symbol.toStringTag&&Object.defineProperty(e,Symbol.toStringTag,{value:"Module"}),Object.defineProperty(e,"__esModule",{value:!0})}},t={};e.r(t);class n{static load(){for(const e in _pageSettings.lang){if(!Object.prototype.hasOwnProperty.call(_pageSettings.lang,e))continue;const t=_pageSettings.lang[e];queryAll(`[lang-text="${e}"]`).forEach((e=>{e.innerText=t})),queryAll(`[lang-title="${e}"]`).forEach((e=>{e.title=t})),queryAll(`[lang-html="${e}"]`).forEach((e=>{let n=t;for(let t=0;t<e.childElementCount;t++)n=n.replaceAll(`{${t}}`,e.children.item(t).outerHTML);e.innerHTML=n}))}}}const a=(e,t)=>new Promise((n=>{setTimeout((()=>n(t)),e)})),o=e=>{const t={},n=[...queryAll(`[tab="${e}"] input[name]`),...queryAll(`[tab="${e}"] select[name]`)];for(const e of n){const n=e.name;let a="";e.name.startsWith("_")||e.checkValidity()&&(a="checkbox"===e.type?e.checked:"number"===e.type?+e.value:e.value,a!==_pageSettings.config[n]&&(t[n]=a))}return t},s=e=>e.replace(/&/g,"&amp;").replace(/</g,"&lt;").replace(/>/g,"&gt;").replace(/"/g,"&quot;"),i=async(e,t,n={},o,s)=>{let i=!1;const r=query(e);for(r.classList.remove("dialog--create","dialog--edit"),r.classList.add(`dialog--${t}`),r.addEventListener("click",(async e=>{const t=e.target,n=t.getBoundingClientRect();(n.left>e.clientX||n.right<e.clientX||n.top>e.clientY||n.bottom<e.clientY)&&(t.classList.add("ani-shaking"),await a(500),t.classList.remove("ani-shaking"))}),!1),r.addEventListener("close",(()=>i=!0),!1),query(`${e} > form`).addEventListener("submit",(async e=>{s&&await Promise.resolve(s(e))})),console.log(n),Object.keys(n).forEach((t=>{const a=query(`${e} [name="_${t}"]`);a&&(a.value=n[t])})),o&&await Promise.resolve(o(r)),r.showModal();!i;)await a(100);return r},r=async(e,t,a)=>{const o=query(e);let s="";for(const e of t)s+=`\n    <li class="hotkey-item">\n      <kbd>${e}</kbd>\n      <button type="button" class="btn--icon" lang-title="_._Delete" data-action="delete">\n        ${_pageSettings.icons.Delete}\n      </button>\n    </li>`;s+='<li>\n    <button type="button" lang-title="_._AddHotkey" data-action="add">[Add hotkey…]</button>\n  </li>',o.innerHTML=s,n.load(),queryAll(`${e} button[data-action]`).forEach((t=>{t.addEventListener("click",(async()=>{const n=t.getAttribute("data-action"),o=queryAll(`${e} .hotkey-item > kbd`).map((e=>e.innerText));if("delete"===n){const e=t.closest(".hotkey-item");e?.remove(),a&&await Promise.resolve(a(n))}else if("add"===n){const t=await(async()=>await postAsync("OpenHotkeyPicker"))();if(!t)return;r(e,[...o,t],a),a&&await Promise.resolve(a(n)),query(`${e} button[data-action="add"]`)?.focus()}}),!1)}))},l=(e,t)=>{_webview.addEvent(e,t)};class d{static loadSettings(){d.loadThemeList(),d.loadThemeListStatus(),d.handleBackgroundColorChanged(),d.handleSlideshowBackgroundColorChanged()}static loadThemeListStatus(){const e=query('[name="DarkTheme"]').value,t=query('[name="LightTheme"]').value,n=query(`[name="_DarkThemeOptions"][value="${e}"]`),a=query(`[name="_LightThemeOptions"][value="${t}"]`);n&&(n.checked=!0),a&&(a.checked=!0)}static addEvents(){query("#Lnk_ResetBackgroundColor").addEventListener("click",d.resetBackgroundColor,!1),query("#Lnk_ResetSlideshowBackgroundColor").addEventListener("click",d.resetSlideshowBackgroundColor,!1),query("#Btn_BackgroundColor").addEventListener("click",(async()=>{const e=query('[name="BackgroundColor"]'),t=await postAsync("Btn_BackgroundColor",e.value);t&&(e.value=t,d.handleBackgroundColorChanged())}),!1),query("#Btn_SlideshowBackgroundColor").addEventListener("click",(async()=>{const e=query('[name="SlideshowBackgroundColor"]'),t=await postAsync("Btn_SlideshowBackgroundColor",e.value);t&&(e.value=t,d.handleSlideshowBackgroundColorChanged())}),!1),query("#Btn_InstallTheme").addEventListener("click",(async()=>{const e=await postAsync("Btn_InstallTheme");d.loadThemeList(e)}),!1),query("#Btn_RefreshThemeList").addEventListener("click",(async()=>{const e=await postAsync("Btn_RefreshThemeList");d.loadThemeList(e)}),!1),query("#Btn_OpenThemeFolder").addEventListener("click",(()=>post("Btn_OpenThemeFolder")),!1)}static exportSettings(){const e=o("appearance");return e.DarkTheme=query('[name="DarkTheme"]').value,e.DarkTheme===_pageSettings.config.DarkTheme&&delete e.DarkTheme,e.LightTheme=query('[name="LightTheme"]').value,e.LightTheme===_pageSettings.config.LightTheme&&delete e.LightTheme,e}static loadBackgroundColorConfig(e){_pageSettings.config.BackgroundColor=e,query('[name="BackgroundColor"]').value=e,d.handleBackgroundColorChanged()}static loadThemeList(e){Array.isArray(e)&&e.length>0&&(_pageSettings.themeList=e);const t=_pageSettings.themeList||[],a=query("#List_ThemeList");let o="";for(const e of t){o+=`\n        <li>\n          <div class="theme-item">\n            <div class="theme-preview">\n              <div class="theme-preview-img" title="${e.FolderPath}">\n                <img src="${e.PreviewImage}" alt="${e.Info.Name}" onerror="this.hidden = true;" />\n                <span class="theme-mode ${e.IsDarkMode?"theme-dark":"theme-light"}"\n                  lang-title="FrmSettings.Tab.Appearance.${e.IsDarkMode?"_DarkTheme":"_LightTheme"}">\n                  ${e.IsDarkMode?"🌙":"☀️"}\n                </span>\n              </div>\n            </div>\n            <div class="theme-info">\n              <div class="theme-heading" title="${e.Info.Name} - v${e.Info.Version}">\n                <span class="theme-name">${e.Info.Name}</span>\n                <span class="theme-version">${e.Info.Version}</span>\n              </div>\n              <div class="theme-description" title="${e.Info.Description}">${e.Info.Description}</div>\n              <div class="theme-author">\n                <span class="me-4" title="${e.Info.Author}">\n                  <span lang-text="FrmSettings.Tab.Appearance._Author">[Author]</span>:\n                  ${e.Info.Author||"?"}\n                </span>\n                <span class="me-4" title="${e.Info.Website}">\n                  <span lang-text="_._Website">[Website]</span>:\n                  ${e.Info.Website||"?"}\n                </span>\n                <span title="${e.Info.Email}">\n                  <span lang-text="_._Email">[Email]</span>:\n                  ${e.Info.Email||"?"}\n                </span>\n              </div>\n              <div class="theme-actions">\n                <label lang-title="FrmSettings.Tab.Appearance._UseThemeForDarkMode">\n                  <input type="radio" name="_DarkThemeOptions" value="${e.FolderName}" />\n                  <span>\n                    ${_pageSettings.icons.Moon}\n                    <span lang-text="FrmSettings.Tab.Appearance._DarkTheme">[Dark]</span> \n                  </span>\n                </label>\n                <label lang-title="FrmSettings.Tab.Appearance._UseThemeForLightMode">\n                  <input type="radio" name="_LightThemeOptions" value="${e.FolderName}" />\n                  <span>\n                    ${_pageSettings.icons.Sun}\n                    <span lang-text="FrmSettings.Tab.Appearance._LightTheme">[Light]</span>\n                  </span>\n                </label>\n\n                <button type="button" class="btn--icon ms-3 px-1"\n                  lang-title="_._Delete"\n                  ${_pageSettings.defaultThemeDir===e.FolderPath?'style="visibility: hidden !important;"':""}\n                  data-delete-theme="${e.FolderPath}">\n                  ${_pageSettings.icons.Delete}\n                </button>\n              </div>\n            </div>\n          </div>\n        </li>`}a.innerHTML=o,n.load(),d.loadThemeListStatus(),queryAll('[name="_DarkThemeOptions"]').forEach((e=>{e.addEventListener("change",(e=>{const t=e.target.value;query('[name="DarkTheme"]').value=t}),!1)})),queryAll('[name="_LightThemeOptions"]').forEach((e=>{e.addEventListener("change",(e=>{const t=e.target.value;query('[name="LightTheme"]').value=t}),!1)})),queryAll("[data-delete-theme]").forEach((e=>{e.addEventListener("click",(async e=>{const t=e.target.getAttribute("data-delete-theme"),n=await postAsync("Delete_Theme_Pack",t);d.loadThemeList(n)}),!1)}))}static resetBackgroundColor(){const e="light"!==document.documentElement.getAttribute("color-mode")?_pageSettings.config.DarkTheme:_pageSettings.config.LightTheme,t=_pageSettings.themeList.find((t=>t.FolderName===e));if(!t)return;const n=t.BgColor||"#00000000";query('[name="BackgroundColor"]').value=n,d.handleBackgroundColorChanged()}static resetSlideshowBackgroundColor(){query('[name="SlideshowBackgroundColor"]').value="#000000",d.handleSlideshowBackgroundColorChanged()}static handleBackgroundColorChanged(){const e=query('[name="BackgroundColor"]').value;e&&(query("#Btn_BackgroundColor > .color-display").style.setProperty("--color-picker-value",e),query("#Lbl_BackgroundColorValue").innerText=e)}static handleSlideshowBackgroundColorChanged(){const e=query('[name="SlideshowBackgroundColor"]').value;e&&(query("#Btn_SlideshowBackgroundColor > .color-display").style.setProperty("--color-picker-value",e),query("#Lbl_SlideshowBackgroundColorValue").innerText=e)}}class c{static addEvents(){const e=Array.from(document.querySelectorAll('input[name="nav"]'));for(let t=0;t<e.length;t++){e[t].addEventListener("change",(e=>{const t=e.target.value;c.setActiveMenu(t)}),!1)}}static setActiveMenu(e){e||="general";const t=query(`.tab-page[tab="${e}"]`);if(!t)return;queryAll(".tab-page").forEach((e=>e.classList.remove("active"))),t.classList.add("active");queryAll('input[type="radio"]').forEach((e=>e.checked=!1));const n=query(`input[type="radio"][value="${e}"]`);n&&(n.checked=!0),"appearance"===e&&d.loadThemeListStatus(),post("Sidebar_Changed",e)}}class g{static get isOriginalAutoUpdateEnabled(){return"0"!==_pageSettings.config.AutoUpdate}static loadSettings(){query("#Lnk_StartupDir").title=_pageSettings.startUpDir||"(unknown)",query("#Lnk_ConfigDir").title=_pageSettings.configDir||"(unknown)",query("#Lnk_UserConfigFile").title=_pageSettings.userConfigFilePath||"(unknown)",query('[name="AutoUpdate"]').checked=g.isOriginalAutoUpdateEnabled}static addEvents(){query("#Lnk_StartupDir").addEventListener("click",(()=>post("Lnk_StartupDir",_pageSettings.startUpDir)),!1),query("#Lnk_ConfigDir").addEventListener("click",(()=>post("Lnk_ConfigDir",_pageSettings.configDir)),!1),query("#Lnk_UserConfigFile").addEventListener("click",(()=>post("Lnk_UserConfigFile",_pageSettings.userConfigFilePath)),!1)}static exportSettings(){const e=o("general");return!0===e.AutoUpdate!==g.isOriginalAutoUpdateEnabled?e.AutoUpdate=e.AutoUpdate?(new Date).toISOString():"0":delete e.AutoUpdate,e}}class u{static loadSettings(){const e=_pageSettings.config.ColorProfile||"";e.includes(".")&&(query('[name="ColorProfile"]').value="Custom",query("#Lnk_CustomColorProfile").innerText=e),u.handleColorProfileChanged(),u.handleUseEmbeddedThumbnailOptionsChanged()}static addEvents(){query("#Btn_BrowseColorProfile").addEventListener("click",(async()=>{const e=await postAsync("Btn_BrowseColorProfile");query("#Lnk_CustomColorProfile").innerText=e}),!1),query("#Lnk_CustomColorProfile").addEventListener("click",(()=>{const e=query("#Lnk_CustomColorProfile").innerText.trim();post("Lnk_CustomColorProfile",e)}),!1),query('[name="ColorProfile"]').addEventListener("change",u.handleColorProfileChanged,!1),query('[name="UseEmbeddedThumbnailRawFormats"]').addEventListener("input",u.handleUseEmbeddedThumbnailOptionsChanged,!1),query('[name="UseEmbeddedThumbnailOtherFormats"]').addEventListener("input",u.handleUseEmbeddedThumbnailOptionsChanged,!1)}static exportSettings(){const e=o("image");e.ImageBoosterCacheCount=+(e.ImageBoosterCacheCount||0),e.ImageBoosterCacheCount===_pageSettings.config.ImageBoosterCacheCount&&delete e.ImageBoosterCacheCount;const t=_pageSettings.config.ColorProfile;return"Custom"===e.ColorProfile&&(e.ColorProfile=query("#Lnk_CustomColorProfile").innerText.trim()),e.ColorProfile&&e.ColorProfile!==t||delete e.ColorProfile,e}static handleColorProfileChanged(){const e="Custom"===query('[name="ColorProfile"]').value;query("#Btn_BrowseColorProfile").hidden=!e,query("#Section_CustomColorProfile").hidden=!e}static handleUseEmbeddedThumbnailOptionsChanged(){const e=query('[name="UseEmbeddedThumbnailRawFormats"]').checked,t=query('[name="UseEmbeddedThumbnailOtherFormats"]').checked,n=e||t;query("#Section_EmbeddedThumbnailSize").hidden=!n}}class m{static loadSettings(){m.handleUseRandomIntervalForSlideshowChanged(),m.handleSlideshowIntervalsChanged()}static addEvents(){query('[name="UseRandomIntervalForSlideshow"]').addEventListener("input",m.handleUseRandomIntervalForSlideshowChanged,!1),query('[name="SlideshowInterval"]').addEventListener("input",m.handleSlideshowIntervalsChanged,!1),query('[name="SlideshowIntervalTo"]').addEventListener("input",m.handleSlideshowIntervalsChanged,!1)}static exportSettings(){return o("slideshow")}static handleSlideshowIntervalsChanged(){const e=query('[name="SlideshowInterval"]'),t=query('[name="SlideshowIntervalTo"]');e.max=t.value,t.min=e.value;const n=+e.value||5,a=+t.value||5,o=m.toTimeString(n),s=m.toTimeString(a),i=query('[name="UseRandomIntervalForSlideshow"]').checked;query("#Lbl_SlideshowInterval").innerText=i?`${o} - ${s}`:o}static handleUseRandomIntervalForSlideshowChanged(){const e=query('[name="UseRandomIntervalForSlideshow"]').checked;query("#Lbl_SlideshowIntervalFrom").hidden=!e,query("#Section_SlideshowIntervalTo").hidden=!e}static toTimeString(e){const t=new Date(1e3*e);let n=t.getUTCMinutes().toString(),a=t.getUTCSeconds().toString();const o=t.getUTCMilliseconds().toString();return n.length<2&&(n=`0${n}`),a.length<2&&(a=`0${a}`),`${n}:${a}.${o}`}}class h{static loadSettings(){query("#Cmb_MouseWheel_Scroll").value=_pageSettings.config.MouseWheelActions?.Scroll||"DoNothing",query("#Cmb_MouseWheel_CtrlAndScroll").value=_pageSettings.config.MouseWheelActions?.CtrlAndScroll||"DoNothing",query("#Cmb_MouseWheel_ShiftAndScroll").value=_pageSettings.config.MouseWheelActions?.ShiftAndScroll||"DoNothing",query("#Cmb_MouseWheel_AltAndScroll").value=_pageSettings.config.MouseWheelActions?.AltAndScroll||"DoNothing"}static addEvents(){query("#Btn_ResetMouseWheelAction").addEventListener("click",h.resetDefaultMouseWheelActions,!1)}static exportSettings(){const e=o("mouse"),t=query("#Cmb_MouseWheel_Scroll").value,n=query("#Cmb_MouseWheel_CtrlAndScroll").value,a=query("#Cmb_MouseWheel_ShiftAndScroll").value,s=query("#Cmb_MouseWheel_AltAndScroll").value,i={};return t!==_pageSettings.config.MouseWheelActions?.Scroll&&(i.Scroll=t),n!==_pageSettings.config.MouseWheelActions?.CtrlAndScroll&&(i.CtrlAndScroll=n),a!==_pageSettings.config.MouseWheelActions?.ShiftAndScroll&&(i.ShiftAndScroll=a),s!==_pageSettings.config.MouseWheelActions?.AltAndScroll&&(i.AltAndScroll=s),Object.keys(i).length>0?e.MouseWheelActions=i:delete e.MouseWheelActions,e}static resetDefaultMouseWheelActions(){query("#Cmb_MouseWheel_Scroll").value="Zoom",query("#Cmb_MouseWheel_CtrlAndScroll").value="PanVertically",query("#Cmb_MouseWheel_ShiftAndScroll").value="PanHorizontally",query("#Cmb_MouseWheel_AltAndScroll").value="BrowseImages"}}class p{static loadSettings(){}static addEvents(){}static exportSettings(){return o("keyboard")}}class y{static loadSettings(){y.handleLanguageChanged()}static addEvents(){query("#Cmb_LanguageList").removeEventListener("change",y.handleLanguageChanged,!1),query("#Btn_RefreshLanguageList").removeEventListener("click",y.onBtn_RefreshLanguageList,!1),query("#Lnk_InstallLanguage").removeEventListener("click",y.onLnk_InstallLanguage,!1),query("#Cmb_LanguageList").addEventListener("change",y.handleLanguageChanged,!1),query("#Btn_RefreshLanguageList").addEventListener("click",y.onBtn_RefreshLanguageList,!1),query("#Lnk_InstallLanguage").addEventListener("click",y.onLnk_InstallLanguage,!1)}static async onBtn_RefreshLanguageList(){const e=await postAsync("Btn_RefreshLanguageList");y.loadLanguageList(e)}static async onLnk_InstallLanguage(){const e=await postAsync("Lnk_InstallLanguage");y.loadLanguageList(e)}static exportSettings(){return o("language")}static handleLanguageChanged(){const e=query("#Cmb_LanguageList").value,t=_pageSettings.langList.find((t=>t.FileName===e));t&&(query("#Section_LanguageContributors").innerText=t.Metadata.Author)}static loadLanguageList(e){const t=query("#Cmb_LanguageList");for(;t.options.length;)t.remove(0);Array.isArray(e)&&e.length>0&&(_pageSettings.langList=e),_pageSettings.langList.forEach((e=>{let n=`${e.Metadata.LocalName} (${e.Metadata.EnglishName})`;e.FileName&&0!==e.FileName.length||(n=e.Metadata.EnglishName);const a=new Option(n,e.FileName);t.add(a)})),t.value=_pageSettings.config.Language,y.handleLanguageChanged()}}class v{static loadSettings(){}static addEvents(){}static exportSettings(){return o("edit")}}class L{static loadSettings(){const e=_pageSettings.config.ZoomLevels||[];query('[name="ZoomLevels"]').value=e.join("; ")}static addEvents(){query('[name="ZoomLevels"]').addEventListener("input",L.handleZoomLevelsChanged,!1),query('[name="ZoomLevels"]').addEventListener("blur",L.handleZoomLevelsBlur,!1)}static exportSettings(){const e=o("viewer");if(e.ZoomLevels=L.getZoomLevels(),query('[name="ZoomLevels"]').checkValidity()){const t=_pageSettings.config.ZoomLevels?.toString(),n=e.ZoomLevels?.toString();n===t&&delete e.ZoomLevels}else delete e.ZoomLevels;return e}static handleZoomLevelsChanged(){const e=query('[name="ZoomLevels"]');L.getZoomLevels().some((e=>!Number.isFinite(e)))?e.setCustomValidity("Value contains invalid characters. Only number, semi-colon are allowed."):e.setCustomValidity("")}static handleZoomLevelsBlur(){const e=query('[name="ZoomLevels"]');e.checkValidity()&&(e.value=L.getZoomLevels().join("; "))}static getZoomLevels(){return query('[name="ZoomLevels"]').value.split(";").map((e=>e.trim())).filter(Boolean).map((e=>parseFloat(e)))}}class S{static loadSettings(){}static addEvents(){}static exportSettings(){return o("toolbar")}}class _{static loadSettings(){}static addEvents(){}static exportSettings(){return o("gallery")}}class f{static loadSettings(){}static addEvents(){}static exportSettings(){return o("file_assocs")}}class b{static HOTKEY_SEPARATOR="#";static loadSettings(){b.loadToolList()}static addEvents(){query("#Btn_AddTool").addEventListener("click",(async()=>{const e={ToolId:"",ToolName:"",Executable:"",Arguments:_pageSettings.FILE_MACRO,Hotkeys:[],IsIntegrated:!1};await i("#Dialog_AddOrEditTool","create",e,(async()=>{b.addEventsForToolDialog(),b.updateToolCommandPreview(),await r("#Dialog_AddOrEditTool .hotkey-list",e.Hotkeys)}));const t=b.getToolDialogFormData();b.setToolItemToList(t.ToolId,t)}),!1)}static exportSettings(){const e=o("tools"),t=JSON.stringify(_pageSettings.toolList||[]);e.Tools=b.getToolListFromDom();return JSON.stringify(e.Tools)===t&&delete e.Tools,e}static loadToolList(e){const t=e??_pageSettings.toolList??[],a=query("#Table_ToolList > tbody");let o="";const i=`\n      <button type="button" class="btn--icon px-1 ms-1" lang-title="_._Delete" data-action="delete">\n        ${_pageSettings.icons.Delete}\n      </button>\n    `;for(const e of t){let t='<i lang-text="_._Empty"></i>';e.Arguments&&(t=`<code>${s(e.Arguments)}</code>`);const n=(e.Hotkeys||[]).map(((e,t)=>`<kbd class="${0===t?"":"ms-1"}">${e}</kbd>`)).join(""),a=`\n        <label class="ig-checkbox">\n          <input type="checkbox" disabled ${!0===e.IsIntegrated?"checked":""} />\n          <div></div>\n        </label>\n      `;o+=`\n        <tr data-tool-id="${e.ToolId}"\n          data-tool-name="${e.ToolName}"\n          data-tool-integrated="${e.IsIntegrated}"\n          data-tool-executable="${e.Executable}"\n          data-tool-arguments="${e.Arguments}"\n          data-tool-hotkeys="${(e.Hotkeys||[]).join(b.HOTKEY_SEPARATOR)}">\n          <td class="cell-counter"></td>\n          <td class="cell-sticky text-nowrap">${e.ToolId}</td>\n          <td class="text-nowrap">${e.ToolName}</td>\n          <td class="text-center">${a}</td>\n          <td class="text-nowrap">${n}</td>\n          <td class="text-nowrap">\n            <code>${s(e.Executable)}</code>\n          </td>\n          <td class="text-nowrap" style="--cell-border-right-color: transparent;">${t}</td>\n          <td class="cell-sticky-right text-nowrap" width="1" style="border-left: 0;">\n            <button type="button" class="btn--icon px-1" lang-title="_._Edit" data-action="edit">\n              ${_pageSettings.icons.Edit}\n            </button>\n            ${"Tool_ExifGlass"!==e.ToolId?i:""}\n          </td>\n        </tr>\n      `}a.innerHTML=o,n.load(),queryAll("#Table_ToolList button[data-action]").forEach((e=>{e.addEventListener("click",(async()=>{const t=e.getAttribute("data-action"),n=e.closest("tr"),a=n.getAttribute("data-tool-id");"delete"===t?n.remove():"edit"===t&&(await b.editTool(a),e.focus())}),!1)}))}static getToolListFromDom(){return queryAll("#Table_ToolList > tbody > tr").map((e=>({ToolId:e.getAttribute("data-tool-id")??"",ToolName:e.getAttribute("data-tool-name")??"",IsIntegrated:"true"===e.getAttribute("data-tool-integrated"),Executable:e.getAttribute("data-tool-executable")??"",Arguments:e.getAttribute("data-tool-arguments")??"",Hotkeys:(e.getAttribute("data-tool-hotkeys")??"").split(b.HOTKEY_SEPARATOR).filter(Boolean)})))}static async editTool(e){const t=query(`#Table_ToolList tr[data-tool-id="${e}"]`),n=(t.getAttribute("data-tool-hotkeys")||"").split(b.HOTKEY_SEPARATOR).filter(Boolean);let a={ToolId:e,ToolName:t.getAttribute("data-tool-name")||"",Executable:t.getAttribute("data-tool-executable")||"",Arguments:t.getAttribute("data-tool-arguments")||"",IsIntegrated:"true"===t.getAttribute("data-tool-integrated"),Hotkeys:n};await i("#Dialog_AddOrEditTool","edit",a,(async()=>{query('[name="_IsIntegrated"]').checked=a.IsIntegrated??!1,b.addEventsForToolDialog(),b.updateToolCommandPreview(),await r("#Dialog_AddOrEditTool .hotkey-list",a.Hotkeys)})),a=b.getToolDialogFormData(),b.setToolItemToList(e,a)}static getToolDialogFormData(){return{ToolId:query('#Dialog_AddOrEditTool [name="_ToolId"]').value.trim(),ToolName:query('#Dialog_AddOrEditTool [name="_ToolName"]').value.trim(),Executable:query('#Dialog_AddOrEditTool [name="_Executable"]').value.trim(),Arguments:query('#Dialog_AddOrEditTool [name="_Arguments"]').value.trim(),Hotkeys:queryAll("#Dialog_AddOrEditTool .hotkey-list > .hotkey-item > kbd").map((e=>e.innerText)),IsIntegrated:query('#Dialog_AddOrEditTool [name="_IsIntegrated"]').checked}}static setToolItemToList(e,t){if(!t.ToolId||!t.ToolName||!t.Executable)return;const n=b.getToolListFromDom(),a=n.findIndex((t=>t.ToolId===e));-1!==a?n[a]=t:n.push(t),b.loadToolList(n)}static addEventsForToolDialog(){query('[name="_Executable"]').removeEventListener("input",b.updateToolCommandPreview,!1),query('[name="_Executable"]').addEventListener("input",b.updateToolCommandPreview,!1),query('[name="_Arguments"]').removeEventListener("input",b.updateToolCommandPreview,!1),query('[name="_Arguments"]').addEventListener("input",b.updateToolCommandPreview,!1),query("#btnBrowseTool").removeEventListener("click",b.handleBtnBrowseToolClickEvent,!1),query("#btnBrowseTool").addEventListener("click",b.handleBtnBrowseToolClickEvent,!1)}static updateToolCommandPreview(){let e=query('[name="_Executable"]').value||"";e=e.trim();let t=query('[name="_Arguments"]').value||"";t=t.trim().replaceAll("<file>",'"C:\\fake dir\\photo.jpg"'),query("#Tool_CommandPreview").innerText=[e,t].filter(Boolean).join(" ")}static async handleBtnBrowseToolClickEvent(){const e=await(async e=>await postAsync("OpenFilePicker",e||{},!0))()??[];e.length&&(query('[name="_Executable"]').value=`"${e[0]}"`,b.updateToolCommandPreview())}}const E=b;class T{static loadSettings(){}static addEvents(){queryAll('[tab="layout"] .app-layout button[draggable="true"]').forEach((e=>{e.removeEventListener("dragstart",T.handleLayoutItemDragStart,!1),e.addEventListener("dragstart",T.handleLayoutItemDragStart,!1)})),queryAll('[tab="layout"] .app-layout .region-drop').forEach((e=>{e.removeEventListener("dragover",(e=>T.handleLayoutItemDragOver(e)),!1),e.addEventListener("dragover",(e=>T.handleLayoutItemDragOver(e)),!1),e.removeEventListener("dragenter",(t=>T.handleLayoutItemDragEnter(t,e)),!1),e.addEventListener("dragenter",(t=>T.handleLayoutItemDragEnter(t,e)),!1),e.removeEventListener("dragleave",(t=>T.handleLayoutItemDragLeave(t,e)),!1),e.addEventListener("dragleave",(t=>T.handleLayoutItemDragLeave(t,e)),!1),e.removeEventListener("dragend",(e=>T.handleLayoutItemDragEnd(e)),!1),e.addEventListener("dragend",(e=>T.handleLayoutItemDragEnd(e)),!1),e.removeEventListener("drop",(t=>T.handleLayoutItemDrop(t,e)),!1),e.addEventListener("drop",(t=>T.handleLayoutItemDrop(t,e)),!1)}))}static exportSettings(){return o("layout")}static handleLayoutItemDragStart(e){const t=e.target,n=t.getAttribute("data-control"),a=t.closest("[data-position]").getAttribute("data-position"),o=t.closest("[data-order]").getAttribute("data-order"),s=JSON.stringify({fromControlName:n,fromPosition:a,fromOrder:o});e.dataTransfer.setData("application/json",s),e.dataTransfer.effectAllowed="move",n.toLowerCase().includes("toolbar")&&(query('[data-position="Left"] .region-drop').classList.add("nodrop"),query('[data-position="Right"] .region-drop').classList.add("nodrop")),queryAll('.app-layout button[draggable="true"]').forEach((e=>{e.style.pointerEvents="none"})),t.style.pointerEvents=""}static handleLayoutItemDragOver(e){e.preventDefault(),e.dataTransfer.dropEffect="move"}static handleLayoutItemDragEnter(e,t){e.preventDefault(),t.classList.add("drag--enter")}static handleLayoutItemDragLeave(e,t){e.preventDefault(),t.classList.remove("drag--enter")}static handleLayoutItemDragEnd(e){e.preventDefault(),query('[data-position="Left"] .region-drop').classList.remove("nodrop"),query('[data-position="Right"] .region-drop').classList.remove("nodrop"),queryAll('.app-layout button[draggable="true"]').forEach((e=>{e.style.pointerEvents=""}))}static handleLayoutItemDrop(e,t){e.preventDefault(),t.classList.remove("drag--enter");const n=t.querySelector('button[draggable="true"]'),a=n?.getAttribute("data-control")||"",o=e.dataTransfer.getData("application/json")||"{}",{fromControlName:s,fromPosition:i,fromOrder:r}=JSON.parse(o)||{};if(!s||!i||!r)return;const l=t.closest("[data-position]").getAttribute("data-position"),d=t.getAttribute("data-order");if(s.toLowerCase().includes("toolbar")&&("Left"===l||"Right"===l)||a.toLowerCase().includes("toolbar")&&("Left"===i||"Right"===i))return;const c=query(`.app-layout [data-position="${i}"] .region-drop[data-order="${r}"]`),g=c.querySelector('button[draggable="true"]');n&&(c?.appendChild(n),query(`select[name="_Layout.${a}.Position"]`).value=i,query(`input[name="_Layout.${a}.Order"]`).value=r),t.appendChild(g),query(`select[name="_Layout.${s}.Position"]`).value=l,query(`input[name="_Layout.${s}.Order"]`).value=d}}class C{static load(){C.loadSelectBoxEnums(),y.loadLanguageList();for(const e in _pageSettings.config){if(!Object.prototype.hasOwnProperty.call(_pageSettings.config,e))continue;const t=_pageSettings.config[e];if(!("string"==typeof t||"number"==typeof t||"boolean"==typeof t))continue;const n=query(`[name="${e}"]`);if(!n)continue;const a=n.tagName.toLowerCase();if("select"===a)n.value=t.toString();else if("input"===a){const e=n.getAttribute("type").toLowerCase(),a=n;if("radio"===e||"checkbox"===e)a.checked=Boolean(t);else if("color"===e){const e=t.toString()||"#00000000";a.value=e.substring(0,e.length-2)}else a.value=t.toString()}}g.loadSettings(),u.loadSettings(),m.loadSettings(),v.loadSettings(),L.loadSettings(),S.loadSettings(),_.loadSettings(),T.loadSettings(),h.loadSettings(),p.loadSettings(),f.loadSettings(),E.loadSettings(),y.loadSettings(),d.loadSettings()}static addEventsForFooter(){query("#BtnCancel").addEventListener("click",(()=>post("BtnCancel")),!1),query("#BtnOK").addEventListener("click",(()=>{const e=C.getAllSettings();C.updateInitSettings(e),post("BtnOK",e,!0)}),!1),query("#BtnApply").addEventListener("click",(()=>{const e=C.getAllSettings();C.updateInitSettings(e),post("BtnApply",e,!0)}),!1)}static getAllSettings(){return{...g.exportSettings(),...u.exportSettings(),...m.exportSettings(),...v.exportSettings(),...L.exportSettings(),...S.exportSettings(),..._.exportSettings(),...T.exportSettings(),...h.exportSettings(),...p.exportSettings(),...f.exportSettings(),...E.exportSettings(),...y.exportSettings(),...d.exportSettings()}}static updateInitSettings(e){Object.keys(e).forEach((t=>{_pageSettings.config.hasOwnProperty(t)&&(_pageSettings.config[t]=e[t])}))}static loadSelectBoxEnums(){for(const e in _pageSettings.enums){if(!Object.prototype.hasOwnProperty.call(_pageSettings.enums,e))continue;const t=_pageSettings.enums[e],n=queryAll(`select[data-enum="${e}"]`);for(const a of n)t.forEach((t=>{const n=new Option(`${t}`,t);n.setAttribute("lang-text",`_.${e}._${t}`),a.add(n)}))}}}return window._webview=new class{eventHandlers={};addEvent(e,t){this.eventHandlers[e]=t}removeEvent(e){delete this.eventHandlers[e]}startListening(){window.chrome.webview?.addEventListener("message",(({data:e})=>{const t=e?.Name??"",n=e?.Data??"",a=this.eventHandlers[t],o=!!a;console.info(`Received event '${t}' (handler=${o}) with data:`,n),o&&a(t,n)}))}},_webview.startListening(),window.query=e=>{try{return document.querySelector(e)}catch{}return null},window.queryAll=e=>{try{return Array.from(document.querySelectorAll(e))}catch{}return[]},window.on=l,window.post=(e,t,n)=>{const a=n?JSON.stringify(t):t;window.chrome.webview?.postMessage({name:e,data:a})},window.postAsync=async(e,t,n)=>{let o=!1,s=null;l(e,((t,n)=>{t===e&&(o=!0,s=n,_webview.removeEvent(e))}));const i=n?JSON.stringify(t):t;for(window.chrome.webview?.postMessage({name:e,data:i});!o;)await a(100);return s},window._pageSettings||(window._pageSettings={initTab:"layout",config:{},lang:{},langList:[],toolList:[],themeList:[],enums:{ImageOrderBy:[],ImageOrderType:[],ColorProfileOption:[],AfterEditAppAction:[],ImageInterpolation:[],MouseWheelAction:[],MouseWheelEvent:[],MouseClickEvent:[],BackdropStyle:[],ToolbarItemModelType:[]},icons:{Delete:"",Edit:"",Moon:"",Sun:""},startUpDir:"",configDir:"",userConfigFilePath:"",defaultThemeDir:"",FILE_MACRO:""}),_pageSettings.setSidebarActiveMenu=c.setActiveMenu,_pageSettings.loadLanguage=n.load,_pageSettings.loadSettings=C.load,_pageSettings.loadLanguageList=y.loadLanguageList,_pageSettings.loadBackgroundColorConfig=d.loadBackgroundColorConfig,c.addEvents(),C.load(),n.load(),C.addEventsForFooter(),g.addEvents(),u.addEvents(),m.addEvents(),v.addEvents(),L.addEvents(),S.addEvents(),_.addEvents(),T.addEvents(),h.addEvents(),f.addEvents(),E.addEvents(),y.addEvents(),d.addEvents(),c.setActiveMenu(_pageSettings.initTab),t})()));
//# sourceMappingURL=FrmSettings.js.map