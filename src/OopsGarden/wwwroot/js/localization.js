import { $, qsa } from "./dom.js";
import { resourceVersion, state } from "./state.js";

export async function loadLanguage(lang) {
    const response = await fetch(`/resources/${lang}.json?v=${resourceVersion}`);
    state.dict = await response.json();
    state.lang = lang;
    document.documentElement.lang = lang;
    localStorage.setItem("oopsGarden.lang", lang);
    $("languageSelect").value = lang;
    qsa("[data-i18n]").forEach((el) => {
        el.textContent = t(el.dataset.i18n);
    });
    qsa("[data-i18n-placeholder]").forEach((el) => {
        el.placeholder = t(el.dataset.i18nPlaceholder);
    });
    qsa("[data-i18n-aria-label]").forEach((el) => {
        el.setAttribute("aria-label", t(el.dataset.i18nAriaLabel));
    });
}

export function t(key) {
    return state.dict[key] || key;
}
