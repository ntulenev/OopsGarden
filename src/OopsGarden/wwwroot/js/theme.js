import { $ } from "./dom.js";
import { state } from "./state.js";

export function applyTheme(theme) {
    state.theme = theme === "dark-forest" ? "dark-forest" : "greenhouse";
    document.documentElement.dataset.theme = state.theme;
    localStorage.setItem("oopsGarden.theme", state.theme);
    $("themeSelect").value = state.theme;
}
