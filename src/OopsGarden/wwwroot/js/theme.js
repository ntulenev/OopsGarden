import { $ } from "./dom.js?v=20260602-1";
import { state } from "./state.js?v=20260602-1";

export function applyTheme(theme) {
    state.theme = theme === "dark-forest" ? "dark-forest" : "greenhouse";
    document.documentElement.dataset.theme = state.theme;
    localStorage.setItem("oopsGarden.theme", state.theme);
    $("themeSelect").value = state.theme;
}
